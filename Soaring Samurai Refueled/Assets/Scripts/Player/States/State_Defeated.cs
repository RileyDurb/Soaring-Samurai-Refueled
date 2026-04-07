using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Defeated : StateManagerPlayer.State
{
    public State_Defeated() : base(PlayerStates.Defeated) { }

    public override void OnEnter()
    {
        mParentObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnExit()
    {
        mParentObject.GetComponent<SpriteRenderer>().enabled = true;
    }
}
