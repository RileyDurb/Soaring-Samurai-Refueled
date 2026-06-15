using AudioEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    // Public definitions
    public enum MixerType
    {
        Master,
        SFX,
        Music,
        Ambience
    }

    // Proivate definitions



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

    [System.Serializable]
    class AudioMixerPackage
    {
        public MixerType SoundGroupType;
        public AudioMixerGroup GroupAsset;
    }


    [SerializeField]
    AudioSource mAudioSourceRef;

    [SerializeField]
    MainAudioBank mMainAudioBank;

    // Public variables

    //public SerializableDictionary<MixerType, AudioMixerGroup> Mixers = new SerializableDictionary<MixerType, AudioMixerGroup>();
    [SerializeField] List<AudioMixerPackage> Mixers = new List<AudioMixerPackage>();

    // Private variables

    List<SpawnedAudioSourcePackage> mSpawnedAudioSources = new List<SpawnedAudioSourcePackage>();

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize audio settings to default values, or saved values if there are any
        foreach (AudioMixerPackage mixerPackage in Mixers)
        {
            float currSavedVolumeLevel = PlayerPrefs.GetFloat("Volume_" + mixerPackage.SoundGroupType.ToString(), 1.0f);

            UpdateSoundGroupVolume(mixerPackage.SoundGroupType, currSavedVolumeLevel); // Set volume on the mixer
        }
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

    // Helper functions
    private AudioMixerPackage GetMixer(MixerType groupType)
    {
        AudioMixerPackage foundMixer = Mixers.Find((AudioMixerPackage other) => { return other.SoundGroupType == groupType; });

        if (foundMixer == null)
        {
            Debug.Log("AudioManager:GetMixer: mixer of type " + groupType.ToString() + " was not found. Make sure it is set up on the audio manager component's prefab");
        }

        return foundMixer;
    }

    // Public interface

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

                audioComp.outputAudioMixerGroup = GetMixer(foundSound.SoundGroup).GroupAsset; // Set the mixer type for the sound to use
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
                    mAudioSourceRef.outputAudioMixerGroup = GetMixer(foundSound.SoundGroup).GroupAsset; // Set the mixer type for the sound to use
                    mAudioSourceRef.PlayOneShot(resourceAsAudioClip);
                }
            }

        }
    }

    // Update sound group volume

    public void UpdateSoundGroupVolume(MixerType soundGroup, float newVolume)
    {
        GetMixer(soundGroup).GroupAsset.audioMixer.SetFloat("Volume_" + soundGroup.ToString(), Mathf.Log10(newVolume) * 20.0f); // Convert 0-100 volume setting to -80 db to 20 db units that the mixer uses
    }

}
