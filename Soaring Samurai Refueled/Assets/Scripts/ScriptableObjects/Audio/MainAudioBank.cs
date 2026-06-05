using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioEvents;

namespace AudioEvents
{
    public enum SoundEvent
    {
        NONE,
        PLAY_PLAYER_SWORDSWING,
        PLAY_PLAYER_SWORDHIT,
        PLAY_PLAYER_DASHATTACK_SHEATHBEFORE,
        PLAY_PLAYER_DASHATTACK_SHEATHAFTER,
        PLAY_PLAYER_DASHATTACK_MOVEMENTSTART,
        PLAY_PLAYER_DASHATTACK_HIT,
        PLAYER_NORMALCLASHIMPACT,
        PLAYER_MOVEMENT_DASHSTART
    }
}

[CreateAssetMenu(fileName = "MainAudioBank", menuName = "Scripts/ScriptableObjects/Audio/MainAudioBank")]
public class MainAudioBank : AudioBank<SoundEvent>
{

}
