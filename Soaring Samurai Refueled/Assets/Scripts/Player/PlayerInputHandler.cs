using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerCombatController playerController;
    PlayerInput input;
    [SerializeField]
    InputAction moveInputAction;

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<PlayerInput>(); // Saves a reference to the input component

        // Looks for an unassigned player object
        PlayerCombatController[] controllerList = FindObjectsOfType<PlayerCombatController>();
        foreach (PlayerCombatController controller in controllerList)
        {
            if (controller.PlayerIndex == input.playerIndex) // If unassigned player found
            {

                playerController = controller; // Saves player object for controlling
                controller.SetPlayerIndex(input.playerIndex, true); // Gives the player this input handler's index, and marks the player as now being player controlled

                // Update character visual based elements of the mobile controller for this character, if any
                MobileInputManager mobileControlsMan = LevelScopeManagers.Instance.GetComponent<MobileInputManager>();
                if (mobileControlsMan.mSpawnedControlsPerPlayer.ContainsKey(controller.PlayerIndex))
                {
                    MobileControlsTheme mobileControlsVisuals = mobileControlsMan.mSpawnedControlsPerPlayer[controller.PlayerIndex].GetComponent<MobileControlsTheme>();
                    controller.OnCharacterChanged += mobileControlsVisuals.UpdateCharacterBasedVisuals;

                    mobileControlsVisuals.UpdateCharacterBasedVisuals(controller.CharacterVisualsName);
                }

                break;
            }
        }

        // TODO: add a case for no player with a matching index existing, and if that's the case, spawn a new player (need to make player spawning, and make that spawn UI elements for the new player)


        //GameObject joystick = GameObject.Find("TestMobileJoystick");
        //OnScreenStick joystickComp = joystick.GetComponent<OnScreenStick>();

        //GameObject northButton = GameObject.Find("UpRightAttackButton");
        //OnScreenButton buttonCom = northButton.GetComponent<OnScreenButton>();

        //InputUser.PerformPairingWithDevice(joystickComp.control.device, input.user);
        //InputUser.PerformPairingWithDevice(buttonCom.control.device, input.user);

        //PlayerInputManager inputManager = LevelScopeManagers.Instance.GetComponent<PlayerInputManager>();
        //inputManager.JoinPlayer(1, -1, "GamePad", joystickComp.control.device);

        //spawnedMobileInput = true;

    }

    // Update is called once per frame
    void Update()
    {
    }

    // Input actions //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Movement
    public void OnMove(InputAction.CallbackContext context)
    {
        if (playerController == null)
        {
            // NOTE: Would want to print the below error message, but since the object is not initialized when this error case occurs, using a print statement triggers an exception
            //print("PlayerInputHandler:OnMove: Input handler of index " +  input.playerIndex + " tried to move it's player, but player was null");
            return;
        }
        playerController.OnMove(context);
    }

    // Attacks
    public void OnDownLeftAttack(InputAction.CallbackContext context)
    {
        if (playerController == null)
        {
            return;
        }
        playerController.OnDownLeftAttack(context);
    }

    public void OnUpLeftAttack(InputAction.CallbackContext context)
    {
        if (playerController == null)
        {
            return;
        }
        playerController.OnUpLeftAttack(context);
    }

    public void OnDownRightAttack(InputAction.CallbackContext context)
    {
        if (playerController == null)
        {
            return;
        }
        playerController.OnDownRightAttack(context);
    }

    public void OnUpRightAttack(InputAction.CallbackContext context)
    {
        if (playerController == null)
        {
            return;
        }
        playerController.OnUpRightAttack(context);
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (playerController == null)
        {
            return;
        }
        playerController.OnDash(context);
    }

    public void OnDashAttack(InputAction.CallbackContext context)
    {
        if (playerController == null)
        {
            return;
        }
        playerController.OnDashAttack(context);
    }

    public void OnPauseTriggered(InputAction.CallbackContext context)
    {
        playerController.OnPauseTriggered(context);
    }
}
