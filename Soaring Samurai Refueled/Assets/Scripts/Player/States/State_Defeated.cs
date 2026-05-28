using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Defeated : StateManagerPlayer.State
{
    public State_Defeated() : base(PlayerStates.Defeated) { }

    PlayerStateAesthetics mAestheticStats;

    PlayerCombatController mCombatController;

    ActionList mDefeatedActionList = new ActionList();

    LayerMask mOriginalCollisionLayerIgnores;

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
            // Play match loss animation, which starts with a pose
            mCombatController.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_MatchEndingHurtPose");

            // Start falling animation after a delay
            mDefeatedActionList.AddActionCallback(StartFallAnimation, mAestheticStats.DefeatedStats.MatchDefeatedPoseTime);
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

        if (mParentObject.transform.position.y < mAestheticStats.DefeatedStats.MatchDefeatedMaxFallDistance) // Fallen more than we could possibly need to go off screen (and if it's more, can tune this value
        {
            mCombatController.GetComponent<Rigidbody2D>().gravityScale = 0.0f; // Turns off gravity
        }
    }


    public override void OnExit()
    {
        mDefeatedActionList.Clear();

        mCombatController = mParentObject.GetComponent<PlayerCombatController>();
        LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().UnblockInputType(InputBlockingManager.InputType.MovementAction, mCombatController.PlayerIndex); // Restore movement input access

        mCombatController.GetComponent<Rigidbody2D>().gravityScale = 0.0f; // Turns off gravity to stop falling

        // Restore collision layers
        Rigidbody2D physics = mCombatController.GetComponent<Rigidbody2D>();
        physics.excludeLayers = mOriginalCollisionLayerIgnores;

        // Restore camera to following both players, and restore normal camera move and zoom speeds
        CameraFollow cameraControl = Camera.main.GetComponent<CameraFollow>();
        cameraControl.CurrFollowMode = CameraFollow.FollowMode.AllPlayers; 
        cameraControl.TurnOverrideCamZoomSpeedOff();
        cameraControl.TurnOverrideCamMoveSpeedOff();

    }

    void StartFallAnimation()
    {
        mCombatController.SpriteObject.GetComponent<AnimationController>().SetAnimationState("Player_MatchEndingHurt"); // Play starting to fall animatiion

        // Start actually falling after a delay
        mDefeatedActionList.AddActionCallback(StartPhysicalFall, mAestheticStats.DefeatedStats.MatchDefeatedGravityDelay);
    }
    void StartPhysicalFall()
    {
        mCombatController.GetComponent<Rigidbody2D>().gravityScale = mAestheticStats.DefeatedStats.MatchDefeatedGravityScale; // Turns on gravity to start fall

        mDefeatedActionList.AddActionCallback(LoseCameraContribution, mAestheticStats.DefeatedStats.MatchDefeatedTimeAfterFallBeforeCameraLoss); // After a delay, stop including this player in the camera so it drops off unless the winning player follows them
        
        // Exclude collision layers we want to for falling, so we at least fall out of match boundaries eventually
        Rigidbody2D physics = mCombatController.GetComponent<Rigidbody2D>();
        mOriginalCollisionLayerIgnores = physics.excludeLayers;
        physics.excludeLayers = mAestheticStats.DefeatedStats.MatchDefeatedFallIgnoreCollidionLayers;

    }

    void LoseCameraContribution()
    {
        // MPTODO: make this support more than two players by removing this as a target, instead of manually setting their opponent to be the only target. Likely handle this in the match manager instead
        CameraFollow cameraControl = Camera.main.GetComponent<CameraFollow>();
        cameraControl.ManualCameraTarget = mCombatController.OpponentRef.gameObject;
        cameraControl.CurrFollowMode = CameraFollow.FollowMode.ManualSetTarget;

        cameraControl.SetOverrideCamMoveSpeed(mAestheticStats.DefeatedStats.MatchDefeatedWinnerCamMoveSpeed);
        cameraControl.SetOverrideCamZoomSpeed(mAestheticStats.DefeatedStats.MatchDefeatedWinnerCamZoomSpeed);
    }
}
