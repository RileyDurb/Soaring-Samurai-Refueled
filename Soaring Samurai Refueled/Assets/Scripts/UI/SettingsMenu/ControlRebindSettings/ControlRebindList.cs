using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering.LookDev;
using UnityEngine.UI;

public class ControlRebindList : MonoBehaviour
{
    [SerializeField] InputRebindingOptionSettings mSettings;

    [SerializeField] GameObject IndividualControlEditorPrefab;
    [SerializeField] GameObject ListParentObject;

    ActiveInputTypeManager mCurrentInputTypeManager;
    ScrollRect mOwnScrollRect;

    // Private variables
    ActiveInputTypeManager.ControlMethodTypes mCurrControlMethod = ActiveInputTypeManager.ControlMethodTypes.Invalid;
    ActiveInputTypeManager.ControlMethodTypes mLastControlMethod = ActiveInputTypeManager.ControlMethodTypes.Invalid;

    InputActionMap mCurrMapToShow;

    bool mControlListDirtyForRepopulating = false;

    public ActiveInputTypeManager.ControlMethodTypes CurrentControlMethod
    {
        set { mCurrControlMethod = value; }
    }

    public InputActionMap ActionMapToShow
    {
        set { mCurrMapToShow = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        mCurrentInputTypeManager = LevelScopeManagers.Instance.GetComponent<ActiveInputTypeManager>();
    }

    private void Awake()
    {
        mOwnScrollRect = GetComponent<ScrollRect>();
    }
    // Update is called once per frame
    void Update()
    {
        //mCurrControlMethod = mCurrentInputTypeManager.mCurrMethodType;

        if (mLastControlMethod != mCurrControlMethod)
        {
            mLastControlMethod = mCurrControlMethod;

            mControlListDirtyForRepopulating = true;
        }

        if (mControlListDirtyForRepopulating)
        {
            // Refresh control menu
            PopulateControlEditingMenu();

            mControlListDirtyForRepopulating = false;
        }
    
    }


    public void PopulateControlEditingMenu()
    {
        //StartCoroutine(RepopulateControlsOverTime());

        //return;
        ListParentObject.transform.DetachChildren(); // Clear children no matter what


        if (mCurrControlMethod == ActiveInputTypeManager.ControlMethodTypes.Invalid)
        {

            print("ControlRebindList:PopulateControlEditingMenu: Current player's control method is either invalid, or not control method is set. Cannot show control menu");
            return;
        }



        ReadOnlyArray<InputAction> inputActionsToShow = mCurrMapToShow.actions;
        for (int i = 0; i < inputActionsToShow.Count; i++)
        {
            
            InputAction currInputAction = inputActionsToShow[i];
  
            // Special case for move on keyboard
            if (currInputAction.name == "Move" && mCurrControlMethod == ActiveInputTypeManager.ControlMethodTypes.Keyboard)
            {
                InputControl[] keyboardControls = currInputAction.controls.Where((InputControl control) => { return control.path.Contains("Keyboard"); }).ToArray();

                // Make an editor for each cardinal direction
                for (int j = 0; j < keyboardControls.Length; j++)
                {
                    GameObject partialControlEditor = Instantiate(IndividualControlEditorPrefab, ListParentObject.transform);

                    // Initialize the editor with the input action to show it's name and controls
                    ControlEditor partialControlEditorComp = partialControlEditor.GetComponent<ControlEditor>();
                    partialControlEditorComp.InitializeWithInputBindingAndControl(currInputAction.GetBindingForControl(keyboardControls[j]).Value, keyboardControls[j], j, ActiveInputTypeManager.ControlMethodTypes.Keyboard);

                    partialControlEditorComp.SetInputAction(currInputAction);
                    partialControlEditorComp.SetInputActionMap(mCurrMapToShow);
                    partialControlEditorComp.SetParentRebindList(this);
                }

                continue;
            }

            // Make an editor for this control
            GameObject newControlEditor = Instantiate(IndividualControlEditorPrefab, ListParentObject.transform);

            // Initialize the editor with the input action to show it's name and controls
            ControlEditor controlEditorComp = newControlEditor.GetComponent<ControlEditor>();
            controlEditorComp.InitializeWithInputAction(currInputAction, mCurrControlMethod);
            controlEditorComp.SetInputAction(currInputAction);
            controlEditorComp.SetParentRebindList(this);
        }
    } 

    //public IEnumerator RepopulateControlsOverTime()
    //{
    //    ListParentObject.transform.DetachChildren(); // Clear children no matter what


    //    if (mCurrControlMethod == ActiveInputTypeManager.ControlMethodTypes.Invalid)
    //    {

    //        print("ControlRebindList:PopulateControlEditingMenu: Current player's control method is either invalid, or not control method is set. Cannot show control menu");
    //        yield break;
    //    }



    //    ReadOnlyArray<InputAction> inputActionsToShow = mCurrMapToShow.actions;
    //    for (int i = 0; i < inputActionsToShow.Count; i++)
    //    {

    //        InputAction currInputAction = inputActionsToShow[i];

    //        //currInputAction.bindings[]
    //        //List<InputBinding> currInputBindings = currInputAction.bindings.ToList();
    //        //int bindingIndexStart = mCurrControlMethod == ActiveInputTypeManager.ControlMethodTypes.Keyboard ? 1 : 0;


    //        // Special case for move on keyboard
    //        if (currInputAction.name == "Move" && mCurrControlMethod == ActiveInputTypeManager.ControlMethodTypes.Keyboard)
    //        {
    //            InputControl[] keyboardControls = currInputAction.controls.Where((InputControl control) => { return control.path.Contains("Keyboard"); }).ToArray();

    //            // Make an editor for each cardinal direction
    //            for (int j = 0; j < keyboardControls.Length; j++)
    //            {
    //                GameObject partialControlEditor = Instantiate(IndividualControlEditorPrefab, ListParentObject.transform);

    //                // Initialize the editor with the input action to show it's name and controls
    //                ControlEditor partialControlEditorComp = partialControlEditor.GetComponent<ControlEditor>();
    //                partialControlEditorComp.InitializeWithInputBindingAndControl(currInputAction.GetBindingForControl(keyboardControls[j]).Value, keyboardControls[j], ActiveInputTypeManager.ControlMethodTypes.Keyboard);

    //                partialControlEditorComp.SetInputAction(currInputAction);
    //                partialControlEditorComp.SetParentRebindList(this);
    //            }

    //            continue;
    //        }

    //        // Make an editor for this control
    //        GameObject newControlEditor = Instantiate(IndividualControlEditorPrefab, ListParentObject.transform);

    //        // Initialize the editor with the input action to show it's name and controls
    //        ControlEditor controlEditorComp = newControlEditor.GetComponent<ControlEditor>();
    //        controlEditorComp.InitializeWithInputAction(currInputAction, mCurrControlMethod);
    //        controlEditorComp.SetInputAction(currInputAction);
    //        controlEditorComp.SetParentRebindList(this);


    //        yield return new WaitForSecondsRealtime(0.5f);
    //    }

    //}

    public void SaveInputBindings(int playerIndex)
    {
        string currBindingOverridesJSON = mCurrMapToShow.SaveBindingOverridesAsJson();

        string saveFilepath = Application.persistentDataPath + "/Settings";

        // Make a telemetry directory if it doesn't already exist
        if (Directory.Exists(saveFilepath) == false)
        {
            Directory.CreateDirectory(saveFilepath);
        }

        saveFilepath += "/" + mSettings.ControlFilenamePrefix + "_Player_" + playerIndex.ToString() + ".json";
        StreamWriter writer = new StreamWriter(saveFilepath, false);

        writer.WriteLine(currBindingOverridesJSON);
        writer.Close();

    }


    public void LoadInputBindings(int playerIndex)
    {
        string filepath = Application.persistentDataPath + "/Settings/" + mSettings.ControlFilenamePrefix + "_Player_" + playerIndex.ToString() + ".json";
        if (File.Exists(filepath) == false)
        {
            print($"ControlRebindList:LoadInputBindings: Bindings for name Player {playerIndex} do not exist");
            return;
        }

        string readBindingOverridesJSON = File.ReadAllText(filepath);

        if (readBindingOverridesJSON == "\r\n")
        {
            print($"ControlRebindList:LoadInputBindings: Bindings for name Player {playerIndex} exist, but contain no remaps");
            return;
        }

        mCurrMapToShow.LoadBindingOverridesFromJson(readBindingOverridesJSON);

        RefreshAllControlDisplays();
    }

    public void SetControlListDirty()
    {
        mControlListDirtyForRepopulating = true;
    }

    public void CheckForAndResolveDuplicates(InputBinding bindingBeingRebound, string desiredBindingCurrentPath, string desiredBindingInputPath)
    {
        bool duplicateHandled = false;
        InputAction reboundDuplicateAction = null;
        // For each action
        ReadOnlyArray<InputAction> actionsToCheck = mCurrMapToShow.actions;
        for (int i = 0; i < actionsToCheck.Count; i++)
        {
            InputAction currAction = actionsToCheck[i];

            // For each binding in the action
            ReadOnlyArray<InputBinding> actionBindings = currAction.bindings;
            for (int j = 0; j < actionBindings.Count; j++)
            {
                InputBinding currBinding = actionBindings[j];
                // Skip the binding we are rebinding. Compares names if there are names, which is for the part of a composite binding. Otherwise compares the action for single input actions. 
                if (bindingBeingRebound.name == "" ? currBinding.action == bindingBeingRebound.action : currBinding.name == bindingBeingRebound.name)
                {
                    continue;
                }
                if (currBinding.effectivePath == desiredBindingInputPath) // if this binding has the binding we want to bind our desired control to
                {

                    // Give the binding we currently have to the action that has the binding we want. This frees up the binding to give to our desired control
                    currAction.ApplyBindingOverride(j, desiredBindingCurrentPath);

                    reboundDuplicateAction = currAction;
                    duplicateHandled = true;
                    break;
                }
            }

            if (duplicateHandled == true) // Only need to handle one duplicate
            {
                break;
            }
        }

        if (duplicateHandled)
        {
            // Find and refresh the display for the rebound control
            ControlEditor[] childEditors = transform.GetComponentsInChildren<ControlEditor>();

            for (int i = 0; i < childEditors.Length; i++)
            {
                if (childEditors[i].GetDisplayingInputAction() == reboundDuplicateAction)
                {
                    childEditors[i].RefreshControlEditorDisplay();
                }
            }


        }
    }

    // Private helpers //////////////////////////////////////////////////////////////////////////////////////
    void RefreshAllControlDisplays()
    {
        // Find and refresh the display for the rebound control
        ControlEditor[] childEditors = transform.GetComponentsInChildren<ControlEditor>();

        for (int i = 0; i < childEditors.Length; i++)
        {
            childEditors[i].RefreshControlEditorDisplay();
        }
    }
}
