using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackDataObject", menuName = "Scripts/Player/ScriptableObjects/AttackDataObject")]
public class AttackDataObject : ScriptableObject
{
    public Hitbox.AttackDefinition mStats;
}
