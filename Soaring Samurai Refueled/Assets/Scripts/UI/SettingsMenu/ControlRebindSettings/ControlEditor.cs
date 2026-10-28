using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlEditor : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ControlNameText;
    [SerializeField] TextMeshProUGUI[] ControlRebindingBoxes;
    [SerializeField] Button mTriggerRebindButton;
    [SerializeField] GameObject mRebindNoticePopupPrefab;
    [SerializeField] InputRebindingOptionSettings mSettings;

    GameObject mSpawnedRebindNotice;
    ControlRebindList mParentBindingsList;

    InputBinding mCurrInputBinding;
    InputControl mCurrInputControl;
    InputAction mCurrInputAction;
    InputActionMap mCurrInputActionMap;

    ActiveInputTypeManager.ControlMethodTypes mCurrControlMethodType;
    int mInputActionRebindIndex = -1;
    string mOldPathBeforeRebind;
    bool mIsCompositeBindingPiece = false;
    int mControlIndex = -1;

    // Input rebinding


    InputActionRebindingExtensions.RebindingOperation mRebindingOperation;

    // Start is called before the first frame update
    void Start()
    {
        mTriggerRebindButton.onClick.AddListener(ActivateRebindingSequence);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        if (mRebindingOperation != null && mRebindingOperation.completed == false)
        {
            mRebindingOperation.Cancel();
        }
    }

    // Public interface
    public void InitializeWithInputAction(InputAction actionToEdit, ActiveInputTypeManager.ControlMethodTypes controlType)
    {
        ControlNameText.text = actionToEdit.name;

        mCurrControlMethodType = controlType;



        // Set input control name
        switch (mCurrControlMethodType)
        {
            case ActiveInputTypeManager.ControlMethodTypes.Gamepad: 
                mInputActionRebindIndex = 0; // All gamepad bindings are manually set to be at the first index
                mCurrInputBinding = actionToEdit.bindings[mInputActionRebindIndex];

                ControlRebindingBoxes[0].text = actionToEdit.GetBindingDisplayString(mInputActionRebindIndex);
                break;

            case ActiveInputTypeManager.ControlMethodTypes.Keyboard:
                mInputActionRebindIndex = 1; // All keyboard bindings are manually set to be at the first index 
                mCurrInputBinding = actionToEdit.bindings[mInputActionRebindIndex];
     
                ControlRebindingBoxes[0].text = actionToEdit.GetBindingDisplayString(mInputActionRebindIndex);

                break;

            default:
                break;
        }


    }

    public void InitializeWithInputBindingAndControl(InputBinding binding, InputControl inputControl, int controlIndex, ActiveInputTypeManager.ControlMethodTypes controlType, bool isCompositeBindingPiece = true)
    {
        ControlNameText.text = binding.name;
        ControlRebindingBoxes[0].text = inputControl.displayName;

        mCurrInputBinding = binding;
        mCurrInputControl = inputControl;

        mCurrControlMethodType = controlType;

        mIsCompositeBindingPiece = isCompositeBindingPiece;

        mControlIndex = controlIndex;
    }

    public void SetInputAction(InputAction actionToRepresent)
    {
        mCurrInputAction = actionToRepresent;
    }

    public void SetParentRebindList(ControlRebindList parentList)
    {
        mParentBindingsList = parentList;
    }
    public void SetInputActionMap(InputActionMap actionMap)
    {
        mCurrInputActionMap = actionMap;
    }

    // Private Helper functions
    private void ActivateRebindingSequence()
    {
        mCurrInputAction.Disable();

        InputRebindingOptionSettings.InputMethodBindingAesthetics mControlMethodRebindingAesthetics = mSettings.GetAestheticsForBindingMethod(mCurrControlMethodType);
       
        if (mCurrControlMethodType == ActiveInputTypeManager.ControlMethodTypes.Keyboard)
        {
            if (mIsCompositeBindingPiece)
            {
                mInputActionRebindIndex = mCurrInputAction.GetBindingIndexForControl(mCurrInputControl);
            }
            else
            {
                mInputActionRebindIndex = 1;
            }

            mOldPathBeforeRebind = mCurrInputAction.bindings[mInputActionRebindIndex].effectivePath;

            mRebindingOperation = mCurrInputAction.PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse") // Ignore non relevant inputs
                .WithMagnitudeHavingToBeGreaterThan(.3f)
                .WithControlsExcluding("<Gamepad>") // Ignore non relevant inputs
                .WithTargetBinding(mInputActionRebindIndex)
                .OnMatchWaitForAnother(mSettings.InputRebindDelayBeforeAcceptance);

        }
        else if (mCurrControlMethodType == ActiveInputTypeManager.ControlMethodTypes.Gamepad)
        {
            mInputActionRebindIndex = 0;
            mOldPathBeforeRebind = mCurrInputAction.bindings[mInputActionRebindIndex].effectivePath;

            mRebindingOperation = mCurrInputAction.PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse") // Ignore non relevant inputs
                .WithMagnitudeHavingToBeGreaterThan(.3f)
                .WithControlsExcluding("<Keyboard>") // Ignore non relevant inputs
                .WithTargetBinding(mInputActionRebindIndex)
                .OnMatchWaitForAnother(mSettings.InputRebindDelayBeforeAcceptance);
        }


        // Spawn popup and set proper rebind message
        mSpawnedRebindNotice = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPopup(mRebindNoticePopupPrefab);

        mSpawnedRebindNotice.GetComponent<GenericNoticeFunctionality>().SetMessageText(mControlMethodRebindingAesthetics.RebindNoticeMainMessage);


        mRebindingOperation.Start();
        mRebindingOperation.OnPotentialMatch(OnRebindingPotentialMatch);
        mRebindingOperation.OnComplete(OnRebindingOperationComplete);
    }

    private void OnRebindingOperationComplete(InputActionRebindingExtensions.RebindingOperation operation)
    {
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PopPopup();

        // Finds the current control using this binding, if any, and swaps that with this one
        mParentBindingsList.CheckForAndResolveDuplicates(mCurrInputBinding, mOldPathBeforeRebind, mCurrInputAction.bindings[mInputActionRebindIndex].effectivePath);

        mCurrInputControl = operation.selectedControl;


        operation.action.Enable(); //Re-enable input action

        mRebindingOperation.Cancel();

        RefreshControlEditorDisplay();
    }
    private void OnRebindingPotentialMatch(InputActionRebindingExtensions.RebindingOperation operation)
    {
        print(operation.candidates[operation.candidates.Count - 1].name);
    }

    public void RefreshControlEditorDisplay()
    {
        if (mIsCompositeBindingPiece)
        {
            InputControl currControl = mCurrInputAction.controls[mControlIndex];
            InitializeWithInputBindingAndControl(mCurrInputAction.GetBindingForControl(currControl).Value, currControl, mControlIndex, mCurrControlMethodType);
        }
        else
        {
            InitializeWithInputAction(mCurrInputAction, mCurrControlMethodType); // Refreshes control box to show current input binding
        }
    }

    //public void 

    public InputAction GetDisplayingInputAction()
    {
        return mCurrInputAction;
    }

    public void SetInputControlForCompositeBinding(InputControl newControl)
    {
        mCurrInputControl = newControl;
    }

}
