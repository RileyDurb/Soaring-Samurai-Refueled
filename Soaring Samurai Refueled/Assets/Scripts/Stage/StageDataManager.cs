using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageDataManager : MonoBehaviour
{
    // Public definitions
    [System.Serializable]
    public class StageInfo
    {
        public string Name;
        public StageStats StatsObject;
    }

    // public variables
    [SerializeField] public StageStats mStageStats;


    [SerializeField] private List<StageInfo> mAvailableStages = new List<StageInfo>();

    public Action<StageInfo> mOnStageChanged;


    public List<StageInfo> GetAvailableStages()
    {
        return mAvailableStages;
    }

    // Tries to set stage to stage with matching name, and returns whether the set was successful
    public bool SetStage(string newStageName)
    {
  
        StageInfo targetStage = mAvailableStages.Find((StageInfo currStage) => { return currStage.Name == newStageName; });
        if (targetStage == null)
        {
            print("StageDataManager:SetStage: Stage of name " + newStageName + " could be found.");
            return false;
        }

        mStageStats = targetStage.StatsObject;

        if (mOnStageChanged != null)
        {
            mOnStageChanged.Invoke(targetStage);
        }

        return true; // Stage exists and was set, return success
    }
}
