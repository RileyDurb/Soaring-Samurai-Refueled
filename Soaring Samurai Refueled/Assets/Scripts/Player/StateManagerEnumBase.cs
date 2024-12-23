using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine;

public class StateManagerEnum <T> : MonoBehaviour where T : struct, System.Enum
{

    [System.Serializable]
    public class State
    {
        // Public Definitions ////////////////////////////////////////////////////
        public delegate void OnStateEnterCallback(T previousState);
        public delegate void OnStateExitCallback(T nextState);

        // Editor Accessible variables ///////////////////////////////////////////
        // State definition
        public T mName;
        public List<T> mStatesCancellableInto = new List<T>();

        // Events
        public OnStateEnterCallback mOnStateEnterEvent;
        public OnStateExitCallback mOnStateExitEvent;

        // Private Variables ///////////////////////////////////////////////////
    }

    // Public variables


    public List<State> mStateList;
    public T mStartingState;

    T mEnumTypeZero;

    public T CurrStateName
    {
        get { return mCurrState.mName; }
    }

    // Private Varianles
    State mCurrState;
    float mCurrStateTimer = -1;
    T mDoneStateName;

    // Start is called before the first frame update
    void Start()
    {
        // Set Starting state
        if (mStartingState.CompareTo(mEnumTypeZero) != 0) // if starting state not blank
        {
            mCurrState = GetState(mStartingState);
        }
        else // If no starting state given
        {
            if (mStateList.Count == 0)
            {
                print("StateManagerStart: State Manager on object " + name + " has no states at initialization");
                mCurrState = null;
            }
            else
            {
                mCurrState = mStateList[0];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mCurrStateTimer >= 0)
        {
            mCurrStateTimer -= Time.deltaTime;

            if (mCurrStateTimer <= 0)
            {
                if (CanEnterState(mDoneStateName))
                {
                    // Enter done state
                    EnterState(mDoneStateName, mEnumTypeZero);
                }
                else
                {
                    print("StateManager(" + name + "): State timer elapsed, but could not enter the done state");
                }

                // Reset variables
                mDoneStateName = mEnumTypeZero;
                mCurrStateTimer = -1;
            }
        }
    }

    // Interface functions ////////////////////////////////////////////////////////////////////////////////////
    public bool CanEnterState(T newStateName)
    {
        return mCurrState.mStatesCancellableInto.Contains(newStateName);
    }

    // Enters the give state, if any
    // If given a poisitive time, and done state name, sets a timer that will return to state when elapsed
    public void EnterState(T newStateName, T doneStateName, float stateTime = -1 )
    {
        State newState = GetState(newStateName);

        if (newState == null) // Check if state exists
        {
            print("StateManager(" + name + "):EnterState; State of name" + newStateName + " does not exist");
            return;
        }

        // State is valid, call events
        if (mCurrState.mOnStateExitEvent != null)
            mCurrState.mOnStateExitEvent.Invoke(newStateName);

        T prevStateName = mCurrState.mName; // Save previous name
        mCurrState = newState;                   // Set new state

        if (mCurrState.mOnStateEnterEvent != null)
            mCurrState.mOnStateEnterEvent.Invoke(prevStateName);



        // Set state timer, if given a time
        if (stateTime > 0)
        {
            mCurrStateTimer = stateTime;
            mDoneStateName = doneStateName;
        }
    }

    // Getters and setters ///////////////////////////////////////////////////////////////////////////////////
    public void AddOnEnter(T stateName, State.OnStateEnterCallback callback)
    {
        State targetState = GetState(stateName);

        if (targetState == null)
        {
            print("AddOnEnter(" + name + "): State of name " + stateName + "could not be found");
            return;

        }
        targetState.mOnStateEnterEvent += callback;
    }



    public void AddOnExit(T stateName, State.OnStateExitCallback callback)
    {
        State targetState = GetState(stateName);

        if (targetState == null)
        {
            print("AddOnExit(" + name + "): State of name " + stateName + "could not be found");
            return;

        }
        targetState.mOnStateExitEvent += callback;
    }


    // Helper functions //////////////////////////////////////////////////////////////////////////////////////
    State GetState(T stateName)
    {
        if (mStateList.Count == 0)
        {
            print("GetState: State of name " + stateName + "could not be found on object named " + name + ", state list was empty");
            return null;
        }

        State foundState = mStateList.Find(state => state.mName.CompareTo(stateName) == 0);

        if (foundState == null)
        {
            print("GetState: State of name " + stateName + " could not be found on object named " + name + ". Though other states exist");
        }

        return foundState;
    }
}
