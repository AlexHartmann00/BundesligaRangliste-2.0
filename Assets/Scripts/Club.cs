using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Club
{
    private int ID;
    private int leagueID;
    private string name;
    private List<Player> players;
    private Coach coach;
    private Color dominantColor, secondaryColor;
    Sprite logo;
    private bool formationOptimized = false;
    private List<Player> optimizedLineup = new List<Player>();
    private Formation lastOptimizedFormation;

    public Club(string n, int id, Sprite lg, int leagueID_ = 0)
    {
        ID = id;
        logo = lg;
        leagueID = leagueID_;
        players = new List<Player>();
        if(lg != null)
        {
            (dominantColor, secondaryColor) = ImageProcessor.DominantColors(lg);
            dominantColor.a = 1;
            secondaryColor.a = 1;
        }
        name = n.Replace('_',' ');
    }

    public int GetID()
    {
        return ID;
    }

    public void SetLeagueID(int id)
    {
        leagueID = id;
    }

    public int GetLeagueID()
    {
        return leagueID;
    }

    public List<Player> GetPlayers()
    {
        players.Sort((Player x,Player y) => string.Compare(x.name,y.name));
        return players;
    }

    public string GetName()
    {
        return name;
    }

    public void AddPlayer(Player p)
    {
        players.Add(p);
    }

    public Sprite GetLogo()
    {
        return logo;   
    }

    public Color GetColor()
    {
        return dominantColor;
    }

    public Color GetSecondaryColor()
    {
        return secondaryColor;
    }

    public void PrintPlayers()
    {
        foreach(Player p in players)
        {
            Debug.Log(name + ", " + p.GetName() + " (" + p.GetID() + ")");
        }
    }

    public override string ToString()
    {
        return name;
    }

    public Dictionary<Position,float> PositionValues()
    {
        return null;
    }

    public List<Player> OptimizeFormation(Formation f)
    {
        if (formationOptimized && lastOptimizedFormation.Equals(f)) return optimizedLineup;
        var coords = f.GetCoordinates();
        List<Player> lineup = new List<Player>();
        float[,] fits = new float[players.Count, 11];
        for (int i = 0; i < players.Count; i++)
        {
            players[i].linedUp = false;
            for(int j = 0; j < 11; j++)
            {
                float positionValue = players[i].GetRawValues().Item2 * coords[j].y + players[i].GetRawValues().Item1 * (1f-coords[j].y);
                float propositionValue = positionValue - 50f*(coords[j]-players[i].GetBestPosition()).magnitude + players[i].meanValueChangeOverLast;
                fits[i, j] = propositionValue;
                if (!Position.FromVector2(coords[j]).Equals(Position.FromVector2(players[i].GetBestPosition())))
                {
                    if (Position.FromVector2(coords[j]).Equals(Position.goalkeeper))
                    {
                        fits[i, j] = float.NegativeInfinity;
                    }
                }
            }
        }
        //fill positions with best position-wise fit, best overall fit first
        List<int> chosen_i = new List<int>();
        List<int> chosen_j = new List<int>();
        //Forced lineup
        for (int ii = 0; ii < 11; ii++)
        {
            float max = 0;
            int maxI = 0;
            int maxJ = 0;
            for (int i = 0; i < players.Count; i++)
            {
                if (!chosen_i.Contains(i) && players[i].lineupStatus == LineupStatus.Forced)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        if (!chosen_j.Contains(j))
                        {
                            if (fits[i, j] > max)
                            {
                                max = fits[i, j];
                                maxI = i;
                                maxJ = j;
                            }
                        }
                    }
                }
            }
            if (max == 0) break;
            chosen_i.Add(maxI);
            chosen_j.Add(maxJ);
            players[maxI].linedUp = true;
            players[maxI].skewness = coords[maxJ].x;
            players[maxI].offensiveness = coords[maxJ].y;
            lineup.Add(players[maxI]);
            Debug.Log("Chose " + players[maxI].name + " for " + Position.FromVector2(coords[maxJ]));
        }

        //Remaining spots
        Debug.Log(chosen_j.Count + " forced players added, " + (11 - chosen_j.Count) + " remaining.");
        int forcedCount = chosen_j.Count;
        for (int ii = 0; ii < 11-forcedCount; ii++)
        {
            float max = 0;
            int maxI = 0;
            int maxJ = 0;
            for (int j = 0; j < 11; j++)
            {
                if (!chosen_j.Contains(j))
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (!chosen_i.Contains(i) && players[i].lineupStatus == LineupStatus.Available)
                        {
                            if (fits[i, j] > max)
                            {
                                max = fits[i, j];
                                maxI = i;
                                maxJ = j;
                            }
                        }
                    }
                }
            }
            chosen_i.Add(maxI);
            chosen_j.Add(maxJ);
            players[maxI].linedUp = true;
            players[maxI].skewness = coords[maxJ].x;
            players[maxI].offensiveness = coords[maxJ].y;
            lineup.Add(players[maxI]);
            Debug.Log("Chose " + players[maxI].name + " for " + Position.FromVector2(coords[maxJ]));
        }
        formationOptimized = true;
        lastOptimizedFormation = f;
        optimizedLineup = lineup;
        return lineup;
    }

    public (PositionValueTuple,PositionValueTuple) OptimizedStrength()
    {
        return (new PositionValueTuple(0,0,0),new PositionValueTuple(0,0,0));
    }

    public string ServerQueryUpdateRepresentation()
    {
        return "id=" + ID.ToString() + "&name=" + name.Replace(' ','_') + "&lid=" + leagueID;
    }
    public float MeanValue()
    {
        float value = 0;
        float count = 0;
        foreach(Player p in players)
        {
            if(p.gamesPlayed > 0)
            {
                value += p.BestPositionValue();
                count++;
            }
        }
        return value / count;
    }

    public Player MaxPlayer()
    {
        float max = 0;
        Player best = null;
        foreach(Player player in players)
        {
            if (player.gamesPlayed > 0)
            {
                if(player.BestPositionValue() > max)
                {
                    max = player.BestPositionValue();
                    best = player;
                }
            }
        }
        return max > 0 ? best : new Player(-1,"-",0,0,0,0,0,0);
    }

    public (float,float) MinMaxValues()
    {
        float min = 99999;
        float max = 0;
        foreach(Player p in players)
        {
            if(p.gamesPlayed > 0)
            {
                if(p.BestPositionValue() > max)
                {
                    max = p.BestPositionValue();
                }
                if(p.BestPositionValue() < min)
                {
                    min = p.BestPositionValue();
                }
            }
        }
        return (min, max);
    }
    public PositionValueTuple StrengthSummary()
    {
        List<float> def = new List<float>();
        List<float> mid = new List<float>();
        List<float> off = new List<float>();
        foreach(Player p in players)
        {
            if (Position.FromVector2(p.GetBestPosition()).IsDefensive())
            {
                if(def.Count < 5)
                    def.Add(p.BestPositionValue());
            }            
            if (Position.FromVector2(p.GetBestPosition()).IsMidfielder())
            {
                if(mid.Count < 5)
                    mid.Add(p.BestPositionValue());
            }            
            if (Position.FromVector2(p.GetBestPosition()).IsOffensive())
            {
                if(off.Count < 5)
                    off.Add(p.BestPositionValue());
            }
        }
        float d = (float)MathNet.Numerics.Statistics.Statistics.Mean(def);
        float m = (float)MathNet.Numerics.Statistics.Statistics.Mean(mid);
        float o = (float)MathNet.Numerics.Statistics.Statistics.Mean(off);
        return new PositionValueTuple(d, m, o);
    }
}
