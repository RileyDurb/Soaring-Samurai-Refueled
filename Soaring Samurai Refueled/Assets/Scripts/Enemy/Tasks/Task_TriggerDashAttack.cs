using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBT;
using System;
using UnityEngine.InputSystem;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/TriggerDashAttack")]
public class Task_TriggerDashAttack : Leaf
{

    ActionList mDashAttackActionList = new ActionList();
    // Private references
    PlayerCombatController mCombatController;

    bool mReleasedAttack = false;
    public override void OnEnter()
    {
        mCombatController = transform.GetComponentInParent<PlayerCombatController>();

        mDashAttackActionList.Clear();

        mReleasedAttack = false;
    }
    public override NodeResult Execute()
    {
        mDashAttackActionList.Update(Time.deltaTime);

        mCombatController.DashAttackInput(InputActionPhase.Performed); // Start charging dash attack

        mDashAttackActionList.AddActionCallback(() => { TriggerAttackRelease(); }, 1.0f); // Release charge a little bit later

        if (mReleasedAttack)
        {
            return NodeResult.success;
        }
        else
        {
            return NodeResult.running;
        }
    }


    void TriggerAttackRelease()
    {
        mCombatController.DashAttackInput(InputActionPhase.Canceled);
        mReleasedAttack = true;
    }
}