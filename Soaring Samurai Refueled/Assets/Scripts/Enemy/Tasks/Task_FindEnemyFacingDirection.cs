using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBT;
using System;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/FindEnemyFacingDirection")]
public class Task_FindEnemyFacingDirection : Leaf
{
    // Blackboard values
    [SerializeField] IntReference AttackDirectionKeyRef; // Optional int that repreresents the attack direction enum value for the closest attack direction toward the opponent
    [SerializeField] Vector2Reference FacingDirectionKeyRef; // Optional exact facing vector


    // Private references
    PlayerCombatController mCombatController;
    public override void OnEnter()
    {
        mCombatController = transform.GetComponentInParent<PlayerCombatController>();
    }
    public override NodeResult Execute()
    {
        // Find the closest attack direction towards the opponent

        // Get vector facing opponent
        Vector2 vecTowardOpponent = mCombatController.OpponentRef.transform.position - mCombatController.transform.position;

        // Scratch variables to store the calculated direction
        AIBehaviour.AttackDirection attackDirectionEnum = AIBehaviour.AttackDirection.UpRight;
        Vector2 attackDirectionVec = Vector2.up + Vector2.right;

        float vecToOpponentUpAngle = Vector2.SignedAngle(vecTowardOpponent, Vector2.up);

        if (vecTowardOpponent.y > 0) // if y value is positive, upper right left or right quadrant  (or exactly straight on, which we'll decide to count as the upper quadrant)
        {
            if (vecTowardOpponent.x >= 0)
            {
                attackDirectionEnum = AIBehaviour.AttackDirection.UpRight;
            }
            else
            {
                attackDirectionEnum = AIBehaviour.AttackDirection.UpLeft;

            }
        }
        else // if y direction to opponent is positive, upper left or right quadrant
        {
            if (vecTowardOpponent.x >= 0.0f)
            {
                attackDirectionEnum = AIBehaviour.AttackDirection.DownRight;
            }
            else
            {
                attackDirectionEnum = AIBehaviour.AttackDirection.DownLeft;
            }
        }

        attackDirectionVec = AIBehaviour.AttackDirectionVectors[(int)attackDirectionEnum];

        if (AttackDirectionKeyRef.key != "") // If we have a key for the attack direction int (don't force user to have one, cause we may just want the direction vector)
        {
            AttackDirectionKeyRef.Value = (int)attackDirectionEnum;
        }

        if (FacingDirectionKeyRef.key != "") // if we have a key for the attack direction vec (don't force user to have one, since we may just want the direction int)
        {
            FacingDirectionKeyRef.Value = attackDirectionVec;
        }


        return NodeResult.success; // We haven't reached the goal, but max time is up
    }

}