using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InformationPanel : MonoBehaviour
{
    public GameObject pitch;
    public IOHandler iohandler;
    public TextMeshProUGUI headline;
    public TextMeshProUGUI offRank,midRank,defRank;
    public GameObject positionIndicator;
    float pitchMinHeight = -220f;
    float pitchMaxHeight = 220f;
    float pitchMinWidth = -103f;
    float pitchMaxWidth = 103f;
    LineupGUIManager manager;
    List<HistoryEntry> entries = new List<HistoryEntry>();
    PositionValueTuple ranks, positionRanks;
    int positionCount;
    Plotter plt;
    Kit parent;

    private void Awake()
    {
        iohandler = GameObject.Find("Canvas").GetComponent<IOHandler>();
        plt = transform.Find("Graph").gameObject.GetComponent<Plotter>();
        parent = transform.parent.GetComponent<Kit>();
        Debug.Log("InfoPanel_DEBUG: " + parent.leftBoundary);
        ranks = Statistics.GetPlayerRank(parent.player,parent.manager.mainReference);
        (positionRanks,positionCount) = Statistics.GetPlayerRankWithEqualPositions(parent.player,parent.manager.mainReference);
        manager = GameObject.Find("LineupSelection").transform.Find("GUIManager").GetComponent<LineupGUIManager>();
        List<Position> positions = new List<Position>();
        string posString = " ";
        int xc = 0;
        List<float> x = new List<float>();
        List<float> y = new List<float>();
        entries = parent.player.GetHistory();//iohandler.LoadHistory(RankingGlobal.Status.online)[parent.player.GetID()];
        Debug.Log(entries.Count + " entries found for " + parent.player.GetName() + " (" + parent.player.GetID() + ")");
        foreach (HistoryEntry entry in entries)
        {
            if (!positions.Contains(entry.GetPosition()))
            {
                positions.Add(entry.GetPosition());
                posString += entry.GetPosition().GetAbbreviation() + ",";
            }
            y.Add(parent.player.GetBestPosition().y < 0.5f ? entry.GetDefValue() : entry.GetOffValue());
            x.Add(xc);
            xc++;
            DrawPoint(entry.GetSkewness(), entry.GetOffensiveness(), 0.5f, Color.red);
        }
        plt.LineGraph(x,y,Color.white);
        plt.Title("Wertentwicklung");
        plt.HLine(parent.player.GetTrueValue(), Color.green);
        plt.Text(x.Count / 2, parent.player.GetTrueValue()+5, "Aktueller Wert", Color.green);
        plt.XLab("Zeit");
        plt.YLab("Wert");
        posString.Remove(posString.Length - 1);
        if (parent.player.GetBestPosition() != Vector2.zero)
        {
            DrawPoint(parent.player.GetBestPosition().x, parent.player.GetBestPosition().y, 1f, Color.black);
        }
        UpdateStarRatings();
        headline.text = parent.player.GetName() + " (ID: " + parent.player.GetID() + ")," + posString;
        offRank.text = "Rang " + ranks.Right() + " (Position: " + positionRanks.Left() + " von " + positionCount + ")";
        midRank.text = "Rang " + ranks.Center() + " (Position: " + positionRanks.Center() + " von " + positionCount + ")";
        defRank.text = "Rang " + ranks.Left() + " (Position: " + positionRanks.Right() + " von " + positionCount + ")";
    }

    void DrawPoint(float skew, float off, float alpha, Color c)
    {
        Vector2 coordinates = new Vector2(skew*(pitchMaxWidth-pitchMinWidth) + pitchMinWidth, off * (pitchMaxHeight-pitchMinHeight) + pitchMinHeight);
        Debug.Log("from " + new Vector2(skew, off) + " to " + coordinates);
        GameObject instance = Instantiate(positionIndicator, coordinates,Quaternion.identity,pitch.transform);
        instance.GetComponent<RectTransform>().localPosition = coordinates;
        instance.GetComponent<Image>().color = new Color(c.r,c.g,c.b,alpha);
    }

    void UpdateStarRatings()
    {
        GameObject ratingParent = transform.Find("Ratings").gameObject;
        GameObject offRating = ratingParent.transform.Find("offRating").gameObject;
        GameObject midRating = ratingParent.transform.Find("midRating").gameObject;
        GameObject defRating = ratingParent.transform.Find("defRating").gameObject;
        float dv = parent.player.GetRawValues().Item1;
        float ov = parent.player.GetRawValues().Item2;
        float offValue = Mathf.Clamp(ov,manager.min,manager.max);
        offRating.GetComponent<RectTransform>().sizeDelta = new Vector2((offValue - manager.min) / (manager.max - manager.min) * 59f, 19);
        float midValue = ov * 0.5f + dv * 0.5f;
        midValue = Mathf.Clamp(midValue,manager.min,manager.max);
        midRating.GetComponent<RectTransform>().sizeDelta = new Vector2((midValue - manager.min) / (manager.max - manager.min) * 59f, 19);
        float defValue = Mathf.Clamp(dv,manager.min,manager.max);
        defRating.GetComponent<RectTransform>().sizeDelta = new Vector2((defValue - manager.min) / (manager.max - manager.min) * 59f, 19);

    }
}
