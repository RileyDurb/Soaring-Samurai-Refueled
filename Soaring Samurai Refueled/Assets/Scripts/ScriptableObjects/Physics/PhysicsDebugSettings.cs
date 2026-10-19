using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsDebugSettings", menuName = "Scripts/ScriptableObjects/Physics/PhysicsDebugSettings")]
public class PhysicsDebugSettings : ScriptableObject
{
    public bool ShowMoveSpeedDebugTextDuringDebugMode = true;
}
