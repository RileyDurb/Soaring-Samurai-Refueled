using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActiveInputTypeManager : MonoBehaviour
{
    public enum ControlMethodTypes
    {
        Invalid,
        Gamepad,
        Keyboard
    }

    public class AnyInputListener : IObserver<InputControl>
    {


        public AnyInputListener(ActiveInputTypeManager parentManager)
        {
            mParentManager = parentManager;
        }

        public void OnNext(InputControl currInput)
        {
            mParentManager.mCurrMethodType = GetControlMethodTypeFromInputDevice(currInput.device);

        }

        public void OnCompleted()
        {
            //print("Hoho");

        }

        public void OnError(Exception error)
        {
            //print("Haha");

        }

        // Private variables

        ActiveInputTypeManager mParentManager;
    }

    // public variables
    public ControlMethodTypes mCurrMethodType;

    // private variables
    IDisposable mAnyInputListenerUnsubscriber = null;
    AnyInputListener mAnyInputListener;



    // Start is called before the first frame update
    void Start()
    {
        mAnyInputListener = new AnyInputListener(this);
        mAnyInputListenerUnsubscriber = InputSystem.onAnyButtonPress.Subscribe(mAnyInputListener);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        mAnyInputListenerUnsubscriber.Dispose();
    }

    public static ControlMethodTypes GetControlMethodTypeFromInputDevice(InputDevice deviceToCheck)
    {

        ControlMethodTypes determinedControlMethod = ControlMethodTypes.Invalid;
        if (deviceToCheck.description.deviceClass == "Keyboard")
        {
            determinedControlMethod = ControlMethodTypes.Keyboard;
        }
        else if (deviceToCheck.description.deviceClass == "Controller" || deviceToCheck.description.deviceClass == "")
        {
            determinedControlMethod = ControlMethodTypes.Gamepad;
        }

        return determinedControlMethod;
    }
}
