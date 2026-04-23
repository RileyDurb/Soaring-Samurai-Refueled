using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoundWinTextVisuals", menuName = "Scripts/ScriptableObjects/UI/Hud/RoundWinTextVisuals")]
public class RoundWinTextVisuals : ScriptableObject{
    [Header("Enter Animation")]
    public float StartingScaleMultiplier = 0.5f;
    public float EndingScaleMultiplier = 0.5f;
    public Action_.EasingTypes MoveEasingType = Action_.EasingTypes.None;
    public Action_.EasingTypes ScaleEasingType = Action_.EasingTypes.None;
    public float EnterTime = 5.0f;
    public float StartOffset = 1000.0f;
    public float EndOffset = -1000.0f;

    [Header("Transition")]
    public float EnteredHoldTime = 2.0f;

    [Header("Exit Animation")]
    public float ExitStartingScaleMultiplier = 0.5f;
    public float ExitEndingScaleMultiplier = 0.5f;
    public Action_.EasingTypes ExitMoveEasingType = Action_.EasingTypes.None;
    public Action_.EasingTypes ExitScaleEasingType = Action_.EasingTypes.None;
    public float ExitEnterTime = 5.0f;
    public float ExitStartOffset = 1000.0f;
    public float ExitEndOffset = -1000.0f;
}
