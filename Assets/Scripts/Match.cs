using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Match
{
    private Club home, away;
    private PositionValueTuple homeDef, homeAtt, awayDef, awayAtt;
    private (PositionValueTuple, PositionValueTuple) rawPrediction;
    public (float,float) prediction;
    public (float, float) trueResult;
    public IOHandler iohandler;
    public Match(Club hm, Club wy)
    {
        home = hm;
        away = wy;
    }

    public static void PredictLineups(Club a, Club b,string lineupa,string lineupb)
    {
        
    }

    public Club GetHomeClub()
    {
        return home;
    }

    public (float,float) Predict()
    {
        homeDef = new PositionValueTuple(0, 0, 0);
        homeAtt = new PositionValueTuple(0,0,0);
        foreach(Player p in home.GetPlayers())
        {
            if (p.linedUp)
            {
                homeDef += p.GetDefensiveValue() + p.GetDefensiveValue().ApplySkewness(p.skewness) * (1f - p.offensiveness);
                homeAtt += p.offensiveness > 0 ? p.GetOffensiveValue() + p.GetOffensiveValue().ApplySkewness(p.skewness) * (p.offensiveness) : new PositionValueTuple(0, 0, 0);
            }
        }
        homeAtt /= 12.5f;//new PositionValueTuple(12f,12.5f,12f);//12.5f?
        homeDef /= new PositionValueTuple(13.5f,14f,13.5f);
        awayDef = new PositionValueTuple(0,0,0);
        awayAtt = new PositionValueTuple(0,0,0);
        foreach( Player p in away.GetPlayers())
        {
            if (p.linedUp)
            {
                awayDef += p.GetDefensiveValue() + p.GetDefensiveValue().ApplySkewness(p.skewness) * (1f - p.offensiveness);
                awayAtt += p.offensiveness > 0 ? p.GetOffensiveValue() + p.GetOffensiveValue().ApplySkewness(p.skewness) * (p.offensiveness) : new PositionValueTuple(0, 0, 0);
            }
        }
        awayAtt /= 12.5f;// new PositionValueTuple(12f, 12.5f, 12f);//12.5f?
        awayDef /= new PositionValueTuple(13.5f, 14f, 13.5f);
        Debug.Log("homeAtt " + homeAtt + " vs. " + awayDef + " awayDef");
        Debug.Log("awayAtt " + awayAtt + " vs. " + homeDef + " homeDef");
        PositionValueTuple prob1 = ProbabilityFromValues(homeAtt, awayDef);
        PositionValueTuple prob2 = ProbabilityFromValues(awayAtt, homeDef);
        //float a = 0.237825f, b = 3.31804f, c = 7.00107e-05f;
        rawPrediction = (XGfromProbability(prob1), XGfromProbability(prob2));
        //prediction = (Mathf.Pow(rawPrediction.Item1.AverageRoot(),2),Mathf.Pow(rawPrediction.Item2.AverageRoot(),2));
        prediction = (rawPrediction.Item1.Average(),rawPrediction.Item2.Average());
        prediction.Item1 *= 1f + RankingGlobal.Constants.SYMMETRIC_HOME_ADVANTAGE;
        prediction.Item2 *= 1f - RankingGlobal.Constants.SYMMETRIC_HOME_ADVANTAGE;
        prediction.Item1 += RankingGlobal.Constants.BIAS_CORRECTION_HOME;
        prediction.Item2 += RankingGlobal.Constants.BIAS_CORRECTION_AWAY;
        return prediction;
    }

    public void PropagateResult((float,float) result)
    {
        trueResult = result;
        float xg1, xg2;
        (xg1, xg2) = prediction;
        float g1, g2;
        (g1,g2) = result;
        (float,float) diff = (g1-xg1, g2-xg2);
        foreach(Player p in home.GetPlayers())
        {
            p.UpdateValues(diff.Item2,diff.Item1);
        }
        foreach(Player p in away.GetPlayers())
        {
            p.UpdateValues(diff.Item1, diff.Item2);
        }
        RankingGlobal.Data.clubs[home.GetID()] = home;
        RankingGlobal.Data.clubs[away.GetID()] = away;
        MostLikelyResults();
    }

    public (PositionValueTuple,PositionValueTuple) GetRawPrediction()
    {
        return rawPrediction;
    }

    public Club GetAwayClub()
    {
        return away;
    }

    PositionValueTuple XGfromProbability(PositionValueTuple prob)
    {
        /*float l = a * Mathf.Exp(k * prob.Left()) + bias;
        float c = a * Mathf.Exp(k * prob.Center()) + bias;
        float r = a * Mathf.Exp(k * prob.Right()) + bias;*/
        float l = (float)MathNet.Numerics.Distributions.Gamma.InvCDF(RankingGlobal.Constants.GAMMA_SHAPE, RankingGlobal.Constants.GAMMA_RATE, prob.Left());
        float c = (float)MathNet.Numerics.Distributions.Gamma.InvCDF(RankingGlobal.Constants.GAMMA_SHAPE, RankingGlobal.Constants.GAMMA_RATE, prob.Center());
        float r = (float)MathNet.Numerics.Distributions.Gamma.InvCDF(RankingGlobal.Constants.GAMMA_SHAPE, RankingGlobal.Constants.GAMMA_RATE, prob.Right());
        l = Mathf.Clamp(l, 0, RankingGlobal.Constants.MAX_PREDICTION);
        c = Mathf.Clamp(c, 0, RankingGlobal.Constants.MAX_PREDICTION);
        r = Mathf.Clamp(r, 0, RankingGlobal.Constants.MAX_PREDICTION);
        PositionValueTuple ret = new PositionValueTuple(l, c, r);
        return ret;
    }

    PositionValueTuple ProbabilityFromValues(PositionValueTuple a, PositionValueTuple b)
    {
        PositionValueTuple probs = new PositionValueTuple(Prob(a.Left(),b.Left()), Prob(a.Center(),b.Center()), Prob(a.Right(),b.Right()));
        return probs;
    }

    float Prob(float a, float b)
    {
        float diff = a-b;
        float sd = RankingGlobal.Constants.CHANGE_THRESHOLD;
        float p = (float)MathNet.Numerics.Distributions.Normal.CDF(0, sd, diff);
        return p;
    }

    public static implicit operator AnalyticsMatch(Match m)
    {
        AnalyticsMatch a = new AnalyticsMatch(
            m.GetHomeClub().GetName(),
            m.GetAwayClub().GetName(),
            m.prediction.Item1,
            m.prediction.Item2,
            m.trueResult.Item1,
            m.trueResult.Item2
            );
        return a;
    }

    public List<KeyValuePair<(int, int), float>> MostLikelyResults()
    {
        return Statistics.MostLikelyResults(this);
    }

    public string GetTopResultsString(int count)
    {
        List<KeyValuePair<(int, int), float>> results = MostLikelyResults();
        string ret = "";
        for(int i = 0; i < count; i++) {
            ret += results[i].Key.Item1 + " : " + results[i].Key.Item2 + " --> " + (100f*results[i].Value).ToString("0.00") + "%\n";
        }
        return ret;
    }

    public (int,int) RandomResult()
    {
        List<KeyValuePair<(int, int), float>> options = Statistics.MostLikelyResults(this);
        float sum = 0;
        float rn = Random.Range(0f, 1f);
        foreach(KeyValuePair<(int, int), float> option in options)
        {
            if(sum > rn)
            {
                return option.Key;
            }
            sum += option.Value;
        }
        return (0, 0);
    }
}
