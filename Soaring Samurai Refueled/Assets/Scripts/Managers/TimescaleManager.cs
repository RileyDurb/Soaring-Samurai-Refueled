using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
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



}
