using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuddenDeathText : MonoBehaviour
{
    // public variables
    [SerializeField] SuddenDeathMessageAesthetics mStats;
    [SerializeField] GameObject mTextObject;
    // Private Variables
    ActionList mSuddenDeathMessageActionList = new ActionList();

    // Start is called before the first frame update
    void Start()
    {
        mSuddenDeathMessageActionList.AddActionFadeCanvasObject(mTextObject, 0.0f, mStats.FadeOutTime, 0.0f, mStats.FadeOutEasing, true);
        mSuddenDeathMessageActionList.AddActionFadeCanvasObject(mTextObject, 1.0f, mStats.FadeInTime, mStats.FadeOutTime, mStats.FadeInEasing, true);


    }

    private void Update()
    {
        mSuddenDeathMessageActionList.Update(Time.deltaTime);
    }

}
