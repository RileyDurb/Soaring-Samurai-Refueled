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
    DashAttack,
    Defeated,
    Flinch
}

public class StateManagerPlayer : StateManagerEnum<PlayerStates>
{
    protected override void Awake()
    {
        // Create any states that are have specfiic code NOTE: Make sure states are added in the same order as the enum, so they get named properly
        mStateList.Add(new State_Ready());
        mStateList.Add(new State_Defeated());
        mStateList.Add(new State_DashAttack());
        mStateList.Add(new State_Dash());
        mStateList.Add(new State_Flinch());

        // Copies info for each state into the derived class
        foreach (PlayerStates currState in Enum.GetValues(typeof(PlayerStates)))
        {
            // If this slot hasn't been filled with a state, create a default one for it
            if (HasState(currState) == false)
            {
                mStateList.Add(new State(currState));
            }

            GetState(currState).mStatesCancellableInto = mStateInfoList[(int)currState].mStatesCancellableInto;
        }

        base.Awake();
    }
}
