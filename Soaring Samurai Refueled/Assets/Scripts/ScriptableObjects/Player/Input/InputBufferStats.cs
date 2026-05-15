using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "InputBufferStats", menuName = "Scripts/ScriptableObjects/Player/Input/InputBufferStats")]
public class InputBufferStats : ScriptableObject
{
    [Header("Buffer Settings")]
    public float MaxBufferTimeLength = 1.0f;
    public float RecordFrequency = 0.05f; // Gap in seconds between recording input for the buffer

    [Header("Flick Settings")]
    public float MinFlickSpeed = 0.3f;
    public float FlickCheckTimeWindow = 0.1f;
}
