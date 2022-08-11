using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GUIManager GUImanager;
    public GameObject lineupWindow;
    public GameObject confirmationWindow;
    public List<Club> clubs = new List<Club>();
    public Match currentMatch = null;
    public IOHandler iohandler;

    // Start is called before the first frame update
    void Start()
    {
        if(RankingGlobal.Data.clubs.Count == 0)
        {
            clubs = iohandler.LoadRawClubData(RankingGlobal.Status.online);
            //clubs = FileManager.LoadPlayersToClubs(RankingGlobal.Paths.MAIN_DATA_PATH, clubs);
            clubs = iohandler.LoadPlayersToClubs(clubs, RankingGlobal.Status.online);
            RankingGlobal.Data.clubs = clubs;
        }
        clubs = RankingGlobal.Data.clubs;
        GUImanager.DisplayClubs(clubs);
        //Debug.Log(Statistics.StandardDeviationOfUsedPlayers(clubs));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Write()
    {
        iohandler.WritePlayers(clubs, RankingGlobal.Status.online);
        RankingGlobal.Data.clubs = clubs;
    }

    public void LineupWindow()
    {
        lineupWindow.SetActive(true);
    }

    public void ConfirmationWindow()
    {
        confirmationWindow.SetActive(true);
    }

    public (float, float) PlayerValueRange()
    {
        float min = 2000000000000;
        float max = 0;
        foreach (Club c in clubs)
        {
            foreach(Player p in c.GetPlayers())
            {
                float cand = Mathf.Max(p.GetRawValues().Item1, p.GetRawValues().Item2);
                if (cand < min)
                {
                    min = cand;
                }
                if (cand > max)
                {
                    max = cand;
                }
            }
        }
        return(min,max);
    }
}
