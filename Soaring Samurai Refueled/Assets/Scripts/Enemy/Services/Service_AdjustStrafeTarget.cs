using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
[MBTNode("Services/AdjustStrafeTarget")]
public class Service_AdjustStrafeTarget : Service
{
    [SerializeField] Vector2Reference TargetOffsetVectorKeyRef;
    [SerializeField] IntReference TargetOffsetIntKeyRef;
    [SerializeField] GameObjectReference TargetObjectKeyRef;
    [SerializeField] BotBehaviourStatsReference BotStats;



    // Non editor set references
    PlayerCombatController mParentController;

    public override void OnEnter()
    {
        mParentController = transform.GetComponentInParent<PlayerCombatController>();
        base.OnEnter();
    }
    public override void Task()
    {
        Vector2 vecToTarget = TargetObjectKeyRef.Value.transform.position - mParentController.transform.position;
        float wideAngleDeg = 180.0f - Vector2.Angle(TargetOffsetVectorKeyRef.Value, vecToTarget);
        if (wideAngleDeg <= BotStats.Value.mStrafeStats.AngleOfOptions) // If the point is not behind the opponent, in reference to our player
        {
            return; // Point is good, keep it and return
        }
        else // Find new strafe target that is visible
        {
            // find the closest adjacent offset point that is visible to our player, to make the new target
            List<int> newOptionsToTry = new List<int>();

            int currOffset = TargetOffsetIntKeyRef.Value;
            
            // Try one offset point clockwise of the current one
            int newDirectionToTry = currOffset + 1;
            if (newDirectionToTry >= BotStats.Value.mStrafeStats.OffsetDirectionOptions.Count)
            {
                newDirectionToTry = 0;
            }

            int currFinalDirection = newDirectionToTry;
            float minWideAngleBetweenOffset = 180.0f - Vector2.Angle(BotStats.Value.mStrafeStats.OffsetDirectionOptions[newDirectionToTry], vecToTarget);

            // Try one offset point countercolockwise of the current one, and use as final if it's closer to our player (angle is amaller

            newDirectionToTry = currOffset - 1;
            if (newDirectionToTry < 0)
            {
                newDirectionToTry = BotStats.Value.mStrafeStats.OffsetDirectionOptions.Count - 1;
            }

            float currWideAngleBetweenOffset = 180.0f - Vector2.Angle(BotStats.Value.mStrafeStats.OffsetDirectionOptions[newDirectionToTry], vecToTarget);
            if (currWideAngleBetweenOffset < minWideAngleBetweenOffset)
            {
                minWideAngleBetweenOffset = currWideAngleBetweenOffset;
                currFinalDirection = newDirectionToTry;
            }

            // ensure new direction is within angle of options
            if (minWideAngleBetweenOffset > BotStats.Value.mStrafeStats.AngleOfOptions) // if not within angle
            {
                print("Service_AdjustStrafeTarget:Task: both adjacent offsets were above the " + BotStats.Value.mStrafeStats.AngleOfOptions + " degree angle of options. Investigate or change this to allow trying more directions");
                return;
            }
            else // Set new strafe target
            {
                TargetOffsetVectorKeyRef.Value = BotStats.Value.mStrafeStats.OffsetDirectionOptions[newDirectionToTry];
                TargetOffsetIntKeyRef.Value = newDirectionToTry;

                if (SimManager.Instance.DebugModeOn)
                {
                    //Vector2 offsetPoint = new Vector2(TargetObjectKeyRef.Value.transform.position.x, TargetObjectKeyRef.Value.transform.position.y) + mOffsetDirectionOptions[mCurrentOffsetIndex] * OffsetDistance;
                    //Debug.DrawLine(transform.position, offsetPoint, Color.yellow, 5.0f);
                    Debug.DrawRay(TargetObjectKeyRef.Value.transform.position, BotStats.Value.mStrafeStats.OffsetDirectionOptions[TargetOffsetIntKeyRef.Value] *
                        BotStats.Value.mStrafeStats.OffsetDistance, Color.yellow, 5.0f);
                }
            }
        }

    }
}