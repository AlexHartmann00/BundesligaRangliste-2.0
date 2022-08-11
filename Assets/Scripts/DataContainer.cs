using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataContainer : MonoBehaviour
{
    public TextAsset[] data;
    // Start is called before the first frame update
    void Start()
    {
        foreach(TextAsset a in data)
        {
            Debug.Log(a.text);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
