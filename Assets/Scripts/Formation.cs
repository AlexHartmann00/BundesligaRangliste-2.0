using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation
{
    List<int> spec = new List<int>();
    List<Vector2> coordinates = new List<Vector2>();

    List<List<float>> SKEW_POSITIONS = new List<List<float>> { new List<float> { 0.5f }, new List<float> { 0.33f, 0.67f }, new List<float> { 0.25f, 0.5f, 0.75f }, new List<float> { 0.1f, 0.367f, 0.633f, 0.9f }, new List<float> { 0.1f,0.3f,0.5f,0.7f,0.9f} };

    public Formation(List<int> spec)
    {
        this.spec = spec;
        Make();
    }

    public Formation(string s)
    {
        string[] elements = s.Split('-');
        foreach(string element in elements)
        {
            spec.Add(int.Parse(element));
        }
        Make();
    }

    public List<int> GetSpecification()
    {
        return spec;
    }

    public List<Vector2> GetCoordinates()
    {
        return coordinates;
    }

    public bool Equals(Formation other)
    {
        return other.spec == spec;
    }
    void Make()
    {
        int formationLength = spec.Count;
        float stepSize = (1f - RankingGlobal.Constants.PENALTY_AREA_START - 0.125f)/((float)formationLength-2);
        float off = 0;
        bool goalkeeperfinished = false;
        foreach(int width in spec)
        {
            for(int i = 0; i < width; i++)
            {
                coordinates.Add(new Vector2(SKEW_POSITIONS[width-1][i], off));
            }
            if (!goalkeeperfinished)
            {
                off += RankingGlobal.Constants.PENALTY_AREA_START + 0.05f;
                goalkeeperfinished = true;
            }
            else
            {
                off += stepSize;
            }
        }
    }
}
