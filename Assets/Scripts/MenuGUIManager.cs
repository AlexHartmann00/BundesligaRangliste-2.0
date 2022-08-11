using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Globalization;

public class MenuGUIManager : MonoBehaviour
{
    List<AnalyticsMatch> analyticsMatchList;
    public Image onlineStatus;
    List<Club> clubs = new List<Club>();
    public TextMeshProUGUI diagnosticsText, logger;
    public Plotter errorPlot, accuracyPlot;
    public IOHandler iohandler;
    public CanvasScaler canvasScaler;
    // Start is called before the first frame update
    void Start()
    {
        //canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        Invoke("Initialize", 1);//wait for connection check to complete
        Debug.Log("MathNet-Test" + (float)MathNet.Numerics.Distributions.Normal.CDF(0, 56, -30));
    }

    void Initialize()
    {
        if (RankingGlobal.Status.online)
        {
            logger.text = "Aktivierung prüfen...";
            Debug.Log(Application.persistentDataPath);
            if (!PlayerPrefs.HasKey("key"))
            {
                PlayerPrefs.SetInt("lastOnline", 1);
            }
            if(RankingGlobal.Data.clubs.Count == 0)
            {
                RankingGlobal.Status.onlineWhenLastSaved = true;// PlayerPrefs.GetInt("lastOnline") == 1;
                logger.text = "Vereinsdaten laden...";
                clubs = iohandler.LoadRawClubData(RankingGlobal.Status.online);
                logger.text = "Spielerdaten laden...";
                clubs = iohandler.LoadPlayersToClubs(clubs, RankingGlobal.Status.online);
                RankingGlobal.Data.clubs = clubs;
            }
            clubs = RankingGlobal.Data.clubs;
            RankingGlobal.Constants.CHANGE_THRESHOLD = Mathf.Sqrt(2f) * Statistics.StandardDeviationOfUsedPlayers(clubs);
            logger.text = "Analysedaten laden...";
            Debug.Log("Threshold set to " + RankingGlobal.Constants.CHANGE_THRESHOLD);
            analyticsMatchList = iohandler.LoadAnalytics(RankingGlobal.Status.online);
            errorPlot.BackgroundColor(Color.magenta);
            Debug.Log("Online: " + RankingGlobal.Status.online + ", last: " + RankingGlobal.Status.onlineWhenLastSaved);
            diagnosticsText.text =
                "Gespeicherte Spiele: " + analyticsMatchList.Count + "\n" +
                "Genauigkeit: " + (100f * Statistics.ComputeExactAccuracy(analyticsMatchList)).ToString("0.00", new CultureInfo("en-us", false)) + "%\n" +
                "Top-1 Genauigkeit: " + (100f * Statistics.TopXAccuracy(analyticsMatchList, 1)).ToString("0.00", new CultureInfo("en-us", false)) + "%\n" +
                "Top-2 Genauigkeit: " + (100f * Statistics.TopXAccuracy(analyticsMatchList, 2)).ToString("0.00", new CultureInfo("en-us", false)) + "%\n" +
                "Top-5 Genauigkeit: " + (100f * Statistics.TopXAccuracy(analyticsMatchList, 5)).ToString("0.00", new CultureInfo("en-us", false)) + "%\n" +
                "Top-10 Genauigkeit: " + (100f * Statistics.TopXAccuracy(analyticsMatchList, 10)).ToString("0.00", new CultureInfo("en-us", false)) + "%\n" +
                "Fisher p: " + Statistics.FisherRandomizationTest(analyticsMatchList, 1000 / analyticsMatchList.Count).ToString("0.0000", new CultureInfo("en-us", false)) + "\n" +
                "R2: " + Statistics.RSquared(analyticsMatchList).ToString("0.0000", new CultureInfo("en-us", false)) + "\n" +
                "Mittlere Abweichung: " + Statistics.ComputeError(analyticsMatchList).ToString("0.0000", new CultureInfo("en-us", false)) + "\n" +
                "Veränderungsschwelle: " + RankingGlobal.Constants.CHANGE_THRESHOLD.ToString("0.00", new CultureInfo("en-us", false)) + "\n";

            var meanResults = Statistics.MeanHomeAwayGoals(analyticsMatchList);
            var meanPredictions = Statistics.MeanHomeAwayPredictedGoals(analyticsMatchList);
            //Debug.Log("Home Bias: " + (meanResults.Item1 - meanPredictions.Item1) + ", away bias: " + (meanResults.Item2 - meanPredictions.Item2));
            RankingGlobal.Constants.BIAS_CORRECTION_HOME = meanResults.Item1 - meanPredictions.Item1;
            RankingGlobal.Constants.BIAS_CORRECTION_AWAY = meanResults.Item2 - meanPredictions.Item2;

            float mean = (meanResults.Item1 + meanResults.Item2)/2f;

            RankingGlobal.Constants.SYMMETRIC_HOME_ADVANTAGE = 0.0375f;// meanResults.Item1 / mean - 1f;
            diagnosticsText.text += "Heimvorteil: " + (100f * RankingGlobal.Constants.SYMMETRIC_HOME_ADVANTAGE).ToString("0.00", new CultureInfo("en-us", false)) + "% (" + (100f * (meanResults.Item1 / mean - 1f)).ToString("0.00", new CultureInfo("en-us", false)) + "%)\n";
            (float,float) shapescale = Statistics.GammaMoMShapeScale(analyticsMatchList);
            diagnosticsText.text += "Parameter: k=" + shapescale.Item1.ToString("0.00", new CultureInfo("en-us", false)) + ", theta=" + shapescale.Item2.ToString("0.00", new CultureInfo("en-us", false));
            //RankingGlobal.Constants.GAMMA_SHAPE = shapescale.Item1;
            //RankingGlobal.Constants.GAMMA_RATE = 1f / shapescale.Item2;

            List<float> errorHist = Statistics.MeanAbsoluteErrorChange(analyticsMatchList);
            errorPlot.LineGraph(errorHist, Color.red);
            errorPlot.BackgroundColor(Color.gray);
            errorPlot.Title("Fehlerentwicklung (MAE)");
            errorPlot.HLine(0.85f, Color.green);
            errorPlot.Text(errorHist.Count / 2, 0.9f, "Literaturreferenz", Color.green);
            errorPlot.XLab("Eingetragene Spiele");
            errorPlot.YLab("Fehler (MAE)");

            List<float> accuracies = Statistics.AccuracyChange(analyticsMatchList);
            List<float> topAccuracies = Statistics.Top1AccuracyChange(analyticsMatchList);
            accuracyPlot.LineGraph(accuracies, Color.red);
            accuracyPlot.LineGraph(topAccuracies, Color.black);
            accuracyPlot.BackgroundColor(Color.gray);
            accuracyPlot.HLine(1.05f/6f,Color.green);
            accuracyPlot.Title("Trefferrate");
            accuracyPlot.YLab("Genauigkeit");
            accuracyPlot.XLab("Eingetragene Spiele");

            logger.text = "Bereit";

            clubs[5].OptimizeFormation(new Formation("1-4-1-2-1-2"));
            clubs[1].OptimizeFormation(new Formation("1-4-4-2"));
            Match m = new Match(clubs[5], clubs[1]);
            (float, float) res = m.Predict();
            Debug.Log("Lineup Prediction Test Match: Dortmund " + res.Item1 + " : " + res.Item2 + " Leipzig");
        }
        else
        {
            Invoke("Initialize", 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
       UpdateOnlineIndicator();
    }

    void UpdateOnlineIndicator()
    {
        onlineStatus.color = RankingGlobal.Status.online ? Color.green : Color.red;
    }

    public void OpenPredictionWindow()
    {
        SceneManager.LoadScene("Prediction");
    }

    public void OpenPredictionWindow_NoLineup()
    {
        SceneManager.LoadScene("Prediction_NoLineup");
    }

    public void OpenEditorWindow()
    {
        SceneManager.LoadScene("Console");
    }

    public void OpenSimulationWindow()
    {
        SceneManager.LoadScene("Simulation");
    }

    public void OpenStatisticsWindow()
    {
        SceneManager.LoadScene("Statistics");
    }
}
