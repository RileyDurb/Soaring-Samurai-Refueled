using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    public class TimeScalePackage
    {
        public TimeScalePackage(float timeScaleValue, string timeScaleReason)
        {
            mTimeScaleValue = timeScaleValue;
            mTimeScaleReason = timeScaleReason;
        }
        public float mTimeScaleValue = 1.0f;
        public string mTimeScaleReason = "";
    };

    public ActionList mHitStops;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        mActionList.Update(Time.unscaledDeltaTime);

    }

    List<string> mActiveHitstops = new List<string>();
    List<TimeScalePackage> mActiveTimeScales = new List<TimeScalePackage>();
    ActionList mActionList = new ActionList();

    // Public interface
    public void AddHitstop(string hitstopReason, float duration)
    {
        mActiveHitstops.Add(hitstopReason);

        // If just went from no hitstops to one, pause time if any hitstops active
        if (mActiveHitstops.Count == 1)
        {
            Time.timeScale = 0.0f;
        }

        mActionList.AddActionCallback(() => { RemoveHitstop(hitstopReason); }, duration);
    }

    public void RemoveHitstop(string hitstopReason)
    {
        if (mActiveHitstops.Find((string currString)=>{ return currString == hitstopReason; }) != null)
        {
            mActiveHitstops.Remove(hitstopReason);

            // if just went to no hitstops, return time scale
            if (mActiveHitstops.Count == 0)
            {
                Time.timeScale = 1.0f;
            }
        }
    }

    public void AddTimeScale(float timeScaleValue, string timeScaleReason)
    {
        TimeScalePackage newTimeScale = new TimeScalePackage(timeScaleValue, timeScaleReason);
        mActiveTimeScales.Add(newTimeScale);

        // Apply current time scale
        Time.timeScale = timeScaleValue;
    }

    public void EditTimeScaleValue(float newTimeScaleValue, string timeScaleReason)
    {
        int targetIndex = mActiveTimeScales.FindIndex((TimeScalePackage currTimeScalePackage) => { return currTimeScalePackage.mTimeScaleReason == timeScaleReason; });

        // if the time scale we want exists
        if (targetIndex >= 0)
        {
            mActiveTimeScales[targetIndex].mTimeScaleValue = newTimeScaleValue;

            // If we're editing the active time scale
            if (targetIndex == mActiveTimeScales.Count - 1)
            {
                // Apply the new time scale value
                Time.timeScale = newTimeScaleValue;
            }
        }
    }
    public void RemoveTimeScale(string timeScaleReason)
    {
        TimeScalePackage targetTimeScale = mActiveTimeScales.Find((TimeScalePackage currTimeScalePackage) => { return currTimeScalePackage.mTimeScaleReason == timeScaleReason; });
        if (targetTimeScale != null)
        {
            mActiveTimeScales.Remove(targetTimeScale);

            // if there are no time scales left
            if (mActiveTimeScales.Count == 0)
            {
                // If any active hitstops, reapply that
                if (mActiveHitstops.Count > 0)
                {
                    Time.timeScale = 0.0f;
                }
                else // If no time manipulations left, return to normal time scalke
                {
                    Time.timeScale = 1.0f;
                }
            }
            else // If there are time scales left, reapply last time scale
            {
                Time.timeScale = mActiveTimeScales[mActiveTimeScales.Count - 1].mTimeScaleValue;
            }
        }
    }



}
