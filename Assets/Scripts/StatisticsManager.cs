using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StatisticsManager : MonoBehaviour
{
    public GameObject binPrefab, binParent;
    float horizontalOffset = 100f;
    List<Player> ranking = new List<Player>();
    public GameObject playerRankingPanel;
    public GameObject playerEntryPrefab;
    public GameObject playerEntryParent;

    private void Awake()
    {
        InstantiateHistogram(20);
    }

    public void InstantiateHistogram(int bins = 20)
    {
        horizontalOffset = 0.6f*Screen.width/bins;
        Dictionary<int, float[]> histogram = Statistics.ValueHistogramBins(RankingGlobal.Data.clubs,bins);
        Color[] colors = new Color[] {Color.red,Color.blue};
        for(int leagueid = 0; leagueid < histogram.Keys.Count; leagueid++)
        {
            for (int i = 0; i < bins; i++)
            {
                GameObject instance = Instantiate(binPrefab, binParent.transform.position + new Vector3(i * horizontalOffset, 0, 0), Quaternion.identity, binParent.transform);
                Debug.Log("Histogram Bin Height: " + histogram[leagueid][i]);
                instance.transform.localScale = new Vector3(bins/20, histogram[leagueid][i], 1);
                instance.GetComponent<Image>().color = colors[leagueid]- new Color(0,0,0,0.5f);
            }
        }
        
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowPlayerRanking()
    {
        float offset = 15f;
        playerRankingPanel.SetActive(true);
        if (ranking.Count > 0) return;
        List<Player> players = new List<Player>();
        float minval = 99999;
        float maxval = 0;
        foreach(Club c in RankingGlobal.Data.clubs)
        {
            foreach(Player p in c.GetPlayers())
            {
                players.Add(p);
                if(p.BestPositionValue() > maxval)
                {
                    maxval = p.BestPositionValue();
                }
                if(p.BestPositionValue() < minval)
                {
                    minval = p.BestPositionValue();
                }
            }
        }
        ranking = Statistics.PlayerRanking(players);
        int rank = 1;
        foreach(Player p in ranking)
        {
            Club club = RankingGlobal.Data.clubs[p.GetClubID()];
            GameObject instance = Instantiate(playerEntryPrefab, playerEntryParent.transform);
            instance.transform.Find("rank").GetComponent<TextMeshProUGUI>().text = rank.ToString() + ".";
            instance.transform.Find("name").GetComponent<TextMeshProUGUI>().text = p.name;
            instance.transform.Find("position").GetComponent<TextMeshProUGUI>().text = Position.FromVector2(p.GetBestPosition()).ToString();
            instance.transform.Find("gamesPlayed").GetComponent<TextMeshProUGUI>().text = p.gamesPlayed.ToString();
            instance.transform.Find("defValue").GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(p.GetRawValues().Item1).ToString();
            instance.transform.Find("offValue").GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(p.GetRawValues().Item2).ToString();
            instance.transform.Find("clubLogo").GetComponent<Image>().sprite = club.GetLogo();
            instance.transform.Find("defValue").transform.Find("bar").localScale = new Vector3(Mathf.Clamp01((p.GetRawValues().Item1-minval)/(maxval-minval)), 1, 1);
            instance.transform.Find("offValue").transform.Find("bar").localScale = new Vector3(Mathf.Clamp01((p.GetRawValues().Item2-minval)/(maxval-minval)), 1, 1);
            //instance.transform.Find("bestposvalue").GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(p.BestPositionValue()).ToString();

            rank++;
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerEntryParent.GetComponent<RectTransform>());
        }
    }
}
