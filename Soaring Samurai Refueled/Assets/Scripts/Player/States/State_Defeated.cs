using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Defeated : StateManagerPlayer.State
{
    public State_Defeated() : base(PlayerStates.Defeated) { }

    PlayerStateAesthetics mAestheticStats;

    PlayerCombatController mCombatController;

    ActionList mDefeatedActionList = new ActionList();
    public override void OnEnter()
    {
        mCombatController = mParentObject.GetComponent<PlayerCombatController>();

        mAestheticStats = mCombatController.StateAesthetics;
        //combatController.SpriteObject.GetComponent<SpriteRenderer>().enabled = false;

        LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().BlockInputType(InputBlockingManager.InputType.MovementAction, mCombatController.PlayerIndex); // Block movement input for this player so they stop properly after we cancel out their movement input


        mCombatController.CurrMoveInput = Vector2.zero; // Cancel out move input

        // Play animation based on if round or match is over TODO: Trigger this state more properly based on how much else we want for the end of match sequence
        if (LevelScopeManagers.Instance.GetComponent<MatchStateManager>().CurrMatchState == MatchStateManager.MatchState.GameEnd) 
        {
            // Play full match defeated animation
            mCombatController.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_MatchEndingHurt"); // Play match loss animation, that transitions into the falling animation

            // Start falling after a delay
            mDefeatedActionList.AddActionCallback(StartFall, mAestheticStats.DefeatedStats.MatchDefeatedGravityDelay);
        }
        else
        {
            // Play round defeated animation
            mCombatController.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_RoundLossHurt"); // Play round loss animation, that transitions into the exhausted animation
        }


    }

    public override void OnUpdate(float dt)
    {

        mDefeatedActionList.Update(Time.deltaTime);
    }


    public override void OnExit()
    {
        mDefeatedActionList.Clear();

        PlayerCombatController combatController = mParentObject.GetComponent<PlayerCombatController>();
        LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().UnblockInputType(InputBlockingManager.InputType.MovementAction, combatController.PlayerIndex); // Restore movement input access

        combatController.GetComponent<Rigidbody2D>().gravityScale = 0.0f; // Turns off gravity
    }

    void StartFall()
    {
        mCombatController.GetComponent<Rigidbody2D>().gravityScale = mAestheticStats.DefeatedStats.MatchDefeatedGravityScale; // Turns off gravity

    }
}
