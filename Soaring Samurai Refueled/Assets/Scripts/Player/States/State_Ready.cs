using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class State_Ready : StateManagerPlayer.State
{

    //Action_.EasingTypes IdleMovementOutEas
    public State_Ready() : base(PlayerStates.Ready) { }

    ActionList mReadyActionList = new ActionList();

    PlayerCombatController mCombatControllerRef;
    public override void OnEnter()
    {

        mCombatControllerRef = mParentObject.GetComponent<PlayerCombatController>();

        mCombatControllerRef.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_Idle"); // Play idle animation

        // Slightly randomize loop time for this idle state
        float idleMovementCycleTime = mCombatControllerRef.StateAesthetics.IdleStats.MovementCycleTime + MyRandom.RandomRange(-mCombatControllerRef.StateAesthetics.IdleStats.MaxTimeOffsetRandom, mCombatControllerRef.StateAesthetics.IdleStats.MaxTimeOffsetRandom);
        StartIdleLoop(idleMovementCycleTime);
        mReadyActionList.AddActionCallback(() => { StartIdleLoop(idleMovementCycleTime); }, idleMovementCycleTime, false, true);
    }

    public override void OnUpdate(float dt)
    {
        mReadyActionList.Update(Time.deltaTime);   

        // Face right direction
        if (mCombatControllerRef.OpponentRef.transform.position.x < mParentObject.transform.position.x)
        {
            mCombatControllerRef.SetFacingDirection(PlayerCombatController.FacingDirection.Left);
        }
        else
        {
            mCombatControllerRef.SetFacingDirection(PlayerCombatController.FacingDirection.Right);
        }

        PlayerMovementStats currMovementStats = mCombatControllerRef.mPlayerBaseStats.mMovementStats;

        // Apply movement

        // Find current movement input, starting with base jert to apply
        float currSpeed = mCombatControllerRef.mPlayerBaseStats.mMovementStats.MoveJerk;
        
        // If using a curve to apply different jerk at different amounts of the input diretion
        if (currMovementStats.UseMaxJerkCurve)
        {
            float inputScalar = currMovementStats.InputValueToMaxJerkCurve.Evaluate(mCombatControllerRef.CurrMoveInput.magnitude);
            currSpeed *= inputScalar;// Scale speed based on how much of the max input we're giving
        }

        Vector2 moveVec = mCombatControllerRef.CurrMoveInput * currSpeed;

        mCombatControllerRef.ApplyCappedMovementJerk(moveVec, Time.deltaTime);

    }

    public override void OnExit()
    {
        mReadyActionList.Clear();
        mCombatControllerRef.SpriteObject.transform.localPosition = Vector2.zero; // Resets local position to 0
    }

    void StartIdleLoop(float cycleTime)
    {
        Vector2 currPos = Vector2.zero;

        float movementCycleTime = cycleTime;
        float movementOffset = mCombatControllerRef.StateAesthetics.IdleStats.MovementOffsetAmount + MyRandom.RandomRange(-mCombatControllerRef.StateAesthetics.IdleStats.MaxDistanceOffsetRandom, mCombatControllerRef.StateAesthetics.IdleStats.MaxDistanceOffsetRandom);

        float currDelay = 0.0f;
        mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos + Vector2.down * movementOffset, movementCycleTime / 4, 0.0f, mCombatControllerRef.StateAesthetics.IdleStats.FirstOutMoveEasing);
        currDelay += movementCycleTime / 4;
        mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos, movementCycleTime / 4, currDelay, mCombatControllerRef.StateAesthetics.IdleStats.FirstInMoveEasing);
        currDelay += movementCycleTime / 4;
        mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos + Vector2.up * movementOffset, movementCycleTime / 4, currDelay, mCombatControllerRef.StateAesthetics.IdleStats.SecondOutMoveEasing);
        currDelay += movementCycleTime / 4;
        mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos, movementCycleTime / 4, currDelay, mCombatControllerRef.StateAesthetics.IdleStats.SecondInMoveEasing);

        //// Old way of doimg it that had 3 actions
        //float currDelay = 0.0f;
        //mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos + Vector2.down * movementOffset, movementCycleTime / 4, 0.0f, mCombatControllerRef.StateAesthetics.IdleStats.FirstQuarterEasing);
        //currDelay += movementCycleTime / 4;
        //mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos + Vector2.up * movementOffset, movementCycleTime / 2, currDelay, mCombatControllerRef.StateAesthetics.IdleStats.MiddleHalfEasing);
        //currDelay += movementCycleTime / 2;
        //mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos, movementCycleTime / 4, currDelay, mCombatControllerRef.StateAesthetics.IdleStats.LastQuarterEasing);

    }
}
