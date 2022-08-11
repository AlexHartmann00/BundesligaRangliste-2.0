using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeyPrompt : MonoBehaviour
{
    public TextMeshProUGUI keyInput;
    public GameObject keyPromptObject, errorText;
    public ConfirmationGUIManager GUIManager;

    string errorMessage = "Ungültiger Schlüssel. Erneut versuchen";

    public void Confirm()
    {
        RankingGlobal.Verification.key = keyInput.text.Remove(keyInput.text.Length - 1);
        if (RankingGlobal.Verification.VerifyKey())
        {
            PlayerPrefs.SetString("key", keyInput.text);
            GUIManager.CommitWithSaveAndCorrectKey();
            keyPromptObject.SetActive(false);
        }
        else
        {
            errorText.SetActive(true);
            errorText.GetComponent<TextMeshProUGUI>().text = errorMessage + ", " + RankingGlobal.Verification.key + ", " + RankingGlobal.Verification.VerifyKey();
            Invoke("RemoveErrorText", 1.5f);
            keyInput.text = "";
        }
    }

    void RemoveErrorText()
    {
        errorText.SetActive(false);
    }

    void Close()
    {
        gameObject.SetActive(false);
    }
}
