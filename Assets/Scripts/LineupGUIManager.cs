using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LineupGUIManager : MonoBehaviour
{
    public GameObject kit;
    public Main mainReference;
    public GameObject homeView, awayView;
    public GameObject homeAnchor, awayAnchor;
    public Image homeLogo, awayLogo;
    public TextMeshProUGUI predictionText;
    public GameObject confirmationButton, confirmationWindow;
    public float min, max;
    public Canvas canvas;
    public Sprite[] kitAdditions;
    public IOHandler iohandler;
    public GameObject formationInput;
    Gradient formGradient;
    Dictionary<int, List<HistoryEntry>> history;
    private bool formationPredictionHome = false;

    public ServerFileManager serverFileManager;
    public GameObject playerAdditionWindow,meanIndicator,minimumIndicator,maximumIndicator,playerAdditionNameText;
    Club playerAdditionClubChoice;

    Match match = null;
    private void Awake()
    {
        iohandler = GameObject.Find("Canvas").GetComponent<IOHandler>();
        (min, max) = mainReference.PlayerValueRange();
        match = mainReference.currentMatch;
        Debug.Log(match.GetHomeClub().GetName() + " : " + match.GetAwayClub().GetName());
        InitiateClubHeaders();
        homeLogo.sprite = match.GetHomeClub().GetLogo();
        awayLogo.sprite = match.GetAwayClub().GetLogo();
        InvokeRepeating("matchTest", 3, 0.25f);
        history = iohandler.LoadHistory(RankingGlobal.Status.online);
        AssignHistories();
        DrawPlayers(match.GetHomeClub(), homeAnchor, false);
        DrawPlayers(match.GetAwayClub(), awayAnchor, true);
    }

    private void Update()
    {
        confirmationButton.SetActive(PitchFilledLegally());
    }

    void InitiateClubHeaders()
    {
        homeView.GetComponentInChildren<TextMeshProUGUI>().text = match.GetHomeClub().GetName();
        homeView.transform.Find("Logo").GetComponent<Image>().sprite = match.GetHomeClub().GetLogo();
        awayView.GetComponentInChildren<TextMeshProUGUI>().text = match.GetAwayClub().GetName();
        awayView.transform.Find("Logo").GetComponent<Image>().sprite = match.GetAwayClub().GetLogo();
    }

    void DrawPlayers(Club c,GameObject anchor,bool away)
    {
        int xoffset = Screen.width / 18;
        int yoffset = -Screen.height / 9 ;
        int x = 0, y = 0;
        int count = 0;
        Sprite addition = ImageProcessor.RandomSprite(kitAdditions);
        foreach (Player player in c.GetPlayers())
        {
            GameObject go = Instantiate(kit,anchor.transform.position + new Vector3(x,y,0),Quaternion.identity,anchor.transform);
            go.GetComponentInChildren<TextMeshProUGUI>().text = player.name;
            go.name = player.name;
            Kit kit_inst = go.GetComponent<Kit>();
            kit_inst.player = player;
            kit_inst.canvas = canvas;
            kit_inst.player.away = away;
            kit_inst.manager = this;
            kit_inst.originalPosition = kit_inst.transform.position;
            go.transform.Find("KitBase").GetComponent<Image>().color = !away ? c.GetColor() : c.GetSecondaryColor();
            go.transform.Find("Addition").GetComponent<Image>().sprite = addition;
            go.transform.Find("Addition").GetComponent<Image>().color = !away ? c.GetSecondaryColor() : c.GetColor();
            go.transform.Find("Logo").GetComponent<Image>().sprite = c.GetLogo();
            go.transform.Find("FormIndicator").GetComponent<Image>().color = kit_inst.positionGradient.Evaluate(1f - player.form);
            go.transform.Find("FormIndicator").GetComponent<Image>().transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180f * (1f - player.form)));
            //go.transform.Find("name").GetComponent<TextMeshProUGUI>().color = Color.white - c.GetColor() + new Color(0,0,0,1);
            GameObject stars = go.transform.Find("rating").gameObject;
            Debug.Log(player.GetTrueValue());
            stars.GetComponent<RectTransform>().sizeDelta = new Vector2((player.GetTrueValue()-min)/(max-min) * 59f,19);
            x += xoffset;
            count++;
            if(count > 4)
            {
                count = 0;
                x = 0;
                y += yoffset;
            }
            if (player.linedUp)
            {
                float xmin = kit_inst.canvasWidth/2 + kit_inst.leftBoundary;
                float xmax = kit_inst.canvasWidth/2 + kit_inst.rightBoundary;
                float ymid = Screen.height / 2f;
                float off = away ? ymid + ymid * (1f - player.offensiveness) : ymid * player.offensiveness;
                Debug.Log("Lining Up " + player.name + ", y-coordinate = " + off + ", off = " + player.offensiveness + ", canvas height = 1080, screen height = " + Screen.height);
                go.GetComponent<RectTransform>().position = new Vector2(xmin + player.skewness * (xmax-xmin), off);
            }
        }
    }

    void MovePlayers(bool home)
    {
        bool away = !home;
        GameObject anchor = home ? homeAnchor : awayAnchor;
        foreach (Transform child in anchor.transform)
        {
            Kit kit_inst = child.gameObject.GetComponent<Kit>();
            Player p = kit_inst.player;
            if (p.linedUp)
            {
                float xmin = kit_inst.canvasWidth / 2 + kit_inst.leftBoundary * (Screen.width/1920f);
                float xmax = kit_inst.canvasWidth / 2 + kit_inst.rightBoundary * (Screen.width/1920f);
                float ymid = Screen.height / 2f;
                float poff = p.offensiveness == 0 ? 0.05f : p.offensiveness;
                float off = away ? ymid + ymid * (1f - poff) : ymid * poff;
                Debug.Log("Lining Up " + p.name + ", y-coordinate = " + off + ", off = " + p.offensiveness + ", canvas height = 1080, screen height = " + Screen.height);
                child.gameObject.GetComponent<RectTransform>().position = new Vector2(xmin + (away ? 1f - p.skewness : p.skewness) * (xmax - xmin), off);
                kit_inst.DragStartPosition = new Vector2(xmin + (away ? 1f - p.skewness : p.skewness) * (xmax - xmin), off);
            }
            else
            {
                child.gameObject.GetComponent<RectTransform>().position = kit_inst.originalPosition;
                kit_inst.DragStartPosition = new Vector2(kit_inst.originalPosition.x, kit_inst.originalPosition.y);
            }
        }

    }

    public void matchTest()
    {
        float h, a;
        (h,a) = match.Predict();
        predictionText.text = Mathf.Round(h) + " : " + Mathf.Round(a);
    }

    bool PitchFilledLegally()
    {
        int count = 0;
        foreach(Player player in match.GetHomeClub().GetPlayers())
        {
            if (player.linedUp) count++;
        }
        if(count != 11)
        {
            return false;
        }
        count = 0;
        foreach(Player player in match.GetAwayClub().GetPlayers())
        {
            if(player.linedUp) count++;
        }
        return count == 11;
    }

    public void ConfirmationWindow()
    {
        confirmationWindow.SetActive(true);
    }

    void AssignHistories()
    {
        foreach(Player p in match.GetHomeClub().GetPlayers())
        {
            if (history.ContainsKey(p.GetID()))
            {
                p.SetHistory(history[p.GetID()]);
            }
        }     
        foreach(Player p in match.GetAwayClub().GetPlayers())
        {
            if (history.ContainsKey(p.GetID()))
            {
                p.SetHistory(history[p.GetID()]);
            }
        }
    }

    public void PredictLineup(bool home)
    {
        Club c = home ? match.GetHomeClub() : match.GetAwayClub();
        formationInput.SetActive(true);
        formationInput.transform.SetAsLastSibling();
        formationPredictionHome = home;
    }

    public void ApplyFormationPrediction()
    {
        string text = formationInput.GetComponent<TMP_InputField>().text;
        if (formationPredictionHome)
        {
            match.GetHomeClub().OptimizeFormation(new Formation("1-" + text));
        }
        else
        {
            match.GetAwayClub().OptimizeFormation(new Formation("1-" + text));
        }
        formationInput.SetActive(false);
        MovePlayers(formationPredictionHome);
    }

    public Kit IntersectingKit(Vector2 position,bool home)
    {
        GameObject anchor = home ? homeAnchor : awayAnchor;
        foreach(Transform t in anchor.transform)
        {
            RectTransform rt = t.GetComponent<RectTransform>();
            if (RoughlyEqual(rt.position, position) && new Vector2(rt.position.x,rt.position.y) != position)
            {
                return t.gameObject.GetComponent<Kit>();
            }
        }
        return null;
    }

    public void InitiatePlayerAddition(int clubid)
    {
        Club club = clubid == 0 ? match.GetHomeClub() : match.GetAwayClub();
        playerAdditionClubChoice = club;
        playerAdditionWindow.SetActive(true);
        (float,float) minmax = club.MinMaxValues();
        Slider slider = playerAdditionWindow.transform.Find("Slider").GetComponent<Slider>();
        float mean = club.MeanValue();
        slider.minValue = minmax.Item1;
        slider.maxValue = minmax.Item2;
        slider.value = mean;
        meanIndicator.GetComponent<TextMeshProUGUI>().text = mean.ToString();
        minimumIndicator.GetComponent<TextMeshProUGUI>().text = minmax.Item1.ToString();
        maximumIndicator.GetComponent<TextMeshProUGUI>().text = minmax.Item2.ToString();
    }

    public void SubmitPlayerAddition()
    {
        if (playerAdditionClubChoice == null) return;
        string name = playerAdditionNameText.GetComponent<TextMeshProUGUI>().text;
        float value = playerAdditionWindow.transform.Find("Slider").GetComponent<Slider>().value;
        int clubid = playerAdditionClubChoice.GetID();
        int playerID = -1;
        List<int> playerIDs = new List<int>();
        foreach(Club c in mainReference.clubs)
        {
            foreach(Player player in c.GetPlayers())
            {
                playerIDs.Add(player.GetID());
            }
        }
        foreach(Player p in serverFileManager.LoadFreeAgents())
        {
            playerIDs.Add(p.GetID());
        }
        for(int i = 0; i < 9999; i++)
        {
            if (!playerIDs.Contains(i))
            {
                playerID = i;
                break;
            }
        }
        Player newplayer = new Player(playerID,name,value,value,0,0,0,clubid);
        mainReference.clubs[clubid].AddPlayer(newplayer);
        serverFileManager.AddPlayerToServer(newplayer);
        ReloadAll();
        playerAdditionWindow.SetActive(false);
    }

    bool RoughlyEqual(Vector2 x, Vector2 y)
    {
        float tol = 70f*(Screen.width/1920f);
        Vector2 diff = x - y;
        return Mathf.Abs(diff.x) < tol && Mathf.Abs(diff.y) < tol;
    }

    void ReloadAll()
    {
        foreach(Transform child in homeAnchor.transform)
        {
            Destroy(child.gameObject);
        }        
        foreach(Transform child in awayAnchor.transform)
        {
            Destroy(child.gameObject);
        }
        DrawPlayers(match.GetHomeClub(), homeAnchor, false);
        DrawPlayers(match.GetAwayClub(), awayAnchor, true);
    }
}
