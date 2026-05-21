using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBT;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/SetSelfReference")]
public class Task_SetSelfReference : Leaf
{
    // Blackboard key references
    [SerializeField] GameObjectReference SelfReferenceKey;


    public override void OnEnter()
    {
        PlayerCombatController combatController = transform.GetComponentInParent<PlayerCombatController>(); // Get the combat controller, as that is the parent we want to use the gameobject of as the self reference
        SelfReferenceKey.Value = combatController.gameObject;
    }
    public override NodeResult Execute()
    {
        return NodeResult.success;
    }


}
