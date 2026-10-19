using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsControlVector2 : SettingsControl
{
    [SerializeField] TMP_InputField[] mInputFields;
    [SerializeField] TMP_Text mDisplayNameText;
    Vector2 mCurrValue;
    
    public override void AssignPropertyOrFieldToSet(ScriptableObject objectToUse, string propertyOrFieldName)
    {
        base.AssignPropertyOrFieldToSet(objectToUse, propertyOrFieldName);

        for (int i = 0; i < mInputFields.Length; i++)
        {
            int inputIndex = i;
            mInputFields[inputIndex].onSubmit.AddListener(value =>
            {
                mCurrValue[inputIndex] = (float)Convert.ChangeType(value, typeof(float));

                // Set the value for the command to use to set the stat
                // NOTE: may want the command to be able to get this, to make it more modular, but seems fine for our use case
                mStatSetCommandObject.SetValue(mCurrValue);

                // Execute the stat command
                mStatSetCommandObject.Execute();

                PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>().TrackStatSetCommand(mStatSetCommandObject);
                PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>().TrackStatEdit(objectToUse, propertyOrFieldName, mCurrValue);

            });
        }


        // Initialize name
        mDisplayNameText.text = mPropertyNameToSet;

        // Put starting value in each of the vector dimension input fields
        mCurrValue = (Vector2)Convert.ChangeType(mInitialValue, typeof(Vector2));
        for (int i = 0; i < mInputFields.Length; i++)
        {
            mInputFields[i].SetTextWithoutNotify(mCurrValue[i].ToString());
        }
        
    }
}
