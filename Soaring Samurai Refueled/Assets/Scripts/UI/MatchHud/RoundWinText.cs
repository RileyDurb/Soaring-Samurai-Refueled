using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundWinText : MonoBehaviour
{
    // Private variables 
    [SerializeField] RoundWinTextVisuals mStats;
    [SerializeField] GameObject mTextObject;

    ActionList mActionList = new ActionList();

    Vector3 mOGScale;

    // Start is called before the first frame update
    void Start()
    {
        TriggerStartAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        mActionList.Update(Time.deltaTime);
        if (Input.GetKeyUp(KeyCode.R))
        {
            mActionList.Clear();
            TriggerStartAnimation();
        }
    }

    void TriggerStartAnimation()
    {
        mOGScale = mTextObject.transform.localScale; // Saves original scale

        mTextObject.transform.localScale = new Vector3(mStats.StartingScaleMultiplier, mStats.StartingScaleMultiplier, mOGScale.z);
        mActionList.AddActionScale(mTextObject, new Vector3(mStats.EndingScaleMultiplier, mStats.EndingScaleMultiplier, mOGScale.z), mStats.EnterTime / 2.0f, 0.0f, mStats.ScaleEasingType);



        float screenHalfHeight = Camera.main.orthographicSize;
        mTextObject.transform.localPosition = new Vector3(0.0f, mStats.StartOffset, mTextObject.transform.position.z); // Start message off screen
        mActionList.AddActionLocalMove(mTextObject, new Vector3(0.0f, mStats.EndOffset, mTextObject.transform.position.z), mStats.EnterTime, 0.0f, mStats.MoveEasingType);

        mActionList.AddActionCallback(() => { TriggerFinishAnimation(); }, mStats.EnterTime + mStats.EnteredHoldTime);
    }

    public void TriggerFinishAnimation()
    {
        mTextObject.transform.localScale = new Vector3(mStats.ExitStartingScaleMultiplier, mStats.ExitStartingScaleMultiplier, mOGScale.z);
        mActionList.AddActionScale(mTextObject, new Vector3(mStats.ExitEndingScaleMultiplier, mStats.ExitEndingScaleMultiplier, mOGScale.z), mStats.ExitEnterTime / 2.0f, 0.0f, mStats.ExitScaleEasingType);



        float screenHalfHeight = Camera.main.orthographicSize;
        mTextObject.transform.localPosition = new Vector3(0.0f, mStats.ExitStartOffset, mTextObject.transform.position.z); // Start message off screen
        mActionList.AddActionLocalMove(mTextObject, new Vector3(0.0f, mStats.ExitEndOffset, mTextObject.transform.position.z), mStats.ExitEnterTime, 0.0f, mStats.ExitMoveEasingType);
        mActionList.AddActionCallback(() => { LevelScopeManagers.Instance.GetComponent<HUDManager>().RemoveInfoItem(gameObject); }, mStats.ExitEnterTime);
    }
}
