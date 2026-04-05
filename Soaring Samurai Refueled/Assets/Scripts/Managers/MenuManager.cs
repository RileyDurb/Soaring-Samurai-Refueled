using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Public class definitions
    public enum UILayer
    {
        HUD,
        Menu,
        Popups
    }

    // Private class definitions

    private class MenuLayer
    {
        public Stack<GameObject> mLayerItems = new Stack<GameObject>();
        public GameObject mLayerParentRef = null; // Parent to instantiate it's objects onto
    }



    List<MenuLayer> mMenuLayers = new List<MenuLayer>();

    GameObject mCanvasRef;



    // Start is called before the first frame update
    void Start()
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
                newLayer.mLayerParentRef = new GameObject(layerParentName);
                newLayer.mLayerParentRef.transform.SetParent(mCanvasRef.transform, false);
            }
        }




    }



    // Public usage functions


    // Creates an object from the given prefab, and puts it as the front of the menu layer
    public void PushMenu(GameObject uiPrefabToUse)
    {
        // Spawms the new object as a child of the menu layer parent
        GameObject newUIObject = Instantiate(uiPrefabToUse, mMenuLayers[(int)UILayer.Menu].mLayerParentRef.transform);

        mMenuLayers[(int)UILayer.Menu].mLayerItems.Push(newUIObject);
    }

    // Removes and destroys top item on the menu layer
    public void PopMenu()
    {
        // Destroys and removes top UI object
        GameObject currTopMenu = mMenuLayers[(int)UILayer.Menu].mLayerItems.Peek();

        mMenuLayers[(int)UILayer.Menu].mLayerItems.Pop();

        Destroy(currTopMenu);
    }

    // Returns number of items in the menu layer
    public int NumItemsInMenu()
    {
        return mMenuLayers[(int)UILayer.Menu].mLayerItems.Count;
    }
}
