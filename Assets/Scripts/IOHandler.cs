using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IOHandler : MonoBehaviour
{
    public ServerFileManager sfm;

    public bool CheckConnection()
    {
        return sfm.CheckConnection();
    }

    public List<Club> LoadRawClubData(bool online)
    {
        if (online && RankingGlobal.Status.onlineWhenLastSaved)
        {
            return sfm.LoadClubsFromServer();
        }
        else
        {
            return FileManager.ReadFromFile(RankingGlobal.Paths.CLUB_DATA_PATH);
        }
    }

    public List<Club> LoadPlayersToClubs(List<Club> clubs, bool online)
    {
        if (online && RankingGlobal.Status.onlineWhenLastSaved)
        {
            return sfm.LoadPlayersToClubs(clubs);
        }
        else
        {
            return FileManager.LoadPlayersToClubs(RankingGlobal.Paths.MAIN_DATA_PATH, clubs);
        }
    }

    public List<Player> LoadFreeAgents(bool online)
    {
        return sfm.LoadFreeAgents();
    }

    public void WritePlayers(List<Club> clubs,bool online)
    {
        if (online)
        {
            /*foreach(Player p in FileManager.ReadFromBufferFile(RankingGlobal.Paths.MAIN_BUFFER_PATH))
            {
                sfm.WritePlayerToServer(p);
            }
            FileManager.Clear(RankingGlobal.Paths.MAIN_BUFFER_PATH);*/
            sfm.WritePlayersToServer(clubs);
        }
        else
        {
            FileManager.WritePlayersToBufferFile(RankingGlobal.Paths.MAIN_BUFFER_PATH,clubs);
        }
        SetLastOnlinePref(online);
        FileManager.WritePlayersToFile(RankingGlobal.Paths.MAIN_DATA_PATH, clubs);
    }

    public void WriteHistory(Match match, bool online)
    {
        if (online)
        {
            /*foreach(HistoryEntry entry in FileManager.ReadUngroupedHistoryFile(RankingGlobal.Paths.HISTORY_BUFFER_PATH))
            {
                sfm.WriteHistoryEntryToServer(entry);
            }
            FileManager.Clear(RankingGlobal.Paths.HISTORY_BUFFER_PATH);*/
            sfm.WriteHistoryEntryToServer(match);
        }
        else
        {
            FileManager.WriteToHistoryFile(RankingGlobal.Paths.HISTORY_BUFFER_PATH, match);
        }
        SetLastOnlinePref(online);
        FileManager.WriteToHistoryFile(RankingGlobal.Paths.HISTORY_DATA_PATH, match);

    }

    public Dictionary<int, List<HistoryEntry>> LoadHistory(bool online)
    {
        if (online && RankingGlobal.Status.onlineWhenLastSaved)
        {
            return sfm.LoadHistoryEntries();
        }
        else
        {
            return FileManager.ReadFromHistoryFile(RankingGlobal.Paths.HISTORY_DATA_PATH);
        }
    }

    public void WriteAnalytics(Match match, bool online)
    {
        if (online)
        {
            /*foreach(AnalyticsMatch m in FileManager.LoadMatchesFromFile(RankingGlobal.Paths.ANALYTICS_BUFFER_PATH))
            {
                sfm.WriteAnalyticsEntryToServer(m);
            }
            FileManager.Clear(RankingGlobal.Paths.ANALYTICS_BUFFER_PATH);*/
            sfm.WriteAnalyticsEntryToServer(match);
        }
        else
        {
            FileManager.WriteToAnalyticsFile(RankingGlobal.Paths.ANALYTICS_BUFFER_PATH, match);
        }
        SetLastOnlinePref(online);
        FileManager.WriteToAnalyticsFile(RankingGlobal.Paths.ANALYTICS_DATA_PATH, match);

    }

    public List<AnalyticsMatch> LoadAnalytics(bool online)
    {
        if (online && RankingGlobal.Status.onlineWhenLastSaved)
        {
            return sfm.LoadAnalyticsMatchesFromServer();
        }
        else
        {
            return FileManager.LoadMatchesFromFile(RankingGlobal.Paths.ANALYTICS_DATA_PATH);
        }
    }

    void SetLastOnlinePref(bool online)
    {
        PlayerPrefs.SetInt("lastOnline", online ? 1 : 0);
        RankingGlobal.Status.onlineWhenLastSaved = online;
    }
}
