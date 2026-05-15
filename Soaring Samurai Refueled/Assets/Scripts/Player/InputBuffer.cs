using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBuffer : MonoBehaviour
{
    // Class and enum definitions
    public enum BufferTrackedInputs
    {
        Move
    }

    public delegate bool VectorInputCheckFunc(Vector2 other, Vector2 yourVec);

    public class InputState
    {
        public InputState(Vector2 inputValue)
        {
            mInputVectorValue = inputValue;
        }

        public InputState(bool inputActive)
        {
            mInputActiveValue = inputActive;
        }

        public Vector2 mInputVectorValue;
        public bool mInputActiveValue;
    }

    public class InputBufferSnapshot
    {
        public Dictionary<BufferTrackedInputs, InputState> mCurrInputs = new Dictionary<BufferTrackedInputs, InputState>();
        public float mTimestamp = -1.0f;
    }

    // Private variables
    [SerializeField] InputBufferStats mStats;

    ActionList mInputBufferActionList = new ActionList();

    PlayerCombatController mCombatController;

    List<InputBufferSnapshot> mMainInputBuffer = new List<InputBufferSnapshot>();

    // Start is called before the first frame update
    void Start()
    {
        mCombatController = GetComponent<PlayerCombatController>();

        // Record first inputs
        RecordMoveInput();
        ClearOldInputs();

        mInputBufferActionList.AddActionCallback(() => { RecordMoveInput(); ClearOldInputs(); }, mStats.RecordFrequency, false, true); // Set to record inputs at a set frequency
    }

    // Update is called once per frame
    void Update()
    {
        mInputBufferActionList.Update(Time.deltaTime);
    }

    void RecordMoveInput()
    {
        InputState input = new InputState(mCombatController.CurrMoveInput);
        input.mInputActiveValue = mCombatController.CurrMoveInput == Vector2.zero ? true : false;

        InputBufferSnapshot currSnapshot = new InputBufferSnapshot();
        currSnapshot.mCurrInputs.Add(BufferTrackedInputs.Move, input);
        currSnapshot.mTimestamp = Time.timeSinceLevelLoad;

        // Adds the input snapshot to the input buffer
        mMainInputBuffer.Add(currSnapshot);
    }

    void ClearOldInputs()
    {
        if (mMainInputBuffer.Count <= 0)
        {
            return;
        }

        float currLatestSnapshotTime = mMainInputBuffer[0].mTimestamp;

        // While the oldest input is older than max buffer time
        while (Time.timeSinceLevelLoad - currLatestSnapshotTime > mStats.MaxBufferTimeLength)
        {
            // Remove oldest input
            mMainInputBuffer.RemoveAt(0);
            currLatestSnapshotTime = mMainInputBuffer[0].mTimestamp;
        }
        // NOTE, could potentially optimize this to find the oldest valid input, and remove all inputs older than that at once, but may not be significant or even better
    }

    // Public interface 
    // Returns if all the vector inputs from the given input type, and in the set period, succeed the predicate checkl
    public bool PreviousVectorInputsSucceedCondition(BufferTrackedInputs inputType, float timePeriodToCheckOver, Vector2 vectorToCheck, VectorInputCheckFunc predicate)
    {
        float currTime = Time.timeSinceLevelLoad;

        bool conditionSuccess = true;

        for (int i = mMainInputBuffer.Count; i > 0; i--)
        {
            // if we're past the target time period
            if (currTime - mMainInputBuffer[i].mTimestamp > timePeriodToCheckOver)
            {
                break; // Finish checking
            }

            conditionSuccess = predicate(mMainInputBuffer[i].mCurrInputs[inputType].mInputVectorValue, vectorToCheck);

            // If condition fails, break out, we know we'll return false
            if (conditionSuccess == false)
            {
                break;
            }
        }

        return conditionSuccess;
    }

    // Returns if all the vector inputs from the given input type, and in the set period, succeed the predicate checkl
    public bool IsFlickingStick(BufferTrackedInputs inputType, Vector2 vectorToCheck)
    {
        float currTime = Time.timeSinceLevelLoad;

        bool conditionSuccess = true;

        for (int i = mMainInputBuffer.Count - 1; i > 0; i--)
        {
            // if we're past the target time period
            if (currTime - mMainInputBuffer[i].mTimestamp > mStats.FlickCheckTimeWindow)
            {
                break; // Finish checking
            }

            InputBufferSnapshot currInputFrame = mMainInputBuffer[i];
            float currSpeed = (vectorToCheck - currInputFrame.mCurrInputs[inputType].mInputVectorValue).magnitude / (currTime - currInputFrame.mTimestamp);
            //Debug.Log("Curr stick change speed: " + currSpeed);

            conditionSuccess = currSpeed >= mStats.MinFlickSpeed;

            // If condition fails, break out, we know we'll return false
            if (conditionSuccess == false)
            {
                break;
            }
        }

        return conditionSuccess;
    }
}
