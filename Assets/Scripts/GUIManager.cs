using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GUIManager : MonoBehaviour
{
    public Main mainController;
    public GameObject clubWindow;
    public GameObject textPrefab;
    public GameObject lineupButton;
    const int CLUB_OFFSET = 40;
    const string CLUB_PLACEHOLDER = "Auswählen...";
    public Button homeArrow, awayArrow;
    public Sprite leftArrow, rightArrow;
    public GameObject homeView, awayView;
    Club homeClub, awayClub;
    Club selectedClub = null;
    bool homeSelected, awaySelected;
    public IOHandler iohandler;
    
    bool changed = false;
    public Image homelogo, awaylogo;
    public Image homedef, homemid, homeatt, awaydef, awaymid, awayatt;


    private void Awake()
    {
        iohandler = GameObject.Find("Canvas").GetComponent<IOHandler>();

    }

    private void Update()
    {
        lineupButton.SetActive(homeSelected & awaySelected);
        if (changed)
        {
            if(homeClub != null)
            {
                homelogo.sprite = homeClub.GetLogo();
            }
            if (awayClub != null)
            {
                awaylogo.sprite = awayClub.GetLogo();
            }
            if(homeClub != null && awayClub != null)
            {
                UpdateStrengthIndicators();
            }
            changed = false;
        }
    }

    void UpdateStrengthIndicators()
    {
        PositionValueTuple home = homeClub.StrengthSummary();
        PositionValueTuple away = awayClub.StrengthSummary();
        float correction = Mathf.Min(home.Min(), away.Min()) - RankingGlobal.Constants.CHANGE_THRESHOLD;
        home -= correction;
        away -= correction;
        float homedefscale = home.Left() / (home.Left() + away.Left());
        float awaydefscale = away.Left() / (home.Left() + away.Left());
        float homemidscale = home.Center() / (home.Center() + away.Center());
        float awaymidscale = away.Center() / (home.Center() + away.Center());
        float homeattscale = home.Right() / (home.Right() + away.Right());
        float awayattscale = away.Right() / (home.Right() + away.Right());

        homedef.transform.localScale = new Vector3(2*homedefscale, 1, 1);
        homemid.transform.localScale = new Vector3(2*homemidscale, 1, 1);
        homeatt.transform.localScale = new Vector3(2*homeattscale, 1, 1);
        awaydef.transform.localScale = new Vector3(2*awaydefscale, 1, 1);
        awaymid.transform.localScale = new Vector3(2*awaymidscale, 1, 1);
        awayatt.transform.localScale = new Vector3(2*awayattscale, 1, 1);

    }

    public void DisplayClubs(List<Club> clubs)
    {
        float offset = 0;
        foreach(Club club in clubs)
        {
            if(club != homeClub && club != awayClub)
            {
                GameObject txt = Instantiate(textPrefab);
                //txt.GetComponent<Image>().color = club.GetColor();
                Button btn = txt.GetComponent<Button>();
                btn.onClick.AddListener((delegate { SelectClub(club); }));
                TextMeshProUGUI text = txt.GetComponentInChildren<TextMeshProUGUI>();
                txt.transform.Find("Logo").GetComponent<Image>().sprite = club.GetLogo();
                txt.transform.SetParent(clubWindow.transform, false);
                txt.transform.position = txt.transform.position + new Vector3(0, -offset, 0);
                text.text = club.GetName();
                offset += CLUB_OFFSET;
            }
        }
    }

    public void SelectClub(Club club)
    {
        selectedClub = club;
        Debug.Log(club.GetName());
    }

    void Redraw()
    {
        foreach(Transform tf in clubWindow.transform)
        {
            Destroy(tf.gameObject);
        }
        DisplayClubs(mainController.clubs);
    }

    public void SetHomeClub()
    {
        homeView.GetComponentInChildren<TextMeshProUGUI>().text = selectedClub.GetName();
        homeArrow.GetComponentInChildren<Image>().sprite = leftArrow;
        homeView.transform.Find("Logo").GetComponent<Image>().sprite = selectedClub.GetLogo();
        homeClub = selectedClub;
        homeArrow.onClick.RemoveAllListeners();
        homeArrow.onClick.AddListener(ResetHomeClub);
        homeSelected = true;
        changed = true;
        Redraw();
    }

    public void ResetHomeClub()
    {
        homeView.GetComponentInChildren<TextMeshProUGUI>().text = CLUB_PLACEHOLDER;
        homeArrow.GetComponentInChildren<Image>().sprite = rightArrow;
        homeView.transform.Find("Logo").GetComponent<Image>().sprite = null;
        homeClub = null;
        homeArrow.onClick.RemoveAllListeners();
        homeArrow.onClick.AddListener(SetHomeClub);
        homeSelected=false;
        changed = true;
        Redraw();
    }

    public void SetAwayClub()
    {
        awayView.GetComponentInChildren<TextMeshProUGUI>().text = selectedClub.GetName();
        awayArrow.GetComponentInChildren<Image>().sprite = leftArrow;
        awayView.transform.Find("Logo").GetComponent<Image>().sprite = selectedClub.GetLogo();
        awayClub = selectedClub;
        awayArrow.onClick.RemoveAllListeners();
        awayArrow.onClick.AddListener(ResetAwayClub);
        awaySelected = true;
        changed = true;
        Redraw();
    }

    public void ResetAwayClub()
    {
        awayView.GetComponentInChildren<TextMeshProUGUI>().text = CLUB_PLACEHOLDER;
        awayArrow.GetComponentInChildren<Image>().sprite = rightArrow;
        awayView.transform.Find("Logo").GetComponent<Image>().sprite = null;
        awayClub = null;
        awayArrow.onClick.RemoveAllListeners();
        awayArrow.onClick.AddListener(SetAwayClub);
        awaySelected=false;
        changed = true;
        Redraw();
    }

    public void MakeMatch()
    {
        Match match = new Match(homeClub,awayClub);
        match.iohandler = iohandler;
        mainController.currentMatch = match;
        Debug.Log(match.GetHomeClub().GetName() + " : " + match.GetAwayClub().GetName());
    }
}
