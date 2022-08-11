using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class HistoryEntry
{
    private float skewness, offensiveness, defValue, offValue;
    private int id;
    private Position pos;

    public HistoryEntry(int id, float skew, float off, float dval, float oval)
    {
        this.id = id;
        this.skewness = skew;
        this.offensiveness = off;
        this.defValue = dval;
        this.offValue = oval;
        this.pos = Position.FromVector2(new Vector2(skew, off));
    }

    public float GetSkewness()
    {
        return skewness;
    }

    public float GetOffensiveness()
    {
        return offensiveness;
    }

    public float GetDefValue()
    {
        return defValue;
    }

    public float GetOffValue()
    {
        return offValue;
    }

    public Position GetPosition()
    {
        return pos;
    }

    public float WeightedValue()
    {
        return offValue * offensiveness + defValue * (1f- offensiveness);
    }

    public string ServerQueryUpdateRepresentation()
    {
        return "id=" + id.ToString(new CultureInfo("en-us", false)) +
            "&skew=" + skewness.ToString(new CultureInfo("en-us", false)) + 
            "&off=" + offensiveness.ToString(new CultureInfo("en-us", false)) +
            "&dv=" + defValue.ToString(new CultureInfo("en-us", false)) +
            "&ov=" + offValue.ToString(new CultureInfo("en-us", false));
    }

    public static List<HistoryEntry> FromMatch(Match match)
    {
        List<HistoryEntry> list = new List<HistoryEntry>();
        foreach (Player p in match.GetHomeClub().GetPlayers())
        {
            if (p.linedUp)
            {
                (float, float) vals = p.GetRawValues();
                float skew = p.away ? 1f - p.skewness : p.skewness;
                HistoryEntry h = new HistoryEntry(p.GetID(), skew, p.offensiveness, vals.Item1, vals.Item2);
                list.Add(h);
            }
        }
        foreach (Player p in match.GetAwayClub().GetPlayers())
        {
            if (p.linedUp)
            {
                (float, float) vals = p.GetRawValues();
                float skew = p.away ? 1f - p.skewness : p.skewness;
                HistoryEntry h = new HistoryEntry(p.GetID(), skew, p.offensiveness, vals.Item1, vals.Item2);
                list.Add(h);
            }
        }
        return list;    
    }
}