using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBT;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/SetStrafeTarget")]
public class Task_SetStrafeTarget : Leaf
{
    // Blackboard key references
    [SerializeField] GameObjectReference TargetObjectKeyRef;
    [SerializeField] Vector2Reference StrafeTargetOffsetKeyRef;

    [SerializeField] BoolReference HasStrafeStarted;

    [SerializeField] float OffsetDistance = 5.0f;
    [SerializeField] float AngleOfOptions = 90.0f;
    // Private variables
    int mCurrentOffsetIndex = 0;
    List<Vector2> mOffsetDirectionOptions = new List<Vector2>{
        Vector2.up,
        Vector2.right,
        Vector2.down,
        Vector2.left
        };

    // Private references
    PlayerCombatController mCombatController;


    public override void OnEnter()
    {
        mCombatController = transform.parent.GetComponent<PlayerCombatController>();
        TargetObjectKeyRef.Value = mCombatController.OpponentRef.gameObject;
    }
    public override NodeResult Execute()
    {
        if (HasStrafeStarted.Value == false) // if strafing has not been started
        {
            // Pick a starting point

            Vector2 vecToTarget = TargetObjectKeyRef.Value.transform.position - transform.parent.position; // Get vector toward opponent
            if (SimManager.Instance.DebugModeOn)
            {
                Debug.DrawRay(transform.parent.position, vecToTarget, Color.magenta, 5.0f);
            }




            // Find all points around opponent that aren't behind it
            List<int> mValidOptions = new List<int>();
            for (int i = 0; i < mOffsetDirectionOptions.Count; i++)
            {
                Vector2 direction = mOffsetDirectionOptions[i];
                float wideAngleDeg = 180.0f -  Vector2.Angle(direction, vecToTarget);
                if (wideAngleDeg <= AngleOfOptions) // If the point is not behind the opponent, in reference to our player
                {
                    mValidOptions.Add(i);
                }
            }

            if (mValidOptions.Count <= 0)
            {
                print("Task_SetStrafeTarget:Execute:  mValidDirections is empty, could not find a valid offset to start strafing to");
                return NodeResult.failure;
            }

            // Pick a random offset to use
            mCurrentOffsetIndex = mValidOptions[MyRandom.RandomRange(0, mValidOptions.Count - 1)];
            StrafeTargetOffsetKeyRef.Value = mOffsetDirectionOptions[mCurrentOffsetIndex] * OffsetDistance;


            // Set that strafing has been started
            HasStrafeStarted.Value = true;
        }
        else // Strafe has already started
        {
            // Move to an random adjacent direction

            bool moveClockwise = MyRandom.RandomBool();

            if (moveClockwise)
            {
                mCurrentOffsetIndex--;
                if (mCurrentOffsetIndex < 0)
                {
                    mCurrentOffsetIndex = mOffsetDirectionOptions.Count - 1;
                }
            }
            else
            {
                mCurrentOffsetIndex++;

                if (mCurrentOffsetIndex >= mOffsetDirectionOptions.Count)
                {
                    mCurrentOffsetIndex = 0;
                }
            }

            StrafeTargetOffsetKeyRef.Value = mOffsetDirectionOptions[mCurrentOffsetIndex] * OffsetDistance;
        }

        if (SimManager.Instance.DebugModeOn)
        {
            //Vector2 offsetPoint = new Vector2(TargetObjectKeyRef.Value.transform.position.x, TargetObjectKeyRef.Value.transform.position.y) + mOffsetDirectionOptions[mCurrentOffsetIndex] * OffsetDistance;
            //Debug.DrawLine(transform.position, offsetPoint, Color.yellow, 5.0f);
            Debug.DrawRay(TargetObjectKeyRef.Value.transform.position, mOffsetDirectionOptions[mCurrentOffsetIndex] * OffsetDistance, Color.yellow, 5.0f);
        }

        return NodeResult.success;
    }


}
