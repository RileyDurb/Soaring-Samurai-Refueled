using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStateAesthetics", menuName = "Scripts/ScriptableObjects/Player/States/PlayerStateAesthetics")]

public class PlayerStateAesthetics : ScriptableObject
{
    [System.Serializable]
    public class IdleData
    {
        public float MovementOffsetAmount = 0.1f;
        public float MovementCycleTime = 5.0f;
        public Action_.EasingTypes FirstOutMoveEasing = Action_.EasingTypes.None;
        public Action_.EasingTypes FirstInMoveEasing = Action_.EasingTypes.None;
        public Action_.EasingTypes SecondOutMoveEasing = Action_.EasingTypes.None;
        public Action_.EasingTypes SecondInMoveEasing = Action_.EasingTypes.None;
        public float MinTimeOffsetRandom = 0.0f;
        public float MaxTimeOffsetRandom = 0.8f;
        public float MinDistanceOffsetRandom = 0.0f;
        public float MaxDistanceOffsetRandom = 0.05f;
        //public Action_.EasingTypes FirstQuarterEasing = Action_.EasingTypes.None;
        //public Action_.EasingTypes SecondQuarterEasing = Action_.EasingTypes.None;
        //public Action_.EasingTypes MiddleHalfEasing = Action_.EasingTypes.None;
        //public Action_.EasingTypes LastQuarterEasing = Action_.EasingTypes.None;
    }

    [System.Serializable]
    public class DefeatedData
    {
        public float MatchDefeatedGravityScale = 10.0f;
        public float MatchDefeatedPoseTime = 2.0f;
        public float MatchDefeatedGravityDelay = 3.0f;
        public float MatchDefeatedTimeAfterFallBeforeCameraLoss = 2.0f;
        public LayerMask MatchDefeatedFallIgnoreCollidionLayers;
        public float MatchDefeatedMaxFallDistance = -2000.0f;
        public float MatchDefeatedWinnerCamZoomSpeed = 2.0f;
        public float MatchDefeatedWinnerCamMoveSpeed = 3.0f;
    }


    public IdleData IdleStats = new IdleData();

    public DefeatedData DefeatedStats = new DefeatedData();
}
