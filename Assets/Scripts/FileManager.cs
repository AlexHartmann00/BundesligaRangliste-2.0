using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;

public class FileManager
{
    public static void CreateIfDoesNotExist(string path)
    {
        if (!File.Exists(path))
        {
            File.Create(path).Dispose();
        }
    }

    public static List<Club> ReadFromFile(string path)
    {
        FileManager.CreateIfDoesNotExist(path);
        List<Club> lst = new List<Club>();
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            string[] elements = line.Split(',');
            int id = Int32.Parse(elements[3]);
            int leagueid = Int32.Parse(elements[4]);
            Sprite logo = Resources.Load<Sprite>("Logos/"+elements[3]);

            foreach (string element in elements)
            {
                Debug.Log(element);
            }
            lst.Add(new Club(elements[2],id,logo,leagueid));
            
        }
        return lst;
    }

    public static List<Club> LoadPlayersToClubs(string path, List<Club> clubs)
    {
        FileManager.CreateIfDoesNotExist(path);
        string[] lines = File.ReadAllLines(path);
        foreach(string line in lines)
        {
            string[] elements = line.Split(',');
            Debug.Log(elements);
            int playerID = Int32.Parse(elements[0]);
            string name = elements[1];
            int clubID = Int32.Parse(elements[2]);
            float off = float.Parse(elements[4], CultureInfo.InvariantCulture.NumberFormat);
            float def = float.Parse(elements[3], CultureInfo.InvariantCulture.NumberFormat);
            int gamesPlayed = int.Parse(elements[5]);
            float x = float.Parse(elements[6], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(elements[7], CultureInfo.InvariantCulture.NumberFormat);
            clubs[clubID].AddPlayer(new Player(playerID,name,def,off,gamesPlayed,x,y,clubID));
        }
        foreach(Club c in clubs)
        {
            c.PrintPlayers();
        }
        return clubs;
    }

    public static void WritePlayersToFile(string path, List<Club> clubs)
    {
        List<string> lines = new List<string>();
        FileManager.CreateIfDoesNotExist(path);
        for (int i = 0; i < clubs.Count; i++)
        {
            Club c = clubs[i];
            foreach(Player p in c.GetPlayers())
            {
                Debug.Log("FileManager_DEBUG: " + p.FileSystemRepresentation());
                lines.Add(p.FileSystemRepresentation());
            }
        }
        File.WriteAllLines(path, lines.ToArray());
    }  
    
    public static void WritePlayersToBufferFile(string path, List<Club> clubs)
    {
        List<Player> players = FileManager.ReadFromBufferFile(path);
        List<string> lines = new List<string>();
        FileManager.CreateIfDoesNotExist(path);
        for (int i = 0; i < clubs.Count; i++)
        {
            Club c = clubs[i];
            foreach(Player p in c.GetPlayers())
            {
                if (p.changed || players.Contains(p))
                {
                    Debug.Log("FileManager_DEBUG: " + p.FileSystemRepresentation());
                    lines.Add(p.FileSystemRepresentation());
                }
            }
        }
        File.WriteAllLines(path, lines.ToArray());
    }

    public static void WriteToHistoryFile(string path, Match match)
    {
        FileManager.CreateIfDoesNotExist(path);
        List<string> lines = new List<string>();
        foreach(Player p in match.GetHomeClub().GetPlayers())
        {
            if (p.linedUp)
            {
                (float,float) vals = p.GetRawValues();
                float skew = p.away ? 1f - p.skewness : p.skewness;
                lines.Add(p.ID + "," + skew.ToString(new CultureInfo("en-us", false)) + "," + p.offensiveness.ToString(new CultureInfo("en-us", false)) + "," + vals.Item1.ToString(new CultureInfo("en-us", false)) + "," + vals.Item2.ToString(new CultureInfo("en-us", false)));
            }
        }
        foreach (Player p in match.GetAwayClub().GetPlayers())
        {
            if (p.linedUp)
            {
                (float, float) vals = p.GetRawValues();
                float skew = p.away ? 1f - p.skewness : p.skewness;
                lines.Add(p.ID + "," + skew.ToString(new CultureInfo("en-us", false)) + "," + p.offensiveness.ToString(new CultureInfo("en-us", false)) + "," + vals.Item1.ToString(new CultureInfo("en-us", false)) + "," + vals.Item2.ToString(new CultureInfo("en-us", false)));
            }
        }

        File.AppendAllLines(path, lines);
    }

    public static Dictionary<int,List<HistoryEntry>> ReadFromHistoryFile(string path)
    {
        FileManager.CreateIfDoesNotExist(path);
        Dictionary<int,List<HistoryEntry>> entries = new Dictionary<int, List<HistoryEntry>>();
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            string[] elements = line.Split(',');
            int playerID = Int32.Parse(elements[0]);
            float skew = float.Parse(elements[1], CultureInfo.InvariantCulture.NumberFormat);
            float off = float.Parse(elements[2], CultureInfo.InvariantCulture.NumberFormat);
            float defv = float.Parse(elements[3], CultureInfo.InvariantCulture.NumberFormat);
            float offv = float.Parse(elements[4], CultureInfo.InvariantCulture.NumberFormat);
            if (entries.ContainsKey(playerID))
            {
                entries[playerID].Add(new HistoryEntry(playerID,skew,off,defv,offv));
            }
            else
            {
                entries[playerID] = new List<HistoryEntry>();
                entries[playerID].Add(new HistoryEntry(playerID,skew, off, defv, offv));
            }
        }
        return entries;
    }

    public static List<HistoryEntry> ReadFromHistoryFileForPlayer(string path, int id)
    {
        FileManager.CreateIfDoesNotExist(path);
        List<HistoryEntry> entries = new List<HistoryEntry>();
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            string[] elements = line.Split(',');
            int playerID = Int32.Parse(elements[0]);
            if(playerID == id)
            {
                float skew = float.Parse(elements[1], CultureInfo.InvariantCulture.NumberFormat);
                float off = float.Parse(elements[2], CultureInfo.InvariantCulture.NumberFormat);
                float defv = float.Parse(elements[3], CultureInfo.InvariantCulture.NumberFormat);
                float offv = float.Parse(elements[4], CultureInfo.InvariantCulture.NumberFormat);
                entries.Add(new HistoryEntry(playerID,skew, off, defv, offv));
            }
        }
        return entries;
    }

    public static List<HistoryEntry> ReadUngroupedHistoryFile(string path)
    {
        FileManager.CreateIfDoesNotExist(path);
        List<HistoryEntry> entries = new List<HistoryEntry>();
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            string[] elements = line.Split(',');
            int playerID = Int32.Parse(elements[0]);

            float skew = float.Parse(elements[1], CultureInfo.InvariantCulture.NumberFormat);
            float off = float.Parse(elements[2], CultureInfo.InvariantCulture.NumberFormat);
            float defv = float.Parse(elements[3], CultureInfo.InvariantCulture.NumberFormat);
            float offv = float.Parse(elements[4], CultureInfo.InvariantCulture.NumberFormat);
            entries.Add(new HistoryEntry(playerID, skew, off, defv, offv));

        }
        return entries;
    }

    public static void WriteToAnalyticsFile(string path, Match match)
    {
        string line = "";
        FileManager.CreateIfDoesNotExist(path);
        line += match.GetHomeClub() + "," + match.GetAwayClub() + "," + match.prediction.Item1.ToString(new CultureInfo("en-us", false))
            + "," + match.prediction.Item2.ToString(new CultureInfo("en-us", false))
            + "," + match.trueResult.Item1.ToString(new CultureInfo("en-us", false))
            + "," + match.trueResult.Item2.ToString(new CultureInfo("en-us", false));
        File.AppendAllLines(path, new string[] { line });
    }

    public static List<AnalyticsMatch> LoadMatchesFromFile(string path)
    {
        FileManager.CreateIfDoesNotExist(path);
        List<AnalyticsMatch> matches = new List<AnalyticsMatch>();
        string[] lines = File.ReadAllLines(path);
        foreach(string line in lines)
        {
            string[] elements = line.Split(',');
            AnalyticsMatch m = new AnalyticsMatch(elements[0], elements[1],
                float.Parse(elements[2], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(elements[3], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(elements[4], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(elements[5], CultureInfo.InvariantCulture.NumberFormat));
            matches.Add(m);
        }
        return matches;
    }

    public static List<Player> ReadFromBufferFile(string path)
    {
        List<Player> players = new List<Player>();
        FileManager.CreateIfDoesNotExist(path);
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            string[] elements = line.Split(',');
            Debug.Log(elements);
            int playerID = Int32.Parse(elements[0]);
            string name = elements[1];
            int clubID = Int32.Parse(elements[2]);
            float off = float.Parse(elements[4], CultureInfo.InvariantCulture.NumberFormat);
            float def = float.Parse(elements[3], CultureInfo.InvariantCulture.NumberFormat);
            int gamesPlayed = int.Parse(elements[5]);
            float x = float.Parse(elements[6], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(elements[7], CultureInfo.InvariantCulture.NumberFormat);
            players.Add(new Player(playerID, name, def, off, gamesPlayed, x, y, clubID));
        }
        return players;
    }

    public static void Clear(string path)
    {
        File.Delete(path);
    }
}
