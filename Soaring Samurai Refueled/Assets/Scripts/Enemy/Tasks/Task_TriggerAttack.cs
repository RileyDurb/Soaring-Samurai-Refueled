using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBT;
using System;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/TriggerAttack")]
public class Task_TriggerAttack : Leaf
{
    // Blackboard values
    [SerializeField] IntReference AttackDirection;


    // Private references
    PlayerCombatController mCombatController;
    public override void OnEnter()
    {
        mCombatController = transform.GetComponentInParent<PlayerCombatController>();
    }
    public override NodeResult Execute()
    {
        AttackDirection.Value = Mathf.Clamp(AttackDirection.Value, 0, Enum.GetValues(typeof(AIBehaviour.AttackDirection)).Length - 1);

        AIBehaviour.AttackDirection direction = (AIBehaviour.AttackDirection)AttackDirection.Value;
        
        mCombatController.GetComponent<AIBehaviour>().TriggerNormalSlashAttack(direction);


        return NodeResult.success; // We haven't reached the goal, but max time is up
    }

}