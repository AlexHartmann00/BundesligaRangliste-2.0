using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ConfirmationGUIManager : MonoBehaviour
{
    public GameObject saveDialog, keyPrompt;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI predictionText;
    public TextMeshProUGUI topResultsText;
    public Image homeLogo, awayLogo;
    public Main mainReference;
    public IOHandler iohandler;
    float xG1, xG2;
    int goals1, goals2;
    Match match;
    // Start is called before the first frame update
    void Awake()
    {
        iohandler = GameObject.Find("Canvas").GetComponent<IOHandler>();
        match = mainReference.currentMatch;
        homeLogo.sprite = match.GetHomeClub().GetLogo();
        awayLogo.sprite = match.GetAwayClub().GetLogo();
        (xG1, xG2) = match.prediction;
        predictionText.text = xG1.ToString("0.0000") + " : " + xG2.ToString("0.0000");
        goals1 = Mathf.RoundToInt(xG1);
        goals2 = Mathf.RoundToInt(xG2);
        resultText.text = Mathf.Round(xG1) + " : " + Mathf.Round(xG2);
        topResultsText.text = match.GetTopResultsString(4);
    }

    public void IncrementGoals(int club)
    {
        
        if(club == 0)
        {
            goals1 ++;
        }
        else
        {
            goals2 ++;
        }
        RedrawResult();
    }

    public void DecrementGoals(int club)
    {
        if (club == 0)
        {
            goals1--;
        }
        else
        {
            goals2--;
        }
        RedrawResult();
    }

    public void RedrawResult()
    {
        resultText.text = goals1 + " : " + goals2;
    }

    public void Commit()
    {
        saveDialog.SetActive(true);
    }

    public void CommitWithSave()
    {
        keyPrompt.SetActive(true);
    }

    public void CommitWithSaveAndCorrectKey()
    {
        match.PropagateResult((goals1, goals2));
        iohandler.WriteAnalytics(match, RankingGlobal.Status.online);
        iohandler.WriteHistory(match, RankingGlobal.Status.online);
        mainReference.Write();
        SceneManager.LoadScene("MainMenu");

    }

    public void CommitWithoutSave()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
