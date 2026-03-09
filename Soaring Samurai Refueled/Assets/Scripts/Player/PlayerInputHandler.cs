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
            if (controller.PlayerIndex < 0) // If unassigned player found
            {

                playerController = controller; // Saves player object for controlling
                controller.PlayerIndex = input.playerIndex; // Gives the player this input handler's index
                break;
            }
        }


        GameObject joystick = GameObject.Find("TestMobileJoystick");
        OnScreenStick joystickComp = joystick.GetComponent<OnScreenStick>();

        GameObject northButton = GameObject.Find("UpRightAttackButton");
        OnScreenButton buttonCom = northButton.GetComponent<OnScreenButton>();

        InputUser.PerformPairingWithDevice(joystickComp.control.device, input.user);
        //InputUser.PerformPairingWithDevice(buttonCom.control.device, input.user);

        //PlayerInputManager inputManager = GameObject.Find("PlayerManager").GetComponent<PlayerInputManager>();
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
}
