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

}
