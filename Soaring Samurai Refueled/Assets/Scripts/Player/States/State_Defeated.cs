using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Defeated : StateManagerPlayer.State
{
    public State_Defeated() : base(PlayerStates.Defeated) { }

    public override void OnEnter()
    {
        PlayerCombatController combatController = mParentObject.GetComponent<PlayerCombatController>();
        //combatController.SpriteObject.GetComponent<SpriteRenderer>().enabled = false;

        LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().BlockInputType(InputBlockingManager.InputType.MovementAction, combatController.PlayerIndex); // Block movement input for this player so they stop properly after we cancel out their movement input


        combatController.CurrMoveInput = Vector2.zero; // Cancel out move input

        // Play defeated animation
        combatController.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_RoundLossHit"); // Play round loss animation, that transitions into the exhausted animation

    }


    public override void OnExit()
    {
        PlayerCombatController combatController = mParentObject.GetComponent<PlayerCombatController>();
        LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().UnblockInputType(InputBlockingManager.InputType.MovementAction, combatController.PlayerIndex); // Restore movement input access
    }
}
