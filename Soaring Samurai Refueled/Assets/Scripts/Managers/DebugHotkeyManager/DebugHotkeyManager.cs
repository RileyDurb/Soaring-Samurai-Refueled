using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for holding debug hokeys, to make them easy to find, and for easily disabling them all at once
public class DebugHotkeyManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // DEBUG KEY: Toggle debug mode
        if (Input.GetKeyUp(KeyCode.Slash))
        {
            SimManager.Instance.DebugModeOn = !SimManager.Instance.DebugModeOn;
        }

        if (SimManager.Instance.DebugModeOn)
        {
            // Turn all players to low health debug hotkey
            if (Input.GetKeyUp(KeyCode.Minus))
            {
                DebugSetPlayerHealthToLow();
            }

            // Sets round time to finish soon
            if (Input.GetKeyUp(KeyCode.Alpha0))
            {
                SetRoundTime(3.0f);
            }
        }

    }

    public void DebugSetPlayerHealthToLow()
    {
        MatchStateManager matchManager = LevelScopeManagers.Instance.GetComponent<MatchStateManager>();

        if  (matchManager == null)
        {
            return;
        }

        List<PlayerCombatController> players = matchManager.PlayerList;

        foreach (PlayerCombatController player in players)
        {
            player.GetComponent<PoolContainer>().GetPool("Health").PoolValue = 5.0f;
        }

    }

    public void SetRoundTime(float newRoundTime)
    {
        LevelScopeManagers.Instance.GetComponent<MatchStateManager>().RoundTimeUntrimmed = newRoundTime;
    }
}
