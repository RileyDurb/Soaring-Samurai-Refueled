using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBT;
using Unity.VisualScripting;
using Unity.Collections;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/MoveTo")]
public class Task_MoveTo : Leaf
{

    [SerializeField] bool UseMaxMoveTime = false;
    [SerializeField] float MaxMoveTime = 5.0f;
    [SerializeField] Vector2 TargetClosnessThresholdXY = new Vector2(2.0f, 2.0f); // How close the player needs to be to the target to be considered there, and for this task to count as succeded. If giving different values for x and y, lerps between them based on the directon the moving object is incoming to the target
    [SerializeField] Vector2 ManualTargetOffset = new Vector2(0, -5.0f);


    [SerializeField] GameObjectReference TargetObjectKeyRef;
    [SerializeField] Vector2Reference TargetOffsetKeyRef;

    float mCurrMoveTime = 0.0f;

    PlayerCombatController mCombatController;
    public override void OnEnter()
    {
        mCurrMoveTime = MaxMoveTime;

        mCombatController = behaviourTree.GetComponentInParent<PlayerCombatController>();
    }
    public override NodeResult Execute()
    {
        if (UseMaxMoveTime == false || mCurrMoveTime > 0.0f)
        {
            mCurrMoveTime -= Time.deltaTime; // Update timer

            // Move toward opponent
            Vector2 vecToTarget = TargetObjectKeyRef.Value.transform.position - mCombatController.transform.position;

            if (TargetOffsetKeyRef.key == "") // If no offsewt key set
            {
                vecToTarget += ManualTargetOffset;
            }
            else
            {
                vecToTarget += TargetOffsetKeyRef.Value;
            }

            float closenessToBeingVertical = Mathf.Abs(Vector2.Dot(Vector2.up, vecToTarget.normalized));
            float targetClosenessMagnitide = Mathf.Lerp(TargetClosnessThresholdXY.x, TargetClosnessThresholdXY.y, closenessToBeingVertical);
            if (vecToTarget.magnitude <= targetClosenessMagnitide)
            {
                return NodeResult.success;
            }

            vecToTarget = Vector2.ClampMagnitude(vecToTarget, 1.0f); // Allow less then full input, but not more than 1, as that's the max movement value of input

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