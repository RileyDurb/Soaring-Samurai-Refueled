using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
[MBTNode("Services/DashToCloseGap")]
public class Service_DashToCloseGap : Service
{
    public float MinCheckTime = 3.0f;
    public float MaxCheckTime = 5.0f;
    public float DashCheckInterval = 1.0f; // Interval to check again if we should dash after the first check when the check time window is reached
    public float DashCheckIntervalAfterDashingMin = 1.0f;
    public float DashCheckINtervalAfterDashingMax = 2.0f;
    public bool AlwaysDashWhenReady = false;
    public bool CanDashMultipleTimes = true;



    // Non editor set references
    float mCurrWaitTime = -1.0f;
    float mCurrDashCheckTimer = -1.0f;
    float mLastTickTimestamp = 0.0f;

    PlayerCombatController mCombatController;

    public override void OnEnter()
    {
        mCurrWaitTime = MinCheckTime;
        mCurrDashCheckTimer = -1.0f;
        mLastTickTimestamp = Time.timeSinceLevelLoad;

        mCombatController = behaviourTree.GetComponentInParent<PlayerCombatController>();
        base.OnEnter();
    }
    public override void Task()
    {
        if (mCurrWaitTime >= 0.0f)
        {
                mCurrWaitTime -= Time.timeSinceLevelLoad - mLastTickTimestamp; // Update timer

                mLastTickTimestamp = Time.timeSinceLevelLoad;

                if (mCurrWaitTime <= 0.0f)
                {
                    mCurrDashCheckTimer += MaxCheckTime - MinCheckTime; // Add time equal to the check window

                    mCurrDashCheckTimer = 0.0f;
                }
            }

        // If dash checking is active
        if (mCurrDashCheckTimer >= 0.0f)
        {
            mCurrDashCheckTimer -= Time.timeSinceLevelLoad - mLastTickTimestamp; // Update timer

            mLastTickTimestamp = Time.timeSinceLevelLoad;


            if (mCurrDashCheckTimer <= 0.0f) // If dash timer is up
            {
                bool shouldDash = MyRandom.RandomBool(); // Get a random on if we should dash

                if (AlwaysDashWhenReady == true)
                {
                    shouldDash = true;
                }


                if (shouldDash)
                {
                    mCombatController.DashInput(UnityEngine.InputSystem.InputActionPhase.Canceled); // Trigger dash
                }

                // Add time to check for dashing again, this time with optionally different values for how long we wait
                if (CanDashMultipleTimes == true)
                {
                    mCurrDashCheckTimer += MyRandom.RandomRange(DashCheckIntervalAfterDashingMin, DashCheckINtervalAfterDashingMax);
                }
                else
                {
                    mCurrDashCheckTimer = -1.0f;
                }
                
            }
        }
    }
}