using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuFunctions : MonoBehaviour
{
    [SerializeField] GameObject DebugMenuToggleRef;
    // Start is called before the first frame update
    void Start()
    {
        DebugMenuToggleRef.GetComponent<Toggle>().SetIsOnWithoutNotify(SimManager.Instance.DebugModeOn);
    }

    public void ToggleDebugMode()
    {
        // Toggle debug mode state
        SimManager.Instance.DebugModeOn = !SimManager.Instance.DebugModeOn;
    }

    public void SetAllPlayersToLowHealth()
    {
        LevelScopeManagers.Instance.GetComponent<DebugHotkeyManager>().DebugSetPlayerHealthToLow();
    }

    public void SetRoundTimeToLow()
    {
        LevelScopeManagers.Instance.GetComponent<DebugHotkeyManager>().SetRoundTime(3.0f);
    }

    public void ResetRoundTime()
    {
        MatchStateManager matchStateMan = LevelScopeManagers.Instance.GetComponent<MatchStateManager>();
        LevelScopeManagers.Instance.GetComponent<DebugHotkeyManager>().SetRoundTime(matchStateMan.MatchStats.MaxRoundTime);
    }
}
