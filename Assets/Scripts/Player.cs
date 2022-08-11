using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class Player : Person
{
    private float defValue, offValue;
    private Vector2 averagePosition;
    private Vector2 bestPosition;
    public int gamesPlayed;
    private float trueValue;
    public float offensiveness;
    public float skewness;
    int clubID;
    public bool away = false;
    public bool linedUp = false;
    public bool changed = false;
    public LineupStatus lineupStatus = LineupStatus.Available;
    List<HistoryEntry> history;
    public float form = 0.5f;
    public float meanValueChangeOverLast = 0;
    public Player(int id, string nm)
    {
        ID = id;
        name = nm;
    }

    public float GetTrueValue()
    {
        trueValue = offensiveness != 0 ? offensiveness * GetOffensiveValue().Max() + (1f - offensiveness) * GetDefensiveValue().Max() : Mathf.Max(defValue,offValue);
        return trueValue;
    }

    public int GetClubID()
    {
        return clubID;
    }

    public void SetClubID(int id)
    {
        clubID = id;
    }

    //TODO: left/center/right calculation not working well...
    public PositionValueTuple GetDefensiveValue() {
        float skew = away ? 1f - skewness : skewness;
        float deviation = averagePosition != new Vector2(0,0) ? (averagePosition - new Vector2(skew,offensiveness)).magnitude : 0;
        return new PositionValueTuple(defValue,defValue,defValue) - deviation * RankingGlobal.Constants.POSITION_PENALTY + meanValueChangeOverLast; 
    }
    public PositionValueTuple GetOffensiveValue() {
        float skew = away ? 1f - skewness : skewness;
        float deviation = averagePosition != new Vector2(0, 0) ? (averagePosition - new Vector2(skew, offensiveness)).magnitude : 0;
        return new PositionValueTuple(offValue, offValue, offValue) - deviation * RankingGlobal.Constants.POSITION_PENALTY + meanValueChangeOverLast;
    }

    public Vector2 GetPosition()
    {
        return away ? new Vector2(1f - skewness,offensiveness) : new Vector2(skewness,offensiveness);
    }

    public (float,float) GetRawValues()
    {
        return (defValue, offValue);
    }

    public Vector2 GetBestPosition()
    {
        return averagePosition;
    }
    
    public Player(string nm, float trueVal)
    {
        name = nm;
        trueValue = trueVal;
        defValue = trueVal;
        offValue = trueVal;
    }

    public void UpdateValues(float defDiff, float offDiff)
    {
        changed = true;
        if (linedUp)
        {
            UpdateAveragePosition();
            float d = -(1f - 2f/(Mathf.Exp(Mathf.Abs(defDiff))+1f)) * Mathf.Sign(defDiff) * (1 - offensiveness);
            float o = (1f - 2f / (Mathf.Exp(Mathf.Abs(offDiff)) + 1f)) * Mathf.Sign(offDiff) * offensiveness;
            float dc = Mathf.Clamp(d * RankingGlobal.Constants.GetChangeConstant(this), -RankingGlobal.Constants.MAX_CHANGE, RankingGlobal.Constants.MAX_CHANGE);
            float oc = Mathf.Clamp(o * RankingGlobal.Constants.GetChangeConstant(this), -RankingGlobal.Constants.MAX_CHANGE, RankingGlobal.Constants.MAX_CHANGE);
            defValue += dc;
            offValue += oc;
            defValue -= offensiveness * RankingGlobal.Constants.MAX_POSITION_DECAY;
            offValue -= (1f - offensiveness) * RankingGlobal.Constants.MAX_POSITION_DECAY;
            gamesPlayed++;
        }
        else
        {
            defValue -= 0.5f * RankingGlobal.Constants.MAX_POSITION_DECAY;
            offValue -= 0.5f * RankingGlobal.Constants.MAX_POSITION_DECAY;
        }
    }

    public Player(int id, string nm, float def, float off, int gp, float x, float y, int cid)
    {
        ID = id;
        name = nm;
        defValue = def;
        offValue = off;
        gamesPlayed = gp;
        averagePosition.x = x;
        averagePosition.y = y;
        clubID = cid;
    }

    void UpdateAveragePosition()
    {
        changed = true;
        float skew = away ? 1f - skewness : skewness;
        if (gamesPlayed > 0)
        {
            Vector2 cand = new Vector2(IncrementAvg(averagePosition.x, skew, gamesPlayed + 1), IncrementAvg(averagePosition.y, offensiveness, gamesPlayed + 1));
            Vector2 prev = new Vector2(averagePosition.x / (float)(gamesPlayed), averagePosition.y / (float)(gamesPlayed - 1));
            Vector2 nw = new Vector2(prev.x + skew / (float)(gamesPlayed+1), prev.y + offensiveness / (float)gamesPlayed);
            averagePosition = cand;
        }
        else
        {
            averagePosition = new Vector2(skew, offensiveness);
        }
    }

    public string FileSystemRepresentation()
    {
        return ID.ToString(new CultureInfo("en-us",false)) + ',' + name + ',' + clubID + ',' + defValue.ToString(new CultureInfo("en-us", false)) + ','
            + offValue.ToString(new CultureInfo("en-us", false)) + ',' + gamesPlayed + ',' + averagePosition.x.ToString(new CultureInfo("en-us", false))
            + ',' + averagePosition.y.ToString(new CultureInfo("en-us", false));
    }

    public string ServerQueryUpdateRepresentation()
    {
        return "id=" + ID.ToString(new CultureInfo("en-us", false)) +
            "&name=" + name +
            "&cid=" + clubID.ToString(new CultureInfo("en-us", false)) +
            "&dv=" + defValue.ToString(new CultureInfo("en-us", false)) +
            "&ov=" + offValue.ToString(new CultureInfo("en-us", false)) +
            "&gp=" + gamesPlayed.ToString(new CultureInfo("en-us", false)) +
            "&skew=" + averagePosition.x.ToString(new CultureInfo("en-us", false)) +
            "&off=" + averagePosition.y.ToString(new CultureInfo("en-us", false));
    }

    float IncrementAvg(float oldAverage, float increment, int newCount)
    {
        float oldProportion = 1.0f - 1.0f / (float)newCount;
        float tempOld = oldAverage * oldProportion;
        float tempNew = increment / (float)newCount;
        return tempOld + tempNew;
    }

    public static bool operator ==(Player p1, Player p2)
    {
        return p1.GetID() == p2.GetID();
    }

    public static bool operator !=(Player p1, Player p2)
    {
        return p1.GetID() != p2.GetID();
    }

    public void SetHistory(List<HistoryEntry> entries)
    {
        history = entries;
        int end = history.Count - 1;
        int start = end > 4 ? end - 5 : 0;
        float endpoint = entries[end].WeightedValue();
        float startpoint = entries[start].WeightedValue();
        float diff = endpoint - startpoint;
        diff /=(float) (end - start + 1);
        meanValueChangeOverLast = diff;
        diff += 10f;
        diff /= 20f;
        diff = Mathf.Clamp01(diff);
        form = diff;
    }

    public List<HistoryEntry> GetHistory()
    {
        return history;
    }

    public void SetName(string n)
    {
        name = n;
    }

    public void SetValues(float def, float off)
    {
        defValue = def;
        offValue= off;
    }

    public float BestPositionValue()
    {
        return (1-averagePosition.y)*defValue + averagePosition.y*offValue;
    }
}
