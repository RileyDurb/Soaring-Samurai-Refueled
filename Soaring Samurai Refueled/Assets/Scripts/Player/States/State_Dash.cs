using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Dash : StateManagerPlayer.State
{
    public State_Dash() : base(PlayerStates.Dash) { }


    // Private variables
    ActionList mDashActionList = new ActionList();


    // References
    PlayerCombatController mCombatController;
    InputBuffer mInputBuffer;

    bool mMoveInputGiven = false;
    Vector2 mOldMovementDirectionToUse;

    public override void OnEnter()
    {
        // Intialize variables
        mCombatController = mParentObject.GetComponent<PlayerCombatController>();
        mInputBuffer = mParentObject.GetComponent<InputBuffer>();

        // Do squash and stretch
        float timeElapsed = 0.0f;

        Vector2 ogScale = mCombatController.OGScale;
        // Scale down to min
        float currDashStageTime = mCombatController.mPlayerBaseStats.mMovementStats.DashDuration / 4;
        mDashActionList.AddActionScale(mParentObject, new Vector2(ogScale.x * 1.2f, ogScale.y * mCombatController.mActionAesthetics.DashStretchMin), currDashStageTime, 0.0f, Action_.EasingTypes.EaseInSmall);
        timeElapsed += currDashStageTime;

        // up to max
        currDashStageTime = mCombatController.mPlayerBaseStats.mMovementStats.DashDuration / 2;
        mDashActionList.AddActionScale(mParentObject, new Vector2(ogScale.x * mCombatController.mActionAesthetics.DashStretchMin, ogScale.y * mCombatController.mActionAesthetics.DashStretchMax), currDashStageTime, timeElapsed, Action_.EasingTypes.EaseInBounce);
        timeElapsed += currDashStageTime;

        // Scale back to normal
        currDashStageTime = mCombatController.mPlayerBaseStats.mMovementStats.DashDuration / 4;
        mDashActionList.AddActionScale(mParentObject, new Vector2(ogScale.x, ogScale.y), currDashStageTime, timeElapsed, Action_.EasingTypes.EaseOutMedium);

        mMoveInputGiven = false;

        // Gets last very recent movement input
        mOldMovementDirectionToUse = mInputBuffer.GetLastInputVector(InputBuffer.BufferTrackedInputs.Move, mCombatController.mPlayerBaseStats.mMovementStats.DashOldInputRecencyLimit);

    }

    public override void OnUpdate(float dt)
    {
        // Check if move input has been given
        //mCombatController.GetComponent<InputBuffer>()
        Vector2 moveInputToUse = mCombatController.CurrMoveInput;

        // If no move input given yet, use last input direction or a default direction
        if (mMoveInputGiven == false && moveInputToUse.magnitude == 0)
        {
            // Gets last very recent movement input
            moveInputToUse = mOldMovementDirectionToUse;

            if (moveInputToUse.magnitude == 0) // If no recent input within the threshold
            {
                // Default to moving toward the opponent
                Vector2 vecTowardOpponent = mCombatController.OpponentRef.transform.position - mParentObject.transform.position;

                moveInputToUse = vecTowardOpponent.normalized;

            }
        }
        else
        {
            mMoveInputGiven = true;
        }

        // Calculate speed and direction
        float currSpeed = mCombatController.mPlayerBaseStats.mMovementStats.DashingJerk;

        Vector2 moveVec = moveInputToUse * currSpeed;

        // Applies jerk
        mCombatController.ApplyUncappedMovementJerk(moveVec, Time.deltaTime);

        // rotate in movement direction, or back to straight up when not moving
        Vector2 rotationTargetDirection = new Vector2(moveVec.x, Mathf.Abs(moveVec.y));
        float targetAngleFromUp = Vector2.SignedAngle(Vector2.up, rotationTargetDirection.normalized);

        targetAngleFromUp = Mathf.Clamp(targetAngleFromUp, -mCombatController.StateAesthetics.DashAestheticStats.MaxMoveRotationAngle, mCombatController.StateAesthetics.DashAestheticStats.MaxMoveRotationAngle);

        mCombatController.SpriteObject.transform.rotation = Quaternion.Lerp(mCombatController.SpriteObject.transform.rotation, Quaternion.AngleAxis(targetAngleFromUp, Vector3.forward), mCombatController.StateAesthetics.DashAestheticStats.MoveRotationSpeed * Time.deltaTime);

    }
    public override void OnExit()
    {
        mDashActionList.Clear();
        mCombatController.SpriteObject.transform.rotation = Quaternion.identity;

    }
}
