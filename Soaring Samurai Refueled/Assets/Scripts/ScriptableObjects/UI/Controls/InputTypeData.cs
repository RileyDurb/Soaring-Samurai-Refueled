using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[CreateAssetMenu(fileName = "InputTypeData", menuName = "Scripts/ScriptableObjects/UI/Controls/InputTypeData")]
public class InputTypeData : ScriptableObject
{

    public enum DeviceInputTypes
    {
        Keyboard,
        Controller,
        Touchscreen
    }

    [System.Serializable]
    public class InputTypeVisuals
    {
        public DeviceInputTypes InputType;
        public Sprite IconSprite;
    }

    [SerializeField] List<InputTypeVisuals> InputTypeInfo = new List<InputTypeVisuals>();
    // Helper functions

    // Public interface
    public static string GetDeviceInputType(InputDevice device)
    {
        if (device is Gamepad)
            if (device.description.empty == true || device.description.product == "Virtual")
            {
                return "Touchscreen";
            }
            else
            {
                return "Controller";
            }
        else if (device is Keyboard)
        {
            if (device.description == null || device.description.product == "Virtual")
            {
                return "Touchscreen";
            }
            return "Keyboard";
        }
        else if (device is Touchscreen)
        {
            return "Touchscreen";
        }
        else
        {
            return "Unrecognized Input (:";
        }
    }

    public InputTypeVisuals GetInputTypeVisuals(string typeName)
    {
        DeviceInputTypes targetInputType = DeviceInputTypes.Keyboard;

        bool typeFound = Enum.TryParse(typeName, out targetInputType);
        if (typeFound == false)
        {
            Console.Write("InputTypeData:GetInputTypeVisuals: type name " + typeName + " did not match any defined input types. Check spelling, or if we need to add a new input type");
            return null;
        }

        InputTypeVisuals typeVisuals = InputTypeInfo.Find((InputTypeVisuals currVisuals) => { return currVisuals.InputType == targetInputType; });

        if (typeVisuals == null)
        {
            Console.Write("InputTypeData:GetInputTypeVisuals: no visuals are defined for type name " + typeName + ". Likely need to add to the list in the InputTypeData scriptable object");
            return null;
        }


        return InputTypeInfo[(int)targetInputType];
    }

}
