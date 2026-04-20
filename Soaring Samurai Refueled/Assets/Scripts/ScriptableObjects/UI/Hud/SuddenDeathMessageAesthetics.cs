using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SuddenDeathMessageAesthetics", menuName = "Scripts/ScriptableObjects/UI/Hud/SuddenDeathMessageAesthetics")]
public class SuddenDeathMessageAesthetics : ScriptableObject
{
    public float FadeOutTime = 1.5f;
    public Action_.EasingTypes FadeOutEasing = Action_.EasingTypes.None;

    public float FadeInTime = 1.5f;
    public Action_.EasingTypes FadeInEasing = Action_.EasingTypes.None;
}
