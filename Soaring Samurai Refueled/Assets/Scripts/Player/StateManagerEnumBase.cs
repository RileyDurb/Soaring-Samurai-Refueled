using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine;

public class StateManagerEnum <T> : MonoBehaviour where T : Enum
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

        [NonSerialized]
        public GameObject mParentObject;

        // Public functions

        public State(T stateName) => mName = stateName; // Make derived classes define a constructor that sets the state name, so each state is unique
        public virtual void OnEnter() { }
        public virtual void OnUpdate(float dt) { }
        public virtual void OnExit() { }


        // Private Variables ///////////////////////////////////////////////////

    }

    // Public variables

    [NonSerialized]
    public List<State> mStateList = new List<State>();

    public List<State> mStateInfoList = new List<State>();
    //public Dictionary<T, State>
    public T mStartingState;

    public T CurrStateName
    {
        get { return mCurrState.mName; }
    }

    // Private Varianles
    State mCurrState;
    float mCurrStateTimer = -1;
    T mDoneStateName;

    // Start is called before the first frame update
    protected virtual void Awake()
    {

        // Gives each state it's parent context
        foreach (State currState in mStateList)
        {
            currState.mParentObject = this.gameObject;
        }

        //// Set Starting state
        //if (Enum.IsDefined(mStartingState.GetType(), mStartingState)) // if starting state not blank
        //{
        //    EnterState(mStartingState);
        //}
        //else // If no starting state given
        //{
        //    if (mStateList.Count == 0)
        //    {
        //        print("StateManagerStart: State Manager on object " + name + " has no states at initialization");
        //        mCurrState = null;
        //    }
        //    else
        //    {
        //        mCurrState = mStateList[0];
        //    }
        //}
    }

    protected virtual void Start()
    {
        // Set Starting state
        if (Enum.IsDefined(mStartingState.GetType(), mStartingState)) // if starting state not blank
        {
            EnterState(mStartingState);
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
                    EnterState(mDoneStateName, default);
                }
                else
                {
                    print("StateManager(" + name + "): State timer elapsed, but could not enter the done state");
                }

                // Reset variables
                mDoneStateName = default;
                mCurrStateTimer = -1;
            }
        }

        mCurrState.OnUpdate(Time.deltaTime);
    }

    // Interface functions ////////////////////////////////////////////////////////////////////////////////////
    public bool CanEnterState(T newStateName)
    {
        return mCurrState.mStatesCancellableInto.Contains(newStateName);
    }

    // Enters the give state, if any
    // If given a poisitive time, and done state name, sets a timer that will return to state when elapsed
    public void EnterState(T newStateName, float stateTime = -1, T doneStateName = default)
    {
        State newState = GetState(newStateName);

        if (newState == null) // Check if state exists
        {
            print("StateManager(" + name + "):EnterState; State of name" + newStateName + " does not exist");
            return;
        }

        // State is valid, call events

        // Call exit event/ function on last state, if any (could be no previous state if this is the first state set)
        if (mCurrState != null)
        {
            mCurrState.OnExit();

            if (mCurrState.mOnStateExitEvent != null)
            {
                mCurrState.mOnStateExitEvent.Invoke(newStateName);
            }
        }


        State prevState = mCurrState; // Save previous state
        
        mCurrState = newState;                   // Set new state


        // Call on enter event/ function

        mCurrState.OnEnter();
        if (mCurrState.mOnStateEnterEvent != null)
        {
            // Gets previous state if any
            T prevStateName = default;
            if (prevState != null)
            {
                prevStateName = prevState.mName;
            }

            mCurrState.mOnStateEnterEvent.Invoke(prevStateName);
        }





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

    public bool HasState(T stateName)
    {
        if (mStateList.Count == 0)
        {
            return false;
        }

        State foundState = mStateList.Find(state => state.mName.CompareTo(stateName) == 0);

        if (foundState == null)
        {
            return false;
        }
        else
        {
            return true;
        }

    }
    public State GetState(T stateName)
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

    // Helper functions //////////////////////////////////////////////////////////////////////////////////////
}
