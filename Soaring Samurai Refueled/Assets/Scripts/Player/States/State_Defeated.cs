using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Defeated : StateManagerPlayer.State
{
    public State_Defeated() : base(PlayerStates.Defeated) { }

    public override void OnEnter()
    {
        PlayerCombatController combatController = mParentObject.GetComponent<PlayerCombatController>();
        combatController.SpriteObject.GetComponent<SpriteRenderer>().enabled = false;

        combatController.CurrMoveInput = Vector2.zero; // Cancel out move input
    }


    public override void OnExit()
    {
        mParentObject.GetComponent<PlayerCombatController>().SpriteObject.GetComponent<SpriteRenderer>().enabled = true;
    }
}
