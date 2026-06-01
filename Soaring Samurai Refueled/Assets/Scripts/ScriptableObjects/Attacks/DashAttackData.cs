using AudioEvents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "DashAttack", menuName = "Scripts/ScriptableObjects/Attacks/DashAttackDataObject")]
public class DashAttackDataObject : AttackDataObject
{

    public float ChargeTime = 1.0f;
    public float RecoveryTime = 0.5f;
    public float DashingJerk = 1200.0f;

    public LayerMask ExcludeLayersForPlayerCollision;

    [Header("Audio")]
    public SoundEvent ChargeStartSound;
    public SoundEvent RecoveryStartSound;

}
