using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerStates
{ 
    
    Ready,
    SlashAttack,
    Dash,
    DashAttack
}

public class StateManagerPlayer : StateManagerEnum<PlayerStates>
{
    protected override void Awake()
    {
        // Create any states that are have specfiic code
        mStateList.Add(new State_Ready());

        // Copies info for each state into the derived class
        foreach (PlayerStates currState in Enum.GetValues(typeof(PlayerStates)))
        {
            // If this slot hasn't been filled with a state, create a default one for it
            if (mStateList.Count <= (int)currState)
            {
                mStateList.Add(new State());
            }
            mStateList[(int)currState].mName = currState;
            mStateList[(int)currState].mStatesCancellableInto = mStateInfoList[(int)currState].mStatesCancellableInto;
        }

        base.Awake();
    }
}
