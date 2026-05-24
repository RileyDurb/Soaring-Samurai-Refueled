using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerCombatController;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class State_DashAttack : StateManagerPlayer.State
{
    public State_DashAttack() : base(PlayerStates.DashAttack) { }

    enum DashAttackStates
    {
        Charge,
        Ready,
        Active,
        Recovery
    }
    // Getters and setters
    public bool DashAttackInputReleased
    {
        set { mDashAttackInputReleased = value; }
    }

    // Private variables
    DashAttackStates mCurrDashAttackState = DashAttackStates.Charge;


    bool mDashAttackInputReleased = false;

    ActionList mDashAttackActionList = new ActionList();

    // References
    [SerializeField] DashAttackDataObject mDashAttackStats;
    PlayerCombatController mCombatController;

    LayerMask mOriginalCollisionExcludeLayers;

    bool mCancelledThisState = false;



    // Start is called before the first frame update
    public override void OnEnter()
    {
        // Initialize variables
        mCombatController = mParentObject.GetComponent<PlayerCombatController>();
        mDashAttackStats = mCombatController.mDashAttackStats; // bring in dash attack stats
        mCurrDashAttackState = DashAttackStates.Charge;
        mDashAttackInputReleased = false;


        // Start charging animation
        mCombatController.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_DashAttackCharge");
        mDashAttackActionList.AddActionCallback(() => mCurrDashAttackState = DashAttackStates.Ready, mDashAttackStats.ChargeTime); // Set timer for charge to be ready
        
        // Set to exclude layers we've defined
        Rigidbody2D physics = mCombatController.GetComponent<Rigidbody2D>();
        mOriginalCollisionExcludeLayers = physics.excludeLayers;
        physics.excludeLayers = mDashAttackStats.ExcludeLayersForPlayerCollision.value;

        mCancelledThisState = false;
    }

    // Update is called once per frame
    public override void OnUpdate(float dt)
    {
        mDashAttackActionList.Update(dt);

        //  Face opponent
        if (mCombatController.OpponentRef.transform.position.x < mParentObject.transform.position.x)
        {
            mCombatController.SetFacingDirection(FacingDirection.Left);
        }
        else
        {
            mCombatController.SetFacingDirection(FacingDirection.Right);
        }


        // Calculate triggering move after ready and releasing the inputt
        if (mCurrDashAttackState == DashAttackStates.Ready && mDashAttackInputReleased == true)
        {
            mCurrDashAttackState = DashAttackStates.Active;

            mCombatController.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_DashAttackActive"); // Play animation

            // Spawns attack hitbox right around the player
            mParentObject.GetComponent<PlayerCombatController>().SpawnDirectionalAttack(new Vector2(0, 0), mDashAttackStats.mStats);

            // Set to go into recovery after active time is done
            mDashAttackActionList.AddActionCallback(() => StartDashAttackRecovery(), mDashAttackStats.mStats.ActiveTime);
        }

        // Calculate current speed
        float currSpeed = mDashAttackStats.DashingJerk;

        PhysicsApplier physics = mParentObject.GetComponent<PhysicsApplier>();

        // Modify ability to move based on state of the attack

        // If not attackng, can't move
        if (mCurrDashAttackState == DashAttackStates.Charge || mCurrDashAttackState == DashAttackStates.Ready || mCurrDashAttackState == DashAttackStates.Recovery)
        {
            currSpeed = 0.0f;

            physics.mUncappedDirectionalForces.ClearAllForces();
            physics.mDirectionalForces.ClearAllForces();

            if (mCurrDashAttackState == DashAttackStates.Recovery)
            {
                mCombatController.CurrMoveInput = Vector2.zero; // Set move input to 0 so we don't have lingering input that keeps the player moving after recovery ends, if the player isn't holding a direction when recovery ends
            }
        }
        else if (mCurrDashAttackState == DashAttackStates.Active) // If attacking, force movement
        {
            if (mCombatController.CurrMoveInput == Vector2.zero) // if not inputting a direction
            {
                if (mCombatController.LastDirectionalMoveInput == Vector2.zero) // if last move input isn't anything
                {
                    // Use facing direction for movement
                    if (mCombatController.transform.localScale.x > 0) // if facing right
                    {
                        mCombatController.CurrMoveInput = Vector2.right; // Go right
                    }
                    else // If facing left
                    {
                        mCombatController.CurrMoveInput = Vector2.left; // go left
                    }
                }
                else // Last directional move input is valid
                {
                    mCombatController.CurrMoveInput = mCombatController.LastDirectionalMoveInput.normalized; // use last directional input
                }
            }
        }


        // Apply movement
        Vector2 moveVec = mCombatController.CurrMoveInput * currSpeed;


        StateManagerPlayer stateManager = mParentObject.GetComponent<StateManagerPlayer>();

        if (stateManager.CurrStateName == PlayerStates.Dash || stateManager.CurrStateName == PlayerStates.DashAttack)
        {
            // Applies jerk
            mCombatController.ApplyUncappedMovementJerk(moveVec, Time.deltaTime);
        }
    }

    public override void OnExit()
    {
        // Only clear if we're not in recovery, as in recovery, the attack states will end normally. Also, clearing the action list while in the update of an action, which can be caused when the endrecovery function changes the state, can cause issues.
        if (mCurrDashAttackState != DashAttackStates.Recovery)
        {
            mDashAttackActionList.Clear(); // Clears action list, cause we're cancelling early, so we want to clear anything that's qeued to happen
        }

        // Set to exclude layers back to original
        Rigidbody2D physics = mCombatController.GetComponent<Rigidbody2D>();
        physics.excludeLayers = mOriginalCollisionExcludeLayers.value;
    }

    void StartDashAttackRecovery()
    {
        mCurrDashAttackState = DashAttackStates.Recovery;

        mCombatController.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_DashAttackRecoverySheathed");

        mDashAttackActionList.AddActionCallback(() => { EndDashAttackRecovery(); }, mDashAttackStats.RecoveryTime);
    }

    void EndDashAttackRecovery()
    {
        StateManagerPlayer stateManager = mParentObject.GetComponent<StateManagerPlayer>();
        if (stateManager.CanEnterState(PlayerStates.Ready))
        {
            stateManager.EnterState(PlayerStates.Ready);
        }

        mCancelledThisState = true;

    }
}
