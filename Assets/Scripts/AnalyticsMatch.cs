using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class AnalyticsMatch
{
    string homeName, awayName;
    (float, float) prediction;
    (float, float) result;
    private List<KeyValuePair<(int, int), float>> mostLikelyResults = new List<KeyValuePair<(int, int),float>>();

    public AnalyticsMatch(string hn, string an, float p1, float p2, float r1, float r2)
    {
        homeName = hn;
        awayName = an;
        prediction = (p1, p2);
        result = (r1, r2);
    }

    public bool IsExactlyCorrectPrediction()
    {
        return Mathf.RoundToInt(prediction.Item1) == Mathf.RoundToInt(result.Item1) && Mathf.RoundToInt(prediction.Item2) == Mathf.RoundToInt(result.Item2);
    }

    public int CorrectPartialPredictions()
    {
        int a = Mathf.RoundToInt(prediction.Item1) == Mathf.RoundToInt(result.Item1) ? 1 : 0;
        int b = Mathf.RoundToInt(prediction.Item2) == Mathf.RoundToInt(result.Item2) ? 1 : 0;   
        return a+b;
    }

    public int RandomizedPartialCorrectness()
    {
        float preda = (float)MathNet.Numerics.Distributions.Gamma.InvCDF(RankingGlobal.Constants.GAMMA_SHAPE, RankingGlobal.Constants.GAMMA_RATE, Random.Range(0f, 1f));
        float predb = (float)MathNet.Numerics.Distributions.Gamma.InvCDF(RankingGlobal.Constants.GAMMA_SHAPE, RankingGlobal.Constants.GAMMA_RATE, Random.Range(0f, 1f));
        int a = Mathf.RoundToInt(preda) == Mathf.RoundToInt(result.Item1) ? 1 : 0;
        int b = Mathf.RoundToInt(predb) == Mathf.RoundToInt(result.Item2) ? 1 : 0;
        return a + b;

    }

    public float MeanAbsoluteError()
    {
        return (Mathf.Abs(prediction.Item1 - result.Item1) + Mathf.Abs(prediction.Item2 - result.Item2)) / 2f;
    }

    public float RandomizedMeanAbsoluteError()
    {
        float preda = (1f + RankingGlobal.Constants.SYMMETRIC_HOME_ADVANTAGE) * (float)MathNet.Numerics.Distributions.Gamma.InvCDF(RankingGlobal.Constants.GAMMA_SHAPE, RankingGlobal.Constants.GAMMA_RATE, Random.Range(0f, 1f));
        float predb = (1f - RankingGlobal.Constants.SYMMETRIC_HOME_ADVANTAGE) * (float)MathNet.Numerics.Distributions.Gamma.InvCDF(RankingGlobal.Constants.GAMMA_SHAPE, RankingGlobal.Constants.GAMMA_RATE, Random.Range(0f, 1f));
        return (Mathf.Abs(preda - result.Item1) + Mathf.Abs(predb - result.Item2)) / 2f;
    }

    public float SumOfSquaredResiduals()
    {
        return Mathf.Pow(prediction.Item1-result.Item1,2) + Mathf.Pow(prediction.Item2-result.Item2,2); 
    }

    public float SumOfSquaredResult()
    {
        return Mathf.Pow(result.Item1, 2) + Mathf.Pow(result.Item2, 2);
    }

    public string GetHomeName() { return homeName; }
    public string GetAwayName() { return awayName; }
    public (float,float) GetPrediction() { return prediction; }
    public (float,float) GetResult() { return result; }

    public string GetServerQueryRepresentation()
    {
        return "hn=" + homeName +
            "&an=" + awayName +
            "&xg1=" + prediction.Item1.ToString(new CultureInfo("en-us", false)) +
            "&xg2=" + prediction.Item2.ToString(new CultureInfo("en-us", false)) +
            "&g1=" + result.Item1.ToString(new CultureInfo("en-us", false)) +
            "&g2=" + result.Item2.ToString(new CultureInfo("en-us", false));
    }

    public bool TrueResultContainedInTopPredictions(int count)
    {
        if(mostLikelyResults.Count == 0)
            mostLikelyResults = MostLikelyResults();
        for(int i = 0; i < count; i++)
        {
            (int, int) predicted = mostLikelyResults[i].Key;
            if(result.Item1 == predicted.Item1 && result.Item2 == predicted.Item2)
            {
                return true;
            }
        }
        return false;
    }

    public List<KeyValuePair<(int, int), float>> MostLikelyResults()
    {
        return Statistics.MostLikelyResults(this);
    }
}