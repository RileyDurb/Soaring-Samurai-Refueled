using System.Collections;
using System.Collections.Generic;
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
    ActiveInputTypeManager.ControlMethodTypes mCurrControlMethodType;
    int mSubInputActionIndex = -1;

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

    // Public interface
    public void InitializeWithInputAction(InputAction actionToEdit, ActiveInputTypeManager.ControlMethodTypes controlType)
    {
        ControlNameText.text = actionToEdit.name;

        mCurrControlMethodType = controlType;

        InputBinding bindingToRead;
        string[] splitPathAndBindingName;
        // Set input control name
        switch (mCurrControlMethodType)
        {
            case ActiveInputTypeManager.ControlMethodTypes.Gamepad:
                //bindingToRead = actionToEdit.bindings[0];
                //splitPathAndBindingName = bindingToRead.effectivePath.Split("/");
                //ControlRebindingBoxes[0].text = splitPathAndBindingName[1];
                ControlRebindingBoxes[0].text = actionToEdit.GetBindingDisplayString(0);
                break;

            case ActiveInputTypeManager.ControlMethodTypes.Keyboard:
                //bindingToRead = actionToEdit.bindings[1];

                //splitPathAndBindingName = bindingToRead.effectivePath.Split("/");
                //ControlRebindingBoxes[0].text = splitPathAndBindingName[1];

                ControlRebindingBoxes[0].text = actionToEdit.GetBindingDisplayString(1);

                break;

            default:
                break;
        }


    }

    public void InitializeWithInputBindingAndControl(InputBinding binding, InputControl inputControl, ActiveInputTypeManager.ControlMethodTypes controlType)
    {
        ControlNameText.text = binding.name;
        ControlRebindingBoxes[0].text = inputControl.displayName;

        mCurrInputBinding = binding;
        mCurrInputControl = inputControl;

        mCurrControlMethodType = controlType;
    }

    public void SetInputAction(InputAction actionToRepresent, int subInputActionIndex = -1)
    {
        mCurrInputAction = actionToRepresent;

        mSubInputActionIndex = subInputActionIndex;
    }

    public void SetParentRebindList(ControlRebindList parentList)
    {
        mParentBindingsList = parentList;
    }


    // Private Helper functions
    private void ActivateRebindingSequence()
    {
        mCurrInputAction.Disable();

        InputRebindingOptionSettings.InputMethodBindingAesthetics mControlMethodRebindingAesthetics = mSettings.GetAestheticsForBindingMethod(mCurrControlMethodType);
       
        if (mCurrControlMethodType == ActiveInputTypeManager.ControlMethodTypes.Keyboard)
        {
            mRebindingOperation = mCurrInputAction.PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse") // Ignore non relevant inputs
                .WithMagnitudeHavingToBeGreaterThan(.3f)
                .WithControlsExcluding("<Gamepad>") // Ignore non relevant inputs
                .WithTargetBinding(1)
                .OnMatchWaitForAnother(1.0f);
        }
        else if (mCurrControlMethodType == ActiveInputTypeManager.ControlMethodTypes.Gamepad)
        {
            mRebindingOperation = mCurrInputAction.PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse") // Ignore non relevant inputs
                .WithMagnitudeHavingToBeGreaterThan(.3f)
                .WithControlsExcluding("<Keyboard>") // Ignore non relevant inputs
                .WithTargetBinding(0)
                .OnMatchWaitForAnother(1.0f);
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
        InitializeWithInputAction(mCurrInputAction, mCurrControlMethodType); // Refreshes control box to show current input binding
    }
}
