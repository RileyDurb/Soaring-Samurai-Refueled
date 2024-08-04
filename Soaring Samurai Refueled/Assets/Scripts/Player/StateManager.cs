using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [System.Serializable]
    public class State
    {
        // Public Definitions ////////////////////////////////////////////////////
        public delegate void OnStateEnterCallback(string previousState);
        public delegate void OnStateExitCallback(string nextState);

        // Editor Accessible variables ///////////////////////////////////////////
        // State definition
        public string mName;
        public List<string> mStatesCancellableInto = new List<string>();

        // Events
        public OnStateEnterCallback mOnStateEnterEvent;
        public OnStateExitCallback mOnStateExitEvent;

        // Private Variables ///////////////////////////////////////////////////
    }

    public List<State> mStateList;
    public string mStartingState;

    // Private Varianles
    State mCurrState;
    float mCurrStateTimer = -1;
    string mDoneStateName;

    // Start is called before the first frame update
    void Start()
    {
        // Set Starting state
        if (mStartingState != null)
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
                    EnterState(mDoneStateName);
                }
                else
                {
                    print("StateManager(" + name + "): State timer elapsed, but could not enter the done state");
                }

                // Reset variables
                mDoneStateName = "";
                mCurrStateTimer = -1;
            }
        }
    }

    // Interface functions ////////////////////////////////////////////////////////////////////////////////////
    public bool CanEnterState(string newStateName)
    {
        return mCurrState.mStatesCancellableInto.Contains(newStateName);
    }

    // Enters the give state, if any
    // If given a poisitive time, and done state name, sets a timer that will return to state when elapsed
    public void EnterState(string newStateName, float stateTime = -1, string doneStateName = "")
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

        string prevStateName = mCurrState.mName; // Save previous name
        mCurrState = newState;                   // Set new state

        if (mCurrState.mOnStateEnterEvent != null)
            mCurrState.mOnStateEnterEvent.Invoke(prevStateName);



        // Set state timer, if given a time and state to enter when done
        if (stateTime > 0 && doneStateName != "")
        {
            mCurrStateTimer = stateTime;
            mDoneStateName = doneStateName;
        }
    }

    // Getters and setters ///////////////////////////////////////////////////////////////////////////////////
    public void AddOnEnter(string stateName, State.OnStateEnterCallback callback)
    {
        State targetState = GetState(stateName);

        if (targetState == null)
        {
            print("AddOnEnter(" + name + "): State of name " + stateName + "could not be found");
            return;

        }
        targetState.mOnStateEnterEvent += callback;
    }



    public void AddOnExit(string stateName, State.OnStateExitCallback callback)
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
    State GetState(string stateName)
    {
        if (mStateList.Count == 0)
        {
            print("GetState: State of name " + stateName + "could not be found on object named " + name + ", state list was empty");
            return null;
        }

        State foundState = mStateList.Find(state => state.mName == stateName);

        if (foundState == null)
        {
            print("GetState: State of name " + stateName + " could not be found on object named " + name + ". Though other states exist");
        }

        return foundState;
    }
}
