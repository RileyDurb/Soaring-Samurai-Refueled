using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ScriptableObjectDebugList : MonoBehaviour
{

    [SerializeField] RuntimeStatEditorSettings mEditorSettings;
    [SerializeField] ScriptableObject mScriptableObjectToShow;

    // Start is called before the first frame update
    void Start()
    {
        PopulateValueList(mScriptableObjectToShow);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopulateValueList(ScriptableObject objectToMakeEditorFor)
    {
        transform.DetachChildren(); // Clears all children, to start the list fresh


        Type type = objectToMakeEditorFor.GetType();
        FieldInfo[] objectFields = type.GetFields();

        // For each field
        foreach (FieldInfo field in objectFields)
        {
            GameObject editorIfAny = null;
            if (field.FieldType == typeof(float) || field.FieldType == typeof(Int32))
            {
                editorIfAny = mEditorSettings.GetVariableTypeEditor(field.FieldType);

            }

            // If editor exists for field type
            if (editorIfAny != null)
            {
                GameObject newValueEditor = Instantiate(editorIfAny, transform); // Spawn editor as a child

                // Assign the object, and this particular field, for the editor to edit
                newValueEditor.GetComponent<SettingsControl>().AssignPropertyOrFieldToSet(mScriptableObjectToShow, field.Name);
            }
        }
    }


}
