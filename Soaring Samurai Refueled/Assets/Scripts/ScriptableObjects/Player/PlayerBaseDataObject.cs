using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class PlayerMovementStats
{
    public float MoveJerk = 18000.0f;
    public float DashingJerk = 18000.0f;
    public float DashDuration = 0.3f;
}




[CreateAssetMenu(fileName = "PlayerBaseDataObject", menuName = "Scripts/ScriptableObjects/Player/PlayerBaseDataObject")]
public class PlayerBaseDataObject : ScriptableObject
{
    // Stat data for adjusting in the editor
    [Header("Movememt Stats")]
    public PlayerMovementStats mMovementStats;

}
