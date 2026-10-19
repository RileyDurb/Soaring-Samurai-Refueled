using AudioEvents;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public float PreRoundMoveAllowDelay = 0.0f;

    [Header("Match End Sequence Aesthetics")]
    public float MatchEndMenuPopupDelay = 1.0f;
    public float MatchEndTimeScaleValue = 1.0f;
    public float MatchEndTimeScaleDuration = 0.0f;
    public Action_.EasingTypes MatchEndTimeScaleEasing = Action_.EasingTypes.None;
    public AnimationCurve MatchEndTimeScaleCurve;

    [Header("SFX")]
    public SoundEvent RoundFinishSFX;
    public SoundEvent MainMatchBGM;

    [Header("Bot AI Behaviour")]
    public bool StartWithCPUActive = false;

    [Header("PrematchSequences")]
    public bool PlayPreMatchSequence = true;
    public Vector2 CinematicCamPosition = Vector2.zero;
    public float CinematicCamSize = 10;
    public Vector2 CinematicStartingOffset = new Vector2(0.0f, -20.0f);
    public float CinematicFlyInTime = 4.5f;
    public Vector2 CinematicFlyInTimeVariationMinMax = Vector2.zero;
    public Action_.EasingTypes CinematicFlyInEasing = Action_.EasingTypes.None;
    public AnimationCurve CinematicFlyInEasingCurve;
    public float CinematicWaitTimeAfterFlyIn = 2.0f;

}
