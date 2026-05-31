using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class State_Ready : StateManagerPlayer.State
{

    //Action_.EasingTypes IdleMovementOutEas
    public State_Ready() : base(PlayerStates.Ready) { }

    ActionList mReadyActionList = new ActionList();

    PlayerCombatController mCombatControllerRef;
    InputBuffer mInputBuffer;
    public override void OnEnter()
    {

        mCombatControllerRef = mParentObject.GetComponent<PlayerCombatController>();

        mInputBuffer = mParentObject.GetComponent<InputBuffer>();

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

        // Set current max speed

        // If all previous move inputs were closer together than a certain threshold, allow dashing
        bool alwaysSprint = mInputBuffer.IsFlickingStick(InputBuffer.BufferTrackedInputs.Move, mCombatControllerRef.CurrMoveInput);

        PhysicsApplier physics = mCombatControllerRef.GetComponent<PhysicsApplier>();

        if (alwaysSprint)
        {
            physics.mDirectionalForces.Stats = mCombatControllerRef.mPlayerBaseStats.mMovementStats.FullInputMovementStats; // Use full max speed
        }
        else // Decide if we're sprinting or not
        {
            if (mCombatControllerRef.CurrMoveInput.magnitude >= mCombatControllerRef.mPlayerBaseStats.mMovementStats.PartialInputMovementStatsThreshold && mCombatControllerRef.IsSprintOn) // If at the threshold for full movement input
            {
                physics.mDirectionalForces.Stats = mCombatControllerRef.mPlayerBaseStats.mMovementStats.FullInputMovementStats; // Use full max speed
            }
            else
            {
                physics.mDirectionalForces.Stats = mCombatControllerRef.mPlayerBaseStats.mMovementStats.PartialInputMovementStats; // Use full max speed
            }
        }


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

        // rotate in movement direction, or back to straight up when not moving
        Vector2 rotationTargetDirection = new Vector2(moveVec.x, Mathf.Abs(moveVec.y));
        float targetAngleFromUp = Vector2.SignedAngle(Vector2.up, rotationTargetDirection.normalized);

        targetAngleFromUp = (targetAngleFromUp / 45.0f) * mCombatControllerRef.StateAesthetics.IdleStats.MaxMoveRotationAngle;

        mCombatControllerRef.SpriteObject.transform.rotation = Quaternion.Lerp(mCombatControllerRef.SpriteObject.transform.rotation, Quaternion.AngleAxis(targetAngleFromUp, Vector3.forward), mCombatControllerRef.StateAesthetics.IdleStats.MoveRotationSpeed * Time.deltaTime);
        
    }

    public override void OnExit()
    {
        mReadyActionList.Clear();
        mCombatControllerRef.SpriteObject.transform.localPosition = Vector2.zero; // Resets local position to 0
        mCombatControllerRef.SpriteObject.transform.rotation = Quaternion.identity; // Return rotation to 0

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
