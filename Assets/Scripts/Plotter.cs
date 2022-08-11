using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;
using UnityEngine.UI;

public class Plotter : MonoBehaviour
{
    private float leftBound, lowerBound, rightBound, upperBound;
    public TextMeshProUGUI title, xlab, xaxis, ylab, yaxis;
    public TextMeshProUGUI[] xlim;
    public TextMeshProUGUI[] ylim;
    public GameObject textPrefab;
    List<float> X, Y;
    public LineRenderer lineRenderer;
    List<GameObject> lines = new List<GameObject>();
    (float, float) fontSize;
    bool initialized = false;
    float minimumY, maximumY, minimumX, maximumX;

    private void Initialize()
    {
        float xMax = -1;
        float xMin = 9999999;
        float yMax = -1;
        float yMin = 9999999;
        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        foreach (Vector3 corner in corners)
        {
            if (corner.x > xMax) xMax = corner.x;
            if (corner.x < xMin) xMin = corner.x;
            if (corner.y > yMax) yMax = corner.y;
            if (corner.y < yMin) yMin = corner.y;
        }
        leftBound = 0;
        lowerBound = 0;
        rightBound = xMax - xMin;
        upperBound = yMax - yMin;
        rightBound *= 1920f /(float) Screen.width;
        upperBound *= 1080f /(float) Screen.height;
        Debug.Log("right: " + rightBound + ", upper: " + upperBound);
    }

    public void LineGraph(List<float> x, List<float> y, Color c)
    {
        if (!initialized) Initialize();
        Debug.Log("Right: " + rightBound + ", upper: " + upperBound);
        fontSize = ((rightBound - leftBound)/30f, (upperBound - lowerBound)/15f);
        X = new List<float>(x);
        Y = new List<float>(y);
        if (!initialized)
        {
            minimumY = MathNet.Numerics.Statistics.Statistics.Minimum(y);
            maximumY = MathNet.Numerics.Statistics.Statistics.Maximum(y);
            minimumX = MathNet.Numerics.Statistics.Statistics.Minimum(x);
            maximumX = MathNet.Numerics.Statistics.Statistics.Maximum(x);
            xlim[0].text = MathNet.Numerics.Statistics.Statistics.Minimum(x).ToString("0.00", CultureInfo.InvariantCulture.NumberFormat);
            xlim[1].text = MathNet.Numerics.Statistics.Statistics.Maximum(x).ToString("0.00", CultureInfo.InvariantCulture.NumberFormat);
            ylim[0].text = MathNet.Numerics.Statistics.Statistics.Minimum(y).ToString("0.00", CultureInfo.InvariantCulture.NumberFormat);
            ylim[1].text = MathNet.Numerics.Statistics.Statistics.Maximum(y).ToString("0.00", CultureInfo.InvariantCulture.NumberFormat);
        }
        x = ConvertToXPlotCoordinates(x,rightBound-leftBound);
        y = ConvertToYPlotCoordinates(y,upperBound-lowerBound);
        for(int i = 0; i < x.Count - 1; i++)
        {
            lineRenderer.SetStartingPoint(new Vector2((float)x[i], (float)y[i]));
            lineRenderer.SetEndPoint(new Vector2((float)x[i + 1], (float)y[i + 1]));
            lineRenderer.SetSize(2.5f);
            GameObject lineInstance = lineRenderer.Draw(transform);
            lineRenderer.SetColor(c);
            lineInstance.GetComponent<RectTransform>().localPosition = new Vector2((float)x[i], (float)y[i]);
            lineInstance.GetComponent<RectTransform>().anchoredPosition.Set((float)x[i], (float)y[i]);
            lines.Add(lineInstance);
        }
        initialized = true;
    }

    public void LineGraph(List<float> y, Color c)
    {
        List<float> x = new List<float>();
        for(int i = 0; i < y.Count; i++)
        {
            x.Add(i + 1);
        }
        LineGraph(x, y, c);
    }

    public void HLine(float x, Color c)
    {
        if (!initialized) Initialize();
        Debug.Log("x: " + x + ", plotC: " + ConvertToPlotCoordinate(x, Y, upperBound - lowerBound));
        x = ConvertToPlotCoordinate(x, Y, upperBound - lowerBound);
        lineRenderer.SetStartingPoint(new Vector2(leftBound, x));
        lineRenderer.SetEndPoint(new Vector2(rightBound, x));
        lineRenderer.SetSize(2.5f);
        GameObject lineInstance = lineRenderer.Draw(transform);
        lineRenderer.SetColor(c);

        lineInstance.GetComponent<RectTransform>().localPosition = new Vector2(leftBound,x);
        lines.Add(lineInstance);
    }

    public void Text(float x, float y, string text, Color c)
    {
        if (!initialized) Initialize();
        x = ConvertToPlotCoordinate((float)x, X, rightBound - leftBound);
        y = ConvertToPlotCoordinate((float)y, Y, upperBound - lowerBound);
        GameObject textInstance = Instantiate(textPrefab, new Vector2(x, y), Quaternion.identity, transform);
        textInstance.GetComponent<RectTransform>().localPosition = new Vector2(x, y);
        textInstance.GetComponent<TextMeshProUGUI>().text = text;
        textInstance.GetComponent<TextMeshProUGUI>().color = c;
        textInstance.GetComponent<TextMeshProUGUI>().fontSize = fontSize.Item1;
    }

    public void BackgroundColor(Color c)
    {
        GetComponent<Image>().color = c;
    }

    public void Title(string x)
    {
        title.text = x;
    }

    public void YLab(string x)
    {
        ylab.text = x;
        ylab.fontSize = fontSize.Item2;
    }
    
    public void XLab(string x)
    {
        xlab.text = x;
        xlab.fontSize = fontSize.Item1;
    }

    public void Destroy()
    {
        foreach(GameObject line in lines)
        {
            Destroy(line);
        }
    }

    List<float> ConvertToYPlotCoordinates(List<float> x, float plotsize)
    { 
        float xRange = maximumY - minimumY;
        for(int i = 0; i < x.Count; i++)
        {
            x[i] -= minimumY;
            x[i] /= xRange;
            x[i] *= plotsize;
        }
        return x;
    } 
    
    List<float> ConvertToXPlotCoordinates(List<float> x, float plotsize)
    { 
        float xRange = maximumX - minimumX;
        for(int i = 0; i < x.Count; i++)
        {
            x[i] -= minimumX;
            x[i] /= xRange;
            x[i] *= plotsize;
        }
        return x;
    }

    float ConvertToPlotCoordinate(float x, List<float> reference, float plotsize)
    {
        float xx = x;
        float min = MathNet.Numerics.Statistics.Statistics.Minimum(reference);
        float max = MathNet.Numerics.Statistics.Statistics.Maximum(reference);
        float xRange = max - min;
        Debug.Log("XX: " + xx + ", xRange = " + xRange + ", plot size: " + plotsize);
        xx -= min;
        xx /= xRange;
        xx *= plotsize;
        Debug.Log("Conversion: " + x + " in " + "(" + min + "," + max + ") ==> " + xx);
        return xx;
    }

}
