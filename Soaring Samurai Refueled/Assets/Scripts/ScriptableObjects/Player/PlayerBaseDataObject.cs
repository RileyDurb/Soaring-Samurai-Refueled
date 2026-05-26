using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class PlayerMovementStats
{
    public float MoveJerk = 18000.0f;
    public float DashingJerk = 18000.0f;
    public bool UseMaxJerkCurve = false;
    public AnimationCurve InputValueToMaxJerkCurve;
    public float PartialInputMovementStatsThreshold; // Percentage of max movement input value before full movement stats are applied
    public PhysicsTuningStatSet PartialInputMovementStats;
    public PhysicsTuningStatSet FullInputMovementStats;
    public float DashDuration = 0.3f;
    public float DashGasCost = 25.0f;
    public float DashOldInputRecencyLimit = 0.5f;
}


[CreateAssetMenu(fileName = "PlayerBaseDataObject", menuName = "Scripts/ScriptableObjects/Player/PlayerBaseDataObject")]
public class PlayerBaseDataObject : ScriptableObject
{
    // Stat data for adjusting in the editor
    [Header("Movememt Stats")]
    public PlayerMovementStats mMovementStats;

}
