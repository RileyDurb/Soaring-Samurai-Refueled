using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour , IBufferableSetting
{
    [SerializeField] AudioManager.MixerType VolumeGroup;

    [SerializeField] Slider SliderRef;

    float mTempNewValue = -1.0f;
    bool mHasUnsavedChanges = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("Volume_" + VolumeGroup.ToString()))
        {
            float currVolumeValue = PlayerPrefs.GetFloat("Volume_" + VolumeGroup.ToString());
            SliderRef.SetValueWithoutNotify(currVolumeValue);
            mTempNewValue = currVolumeValue;
        }
        else
        {
            SliderRef.SetValueWithoutNotify(1.0f);
        }

        SliderRef.onValueChanged.AddListener(UpdateVolumeValue);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateVolumeValue(float newValue)
    {
        mTempNewValue = newValue;

        mHasUnsavedChanges = true;

        PersistentScopeManagers.Instance.GetComponent<AudioManager>().UpdateSoundGroupVolume(VolumeGroup, mTempNewValue); // Set volume on the mixer
    }

    private void ConfirmNewValue()
    {
        // Save value in player prefs
        PlayerPrefs.SetFloat("Volume_" + VolumeGroup.ToString(), mTempNewValue);

        PersistentScopeManagers.Instance.GetComponent<AudioManager>().UpdateSoundGroupVolume(VolumeGroup, mTempNewValue); // Set volume on the mixer

        mHasUnsavedChanges = false;

    }


    // Interface implementations

    public void ConfirmSettingChanges()
    {
        ConfirmNewValue();
    }

    public void UndoTemporarySettingsChanges()
    {
        if (mHasUnsavedChanges)
        {
            float currSavedVolumeLevel = PlayerPrefs.GetFloat("Volume_" + VolumeGroup.ToString(), 1.0f);

            PersistentScopeManagers.Instance.GetComponent<AudioManager>().UpdateSoundGroupVolume(VolumeGroup, currSavedVolumeLevel); // Set volume on the mixer
        }
    }
}
