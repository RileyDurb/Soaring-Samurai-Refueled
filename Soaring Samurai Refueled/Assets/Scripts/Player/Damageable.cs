using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Hitbox;

// Interface for everything that can take damage
public interface IDamageable
{
    public void TakeDamage(AttackCurrentData attackData, AttackDefinition baseAttackInfo);
}
