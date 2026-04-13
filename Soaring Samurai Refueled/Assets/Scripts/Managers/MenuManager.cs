using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Public class definitions
    public enum UILayer
    {
        HUD,
        Controls,
        Menu,
        Popups
    }

    // Private class definitions

    private class MenuLayer
    {
        public Stack<GameObject> mLayerItems = new Stack<GameObject>();
        public GameObject mLayerParentRef = null; // Parent to instantiate it's objects onto
        public List<int> mAdditiveLevelCounts = new List<int>();
    }

    [System.Serializable]
    private class UILayerSettings
    {
        public UILayer LayerToApplyTo;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
    }


    [SerializeField] List<UILayerSettings> LayerSettings;

    List<MenuLayer> mMenuLayers = new List<MenuLayer>();

    int mNumUIItems = 0;
    [SerializeField] bool mUseDebugNames = false;


    // References
    GameObject mCanvasRef;


    // Start is called before the first frame update
    void Awake()
    {
        // Save a reference to the canvas to put UI on to
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        if (canvases.Length <= 0)
        {
            print("MenuManager:Start: No canvas could be found, make sure one is in the scene so we can place menus onto it. Or, remove this menu manager if you don't want menus");
            return;
        }
        else
        {
            mCanvasRef = canvases[0].gameObject;
        }

        Transform currParentObjectTransform = null;

        // Creates a stack for each level of the menu
        foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
        {
            // Creates a new layer
            MenuLayer newLayer = new MenuLayer();
            mMenuLayers.Add(newLayer);

            // Finds the parent under the canvas to put all ui on this layer on, or creates a parent if one doesn't exist
            string layerParentName = layer.ToString() + "Parent";
            currParentObjectTransform = mCanvasRef.transform.Find(layerParentName);

            if (currParentObjectTransform != null)
            {
                newLayer.mLayerParentRef = currParentObjectTransform.gameObject;
            }
            else
            {
                // Creates a new object under the canvas to be the parent of the layer that we just created
                newLayer.mLayerParentRef = new GameObject(layerParentName, typeof(RectTransform));
                newLayer.mLayerParentRef.transform.SetParent(mCanvasRef.transform, false);
            }

            // Apply layer settings if any
            UILayerSettings layerSettings = LayerSettings.Find((UILayerSettings settings) => { return settings.LayerToApplyTo == layer; });
            if (layerSettings != null)
            {
                RectTransform layerTransform = newLayer.mLayerParentRef.GetComponent<RectTransform>();
                layerTransform.anchorMin = layerSettings.AnchorMin;
                layerTransform.anchorMax = layerSettings.AnchorMax;
                layerTransform.pivot = layerSettings.Pivot;
            }
        }




    }

    // Helper functions ///////////////////////////////////////////////////////////////////////////////////////////////////

    // Creates an object from the given prefab, and puts it as the front of the menu layer
    public GameObject PushItem(UILayer layer, GameObject uiPrefabToUse, bool additiveToTopLayer = false)
    {
        mNumUIItems++;

        // Spawms the new object as a child of the menu layer parent
        GameObject newUIObject = Instantiate(uiPrefabToUse, mMenuLayers[(int)layer].mLayerParentRef.transform);
        if (mUseDebugNames)
        {
            newUIObject.name = newUIObject.name + "UI " + mNumUIItems.ToString();
        }

        // Handles adding an additive item to the current level, or moving past the current level and hiding everything in the previous one
        List<int> additiveCounts = mMenuLayers[(int)layer].mAdditiveLevelCounts;
        if (additiveToTopLayer == true)
        {
            if (additiveCounts.Count == 0)
            {
                additiveCounts.Add(1);
            }
            else
            {
                additiveCounts[additiveCounts.Count - 1]++; // Increments the count of the number of additive items for this level in the layer
            }
        }
        else
        {
            // Hide previous top layer if any
            if (mMenuLayers[(int)layer].mLayerItems.Count > 0)
            {
                for (int i = additiveCounts[additiveCounts.Count - 1]; i > 0; i--) // For however many items were in the previous level
                {
                    GameObject currItemToHide = mMenuLayers[(int)layer].mLayerItems.ElementAt(i - 1);
                    currItemToHide.SetActive(false);
                }
            }

            mMenuLayers[(int)layer].mAdditiveLevelCounts.Add(1); // Add an empty additive count for this new level

        }

        // Adds new UI item to the layer
        mMenuLayers[(int)layer].mLayerItems.Push(newUIObject);

        return newUIObject;
    }


    // Removes and destroys top level of the UI layer
    public void PopItem(UILayer layer)
    {
        List<int> additiveCounts = mMenuLayers[(int)layer].mAdditiveLevelCounts;

        // For all the UI elements added in the current top level
        for (int i = additiveCounts[additiveCounts.Count - 1]; i > 0; i--)
        {
            // Destroys and removes the current UI item
            GameObject currTopItem = mMenuLayers[(int)layer].mLayerItems.Peek();

            mMenuLayers[(int)layer].mLayerItems.Pop();

            Destroy(currTopItem);

            mNumUIItems--; // Update number of UI items
        }

        additiveCounts.RemoveAt(additiveCounts.Count - 1); // Removes the additive count for this level


        // Activate new top level if any
        if (mMenuLayers[(int)layer].mLayerItems.Count > 0)
        {
            int numItemsInCurrentLevel = additiveCounts[additiveCounts.Count - 1];

            for (int i = additiveCounts[additiveCounts.Count - 1]; i > 0; i--) // For however many items were in the previous level
            {
                GameObject currItemToHide = mMenuLayers[(int)layer].mLayerItems.ElementAt(i - 1);
                currItemToHide.SetActive(true);
            }
        }
    }


    public int NumItemsInLayer(UILayer layer)
    {
        return mMenuLayers[(int)layer].mLayerItems.Count;
    }


    // Public usage functions ///////////////////////////////////////////////////////////////////////////////////////////


    // Creates an UI object from the given prefab. By default, puts it in the front of the menu layer
    public GameObject PushMenu(GameObject uiPrefabToUse, bool addToCurrentLayerLevel = false)
    {
        return PushItem(UILayer.Menu, uiPrefabToUse);
    }

    // Removes and destroys top item on the menu layer
    public void PopMenu()
    {
        PopItem(UILayer.Menu);
    }

    // Creates an UI object from the given prefab. By default, puts it active with the top level of the Controls UI layer
    public GameObject PushControls(GameObject controlsPrefabToUse, bool addToCurrentLayerLevel = true)
    {
        return PushItem(UILayer.Controls, controlsPrefabToUse, addToCurrentLayerLevel);
    }


    public void PopControls()
    {
        PopItem(UILayer.Controls);
    }

    // Creates an UI object from the given prefab. By default, puts it active with the top level of the HUD layer
    public GameObject PushHUDItem(GameObject hudUIPrefabToUse, bool addToCurrentLayerLevel = true)
    {
        return PushItem(UILayer.HUD, hudUIPrefabToUse, addToCurrentLayerLevel);
    }

    // Creates an UI object from the given prefab. By default, puts it above the current top layer and hides the previous top
    public GameObject PushPopup(GameObject controlsPrefabToUse, bool addToCurrentLayerLevel = false)
    {
        return PushItem(UILayer.Popups, controlsPrefabToUse, addToCurrentLayerLevel);
    }

    public void PopPopup()
    {
        PopItem(UILayer.Popups);
    }

    // Returns number of items in the menu layer
    public int NumItemsInMenu()
    {
        return NumItemsInLayer(UILayer.Menu);
    }




}
