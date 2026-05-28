using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MatchTuningStats", menuName = "Scripts/ScriptableObjects/Match/MatchTuningStats")]
public class MatchTuningStats : ScriptableObject
{
    [Header("Round Start Behaviour")]
    public bool ResetPositionsOnMatchStart = true;
    public bool ResetPositionsOnRoundStart = true;
    public float PlayerStartOffsetDistance = 1.05f;
    public bool ClearForcesOnRestart = true;

    [Header("Rounds")]
    public float MaxRoundTime = 45.0f;
    public int NumRoundsToWin = 2;

    [Header("Sudden Death")]
    public float SuddenDeathHealthValue = 2.0f;

    [Header("Balance Related Aesthetics")]
    public float MatchEndRestartDelay = 5.0f;
    public float FirstPreRoundLength = 4.0f;
    public float SubsequentPreRoundsLength = 3.0f;

    [Header("Match End Sequence Aesthetics")]
    public float MatchEndMenuPopupDelay = 1.0f;

}
