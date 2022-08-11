using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optimization : MonoBehaviour
{
    public float LinearRegressionSlopeParameter(List<float> y, List<float> x)
    {
        return Covariance(x,y)/Variance(x);
    }

   
    float Covariance(List<float> y, List<float> x)
    {
        float xMean = Mean(x), yMean  = Mean(y);
        float sum = 0;
        for(int i = 0; i < y.Count; i++)
        {
            sum += (y[i] - yMean) * (x[i] - xMean);
        }
        return sum / ((float)y.Count-1);
    }

    float Variance(List<float> x)
    {
        float mean = Mean(x);
        float sum = 0;
        foreach(float y in x)
        {
            sum += Mathf.Pow(y - mean, 2);
        }
        return mean / ((float)x.Count-1);
    }

    float Mean(List<float> x)
    {
        float sum = 0;
        float count = 0;
        for(int i = 0; i < x.Count; i++)
        {
            sum += x[i];
            count++;
        }
        return sum / count;
    }
}
