using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class State_Ready : StateManagerPlayer.State
{
    PlayerCombatController mCombatControllerRef;
    public override void OnEnter()
    {
        mParentObject.GetComponent<AnimationController>().SetAnimationState("Player_Idle");

        mCombatControllerRef = mParentObject.GetComponent<PlayerCombatController>();
    }

    public override void OnUpdate(float dt)
    {
        
        if (mCombatControllerRef.OpponentRef.transform.position.x < mParentObject.transform.position.x)
        {
            mCombatControllerRef.SetFacingDirection(PlayerCombatController.FacingDirection.Left);
        }
        else
        {
            mCombatControllerRef.SetFacingDirection(PlayerCombatController.FacingDirection.Right);
        }
    }

}
