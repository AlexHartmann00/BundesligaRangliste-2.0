using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator
{
    List<TableEntry> entries;

    public Simulator(List<Club> clubs)
    {
        entries = new List<TableEntry>();
        foreach(Club club in clubs)
        {
            entries.Add(new TableEntry(club));
        }
    }

    public List<TableEntry> SimulateSeason(Formation formation, bool deterministic = false)
    {
        bool[,] plan = new bool[entries.Count, entries.Count];
        foreach(TableEntry c in entries)
        {
            c.club.OptimizeFormation(formation);
        }
        for(int i = 0; i < entries.Count; i++)
        {
            for(int j = 0; j < entries.Count; j++)
            {
                if(!plan[i, j] && i != j)
                {
                    Match m = new Match(entries[i].club, entries[j].club);
                    //m.GetHomeClub().OptimizeFormation(formation);
                    //m.GetAwayClub().OptimizeFormation(formation);
                    (float,float) res_deterministic = m.Predict();
                    (int,int) result = m.RandomResult();
                    if (deterministic)
                    {
                        result.Item1 = Mathf.RoundToInt(res_deterministic.Item1);
                        result.Item2 = Mathf.RoundToInt(res_deterministic.Item2);
                    }
                    entries[i].points += (result.Item1 > result.Item2 ? 3 : (result.Item1 < result.Item2 ? 0 : 1));
                    entries[j].points += (result.Item1 > result.Item2 ? 0 : (result.Item1 < result.Item2 ? 3 : 1));
                    entries[i].goalsFor += result.Item1;
                    entries[i].goalsAgainst += result.Item2;
                    entries[j].goalsFor += result.Item2;
                    entries[j].goalsAgainst += result.Item1;
                    plan[i, j] = true;
                }
            }
        }
        return entries;
    }

}

public class TableEntry
{
    public Club club;
    public float points;
    public float goalsFor, goalsAgainst;

    public TableEntry(Club c)
    {
        club = c;
    }


}
