using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    // Private class definitions ////////////////////////////////////////////////////////////////////////////////////
    private class HUDLayer
    {
        public List<GameObject> mHUDObjects = new List<GameObject>();
        public GameObject mLayerParent = null;
    }

    [System.Serializable]
    private class HUDLayerSettings
    {
        public HUDLayers LayerToApplyTo = HUDLayers.MatchInfo;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
    }

    public enum HUDLayers
    {
        MatchInfo,
        Controls
    }


    // Privatre variables ///////////////////////////////////////////////////////////////////////////////////////////

    [SerializeField] List<HUDLayerSettings> mHudLayerSettings = new List<HUDLayerSettings>();

    List<HUDLayer> mHUDLayers = new List<HUDLayer>();

    GameObject mCanvasRef;

    // Start is called before the first frame update
    void Awake()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        if (canvases.Length <= 0)
        {
            print("HUDManager:Start: No canvas could be found, make sure one is in the scene so we can place menus onto it. Or, remove this menu manager if you don't want menus");
            return;
        }
        else
        {
            mCanvasRef = canvases[0].gameObject;
        }


        Transform currParentObjectTransform;

        foreach (HUDLayers layer in Enum.GetValues(typeof(HUDLayers)))
        {
            // Creates new layer
            HUDLayer newLayer = new HUDLayer();
            mHUDLayers.Add(newLayer);

            // Finds parent if it exists, or creates it if not
            string hudParentName = layer.ToString() + "Parent";
            currParentObjectTransform = mCanvasRef.transform.Find(hudParentName);

            if (currParentObjectTransform != null)
            {
                newLayer.mLayerParent = currParentObjectTransform.gameObject;
            }
            else
            {
                // Creates a new object under the canvas to be the parent of the layer that we just created
                newLayer.mLayerParent = new GameObject(hudParentName, typeof(RectTransform));
                newLayer.mLayerParent.transform.SetParent(mCanvasRef.transform, false);
            }

            // Apply layer settings if any
            HUDLayerSettings layerSettings = mHudLayerSettings.Find((HUDLayerSettings settings) => { return settings.LayerToApplyTo == layer; });
            if (layerSettings != null)
            {
                RectTransform layerTransform = newLayer.mLayerParent.GetComponent<RectTransform>();
                layerTransform.anchorMin = layerSettings.AnchorMin;
                layerTransform.anchorMax = layerSettings.AnchorMax;
                layerTransform.pivot = layerSettings.Pivot;
            }

        }
    }

    // Helper functions ////////////////////////////////////////////////////////////////////////////////////////////////////
    GameObject AddHudItem(HUDLayers layer, GameObject UIPrefabToUse)
    {
        // Spanws a UI object from the given prefab
        GameObject spawnedObject = Instantiate(UIPrefabToUse, mHUDLayers[(int)layer].mLayerParent.transform);

        // Adds to hud list
        mHUDLayers[(int)layer].mHUDObjects.Add(spawnedObject);

        return spawnedObject;
    }

    void RemoveHudItem(HUDLayers layer, GameObject UIObjectRef)
    {
        GameObject objectToRemove = mHUDLayers[(int)layer].mHUDObjects.Find((GameObject objectToFind) => { return objectToFind == UIObjectRef; });

        if (objectToRemove != null)
        {
            mHUDLayers[(int)layer].mHUDObjects.Remove(UIObjectRef);
            Destroy(objectToRemove);
        }
    }

    // Public interface //////////////////////////////////////////////////////////////////////////////////////////////////
    public GameObject AddInfoItem(GameObject UIPrefabToUse)
    {
        return AddHudItem(HUDLayers.MatchInfo, UIPrefabToUse);
    }

    public void RemoveInfoItem(GameObject UIObjectToRemove)
    {
        RemoveHudItem(HUDLayers.MatchInfo, UIObjectToRemove);
    }

    public GameObject AddControlsItem(GameObject UIPrefabToUse)
    {
        return AddHudItem(HUDLayers.Controls, UIPrefabToUse);
    }

    public void RemoveControlsItem(GameObject UIObjectToRemove)
    {
        RemoveHudItem(HUDLayers.Controls, UIObjectToRemove);
    }
}
