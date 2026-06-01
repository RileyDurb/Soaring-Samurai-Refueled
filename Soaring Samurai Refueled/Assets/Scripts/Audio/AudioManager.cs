using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public enum SoundEvent
    {
        PLAY_PLAYER_SWORDSWING
    }

    [SerializeField]
    AudioSource mAudioSourceRef;

    [SerializeField]
    MainAudioBank mMainAudioBank;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayEvent(SoundEvent eventName)
    {
        AudioBank<SoundEvent>.SoundPackage<SoundEvent> foundSound = mMainAudioBank.mSounds.Find((AudioBank<SoundEvent>.SoundPackage<SoundEvent> soundPackage) => { return soundPackage.Name == eventName; });

        if (foundSound != null)
        {
            mAudioSourceRef.PlayOneShot(foundSound.ClipOptions.ElementAt(MyRandom.RandomRange(0, foundSound.ClipOptions.Length - 1)));
        }
    }
}
