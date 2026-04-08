using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_SlashAttack : StateManagerPlayer.State
{
    State_SlashAttack() : base(PlayerStates.SlashAttack) { }

    // References
    PlayerCombatController mCombatControllerRef;

    public override void OnEnter()
    {
        mCombatControllerRef = mParentObject.GetComponent<PlayerCombatController>();
    }
    public override void OnUpdate(float dt)
    {
        float currSpeed = mCombatControllerRef.mPlayerBaseStats.mMovementStats.MoveJerk;

        Vector2 moveVec = mCombatControllerRef.CurrMoveInput * currSpeed;

        mCombatControllerRef.ApplyCappedMovementJerk(moveVec, Time.deltaTime);
    }


}
