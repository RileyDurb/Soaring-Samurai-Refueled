using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PhysicsApplier;

[CreateAssetMenu(fileName = "PhysicsTuningStatSet", menuName = "Scripts/ScriptableObjects/Player/PhysicsTuningStatSet")]
[System.Serializable]
public class PhysicsTuningStatSet : ScriptableObject
{
    [Header("ForceMaxes")]
    public float mMaxVelocity;
    public float mMaxAcceleration;
    public float mMaxJerk;

    [Header("Coefficients")]
    public float DampeningMultiplier = 0.9f;
    public float DragCoeff = 0.3f;

    public float mDampeningZeroThreshold = 0.1f; // NOT used, may revisit zeroing out a force once it hits a certain low threshold after applying drag
    public DampeningType mDampeningType = DampeningType.Percentage;
    public float mMaxDampeningTime = 1.0f; // Only for interpolation dampening. The time it takes to dampen when at max velocity. Lower velocities will take less time

    [Header("Switches")]
    public bool ApplyDragAsAcceleration = false;

}
