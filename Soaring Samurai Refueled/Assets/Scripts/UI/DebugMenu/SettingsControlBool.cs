using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsControlBool : SettingsControl
{
    [SerializeField] Toggle mCheckbox;
    [SerializeField] TMP_Text mDisplayNameText;
    public override void AssignPropertyOrFieldToSet(ScriptableObject objectToUse, string propertyOrFieldName)
    {
        base.AssignPropertyOrFieldToSet(objectToUse, propertyOrFieldName);

        mCheckbox.onValueChanged.AddListener(value =>
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
        mCheckbox.SetIsOnWithoutNotify((bool)Convert.ChangeType(mInitialValue, typeof(bool)));
    }
}
