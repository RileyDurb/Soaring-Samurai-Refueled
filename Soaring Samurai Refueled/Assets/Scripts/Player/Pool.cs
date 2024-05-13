using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Pool : MonoBehaviour
{
    // Editor accessible values
    public float PoolMax = 100;
    public float RegenTickValue = 0;
    public float RegenDelay = 1.0f;
    public float DangerThreshold = 0;
    public Action RegenAboveDangerLevel;
    public Action FallBelowDangerLevel;

    // Private variables
    [SerializeField]
    float PoolCurr = 0;

    float RegenTimer = -1.0f;


    public float PoolValue { 
        get { return PoolCurr; }
        set { PoolCurr = Mathf.Clamp(value, 0.0f, PoolMax); }
    }


    // Start is called before the first frame update
    void Start()
    {
        PoolCurr = PoolMax;
    }
    // Update is called once per frame
    void Update()
    {
        if (PoolCurr < PoolMax)
        {
            if (RegenTimer < 0.0f)
            {
                RegenTimer = RegenDelay; // Turns on regen timer
            }
        }
        else
        {
            RegenTimer = -1.0f; // Turns off regen timer
        }

        if (RegenTimer > 0.0f)
        {
            RegenTimer -= Time.unscaledDeltaTime;
            if (RegenTimer <= 0.0f)
            {
                float valueBeforeRegen = PoolCurr;
                PoolCurr += RegenTickValue; // Regenerates for the current tick

                // If just regenerating above danger threshold
                if (PoolCurr > DangerThreshold && valueBeforeRegen <= DangerThreshold)
                {
                    RegenAboveDangerLevel.Invoke(); // Notifies listeners of regenerating above danger level
                }

                RegenTimer = RegenDelay;
            }
        }
    }

    // Getter and setter functions
    // Decreases pool, and returns if pool just became empty
    public bool DecreasePool (float amount)
    {
        if (PoolCurr <= 0)
        {
            return false;
        }

        bool justEmptied = false;
        float valueBeforeModify = PoolCurr;
        PoolCurr -= amount;

        // If just decreasing to the danger threshold or below
        if (valueBeforeModify > DangerThreshold && PoolCurr <= DangerThreshold)
        {
            if (FallBelowDangerLevel != null)
            {
                FallBelowDangerLevel.Invoke(); // Notifies listeners of falling below danger level

            }
        }

        if (PoolCurr <= 0)
        {
            justEmptied = true;
        }

        PoolCurr = Math.Clamp(PoolCurr, 0, PoolMax);
        return justEmptied;
    }





    //public void ResetPool()
    //{

    //}
}
