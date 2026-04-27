using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GasMeterStats", menuName = "Scripts/ScriptableObjects/Player/Meter/GasMeterStats")]
public class GasMeterStats : ScriptableObject
{
    public float GasPerSecondMovingForward = 8.9f;
    public float MovingForwardAngleForgiveness = 45.0f;
    public float GasGainOnClash = 25.0f;
}
