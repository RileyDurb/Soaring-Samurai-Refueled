using AudioEvents;
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

    List<AudioSource> mSpawnedAudioSources = new List<AudioSource>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = mSpawnedAudioSources.Count - 1; i >= 0; i--)
        {
            // If source has stopped playing
            if (mSpawnedAudioSources[i].isPlaying == false)
            {
                Destroy(mSpawnedAudioSources[i].gameObject);

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
                mSpawnedAudioSources.Add(audioComp); // Add to tracked list of spawned audio sources

                audioComp.resource = resourceToUse;
                audioComp.Play();

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
