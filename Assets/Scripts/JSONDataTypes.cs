using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONDataTypes : MonoBehaviour
{
    [System.Serializable]
    public struct playerData
    {
        [System.Serializable]
        public struct player
        {
            public int id;
            public string name;
            public int clubid;
            public float defvalue;
            public float offvalue;
            public int gp;
            public float skew;
            public float off;
        }

        public List<player> data;
    }   


    [System.Serializable]
    public struct clubData
    {
        [System.Serializable]
        public struct club
        {
            public int id;
            public string name;
            public int leagueid;
        }

        public List<club> data;
    }

    [System.Serializable]
    public struct analyticsData
    {
        [System.Serializable]
        public struct analytics
        {
            public string homename;
            public string awayname;
            public float xg1;
            public float xg2;
            public int g1;
            public int g2;
        }

        public List<analytics> data;
    }

    [System.Serializable]
    public struct historyData
    {
        [System.Serializable]
        public struct history
        {
            public int id;
            public float skew;
            public float off;
            public float defvalue;
            public float offvalue;
        }

        public List<history> data;
    }
}
