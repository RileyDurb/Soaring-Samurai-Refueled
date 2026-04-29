using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Flinch : StateManagerPlayer.State
{
    public State_Flinch() : base(PlayerStates.Flinch) { }

    public override void OnEnter()
    {
        mParentObject.GetComponent<PlayerCombatController>().SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_HitFlinch");
    }
}
