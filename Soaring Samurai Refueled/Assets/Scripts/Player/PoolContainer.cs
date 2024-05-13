using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionary<keyType, valueType> : Dictionary<keyType,valueType>, ISerializationCallbackReceiver
{
    public void OnBeforeSerialize()
    {
       
    }
    public void OnAfterDeserialize()
    {

    }

    void OnGUI()
    {
        foreach (var kvp in  this)
        {
            GUILayout.Label("Key: " + kvp.Key + " value: " + kvp.Value);
        }
    }
}

public class PoolContainer : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        // Editor Accessible variables
        [SerializeField] string Name = "NameMe";
        [SerializeField] float PoolMax = 100;
        [SerializeField] float RegenPerSec = 0;
        [SerializeField] float RegenDelay = 0.0f; // Time before regen begins, this wait occurs each time the pool drops below max TODO: Experiment wit this if I use the functionality
        [SerializeField] float PoolCurr = 0;
        public float CriticalThresholdPercent = 0.5f;
        public Action RegenAboveCritical;
        public Action FallBelowCritical;
        
        // Private variables 
        float RegenDelayTimer = -1.0f; // Timer initialized to negative
        float lastPoolValue = 0.0f;


        public virtual void Init()
        {
            PoolCurr = PoolMax;
            RegenDelayTimer = RegenDelay;
        }

        public virtual void Update()
        {
            if (PoolCurr < PoolMax)
            {
                if (lastPoolValue >= PoolMax) // If newwly damaged
                {
                    RegenDelayTimer = RegenDelay; // Sets regen delay timer
                }

                if (RegenDelayTimer >= 0) // If regen timer active
                {
                    RegenDelayTimer -= Time.deltaTime;
                    if (RegenDelayTimer <= 0) // If timer is finished
                    {
                        RegenDelayTimer = -1.0f; // Turn off timer
                    }
                }

                // If no more delay
                if (RegenDelayTimer < 0)
                {
                    // Regen

                    float valueBeforeRegen = PoolCurr;
                    PoolCurr += RegenPerSec * Time.deltaTime;

                    // If the regen just caused the pool to rise above critical
                    if (PoolCurr > CriticalThresholdPercent * PoolMax)
                    {
                        if (RegenAboveCritical != null)
                        {
                            RegenAboveCritical.Invoke(); // Calls critical regen event
                        }
                    }

                    PoolCurr = Mathf.Clamp(PoolCurr, 0.0f, PoolMax);
                }
            }


            lastPoolValue = PoolCurr;
        }

        // Getters and setters
        public float PoolValue
        {
            get { return PoolCurr; }
            set { PoolCurr = Mathf.Clamp(value, 0.0f, PoolMax); }
        }

        public string PoolName
        { get { return Name; } }

        // Decreases pool by given amount, and returns whether the pool became empty by that decrease
        public virtual bool DecreasePool(float amount)
        {
            if (PoolCurr <= 0)
            {
                return false;
            }

            bool justEmptied = false;
            float valueBeforeModify = PoolCurr;
            PoolCurr -= amount;

            // If the decrease just caused the pool to drop below critical
            float criticalThreshold = CriticalThresholdPercent * PoolMax;
            if (valueBeforeModify > criticalThreshold && PoolCurr <= criticalThreshold)
            {
                if (FallBelowCritical != null)
                {
                    FallBelowCritical.Invoke(); // Calls event for falling below critical
                }
            }

            
            if (PoolCurr <= 0.0f)
            {
                justEmptied = true;
            }

            PoolCurr = Mathf.Clamp(PoolCurr, 0.0f, PoolMax);

            return justEmptied;
        }
    }

    [SerializeField]
    List<Pool> mPoolList = new List<Pool>();
    // Start is called before the first frame update
    void Start()
    {
        // Initialize each pool
        foreach (Pool pool in mPoolList)
        {
            pool.Init();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update each pool
        foreach (Pool pool in mPoolList)
        {
            pool.Update();
        }
    }

    // Getters and setters
    public Pool GetPool(string poolName)
    {
        if (mPoolList.Count == 0)
        {
            print("GetPool: Pool of name " + poolName + "could not be found on object named " + name + ", pool container was empty");
            return null;
        }
        Pool targetPool = mPoolList.Find(pool => pool.PoolName == poolName);

        if (targetPool == null)
        {
            print("GetPool: Pool of name " + poolName + " could not be found on object named " + name + ". Other Pools Exist");
        }

        return targetPool;
    }
}
