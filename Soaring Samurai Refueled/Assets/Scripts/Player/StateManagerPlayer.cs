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
    protected override void Start()
    {
        mStateList.Add(new State_Ready());

        // Copies info for each state into the derived class
        foreach (PlayerStates currState in Enum.GetValues(typeof(PlayerStates)))
        {
            mStateList[(int)currState].mName = currState;
            mStateList[(int)currState].mStatesCancellableInto = mStateInfoList[(int)currState].mStatesCancellableInto;
        }

        base.Start();
    }
}
