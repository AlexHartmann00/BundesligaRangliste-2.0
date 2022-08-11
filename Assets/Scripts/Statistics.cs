using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RankingGlobal.Utilities;

public static class Statistics
{
    public static float ComputeExactAccuracy(List<AnalyticsMatch> matches)
    {
        float sum = 0;
        float count = 0;
        foreach (AnalyticsMatch match in matches)
        {
            sum += match.IsExactlyCorrectPrediction() ? 1 : 0;
            count++;
        }
        return sum /(float) count;
    }

    public static float ComputeAccuracyScore(List<AnalyticsMatch> matches)
    {
        float sum = 0;
        float count = 0;
        foreach (AnalyticsMatch match in matches)
        {
            sum += match.CorrectPartialPredictions();
            count++;
        }
        return sum / (float)count;
    }

    public static float ComputeRandomAccuracyScore(List<AnalyticsMatch> matches)
    {
        float sum = 0;
        float count = 0;
        foreach (AnalyticsMatch match in matches)
        {
            sum += match.RandomizedPartialCorrectness();
            count++;
        }
        return sum / (float)count;
    }

    public static float ComputeError(List<AnalyticsMatch> matches)
    {
        float sum = 0;
        float count = 0;
        foreach (AnalyticsMatch match in matches)
        {
            sum += match.MeanAbsoluteError();
            count++;
        }
        return sum / (float)count;
    }
    
    public static float ComputeRandomizedError(List<AnalyticsMatch> matches)
    {
        float sum = 0;
        float count = 0;
        foreach (AnalyticsMatch match in matches)
        {
            sum += match.RandomizedMeanAbsoluteError();
            count++;
        }
        return sum / (float)count;
    }

    public static float FisherRandomizationTest(List<AnalyticsMatch> matches, int replications)
    {
        int count = 0, total = 0;
        float referenceScore = ComputeError(matches);
        for(int i = 0; i < replications; i++)
        {
            float score = ComputeRandomizedError(matches);
            if(score < referenceScore)
            {
                count++;
            }
            total++;
        }
        return count /(float) total;
    }

    public static float StandardDeviationOfUsedPlayers(List<Club> clubs)
    {
        List<float> nums = new List<float>();
        foreach(Club club in clubs)
        {
            if(club.GetLeagueID() == 0)
            {
                foreach (Player p in club.GetPlayers())
                {
                    if (p.gamesPlayed > 0)
                    {
                        nums.Add(p.GetTrueValue());
                    }
                }
            }
        }
        return (float)MathNet.Numerics.Statistics.Statistics.StandardDeviation(nums);
    }

    public static PositionValueTuple GetPlayerRank(Player p, Main mainReference)
    {
        List<Club> clubs = mainReference.clubs;
        PositionValueTuple ranks = new PositionValueTuple(0, 0, 0);
        foreach(Club club in clubs)
        {
            foreach(Player player in club.GetPlayers())
            {
                float off = 0, mid = 0, def = 0;
                if (p.GetRawValues().Item2 <= player.GetRawValues().Item2)
                {
                    off = 1;
                }
                if (p.GetRawValues().Item2 * 0.5f + p.GetRawValues().Item1 * 0.5f <= player.GetRawValues().Item2 * 0.5f + player.GetRawValues().Item1 * 0.5f)
                {
                    mid = 1;
                }
                if (p.GetRawValues().Item1 <= player.GetRawValues().Item1)
                {
                    def = 1;
                }
                PositionValueTuple add = new PositionValueTuple(def, mid, off);
                ranks += add;
            }
        }
        return ranks;
    }

    public static (PositionValueTuple,int) GetPlayerRankWithEqualPositions(Player p, Main mainReference)
    {
        int count = 0;
        List<Club> clubs = mainReference.clubs;
        PositionValueTuple ranks = new PositionValueTuple(0, 0, 0);
        foreach (Club club in clubs)
        {
            foreach (Player player in club.GetPlayers())
            {
                if (Position.FromVector2(player.GetBestPosition()).Equals(Position.FromVector2(p.GetBestPosition()))){
                    Debug.Log(Position.FromVector2(player.GetBestPosition()) + " = " + Position.FromVector2(p.GetBestPosition()));
                    float off = 0, mid = 0, def = 0;
                    if (p.GetRawValues().Item2 <= player.GetRawValues().Item2)
                    {
                        off = 1;
                    }
                    if (p.GetRawValues().Item2 * 0.5f + p.GetRawValues().Item1 * 0.5f <= player.GetRawValues().Item2 * 0.5f + player.GetRawValues().Item1 * 0.5f)
                    {
                        mid = 1;
                    }
                    if (p.GetRawValues().Item1 <= player.GetRawValues().Item1)
                    {
                        def = 1;
                    }
                    PositionValueTuple add = new PositionValueTuple(def, mid, off);
                    ranks += add;
                    count++;
                }
            }
        }
        return (ranks,count);
    }

    public static float RSquared(List<AnalyticsMatch> matches)
    {
        float SST = 0;
        float SSR = 0;
        foreach(AnalyticsMatch match in matches)
        {
            SST += match.SumOfSquaredResult();
            SSR += match.SumOfSquaredResiduals();
        }
        return 1f - SSR / SST;
    }

    public static List<float> MeanAbsoluteErrorChange(List<AnalyticsMatch> matches)
    {
        List<float> result = new List<float>();
        float sum = 0;
        float count = 0;
        foreach(AnalyticsMatch match in matches)
        {
            sum += match.MeanAbsoluteError();
            count++;
            result.Add(sum / count);
        }
        return result;  
    }

    public static List<float> AccuracyChange(List<AnalyticsMatch> matches)
    {
        List<float> result = new List<float>();
        float sum = 0;
        float count = 0;
        foreach (AnalyticsMatch match in matches)
        {
            sum += match.IsExactlyCorrectPrediction() ? 1 : 0;
            count++;
            result.Add(sum / count);
        }
        return result;
    }  
    
    public static List<float> Top1AccuracyChange(List<AnalyticsMatch> matches)
    {
        List<float> result = new List<float>();
        float sum = 0;
        float count = 0;
        foreach (AnalyticsMatch match in matches)
        {
            sum += match.TrueResultContainedInTopPredictions(1) ? 1 : 0;
            count++;
            result.Add(sum / count);
        }
        return result;
    }

    /*public static List<KeyValuePair<(int, int), float>> MostLikelyResults(AnalyticsMatch m)
    {
        float N = 10000;
        float homeLambda, awayLambda;
        (homeLambda, awayLambda) = m.GetPrediction();
        Dictionary<(int,int),float> resultCounts = new Dictionary<(int, int),float>();
        for(int i = 0; i < N; i++)
        {
            (int, int) sample = (MathNet.Numerics.Distributions.Poisson.Sample(homeLambda), MathNet.Numerics.Distributions.Poisson.Sample(awayLambda));
            if (!resultCounts.ContainsKey(sample))
            {
                resultCounts.Add(sample, 1);
            }
            else
            {
                resultCounts[sample] ++;
            }
        }
        for(int i = 0; i < resultCounts.Count; i++)
        {
            resultCounts[resultCounts.Keys.ToList()[i]] /= (float)N;
            resultCounts[resultCounts.Keys.ToList()[i]] *= (float)100f;
        }
        List<KeyValuePair<(int, int),float>> result = new List<KeyValuePair<(int, int), float>>();
        while(resultCounts.Count > 0)
        {
            (int, int) maxKey = MaxDictionaryKey(resultCounts);
            result.Add(new KeyValuePair<(int, int), float>(maxKey, resultCounts[maxKey]));
            resultCounts.Remove(maxKey);
        }
        return result;
    }*/

    public static List<KeyValuePair<(int, int), float>> MostLikelyResults(AnalyticsMatch m)
    {
        float homeLambda, awayLambda;
        (homeLambda, awayLambda) = m.GetPrediction();
        Dictionary<(int, int), float> resultCounts = new Dictionary<(int, int), float>();

        for (int i  = 0; i < 7; i++)
        {
            for(int j = 0; j < 7; j++)
            {
                float p1 = (float)MathNet.Numerics.Distributions.Poisson.PMF(homeLambda, i);
                float p2 = (float)MathNet.Numerics.Distributions.Poisson.PMF(awayLambda, j);
                resultCounts.Add((i, j), p1 * p2);
            }
        }
        List<KeyValuePair<(int, int), float>> result = new List<KeyValuePair<(int, int), float>>();
        while (resultCounts.Count > 0)
        {
            (int, int) maxKey = MaxDictionaryKey(resultCounts);
            result.Add(new KeyValuePair<(int, int), float>(maxKey, resultCounts[maxKey]));
            resultCounts.Remove(maxKey);
        }
        return result;
    }

    public static List<float> UnlistTrueResults(List<AnalyticsMatch> matches)
    {
        List<float> result = new List<float>();
        foreach(AnalyticsMatch match in matches)
        {
            result.Add(match.GetResult().Item1);
            result.Add(match.GetResult().Item2);
        }
        return result;
    }

    public static (int,int) MaxDictionaryKey(Dictionary<(int,int),float> dict)
    {
        float max = -999999;
        (int, int) maxKey = (-1,-1);
        foreach((int,int) key in dict.Keys)
        {
            if (dict[key] > max)
            {
                max = dict[key];
                maxKey = key;
            }
        }
        Debug.Log(maxKey + " --> " + dict[maxKey].ToString());
        return maxKey;
    }

    public static float Moment(List<float> X,int n)
    {
        float sum = 0;
        float count = 0;
        foreach(float x in X)
        {
            sum += Mathf.Pow(x, n);
            count++;
        }
        return sum /(float) count;
    }

    public static Dictionary<string,float> EstimateMoM(Distribution dist, List<float> X)
    {
        Dictionary<string, float> dict = new Dictionary<string, float>();
        if (dist == Distribution.Gamma)
        {
            float rate = (Moment(X,1)/Moment(X,2))/(1f + (Mathf.Pow(Moment(X,1),2)/Moment(X,2)));
            float shape = 1f / (float) (Moment(X, 1) / rate);
            dict.Add("Rate", rate);
            dict.Add("Shape", shape);
            Debug.Log("rate: " + dict["Rate"] + ", shape: " + dict["Shape"]);
            //MathNet.Numerics.FindMinimum.OfFunction
        }
        return dict;
    }

    public static float TopXAccuracy(List<AnalyticsMatch> matches,int count)
    {
        float correct = 0;
        float total = 0;
        foreach(AnalyticsMatch match in matches)
        {
            if (match.TrueResultContainedInTopPredictions(count))
            {
                correct++;
            }
            total++;
        }
        return correct / total;
    }

    public static (float,float) MeanHomeAwayGoals(List<AnalyticsMatch> matches,float trim=0.1f)
    {
        List<float> hg = new List<float>(), ag = new List<float>();
        float count = 0;
        foreach (AnalyticsMatch m in matches)
        {
            hg.Add(m.GetResult().Item1);
            ag.Add(m.GetResult().Item2);
            count++;
        }
        hg = Trim(hg, trim);
        ag = Trim(ag, trim);
        return ((float)MathNet.Numerics.Statistics.Statistics.Mean(hg), (float)MathNet.Numerics.Statistics.Statistics.Mean(ag));
    }

    public static List<float> Trim(List<float> list, float trim)
    {
        list.Sort();
        List<float> newList = new List<float>();
        for(int i = 0; i < list.Count; i++)
        {
            if(i > trim*(float)list.Count && i < (1 - trim) * (float)list.Count)
            {
                newList.Add(list[i]);
            }
        }
        return newList;
    }

    public static (float,float) MeanHomeAwayPredictedGoals(List<AnalyticsMatch> matches, float trim=0.1f)
    {
        List<float> hg = new List<float>(), ag = new List<float>();
        float count = 0;
        foreach (AnalyticsMatch m in matches)
        {
            hg.Add(m.GetPrediction().Item1);
            ag.Add(m.GetPrediction().Item2);
            count++;
        }
        hg = Trim(hg, trim);
        ag = Trim(ag, trim);
        return ((float)MathNet.Numerics.Statistics.Statistics.Mean(hg), (float)MathNet.Numerics.Statistics.Statistics.Mean(ag));
    }

    public static (float,float) GammaMoMShapeScale(List<AnalyticsMatch> matches)
    {
        List<float> goals = new List<float>();
        foreach(AnalyticsMatch m in matches)
        {
            goals.Add(m.GetResult().Item1);
            goals.Add(m.GetResult().Item2);
        }
        float theta = (float)MathNet.Numerics.Statistics.Statistics.Variance(goals) / (float)MathNet.Numerics.Statistics.Statistics.Mean(goals);
        float k = (float)MathNet.Numerics.Statistics.Statistics.Mean(goals) / theta;
        return (k, theta);
    }

    //Dict<leagueID,histogram bins>
    public static Dictionary<int,float[]> ValueHistogramBins(List<Club> clubs,int bincount = 20)
    {
        //bin populations by league id
        Dictionary<int,float[]> histogram = new Dictionary<int, float[]>();
        //Sorted player values by league id
        Dictionary<int,PriorityQueue<float>> playerValues = new Dictionary<int,PriorityQueue<float>>();
        float min = 99999;
        float max = 0;
        foreach(Club c in clubs)
        {
            foreach(Player p in c.GetPlayers())
            {
                if (!playerValues.ContainsKey(c.GetLeagueID()))
                {
                    histogram[c.GetLeagueID()] = new float[bincount];
                    playerValues[c.GetLeagueID()] = new PriorityQueue<float>();
                }

                if (p.gamesPlayed > 0)
                {
                    float value = p.BestPositionValue();
                    if(value < min)
                    {
                        min = value;
                    }
                    if(value > max)
                    {
                        max = value;
                    }
                    playerValues[c.GetLeagueID()].Insert(value,value);
                }
            }
        }
        float step = (max - min) / (float)bincount;
        float maximumCount = 0;
        //TODO: also return midpoints of bins?
        for(int leagueid = 0; leagueid < playerValues.Keys.Count; leagueid++)
        {
            int binid = 0;
            int count = 0;
            int playerID = 0;
            while(playerID < playerValues[leagueid].ToList().Count)
            {
                if (playerValues[leagueid].ToList()[playerID] < (binid + 1) * step + min)
                {
                    count++;
                    playerID++;
                }
                else
                {
                    Debug.Log(leagueid + ", " + binid + " - in HistogramCalculation");
                    histogram[leagueid][binid] = count;
                    if(count > maximumCount)
                    {
                        maximumCount = count;
                    }
                    binid++;
                    count = 0;
                }
            }
        }
        for(int leagueid = 0; leagueid < playerValues.Keys.Count; leagueid++)
        {
            for(int binid = 0; binid < histogram[leagueid].Length; binid++)
            {
                histogram[leagueid][binid] /= (float)maximumCount;
            }
        }
        return histogram;
    }

    public static List<Player> PlayerRanking(List<Player> players)
    {
        PriorityQueue<Player> ranking = new PriorityQueue<Player>();
        foreach(Player player in players)
        {
            if(player.gamesPlayed > 0)
                ranking.Insert(player, -player.BestPositionValue());
        }
        return ranking.ToList();
    }
}
