using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{
    // Class and other Definitions

    enum CombatStates
    {
        Ready,
        SlashAttack
    }

    // Editor Accessible variables
    public float MoveJerk = 5.0f;
    [SerializeField] Hitbox.AttackDefinition DirectionalSlashAttackStats = new Hitbox.AttackDefinition();
    [SerializeField] float AttackOffsetDistance = .7f;
    // Private variables
    Vector2 mMoveInput;
    ActionList mActionList = new ActionList();
    CombatStates mCurrCombatState = CombatStates.Ready;
    float mCombatStateTransitionTimer = -1.0f; // Initializes to a negative value, which means timer is off
    CombatStates mNextCombatState = CombatStates.Ready;


    [SerializeField]
    private int playerIndex = -1; // Index of player, inits to less than 0 to represent no player assigned


    // Update is called once per frame
    void Update()
    {
        mActionList.Update(Time.deltaTime);
        
        // Update State transition timer
        if (mCombatStateTransitionTimer >= 0.0f) // If timer is on
        {
            mCombatStateTransitionTimer -= Time.deltaTime;

            if (mCombatStateTransitionTimer <= 0.0f) // If timer is done
            {
                mCurrCombatState = mNextCombatState;
                mCombatStateTransitionTimer = -1.0f; // Turns timer off
            }
        }

        // Apply movement from current input value
        Vector2 moveVec = mMoveInput * MoveJerk;
        PhysicsApplier physics = GetComponent<PhysicsApplier>();

        // Applies jerk
        physics.mDirectionalForces.ApplyJerk(moveVec);

        // Since things like dampening can be applied differently based in if input is being given, tell the physics the current state
        if (mMoveInput == Vector2.zero)
        {
            physics.mDirectionalForces.InputBeingApplied = false;
        }
        else
        {
            physics.mDirectionalForces.InputBeingApplied = true;

        }
    }

    // Getters and setters
    public int PlayerIndex
    {
        get { return playerIndex; }
        set { playerIndex = value; }
    }



    // Action functions
    public void OnMove(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                {
                    mMoveInput = context.ReadValue<Vector2>();
                }
                break;

            case InputActionPhase.Canceled:
                {
                    mMoveInput = Vector2.zero;
                }
                break;

            default:
                break;
        }
    }

    //public void OnTestAttack(InputAction.CallbackContext context)
    //{
    //    if (context.phase == InputActionPhase.Started)
    //    {
    //        GameObject newHitbox = Instantiate(SimManager.Instance.GetPrefab("BaseHitbox"), transform);
    //        newHitbox.transform.localScale = new Vector3(transform.lossyScale.x, transform.lossyScale.y, newHitbox.transform.lossyScale.z); // Sets scale equal to the player's

    //        newHitbox.GetComponent<Hitbox>().InitAttack(TestAttackInfo);
    //    }

    //}

    public void OnDownLeftAttack(InputAction.CallbackContext context)
    {
        if (mCurrCombatState != CombatStates.Ready)
        {
            return;
        }
        if (context.phase == InputActionPhase.Started)
        {
            SpawnDirectionalAttack(new Vector2(-1, -1) * AttackOffsetDistance, DirectionalSlashAttackStats);
            // Turns to slash state
            mCurrCombatState = CombatStates.SlashAttack;

            // Sets up timer for when player can attack again
            mCombatStateTransitionTimer = DirectionalSlashAttackStats.ActiveTime;
            mNextCombatState = CombatStates.Ready;
        }
    }

    public void OnUpLeftAttack(InputAction.CallbackContext context)
    {
        if (mCurrCombatState != CombatStates.Ready)
        {
            return;
        }
        if (context.phase == InputActionPhase.Started)
        {
            SpawnDirectionalAttack(new Vector2(-1, 1) * AttackOffsetDistance, DirectionalSlashAttackStats);
            // Turns to slash state
            mCurrCombatState = CombatStates.SlashAttack;

            // Sets up timer for when player can attack again
            mCombatStateTransitionTimer = DirectionalSlashAttackStats.ActiveTime;
            mNextCombatState = CombatStates.Ready;
        }
    }


    public void OnDownRightAttack(InputAction.CallbackContext context)
    {
        if (mCurrCombatState != CombatStates.Ready)
        {
            return;
        }
        if (context.phase == InputActionPhase.Started)
        {
            SpawnDirectionalAttack(new Vector2(1, -1) * AttackOffsetDistance, DirectionalSlashAttackStats);
            // Turns to slash state
            mCurrCombatState = CombatStates.SlashAttack;

            // Sets up timer for when player can attack again
            mCombatStateTransitionTimer = DirectionalSlashAttackStats.ActiveTime;
            mNextCombatState = CombatStates.Ready;
        }
    }

    public void OnUpRightAttack(InputAction.CallbackContext context)
    {
        if (mCurrCombatState != CombatStates.Ready)
        {
            return;
        }
        if (context.phase == InputActionPhase.Started)
        {
            SpawnDirectionalAttack(new Vector2(1, 1) * AttackOffsetDistance, DirectionalSlashAttackStats);
            // Turns to slash state
            mCurrCombatState = CombatStates.SlashAttack;

            // Sets up timer for when player can attack again
            mCombatStateTransitionTimer = DirectionalSlashAttackStats.ActiveTime;
            mNextCombatState = CombatStates.Ready;
        }
    }


    // Combat related functions
    public void TakeDamage(Hitbox.AttackData attackData)
    {
        GetComponent<PoolContainer>().GetPool("Health").DecreasePool(attackData.Damage);

        if (SimManager.Instance.DebugMode)
        {
            Debug.DrawRay(transform.position, attackData.Knockback, Color.yellow, .5f, false);
        }
        mActionList.AddActionEqualizedKnockback(gameObject, attackData.Knockback, attackData.KnockbackEqualizationPercent, attackData.KnockbackDuration);
    }


    // Helper functions
    void SpawnDirectionalAttack(Vector2 offsetFromPlayer, Hitbox.AttackDefinition attackInfo)
    {
        GameObject newHitbox = Instantiate(SimManager.Instance.GetPrefab("BaseHitbox"), transform);
        newHitbox.transform.localScale = new Vector3(transform.lossyScale.x, transform.lossyScale.y, newHitbox.transform.lossyScale.z); // Sets scale equal to the player's
        newHitbox.transform.localPosition = new Vector3(offsetFromPlayer.x, offsetFromPlayer.y, newHitbox.transform.localPosition.z); // Sets position to the given offset
        newHitbox.GetComponent<Hitbox>().InitAttack(attackInfo);
    }
}
