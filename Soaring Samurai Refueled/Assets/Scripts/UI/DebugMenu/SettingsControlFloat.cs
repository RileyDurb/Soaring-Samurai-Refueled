using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsControlFloat : SettingsControl
{
    [SerializeField] TMP_InputField mInputField;
    [SerializeField] TMP_Text mDisplayNameText;
    public override void AssignPropertyOrFieldToSet(ScriptableObject objectToUse, string propertyOrFieldName)
    {
        base.AssignPropertyOrFieldToSet(objectToUse, propertyOrFieldName);

        mInputField.onSubmit.AddListener(value =>
        {
            // Set the value for the command to use to set the stat
            // NOTE: may want the command to be able to get this, to make it more modular, but seems fine for our use case
            mStatSetCommandObject.SetValue(value);

            // Execute the stat command
            mStatSetCommandObject.Execute();

        });

        // Initialize name
        mDisplayNameText.text = mPropertyNameToSet;

        // Put starting value in the input field
        mInputField.SetTextWithoutNotify((string)Convert.ChangeType(mInitialValue, typeof(string)));
    }
}
