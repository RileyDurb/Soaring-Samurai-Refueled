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

            if (Input.GetKeyUp(KeyCode.RightBracket))
            {
                SetTimerPaued(!LevelScopeManagers.Instance.GetComponent<MatchStateManager>().TimerPaused);
            }

            if (Input.GetKey(KeyCode.O))
            {
                List<int> playersToCPUToggle = new List<int>();
                if (Input.GetKeyUp(KeyCode.Alpha1))
                {
                    playersToCPUToggle.Add(0);
                }
                else if (Input.GetKeyUp(KeyCode.Alpha2))
                {
                    playersToCPUToggle.Add(1);
                }

                // Toggle AI modes between player Input and CPU controller if they are in either mode currently
                foreach (int playerIndex in playersToCPUToggle)
                {
                    List<PlayerCombatController> players = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList;
                    PlayerCombatController targetPlayer = players.Find((PlayerCombatController player) => { return player.PlayerIndex == playerIndex; });
                    if (targetPlayer == null)
                    {
                        print("DebugHotkeyManager:Update(ToggleCPUHotkey): no player of index " + playerIndex.ToString() + " could be found.");
                        continue;
                    }

                    // Based on current mode, may toggle to another mode, and prints if it does
                    AIBehaviour aiComp = targetPlayer.GetComponent<AIBehaviour>();
                    AIBehaviour.AIMode newAIMode = aiComp.CurrAIMode;
                    if (aiComp.CurrAIMode == AIBehaviour.AIMode.PlayerInput)
                    {
                        newAIMode = AIBehaviour.AIMode.BehaviourTree;
                        aiComp.SetAIMode(newAIMode);
                        print(targetPlayer.name + " was changed to AI Mode: " + newAIMode.ToString());
                    }
                    else if (aiComp.CurrAIMode == AIBehaviour.AIMode.BehaviourTree)
                    {
                        newAIMode = AIBehaviour.AIMode.PlayerInput;
                        aiComp.SetAIMode(newAIMode);
                        print(targetPlayer.name + " was changed to AI Mode: " + newAIMode.ToString());
                    }
                }
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

    public void SetTimerPaued(bool newIsPaused)
    {
        LevelScopeManagers.Instance.GetComponent<MatchStateManager>().TimerPaused = newIsPaused;
    }

    public void ToggleTimerPaused()
    {
        SetTimerPaued(!LevelScopeManagers.Instance.GetComponent<MatchStateManager>().TimerPaused);
    }
}
