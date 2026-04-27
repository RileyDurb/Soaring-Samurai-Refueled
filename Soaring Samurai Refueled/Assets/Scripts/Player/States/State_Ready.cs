using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class State_Ready : StateManagerPlayer.State
{

    public State_Ready() : base(PlayerStates.Ready) { }

    PlayerCombatController mCombatControllerRef;
    public override void OnEnter()
    {

        mCombatControllerRef = mParentObject.GetComponent<PlayerCombatController>();

        mCombatControllerRef.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_Idle"); // Play idle animation
    }

    public override void OnUpdate(float dt)
    {
        // Face right direction
        if (mCombatControllerRef.OpponentRef.transform.position.x < mParentObject.transform.position.x)
        {
            mCombatControllerRef.SetFacingDirection(PlayerCombatController.FacingDirection.Left);
        }
        else
        {
            mCombatControllerRef.SetFacingDirection(PlayerCombatController.FacingDirection.Right);
        }

        float currSpeed = mCombatControllerRef.mPlayerBaseStats.mMovementStats.MoveJerk;

        Vector2 moveVec = mCombatControllerRef.CurrMoveInput * currSpeed;

        mCombatControllerRef.ApplyCappedMovementJerk(moveVec, Time.deltaTime);

    }


}
