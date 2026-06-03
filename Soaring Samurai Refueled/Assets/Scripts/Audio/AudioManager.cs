using AudioEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{


    [SerializeField]
    AudioSource mAudioSourceRef;

    [SerializeField]
    MainAudioBank mMainAudioBank;

    class SpawnedAudioSourcePackage
    {
        public SpawnedAudioSourcePackage(AudioSource audioSource, float delay)
        {
            mAudioSource = audioSource;
            mDelayTime = delay;
        }

        public AudioSource mAudioSource;
        public float mCurrDelayTimer = 0.0f;
        public float mDelayTime = -1.0f;
    }

    List<SpawnedAudioSourcePackage> mSpawnedAudioSources = new List<SpawnedAudioSourcePackage>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = mSpawnedAudioSources.Count - 1; i >= 0; i--)
        {
            SpawnedAudioSourcePackage currAudioSource = mSpawnedAudioSources[i];

            if (currAudioSource.mCurrDelayTimer <= currAudioSource.mDelayTime)
            {
                currAudioSource.mCurrDelayTimer += Time.deltaTime;
            }
            // If source has stopped playing
            if (currAudioSource.mAudioSource.isPlaying == false && currAudioSource.mCurrDelayTimer > currAudioSource.mDelayTime)
            {
                Destroy(currAudioSource.mAudioSource.gameObject);

                // Destroy the object now that it's stopped playing
                mSpawnedAudioSources.RemoveAt(i);
            }
        }
    }

    public void PlayEvent(SoundEvent eventName)
    {
        AudioBank<SoundEvent>.SoundPackage foundSound = mMainAudioBank.mSounds.Find((AudioBank<SoundEvent>.SoundPackage soundPackage) => { return soundPackage.Name == eventName; });

        if (foundSound != null)
        {
            AudioResource resourceToUse = foundSound.AudioAssetOptions[MyRandom.RandomRange(0, foundSound.AudioAssetOptions.Length - 1)];
            if (foundSound.UseSeparateAudioSource)
            {
                GameObject newAudioSource = Instantiate(foundSound.AudioSourcePrefab, PersistentScopeManagers.Instance.GetComponent<AudioManager>().transform);
                AudioSource audioComp = newAudioSource.GetComponent<AudioSource>();
                mSpawnedAudioSources.Add(new SpawnedAudioSourcePackage(audioComp, foundSound.Delay)); // Add to tracked list of spawned audio sources

                audioComp.resource = resourceToUse;
                if (foundSound.Delay >=0)
                {
                    audioComp.PlayDelayed(foundSound.Delay);
                }
                else
                {
                    audioComp.Play();
                }

            }
            else // Just play the audio clip as a one shot (can only play audio clips
            {
                mAudioSourceRef.resource = resourceToUse;
                AudioClip resourceAsAudioClip = resourceToUse as AudioClip;
                if (resourceAsAudioClip != null)
                {
                    mAudioSourceRef.PlayOneShot(resourceAsAudioClip);
                }
            }

        }
    }
}
