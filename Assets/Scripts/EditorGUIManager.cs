using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EditorGUIManager : MonoBehaviour
{
    const int CLUB_OFFSET = 40;
    const string CLUB_PLACEHOLDER = "Auswählen...";
    public GameObject textPrefab;
    public GameObject buttonParent;
    public GameObject clubWindow;
    Club selectedClub;

    // Start is called before the first frame update
    void Start()
    {
        DisplayClubs(FileManager.ReadFromFile(RankingGlobal.Paths.CLUB_DATA_PATH));
    }

    public void DisplayClubs(List<Club> clubs)
    {
        float offset = 0;
        foreach (Club club in clubs)
        {
            GameObject txt = Instantiate(textPrefab);
            //txt.GetComponent<Image>().color = club.GetColor();
            Button btn = txt.GetComponent<Button>();
            btn.onClick.AddListener(delegate { SelectClub(club); });
            TextMeshProUGUI text = txt.GetComponentInChildren<TextMeshProUGUI>();
            txt.transform.Find("Logo").GetComponent<Image>().sprite = club.GetLogo();
            txt.transform.SetParent(clubWindow.transform, false);
            txt.transform.position = txt.transform.position + new Vector3(0, -offset, 0);
            text.text = club.GetName();
            offset += CLUB_OFFSET;
        }
    }

    void SelectClub(Club club)
    {
        selectedClub = club;
    }
}
