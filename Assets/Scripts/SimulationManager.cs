using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RankingGlobal.Utilities;

public class SimulationManager : MonoBehaviour
{
    const int CLUB_OFFSET = 40;
    public IOHandler iohandler;
    public ServerFileManager serverFileManager;
    Simulator simulator;
    List<Club> clubs = new List<Club>();
    public GameObject textPrefab;
    public GameObject clubWindow;
    List<Club> includes = new List<Club>();
    public TextMeshProUGUI table_field;
    public int iterations = 10;
    public TextMeshProUGUI progressText;
    public GameObject tableEntryPrefab;
    public GameObject tableEntryContainer;
    List<GameObject> tableEntries = new List<GameObject>();
    public float tableEntryVerticalOffset = 10f;

    List<PriorityQueue<TableEntry>> simulationResults = new List<PriorityQueue<TableEntry>>();

    void Start()
    {
        iohandler.CheckConnection();
        Invoke("Initialize", 1);
    }

    void Initialize()
    {
        clubs = iohandler.LoadRawClubData(RankingGlobal.Status.online);
        clubs = iohandler.LoadPlayersToClubs(clubs, RankingGlobal.Status.online);
        //Invoke("Simulate", 1);
    }

    public void DisplayClubs()
    {
        float offset = 0;
        foreach (Club club in clubs)
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

    void SelectClub(Club c)
    {
        includes.Add(c);
        Debug.Log(includes.Count + " clubs selected for simulation");
    }

    public PriorityQueue<TableEntry> SimulateSingle(bool determ = false, int leagueID = 0)
    {
        includes = new List<Club>();
        foreach (Club c in clubs)
        {
            if (c.GetLeagueID() == leagueID)
                includes.Add(c);
        }
        simulator = new Simulator(includes);
        List<TableEntry> table = simulator.SimulateSeason(new Formation("1-4-2-0-3-1"), deterministic: determ);
        PriorityQueue<TableEntry> sortedtable = new PriorityQueue<TableEntry>();
        foreach (TableEntry t in table)
        {
            sortedtable.Insert(t, -t.points * 1000 - (t.goalsFor - t.goalsAgainst));
        }
        return sortedtable;
    }

    public PriorityQueue<TableEntry> ShowTable(Dictionary<Club, (float, float,float,float)> positionRanges)
    {
        PriorityQueue<TableEntry> sortedtable = SimulateSingle(true);
        string tabletext = "";
        int i = 1;
        for(int ii = 0; ii < tableEntries.Count; ii++)
        {
            Destroy(tableEntries[ii]);
        }
        tableEntries.Clear();
        foreach (TableEntry t in sortedtable.ToList())
        {
            GameObject GUIentry = Instantiate(tableEntryPrefab, tableEntryContainer.transform.position - new Vector3(0,(i-1)*tableEntryVerticalOffset,0),Quaternion.identity,tableEntryContainer.transform);
            GUIentry.transform.Find("clubname").GetComponent<TextMeshProUGUI>().text = t.club.GetName();
            GUIentry.transform.Find("clubLogo").GetComponent<Image>().sprite = t.club.GetLogo();
            GUIentry.transform.Find("points").GetComponent<TextMeshProUGUI>().text = t.points.ToString();
            GUIentry.transform.Find("goalsfor").GetComponent<TextMeshProUGUI>().text = t.goalsFor.ToString();
            GUIentry.transform.Find("goalsagainst").GetComponent<TextMeshProUGUI>().text = t.goalsAgainst.ToString();
            GUIentry.transform.Find("position").GetComponent<TextMeshProUGUI>().text = i.ToString();
            GUIentry.transform.Find("champProb").GetComponent<TextMeshProUGUI>().text = (100f*positionRanges[t.club].Item3).ToString("#0")+"%";
            GUIentry.transform.Find("relegationProb").GetComponent<TextMeshProUGUI>().text = (100f * positionRanges[t.club].Item4).ToString("#0") + "%";
            tableEntries.Add(GUIentry);
            i++;
        }
        table_field.text = tabletext;
        return sortedtable;
    }

    //Returns dict of club --> (worst, mean, best)
    public Dictionary<Club, (float, float,float,float)> ExtractFromMultiple()
    {
        Dictionary<Club, List<float>> collection = new Dictionary<Club, List<float>>();
        for (int i = 0; i < simulationResults.Count; i++)
        {
            PriorityQueue<TableEntry> tab = simulationResults[i];
            int position = 1;
            foreach(TableEntry t in tab.ToList())
            {
                if (collection.ContainsKey(t.club))
                {
                    collection[t.club].Add(position);
                }
                else
                {
                    collection[t.club] = new List<float> { position };
                }
                position++;
            }
        }
        Dictionary < Club, (float,float,float,float)> minmaxpositions = new Dictionary<Club, (float, float,float,float)> ();

        foreach (KeyValuePair<Club, List<float>> kv in collection)
        {   
            float champProb = 0.0f;
            float relegationProb = 0.0f;
            kv.Value.Sort();
            foreach(float pos in kv.Value)
            {
                if(pos == 1)
                {
                    champProb += 1f / (float)kv.Value.Count;
                }
                if(pos >= 16)
                {
                    relegationProb += 1f / (float)kv.Value.Count;
                }
            }
            float min = kv.Value[Mathf.RoundToInt((float)kv.Value.Count * 0.2f)];
            float max = kv.Value[Mathf.RoundToInt((float)kv.Value.Count * 0.8f)];
            float se = (float)MathNet.Numerics.Statistics.Statistics.StandardDeviation(kv.Value)/Mathf.Sqrt((float)collection.Count);
            float mean = (float)MathNet.Numerics.Statistics.Statistics.Mean(kv.Value);
            //minmaxpositions[kv.Key] = (Mathf.RoundToInt(mean - 1.96f*se), Mathf.RoundToInt(mean + 1.96f * se));
            minmaxpositions[kv.Key] = (min,max,champProb,relegationProb);
        }
        return minmaxpositions;
    }

    // Update is called once per frame
    void Update()
    {
        if(clubs.Count == 0)
        {
            return;
        }

        if (simulationResults.Count < iterations)
        {
            simulationResults.Add(SimulateSingle(leagueID: 0));
            ShowTable(ExtractFromMultiple());
            progressText.text = simulationResults.Count.ToString() + " / " + iterations.ToString();
        }
        if (simulationResults.Count == iterations)
        {
            iterations /= 2;
        }


    }
}
