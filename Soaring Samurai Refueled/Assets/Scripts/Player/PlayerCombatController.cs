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
    public Hitbox.AttackData TestAttackData = new Hitbox.AttackData();
    float TestAttackActiveDuration = 1.0f;

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

    public void OnTestAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            GameObject newHitbox = Instantiate(SimManager.Instance.GetPrefab("BaseHitbox"), transform);
            newHitbox.transform.localScale = new Vector3(transform.lossyScale.x, transform.lossyScale.y, newHitbox.transform.lossyScale.z); // Sets scale equal to the player's

            newHitbox.GetComponent<Hitbox>().InitAttack(TestAttackData, TestAttackActiveDuration);
        }

    }

    // Combat related functions
    public void TakeDamage(Hitbox.AttackData attackData)
    {
        Pool pool = GetComponent<Pool>();
        pool.DecreasePool(attackData.damage);
    }
}
