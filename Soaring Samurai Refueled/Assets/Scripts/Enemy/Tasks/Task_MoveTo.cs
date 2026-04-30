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


    float mCurrMoveTime = 0.0f;
    Vector2 TargetOffset = Vector2.zero;

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
            Vector2 vecToOpponent = mCombatController.OpponentRef.transform.position - mCombatController.transform.position;

            if (vecToOpponent.magnitude <= TargetClosnessThreshold)
            {
                return NodeResult.success;
            }

            // Not there yet, apply mvement toward target
            mCombatController.OnMove(UnityEngine.InputSystem.InputActionPhase.Performed, vecToOpponent);



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
