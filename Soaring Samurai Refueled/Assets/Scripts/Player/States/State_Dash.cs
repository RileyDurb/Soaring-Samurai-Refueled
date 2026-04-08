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
    public override void OnEnter()
    {
        // Intialize variables
        mCombatController = mParentObject.GetComponent<PlayerCombatController>();

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
    }

    public override void OnUpdate(float dt)
    {
        // Calculate speed and direction
        float currSpeed = mCombatController.mPlayerBaseStats.mMovementStats.DashingJerk;

        Vector2 moveVec = mCombatController.CurrMoveInput * currSpeed;

        // Applies jerk
        mCombatController.ApplyUncappedMovementJerk(moveVec, Time.deltaTime);

    }
    public override void OnExit()
    {
        mDashActionList.Clear();
    }
}
