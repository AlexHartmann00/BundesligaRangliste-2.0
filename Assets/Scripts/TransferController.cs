using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Globalization;
using RankingGlobal.Utilities;
using UnityEngine.SceneManagement;


public class TransferController : MonoBehaviour
{
    public IOHandler iohandler;
    public TMP_InputField inputField;
    public TMP_Text LogTextField;
    public TMP_Text clubText;
    List<Club> clubs;
    public ServerFileManager serverFileManager;
    List<Player> foundPlayers = new List<Player>();
    List<Player> freeAgents = new List<Player> ();
    List<int> playerIndices = new List<int>();


    private void Start()
    {
        iohandler.CheckConnection();
        Invoke("Initialize", 1);

    }

    void Initialize()
    {
        Log(RankingGlobal.Status.online.ToString());
        if(RankingGlobal.Data.clubs.Count == 0)
        {
            clubs = iohandler.LoadRawClubData(RankingGlobal.Status.online);
            clubs = iohandler.LoadPlayersToClubs(clubs, RankingGlobal.Status.online);
            RankingGlobal.Data.clubs = clubs;
        }
        clubs = RankingGlobal.Data.clubs;
        freeAgents = iohandler.LoadFreeAgents(RankingGlobal.Status.online);
        clubs.Add(new Club("Free Agents", -99, null));
        foreach (Club c in clubs)
        {
            clubText.text += c.GetID() + ". " + c.GetName() + " - " + c.MeanValue() + " / " + c.MaxPlayer().BestPositionValue() + " (" + c.MaxPlayer().name + ")" + "\n";
            if(c.GetName() == "Free Agents")
            {
                foreach(Player p in freeAgents)
                {
                    c.AddPlayer(p);
                }
            }
            foreach (Player p in c.GetPlayers())
            {
                playerIndices.Add(p.GetID());
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return)) { Evaluate(inputField.text); }
    }

    void Evaluate(string input)
    {
        string command = Parse(input);
        inputField.text = "";
        switch (command.ToUpper()) {
            case "ADD":
                Add();
                string player = input.Split(' ')[1];
                int targetClub = int.Parse(input.Split(' ')[2]);
                float defvalue = float.Parse(Parse(input.Split(' ')[3]), CultureInfo.InvariantCulture.NumberFormat);
                float offvalue = float.Parse(Parse(input.Split(' ')[4]), CultureInfo.InvariantCulture.NumberFormat);
                int index = 0;
                for(int i = 0; i < 9999; i++)
                {
                    if (!playerIndices.Contains(i))
                    {
                        index = i;
                        break;
                    }
                }
                Player p = new Player(index,player,defvalue,offvalue,0,0,0,targetClub);
                serverFileManager.AddPlayerToServer(p);
                clubs[targetClub].AddPlayer(p);
                playerIndices.Add(index);
                break;
                //Log("adding " + player + " to " + clubs[targetClub].GetName() + ", " + samenameplayers.Count + " players found");
            case "FIND":
                Find(input.Split(' '));
                break;
            case "CLUBLIST":
                Find(input.Split (' '),int.Parse(input.Split(' ')[1]));
                break;
            case "TRANSFER":
                Transfer(input.Split(' '));
                break;
            case "RELOAD":
                Initialize();
                break;
            case "CHANGECLUB":
                ChngClub(input.Split(' '));
                break;
            case "ADDCLUB":
                AddClub(input.Split(' '));
                break;
            case "CHANGEPLAYER":
                ChngPlayer(input.Split(' '));
                break;

        }

    }

    void Add()
    {

    }

    void ChngPlayer(string[] input)
    {
        int foundid = int.Parse(input[1].Replace("$",""));
        string name  = input[2];
        int clubid = int.Parse(input[3]);
        float dv = float.Parse(input[4]);
        float ov = float.Parse(input[5]);
        Player p_ = foundPlayers[foundid];
        p_.SetClubID(clubid);
        p_.SetName(name);
        p_.SetValues(dv, ov);
        serverFileManager.WritePlayerToServer(p_);
    }

    void ChngClub(string[] input)
    {
        int clubid = int.Parse(input[1]);
        int leagueID = int.Parse(input[2]);
        Debug.Log("CHNGCLUB - " + clubs[clubid].GetName() + " (" + clubs[clubid].GetLeagueID() + ")");
        clubs[clubid].SetLeagueID(leagueID);
        Debug.Log("CHNGCLUB - " + clubs[clubid].GetName() + " (" + clubs[clubid].GetLeagueID() + ")");
        Debug.Log(RankingGlobal.URLs.CHANGE_CLUB_URL + clubs[clubid].ServerQueryUpdateRepresentation());
        serverFileManager.WriteClubToServer(clubs[clubid]);
    }

    void AddClub(string[] input)
    {
        int clubid = clubs.Count - 1;
        string clubname = input[1];
        int leagueID = int.Parse(input[2]);
        serverFileManager.AddClubToServer(new Club(clubname,clubid, Resources.Load<Sprite>("Logos/" + 0),leagueID));
    }

    void Find(string[] input, int clubid = -1)
    {
        string name = input[1];
        int dist = 1;
        if(input.Length > 2)
        {
            dist = int.Parse(input[2]);
        }
        if(clubid != -1)
        {
            dist = 99999;
        }
        foundPlayers = NameSearch(name,clubid: clubid,distance: dist);
        if(clubid != -1)
        {
            PriorityQueue<Player> foundPlayers2 = new PriorityQueue<Player>();
            foreach(Player p in foundPlayers)
            {
                foundPlayers2.Insert(p, -p.BestPositionValue());
            }
            foundPlayers = foundPlayers2.ToList();
        }
        string logstring = "";
        for (int i = 0; i < foundPlayers.Count; i++)
        {
            Player p = foundPlayers[i];
            logstring += i + ". " + p.name + " - " + 
                (p.GetClubID() == RankingGlobal.Constants.FREE_AGENT_INDEX ? "Free Agents" : clubs[p.GetClubID()].GetName()) + " (" + p.GetClubID() + ") - " + Position.FromVector2(p.GetBestPosition()) + " - " + Mathf.RoundToInt(p.BestPositionValue()) + "\n";
        }
        Log(logstring);
    }

    void Transfer(string[] input)
    {
        string name = "";
        int club1id = -1;
        int club2id = -1;
        if (input[1].Contains("$"))
        {
            string s = input[1].Replace("$", "");
            int index = int.Parse(s);
            name = foundPlayers[index].name;
            club1id = foundPlayers[index].GetClubID();
            club2id = int.Parse(input[2]);
        }
        else
        {
            name = input[1];
            club1id = int.Parse(input[2]);
            club2id = int.Parse(input[3]);
        }
        club1id = club1id == RankingGlobal.Constants.FREE_AGENT_INDEX ? clubs.Count - 1 : club1id;
        club2id = club2id == RankingGlobal.Constants.FREE_AGENT_INDEX ? clubs.Count - 1 : club2id;
        Player p_ = NameSearch(name, club1id)[0];
        clubs[club1id].GetPlayers().Remove(p_);
        clubs[club2id].AddPlayer(p_);
        string logstring = p_.ServerQueryUpdateRepresentation() + "\n";
        p_.SetClubID(club2id == clubs.Count - 1 ? -99 : club2id);
        Log(logstring + p_.ServerQueryUpdateRepresentation());
        serverFileManager.WritePlayerToServer(p_);
    }

    List<Player> NameSearch(string name, int clubid = -1, int distance = 1)
    {
        List<Player> list = new List<Player>();
        List<int> distances = new List<int>();
        PriorityQueue<Player> players = new PriorityQueue<Player>();
        if(clubid == -1)
        {
            foreach (Club c in clubs)
            {
                foreach (Player player in c.GetPlayers())
                {
                    int dist = GetDamerauLevenshteinDistance(name.ToLower(), player.name.ToLower());
                    if (dist <= distance)
                    {
                        list.Add(player);
                        players.Insert(player, dist);
                    }
                }
            }
        }
        else
        {
            Club c = clubid == RankingGlobal.Constants.FREE_AGENT_INDEX ? clubs[clubs.Count-1] : clubs[clubid];
            foreach (Player player in c.GetPlayers())
            {
                int dist = GetDamerauLevenshteinDistance(name.ToLower(), player.name.ToLower());
                if (dist <= distance)
                {
                    list.Add(player);
                    players.Insert(player, dist);
                }
            }
        }
        return players.ToList();
    }

    string Parse(string input)
    {
        string type = input.Split(' ')[0];
        return type;
    }

    void Log(string content)
    {
        LogTextField.text = content;
    }

    string ReplaceUmlaut(string s)
    {
        return s.Replace("ä", "ae").Replace("ü", "ue").Replace("ö", "oe");
    }

    public int GetDamerauLevenshteinDistance(string s, string t)
    {
        s = ReplaceUmlaut(s);
        t = ReplaceUmlaut(t);
        var bounds = new { Height = s.Length + 1, Width = t.Length + 1 };

        int[,] matrix = new int[bounds.Height, bounds.Width];

        for (int height = 0; height < bounds.Height; height++) { matrix[height, 0] = height; };
        for (int width = 0; width < bounds.Width; width++) { matrix[0, width] = width; };

        for (int height = 1; height < bounds.Height; height++)
        {
            for (int width = 1; width < bounds.Width; width++)
            {
                int cost = (s[height - 1] == t[width - 1]) ? 0 : 1;
                int insertion = matrix[height, width - 1] + 1;
                int deletion = matrix[height - 1, width] + 1;
                int substitution = matrix[height - 1, width - 1] + cost;

                int distance = Mathf.Min(insertion, Mathf.Min(deletion, substitution));

                if (height > 1 && width > 1 && s[height - 1] == t[width - 2] && s[height - 2] == t[width - 1])
                {
                    distance = Mathf.Min(distance, matrix[height - 2, width - 2] + cost);
                }

                matrix[height, width] = distance;
            }
        }

        return matrix[bounds.Height - 1, bounds.Width - 1];
    }

    public void ToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
