using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Script for managing a health bar's fill percent, and doing so automatically if representing a pool class' value
public class HealthBarController : MonoBehaviour
{
    [System.Serializable]
    class UpdateBarGroup
    {
        public string PoolName = "";
        public GameObject BarImageObject = null;
        [DoNotSerialize] public PoolContainer.Pool PoolToTrack = null;
    }



    // Private variables
    [SerializeField] List<UpdateBarGroup> mBarsToShow = new List<UpdateBarGroup>();
    [SerializeField] TextMeshProUGUI mPlayerNameTextObject;
    [SerializeField] Image CharacterPortraitObject;

    //float mCurrValue = 0.0f;


    // Update is called once per frame
    void Update()
    {
        foreach (UpdateBarGroup barToShow in mBarsToShow)
        {
            if (barToShow.PoolToTrack == null)
            {
                continue;
            }
            // Set the current value to the pool's current fill percentage

            float currValue = barToShow.PoolToTrack.PoolValue / barToShow.PoolToTrack.PoolMaxValue;

            barToShow.BarImageObject.GetComponent<Image>().fillAmount = currValue;
        }

    }

    // Public Interface /////////////////////////////////////////////////////////////////////////////////
    // Set which pool this bar will show the percentage of (health, gas, etc.)
    public void SetPoolToRepresent(PoolContainer.Pool poolToUse)
    {
        UpdateBarGroup barGroup = mBarsToShow.Find((UpdateBarGroup currBarGroup) => { return currBarGroup.PoolName.CompareTo(poolToUse.PoolName) == 0; });

        if (barGroup == null)
        {
            print("HealthBarController:SetPoolToRepresent: No bar group could be found for pool named " + poolToUse.PoolName);
            return;
        }

        barGroup.PoolToTrack = poolToUse;
    }

    //// Set the bar fill percent manually. If a pool has been set for this bar to represent, the pool's value will overwrite this manual value
    //public void ManualSetFillPercent(float newValue)
    //{
    //    mCurrValue = newValue;
    //}

    public void SetPlayerNameText(string newNameText)
    {
        mPlayerNameTextObject.text = newNameText;
    }

    public void SetPlayerPortrait(CharacterDataManager.Characters characterToBe)
    {
        CharacterVisuals.CharacterVisualData characterInfo = PersistentScopeManagers.Instance.GetComponent<CharacterDataManager>().GetCharacterVisualData().GetCharacterVisuals(characterToBe);
        CharacterPortraitObject.sprite = characterInfo.HealthBarPortrait;
    }

}
