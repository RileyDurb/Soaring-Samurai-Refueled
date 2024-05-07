using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{
    // Editor Accessible variables
    public float MoveJerk = 5.0f;



    // Private variables
    Vector2 moveInput;

    [SerializeField]
    private int playerIndex = -1; // Index of player, inits to less than 0 to represent no player assigned
    public int PlayerIndex
    { 
        get { return playerIndex; }
        set { playerIndex = value; }
    }

    // Update is called once per frame
    void Update()
    {

        // Apply movement from current input value
        Vector2 moveVec = moveInput * MoveJerk;
        PhysicsApplier physics = GetComponent<PhysicsApplier>();

        // Applies jerk
        physics.mDirectionalForces.ApplyJerk(moveVec);

        // Since things like dampening can be applied differently based in if input is being given, tell the physics the current state
        if (moveInput == Vector2.zero)
        {
            physics.mDirectionalForces.InputBeingApplied = false;
        }
        else
        {
            physics.mDirectionalForces.InputBeingApplied = true;

        }
    }



    // Action functions
    void Grow()
    {
        transform.localScale *= 1.6f;
    }

    //private void OnEnable()
    //{ 
    //    //controls.Combat.Enable();
    //    //// Subscribe grow function
    //    //controls.Combat.Grow.performed += context => Grow();

    //    //// Subscribe movement callbacks
    //    //controls.Combat.Move.performed += context => moveInput = context.ReadValue<Vector2>();
    //    //controls.Combat.Move.canceled += context => moveInput = Vector2.zero;

    //}

    //private void OnDisable()
    //{
    //    controls.Combat.Disable();
    //}


    public void OnMove(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                {
                    moveInput = context.ReadValue<Vector2>();
                }
                break;

            case InputActionPhase.Canceled:
                {
                    moveInput = Vector2.zero;
                }
                break;

            default:
                break;
        }
    }
}
