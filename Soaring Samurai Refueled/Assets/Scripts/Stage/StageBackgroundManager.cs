using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageBackgroundManager : MonoBehaviour
{
    // NOTE: Not used, previous WIP stuff, probably will use this for the bakground manager, but do differently
    enum BackgroundMode
    {
        StillImage,
        Panorama
    }


    // Private Variables ///////////////////////////////////////////////

    [SerializeField] BackgroundMode mCurrBackgroundmode = BackgroundMode.StillImage;

    [SerializeField] List<GameObject> panoramaTextureObjects;
    [SerializeField] List<float> panoramaTextureXOffsets;

    [SerializeField] GameObject mBackgroundImageObject = null;

    StageStats mStats;

    SpriteRenderer mBackgroundSpriteComp;

    float mCurrBackgroundScale = -1.0f;


    // /////////////////////////////////////////////////////////////////
    // Start is called before the first frame update
    void Start()
    {
        mStats = GetComponent<StageDataManager>().mStageStats;
        if (mBackgroundImageObject != null)
        {
            mBackgroundSpriteComp = mBackgroundImageObject.GetComponent<SpriteRenderer>();
            mBackgroundSpriteComp.sprite = mStats.BackgroundImage;
            mBackgroundImageObject.transform.localScale = Vector2.one * mStats.BackgroundImageScale;
            mCurrBackgroundScale = mStats.BackgroundImageScale;
        }

        GetComponent<StageDataManager>().mOnStageChanged += UpdateInfoFromStageChange;
        //Vector3 centerPanoramaTexturePos = panoramaTextureObjects[panoramaTextureObjects.Count / 2].transform.position;

        //// T
        //for (int i = 0; i < panoramaTextureObjects.Count; i++)
        //{
        //    GameObject currTextureObject = panoramaTextureObjects[i];

        //    panoramaTextureXOffsets[i] = centerPanoramaTexturePos.x - currTextureObject.transform.position.x;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        // Check for background changes
        if (mBackgroundSpriteComp.sprite != mStats.BackgroundImage)
        {
            mBackgroundSpriteComp.sprite = mStats.BackgroundImage; // Set new background image
            mBackgroundImageObject.transform.localScale = Vector2.one * mStats.BackgroundImageScale; // Set new background image scale
        }

        if (mCurrBackgroundScale != mStats.BackgroundImageScale)
        {
            mBackgroundImageObject.transform.localScale = Vector2.one * mStats.BackgroundImageScale; // Set new background image scale

        }
    }

    void InitPanoramaMode()
    {

    }

    void UpdateInfoFromStageChange(StageDataManager.StageInfo newStage)
    {
        mStats = newStage.StatsObject;
    }
}
