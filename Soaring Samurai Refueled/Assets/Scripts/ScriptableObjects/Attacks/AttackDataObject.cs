using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackDataObject", menuName = "Scripts/ScriptableObjects/Attacks/AttackDataObject")]
public class AttackDataObject : ScriptableObject
{
    public Hitbox.AttackDefinition mStats;
}
