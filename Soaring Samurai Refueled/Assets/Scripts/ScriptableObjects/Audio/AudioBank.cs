using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBank<T> : ScriptableObject where T : Enum
{
    [System.Serializable]
    public class SoundPackage<T>
    {
        public T Name;
        public AudioClip[] ClipOptions;
    }

    public List<SoundPackage<T>> mSounds;
}
