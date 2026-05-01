using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBT;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/MoveTo")]
public class Task_MoveTo : Leaf
{

    [SerializeField] bool UseMaxMoveTime = false;
    [SerializeField] float MaxMoveTime = 5.0f;
    [SerializeField] float TargetClosnessThreshold = 2.0f; // How close the player needs to be to the target to be considered there, and for this task to count as succeded
    [SerializeField] Vector2 ManualTargetOffset = new Vector2(0, -5.0f);


    [SerializeField] GameObjectReference TargetObjectKeyRef;
    [SerializeField] Vector2Reference TargetOffsetKeyRef;

    float mCurrMoveTime = 0.0f;

    PlayerCombatController mCombatController;
    public override void OnEnter()
    {
        mCurrMoveTime = MaxMoveTime;

        mCombatController = behaviourTree.transform.parent.GetComponent<PlayerCombatController>();
    }
    public override NodeResult Execute()
    {
        if (UseMaxMoveTime == false || mCurrMoveTime > 0.0f)
        {
            mCurrMoveTime -= Time.deltaTime; // Update timer

            // Move toward opponent
            Vector2 vecToTarget = TargetObjectKeyRef.Value.transform.position - mCombatController.transform.position;

            if (TargetOffsetKeyRef == null) // If no offsewt key set
            {
                vecToTarget += ManualTargetOffset;
            }
            else
            {
                vecToTarget += TargetOffsetKeyRef.Value;
            }


            if (vecToTarget.magnitude <= TargetClosnessThreshold)
            {
                return NodeResult.success;
            }

            // Not there yet, apply mvement toward target
            mCombatController.OnMove(UnityEngine.InputSystem.InputActionPhase.Performed, vecToTarget);



            return NodeResult.running;
        }
        else // If our timer is up
        {
            return NodeResult.failure; // We haven't reached the goal, but max time is up
        }
    }

    public override void OnExit()
    {
        mCombatController.OnMove(UnityEngine.InputSystem.InputActionPhase.Canceled, Vector2.zero);
    }

}


//// Empty Menu attribute prevents Node to show up in "Add Component" menu.
//[AddComponentMenu("")]
//// Register node in visual editor node finder
//[MBTNode(name = "Tasks/ExampleTask")]
//public class Task_ExampleTask : Leaf
//{

//    PlayerCombatController mCombatController;
//    public override void OnEnter()
//    {

//    }
//    public override NodeResult Execute()
//    {
//        return NodeResult.failure; // We haven't reached the goal, but max time is up
//    }

//    public override void OnExit()
//    {

//    }

//}