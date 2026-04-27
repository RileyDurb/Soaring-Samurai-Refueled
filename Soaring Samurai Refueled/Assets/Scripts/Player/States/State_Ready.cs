using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class State_Ready : StateManagerPlayer.State
{
    float IdleMovementOffsetAmount = 0.1f;
    float IdleMovementCycleTime = 5.0f;
    //Action_.EasingTypes IdleMovementOutEas
    public State_Ready() : base(PlayerStates.Ready) { }

    ActionList mReadyActionList = new ActionList();

    PlayerCombatController mCombatControllerRef;
    public override void OnEnter()
    {

        mCombatControllerRef = mParentObject.GetComponent<PlayerCombatController>();

        mCombatControllerRef.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_Idle"); // Play idle animation

        StartIdleLoop();
        mReadyActionList.AddActionCallback(() => { StartIdleLoop(); }, IdleMovementCycleTime, false, true);


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

        // Apply movement
        float currSpeed = mCombatControllerRef.mPlayerBaseStats.mMovementStats.MoveJerk;

        Vector2 moveVec = mCombatControllerRef.CurrMoveInput * currSpeed;

        mCombatControllerRef.ApplyCappedMovementJerk(moveVec, Time.deltaTime);

    }

    public override void OnExit()
    {
        mReadyActionList.Clear();
        mCombatControllerRef.SpriteObject.transform.localPosition = Vector2.zero; // Resets local position to 0
    }

    void StartIdleLoop()
    {
        Vector2 currPos = Vector2.zero;

        float currDelay = 0.0f;
        mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos + Vector2.down * IdleMovementOffsetAmount, IdleMovementCycleTime / 4);
        currDelay += IdleMovementCycleTime / 4;
        mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos + Vector2.up * IdleMovementOffsetAmount, IdleMovementCycleTime / 2, currDelay);
        currDelay += IdleMovementCycleTime / 2;
        mReadyActionList.AddActionLocalMove(mCombatControllerRef.SpriteObject, currPos, IdleMovementCycleTime / 4, currDelay);
    }
}
