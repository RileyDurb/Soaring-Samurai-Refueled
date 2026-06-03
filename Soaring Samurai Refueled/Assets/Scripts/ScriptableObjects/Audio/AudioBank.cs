using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioBank<T> : ScriptableObject where T : Enum
{
    [System.Serializable]
    public class SoundPackage
    {
        public T Name;
        public AudioResource[] AudioAssetOptions; // Audio to play, likely either audio clips or random containers
        [Header("Separate Audio Source Settings")]
        public bool UseSeparateAudioSource = false; // Whether to instantiate a prefab with an audio source to play the sound on. If this is false, audio assets MUST be audio clips
        public GameObject AudioSourcePrefab;
        public float Delay = 0.0f; // Optional delay to apply, if using the separate audio source
    }

    public List<SoundPackage> mSounds;
}
