using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RankingGlobal
{
    public class Constants//These are changed on startup
    {
        public static float CHANGE_CONSTANT = 24f;

        public static float GetChangeConstant(Player p)
        {
            if(p.gamesPlayed < 5)
            {
                return CHANGE_CONSTANT*1.25f;
            }
            else if(p.gamesPlayed < 15)
            {
                return CHANGE_CONSTANT;
            }
            else
            {
                return CHANGE_CONSTANT * 0.75f;
            }
        }

        public static float GAMMA_SHAPE = 1.305842f;
        public static float GAMMA_RATE = 0.8035949f;
        public static float POSITION_PENALTY = 5f;//TODO: change when trained
        public static float CHANGE_THRESHOLD = 80f;
        public static float MAX_PREDICTION = 5.5f;
        public static float MAX_CHANGE = 20f;
        public static float PENALTY_AREA_START = 0.2f;
        public static float MAX_OFFENSIVENESS_DEFENDER = 0.35f;
        public static float MAX_OFFENSIVENESS_DEF_MID = 0.5f;
        public static float MAX_OFFENSIVENESS_MIDFIELDER = 0.65f;
        public static float MAX_OFFENSIVENESS_OFF_MID = 0.8f;
        public static float MAX_OFFENSIVENESS_STRIKER = 1f;
        public static float MAX_OFFENSIVENESS = 0.95f;
        public static float MAX_POSITION_DECAY = 3f;
        public static float SYMMETRIC_HOME_ADVANTAGE = 0.13f;
        public static int FREE_AGENT_INDEX = -99;
        public static float BIAS_CORRECTION_HOME = 0f;
        public static float BIAS_CORRECTION_AWAY = 0f;
    }

    public class Data
    {
        public static List<Club> clubs = new List<Club>();
    }
    
    public class Paths
    {
        public static string MAIN_DATA_PATH = Application.persistentDataPath + "/Data/data.txt";
        public static string CLUB_DATA_PATH = Application.persistentDataPath + "/Data/clubData.txt";
        public static string ANALYTICS_DATA_PATH = Application.persistentDataPath + "/analytics.txt";
        public static string HISTORY_DATA_PATH = Application.persistentDataPath + "/history.txt";
        public static string MAIN_BUFFER_PATH = Application.persistentDataPath + "/Buffer/data.txt";
        public static string CLUB_BUFFER_PATH = Application.persistentDataPath + "/Buffer/clubData.txt";
        public static string ANALYTICS_BUFFER_PATH = Application.persistentDataPath + "/Buffer/analytics.txt";
        public static string HISTORY_BUFFER_PATH = Application.persistentDataPath + "/Buffer/history.txt";

    }

    public class URLs
    {
        public static string MAIN_SERVER_ADDRESS = "http://alexhartmann.eu/";
        public static string GET_PLAYERS_URL = MAIN_SERVER_ADDRESS + "getplayers.php?";
        public static string CHANGE_PLAYER_URL = MAIN_SERVER_ADDRESS + "changeplayer.php?";
        public static string ADD_PLAYER_URL = MAIN_SERVER_ADDRESS + "addplayer.php?";
        public static string GET_CLUBS_URL = MAIN_SERVER_ADDRESS + "getclubs.php?";
        public static string ADD_CLUB_URL = MAIN_SERVER_ADDRESS + "addclub.php?";
        public static string CHANGE_CLUB_URL = MAIN_SERVER_ADDRESS + "changeclub.php?";
        public static string GET_ANALYTICS_URL = MAIN_SERVER_ADDRESS + "getanalytics.php?";
        public static string ADD_ANALYTICS_URL = MAIN_SERVER_ADDRESS + "addanalytics.php?";
        public static string GET_HISTORY_URL = MAIN_SERVER_ADDRESS + "gethistory.php?";
        public static string ADD_HISTORY_URL = MAIN_SERVER_ADDRESS + "addhistory.php?";
        public static string CONNECTION_CHECK_URL = MAIN_SERVER_ADDRESS + "connectiontest.php?";
    }

    public class Status
    {
        public static bool online = true;
        public static bool onlineWhenLastSaved = true;
    }

    public class Verification
    {
        public static string key;

        public static bool VerifyKey()
        {
            return key == "alex";
        }
    }

    public class Calculations
    {

    }

    namespace Utilities
    {
        public class PriorityQueue<T>
        {
            List<T> list;
            List<float> values;

            public PriorityQueue()
            {
                list = new List<T>();
                values = new List<float>();
            }

            public void Insert(T element, float value)
            {
                if (values.Count == 0)
                {
                    list.Add(element);
                    values.Add(value);
                }
                else
                {
                    int index = values.BinarySearch(value);
                    index = index < 0 ? ~index : index;
                    list.Insert(index, element);
                    values.Insert(index, value);
                }
            }

            public List<T> ToList()
            {
                return list;
            }
        }
    }
    
}
