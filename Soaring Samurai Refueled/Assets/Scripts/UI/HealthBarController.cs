using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Script for managing a health bar's fill percent, and doing so automatically if representing a pool class' value
public class HealthBarController : MonoBehaviour
{


    // Private variables
    [SerializeField] GameObject mBarImageObject; // Game object that holds the health bar image
    PoolContainer.Pool mPoolToTrack;

    float mCurrValue = 0.0f;


    // Update is called once per frame
    void Update()
    {
        // If a pool has been set, set the current value to the pool's current fill percentage
        if (mPoolToTrack != null)
        {
            mCurrValue = mPoolToTrack.PoolValue / mPoolToTrack.PoolMaxValue;
        }

        mBarImageObject.GetComponent<Image>().fillAmount = mCurrValue;
    }

    // Public Interface /////////////////////////////////////////////////////////////////////////////////
    // Set which pool this bar will show the percentage of (health, gas, etc.)
    public void SetPoolToRepresent(PoolContainer.Pool poolToUse)
    {
        mPoolToTrack = poolToUse;
    }

    // Set the bar fill percent manually. If a pool has been set for this bar to represent, the pool's value will overwrite this manual value
    public void ManualSetFillPercent(float newValue)
    {
        mCurrValue = newValue;
    }
}
