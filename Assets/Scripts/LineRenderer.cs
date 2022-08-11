using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineRenderer : MonoBehaviour
{
    public GameObject line;
    private Vector2 start, end;
    private float length, width = 10;
    private bool show = true;
    private bool disappearing = true;
    private GameObject lineInstance;

    public LineRenderer(GameObject line = null)
    {
        if (line != null) this.line = line;
    }

    public void SetDisappearing(bool arg)
    {
        disappearing = arg;
    }

    public void SetSize(float size)
    {
        width = size;
    }

    public void SetStartingPoint(Vector2 p)
    {
        start = p;  
    }

    public void SetEndPoint(Vector2 p)
    {
        end = p;    
    }

    public void Destroy()
    {
        GameObject.Destroy(lineInstance);
    }

    public GameObject Draw(Transform parent)
    {
        if (disappearing)
        {
            if (show)
            {
                length = (end - start).magnitude;
                lineInstance = Instantiate(line, start, Quaternion.identity, parent);
                lineInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(length, width);
                Rotate();
            }
            if (!show)
            {
                GameObject.Destroy(lineInstance);
            }
            show = false;
        }
        else
        {
            Debug.Log("Length of " + end + " - " + start + " = " + (end-start).magnitude);
            length = (end - start).magnitude;
            lineInstance = Instantiate(line, start, Quaternion.identity, parent);
            lineInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(length, width);
            Rotate();
        }
        lineInstance.transform.SetAsFirstSibling();
        return lineInstance;
        //lineInstance.GetComponent<RectTransform>().localPosition = start;
    }

    public void SetColor(Color c)
    {
        lineInstance.GetComponent<Image>().color = c;
    }

    void Rotate()
    {
        Vector2 normDirection = (end- start).normalized;
        lineInstance.transform.right = normDirection;
    }
}
