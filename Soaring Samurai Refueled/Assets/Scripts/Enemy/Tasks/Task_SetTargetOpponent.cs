using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBT;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/SetTargetOpponent")]
public class Task_SetTargetOpponent : Leaf
{
    // Blackboard key references
    [SerializeField] GameObjectReference TargetObjectKeyRef;

    // Private references
    PlayerCombatController mCombatController;


    public override void OnEnter()
    {
        mCombatController = transform.GetComponentInParent<PlayerCombatController>();
        TargetObjectKeyRef.Value = mCombatController.OpponentRef.gameObject;
    }
    public override NodeResult Execute()
    {
        return NodeResult.success;
    }


}
