using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchEndMenuFeatures : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI MatchWinPlayerTitle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetWinnerNameMessage(string winMessage)
    {
        MatchWinPlayerTitle.SetText(winMessage);
    }

    public void StartNewMatch()
    {
        LevelScopeManagers.Instance.GetComponent<MatchStateManager>().RestartMatch();

        LevelScopeManagers.Instance.GetComponent<MenuManager>().PopMenu();
    }
}
