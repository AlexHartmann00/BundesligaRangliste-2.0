using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Globalization;

public class ServerFileManager : MonoBehaviour
{
    string playerURL = "http://192.168.178.34/getplayers.php";
    string clubURL = "http://192.168.178.34/getclubs.php";
    static JSONDataTypes.playerData players;
    static JSONDataTypes.clubData clubs;
    static JSONDataTypes.analyticsData analytics;
    static JSONDataTypes.historyData history;
    string connectionText;
    bool loaded = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(RankingGlobal.URLs.GET_PLAYERS_URL + " PATHDEBUG");
        CheckConnection();
        StartCoroutine(getData());
    }

    public List<Club> LoadClubsFromServer()
    {
        List<Club> clubList = new List<Club>();
        foreach(var club in clubs.data)
        {
            Debug.Log(club.name + ", " + club.id + " - JSONDEBUG");
            Sprite logo = Resources.Load<Sprite>("Logos/" + club.id);
            clubList.Add(new Club(club.name, club.id, logo,club.leagueid));
        }
        return clubList;
    }

    public List<Club> LoadPlayersToClubs(List<Club> clubs)
    {
        foreach (var player in players.data)
        {
            Player p = new Player(player.id,
                player.name,
                player.defvalue,
                player.offvalue,
                player.gp,
                player.skew,
                player.off,
                player.clubid);

            if (player.clubid == 39)
                p.SetClubID(RankingGlobal.Constants.FREE_AGENT_INDEX);

            if (p.GetClubID() != RankingGlobal.Constants.FREE_AGENT_INDEX)
                clubs[p.GetClubID()].AddPlayer(p);
        }
        return clubs;
    }

    public List<Player> LoadFreeAgents()
    {
        List<Player> playerList = new List<Player>();
        foreach (var player in players.data)
        {
            Player p = new Player(player.id,
                player.name,
                player.defvalue,
                player.offvalue,
                player.gp,
                player.skew,
                player.off,
                player.clubid);

            if (player.clubid == 36)
                p.SetClubID(RankingGlobal.Constants.FREE_AGENT_INDEX);

            if (p.GetClubID() == RankingGlobal.Constants.FREE_AGENT_INDEX)
                playerList.Add(p);
        }
        return playerList;
    }

    IEnumerator getData()
    {
        Debug.Log("Processing Data");
        WWW _www = new WWW(RankingGlobal.URLs.GET_PLAYERS_URL);
        yield return _www;
        if (_www.error == null)
        {
            processJsonPlayerData(_www.text);
        }
        else
        {
            Debug.Log("Error fetching Json");
        }
        _www = new WWW(RankingGlobal.URLs.GET_CLUBS_URL);
        yield return _www;
        if (_www.error == null)
        {
            processJsonClubData(_www.text);
        }
        else
        {
            Debug.Log("Error fetching Json");
        }
        _www = new WWW(RankingGlobal.URLs.GET_ANALYTICS_URL);
        yield return _www;
        if (_www.error == null)
        {
            processJsonAnalyticsData(_www.text);
        }
        else
        {
            Debug.Log("Error fetching Json");
        }
        _www = new WWW(RankingGlobal.URLs.GET_HISTORY_URL);
        yield return _www;
        if (_www.error == null)
        {
            processJsonHistoryData(_www.text);
        }
        else
        {
            Debug.Log("Error fetching Json");
        }
        _www = new WWW(RankingGlobal.URLs.GET_ANALYTICS_URL);
        yield return _www;
        if (_www.error == null)
        {
            processJsonAnalyticsData(_www.text);
        }
        else
        {
            Debug.Log("Error fetching Json");
        }
        Debug.Log("FROM JSON: " + JsonUtility.ToJson(players));
        Debug.Log("FROM JSON: " + players.data[0].name);
        loaded = true;
    }

    void processJsonPlayerData(string txt)
    {
        players = JsonUtility.FromJson<JSONDataTypes.playerData>(txt);
    }  
    
    void processJsonClubData(string txt)
    {
        clubs = JsonUtility.FromJson<JSONDataTypes.clubData>(txt);
    }

    void processJsonAnalyticsData(string txt)
    {
        analytics = JsonUtility.FromJson<JSONDataTypes.analyticsData>(txt);
    }

    void processJsonHistoryData(string txt)
    {
        history = JsonUtility.FromJson<JSONDataTypes.historyData>(txt);
    }

    public void WritePlayersToServer(List<Club> clubs)
    {
        for (int i = 0; i < clubs.Count; i++)
        {
            Club c = clubs[i];
            foreach (Player p in c.GetPlayers())
            {
                if (p.changed)
                {
                    string url = RankingGlobal.URLs.CHANGE_PLAYER_URL + p.ServerQueryUpdateRepresentation();
                    StartCoroutine(GetRequest(url));
                    p.changed = false;
                }

            }
        }
    }

    public void WritePlayerToServer(Player p)
    {
        string url = RankingGlobal.URLs.CHANGE_PLAYER_URL + p.ServerQueryUpdateRepresentation();
        StartCoroutine(GetRequest(url));
    }

    public void AddPlayerToServer(Player p)
    {
        string url = RankingGlobal.URLs.ADD_PLAYER_URL + p.ServerQueryUpdateRepresentation();
        StartCoroutine(GetRequest(url));
    }

    public Dictionary<int,List<HistoryEntry>> LoadHistoryEntries()
    {

        Dictionary<int, List<HistoryEntry>> entries = new Dictionary<int, List<HistoryEntry>>();
        foreach(var entry in history.data)
        {
            HistoryEntry h = new HistoryEntry(entry.id, entry.skew, entry.off, entry.defvalue, entry.offvalue);
            if (entries.ContainsKey(entry.id))
            {
                entries[entry.id].Add(h);
            }
            else
            {
                entries[entry.id] = new List<HistoryEntry>();
                entries[entry.id].Add(h);
            }
        }
        return entries;
    }

    public void WriteHistoryEntryToServer(Match match)
    {
        List<HistoryEntry> historyEntries = HistoryEntry.FromMatch(match);
        foreach(HistoryEntry historyEntry in historyEntries)
        {
            string url = RankingGlobal.URLs.ADD_HISTORY_URL + historyEntry.ServerQueryUpdateRepresentation();
            StartCoroutine(GetRequest(url));
        }
    }   
    
    public void WriteHistoryEntryToServer(HistoryEntry entry)
    {
        string url = RankingGlobal.URLs.ADD_HISTORY_URL + entry.ServerQueryUpdateRepresentation();
        StartCoroutine(GetRequest(url));
    }

    public void WriteAnalyticsEntryToServer(AnalyticsMatch match)
    {
        string url = RankingGlobal.URLs.ADD_ANALYTICS_URL + match.GetServerQueryRepresentation();
        StartCoroutine(GetRequest(url));
    }

    public List<AnalyticsMatch> LoadAnalyticsMatchesFromServer()
    {
        List<AnalyticsMatch> matches = new List<AnalyticsMatch>();
        foreach(var entry in analytics.data)
        {
            AnalyticsMatch m = new AnalyticsMatch(entry.homename, entry.awayname, entry.xg1, entry.xg2, entry.g1, entry.g2);
            matches.Add(m);
        }
        return matches;
    }

    public void WriteClubToServer(Club c)
    {
        string url = RankingGlobal.URLs.CHANGE_CLUB_URL + c.ServerQueryUpdateRepresentation();
        StartCoroutine(GetRequest(url));
    }

    public void AddClubToServer(Club c)
    {
        string url = RankingGlobal.URLs.ADD_CLUB_URL + c.ServerQueryUpdateRepresentation();
        StartCoroutine(GetRequest(url));
    }

    public bool CheckConnection()
    {
        StartCoroutine(ConnectionCheck());
        Debug.Log("Connection text: " + connectionText);
        return connectionText == "connected";
    }

    IEnumerator ConnectionCheck()
    {
        WWW _www = new WWW(RankingGlobal.URLs.CONNECTION_CHECK_URL);
        yield return _www;
        if(_www.error != null)
        {
            connectionText = "";
        }
        else
        {
            connectionText = _www.text;
        }
        RankingGlobal.Status.online = connectionText == "connected";
        Debug.Log("Connection text: " + connectionText);
    }

    IEnumerator GetRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            uwr.Abort();
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text + "\n at " + uri);
        }
    }
}