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

    // /////////////////////////////////////////////////////////////////
    // Start is called before the first frame update
    void Start()
    {
        Vector3 centerPanoramaTexturePos = panoramaTextureObjects[panoramaTextureObjects.Count / 2].transform.position;
        
        // T
        for (int i = 0; i < panoramaTextureObjects.Count; i++)
        {
            GameObject currTextureObject = panoramaTextureObjects[i];

            panoramaTextureXOffsets[i] = centerPanoramaTexturePos.x - currTextureObject.transform.position.x;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitPanoramaMode()
    {

    }
}
