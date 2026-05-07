using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerListMenu : MonoBehaviour
{
    // private variables

    // References
    [SerializeField] VerticalLayoutGroup PlayerListsRef;



    int mLastPlayerCount = 0;
    bool mPlayerInfoDirty = false;

    // Start is called before the first frame update
    void Start()
    {
        UpdatePlayerInputPanels();
    }

    private void Update()
    {
        if (PlayerInputManager.instance.playerCount != mLastPlayerCount || mPlayerInfoDirty)
        {
            UpdatePlayerInputPanels();
        }
    }

    // Helper functions
    void UpdatePlayerInputPanels()
    {
        List<PlayerCombatController> players = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList;

        PlayerInputManager inputMan = PlayerInputManager.instance;

        PlayerInputPanel[] playerPanels = PlayerListsRef.GetComponentsInChildren<PlayerInputPanel>();

        for (int i = 0; i < inputMan.playerCount; i++)
        {
            // tries to get the player for the current player index we're on
            PlayerCombatController currentNumberedPlayer = players.Find((PlayerCombatController controller) => { return controller.PlayerIndex == i; });

            if (currentNumberedPlayer == null)
            {
                // Print error message
                print("PlayerListMenu:Start: Player " + i + " could not be found, but should exist with a player count of " + inputMan.playerCount);
                continue; // Skip this player
            }


            // Ensures we have enough player panels (only need more if we're allowing more than 2 players, and we'll need to update the UI setup if so, and maybe add dynamically resizing the number of panels)
            if (playerPanels.Length <= i)
            {
                print("PlayerListMenu:Start: More players are needing a panel than panels that exist for the. " + playerPanels.Length + " panels exist, but needs a panel for player index " + i);
                continue;
            }

            PlayerInputPanel currPanel = playerPanels[i];
            currPanel.SetPlayerIndexNumber(i);
            string inputTypeName = InputTypeData.GetDeviceInputType(PlayerInput.all[i].devices[0]);
            currPanel.SetInputTypeName(inputTypeName);
        }


        // Set rest of panels that don't have players connected for them to be blank

        for (int i = inputMan.playerCount; i < inputMan.maxPlayerCount; i++)
        {
            // Ensures we have enough player panels (only need more if we're allowing more than 2 players, and we'll need to update the UI setup if so, and maybe add dynamically resizing the number of panels)
            if (playerPanels.Length <= i)
            {
                print("PlayerListMenu:Start: More players are needing a panel than panels that exist for the. " + playerPanels.Length + " panels exist, but needs a panel for player index " + i);
                continue;
            }

            // Set panel to be blank
            PlayerInputPanel currPanel = playerPanels[i];
            currPanel.SetPlayerIndexNumber(-1);
            currPanel.SetInputTypeName("");
        }


        // Save current info for knowing when to update again
        mLastPlayerCount = inputMan.playerCount;
    }


}
