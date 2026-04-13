using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MatchTuningStats", menuName = "Scripts/ScriptableObjects/Match/MatchTuningStats")]
public class MatchTuningStats : ScriptableObject
{
    [Header("Round Start Behaviour")]
    public bool ResetPositions = true;
    public float PlayerStartOffsetDistance = 1.05f;
    public bool ClearForcesOnRestart = true;

    [Header("Rounds")]
    public float MaxRoundTime = 45.0f;
    public int NumRoundsToWin = 2;

    [Header("Balance Related Aesthetics")]
    public float MatchEndRestartDelay = 5.0f;
    public float PreRoundLength = 4.0f;

}
