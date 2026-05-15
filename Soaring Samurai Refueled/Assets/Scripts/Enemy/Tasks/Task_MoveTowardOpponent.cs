using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBT;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/MoveTowardOpponent")]
public class Task_MoveTowardOpponent : Leaf
{
    [SerializeField] float MaxMoveTime = 5.0f;

    float mCurrMoveTime = 0.0f;

    PlayerCombatController mCombatController;
    public override void OnEnter()
    {
        mCurrMoveTime = MaxMoveTime;

        mCombatController = behaviourTree.GetComponentInParent<PlayerCombatController>();
    }
    public override NodeResult Execute()
    {
        if (mCurrMoveTime > 0.0f)
        {
            mCurrMoveTime -= Time.deltaTime; // Update timer

            // Move toward opponent
            Vector2 vecToOpponent = mCombatController.OpponentRef.transform.position - mCombatController.transform.position;
            vecToOpponent = Vector2.ClampMagnitude(vecToOpponent, 1.0f); // Allow less then full input, but not more than 1, as that's the max movement value of input

            mCombatController.OnMove(UnityEngine.InputSystem.InputActionPhase.Performed, vecToOpponent);

            return NodeResult.running;
        }
        else // If our timer is up
        {
            return NodeResult.success;
        }
    }

    public override void OnExit()
    {
        mCombatController.OnMove(UnityEngine.InputSystem.InputActionPhase.Canceled, Vector2.zero);
    }

}
