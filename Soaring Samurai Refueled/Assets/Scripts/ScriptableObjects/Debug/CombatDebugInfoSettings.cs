using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatDebugInfoSettings", menuName = "Scripts/ScriptableObjects/Debug/CombatDebugInfoSettings")]
public class CombatDebugInfoSettings : ScriptableObject
{
    public Color mHitboxInactiveColor;
    public Color mHitboxActiveColor;
}
