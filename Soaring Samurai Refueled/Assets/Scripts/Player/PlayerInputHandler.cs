using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerCombatController playerController;
    PlayerInput input;

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
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (playerController == null)
        {
            print("PlayerInputHandler:OnMove: Input handler of index " +  input.playerIndex + " tried to move it's player, but player was null");
            return;
        }
        playerController.OnMove(context);
    }

    public void OnTestAttack(InputAction.CallbackContext context)
    {
        if (playerController == null)
        {
            print("PlayerInputHandler:OnTestAttack: Input handler of index " + input.playerIndex + " tried to do a test attack with it's player, but player was null");
            return;
        }
        playerController.OnTestAttack(context);
    }
}
