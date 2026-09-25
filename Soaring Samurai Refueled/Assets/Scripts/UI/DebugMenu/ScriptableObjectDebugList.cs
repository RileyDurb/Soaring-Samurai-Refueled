using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ScriptableObjectDebugList : MonoBehaviour
{

    [SerializeField] RuntimeStatEditorSettings mEditorSettings;
    [SerializeField] ScriptableObject mScriptableObjectToShow;

    public ScriptableObject ScriptableObjectToShow { 
        get { return mScriptableObjectToShow; } 
        set { mScriptableObjectToShow = value; }
    }

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
        mScriptableObjectToShow = objectToMakeEditorFor;
        transform.DetachChildren(); // Clears all children, to start the list fresh


        Type type = objectToMakeEditorFor.GetType();

        DisplayObjectFields_Rec(type, 0);

    }

    // Helper functions

    private void DisplayObjectFields_Rec(Type typeToShow, int fieldDepth)
    {
        if (fieldDepth > mEditorSettings.mMaxFieldDepthToShow)
        {
            return;
        }

        // Adds a header for this object
        GameObject currentObjectHeader = Instantiate(mEditorSettings.mNestedObjectHeaderPrefab, transform);
        currentObjectHeader.GetComponent<ObjectListHeader>().SetText(typeToShow.Name);

        FieldInfo[] objectFields = typeToShow.GetFields();
        //PropertyInfo[] objectProperties = typeToShow.GetProperties();

        // For each field, show the individually editable variables, and their sub-objects, to the maximum depth
        foreach (FieldInfo field in objectFields)
        {
            GameObject editorIfAny = null;

            editorIfAny = mEditorSettings.GetVariableTypeEditor(field.FieldType);
            // If editor exists for field type
            if (editorIfAny != null)
            {
                GameObject newValueEditor = Instantiate(editorIfAny, transform); // Spawn editor as a child

                // Assign the object, and this particular field, for the editor to edit
                newValueEditor.GetComponent<SettingsControl>().AssignPropertyOrFieldToSet(mScriptableObjectToShow, field.Name);

                newValueEditor.GetComponent<HorizontalLayoutGroup>().padding = mEditorSettings.mListItemPadding;
            }
            else if (fieldDepth + 1 <= mEditorSettings.mMaxFieldDepthToShow) // if not a regular editor type, try showing nested fields
            {

                if (field.FieldType.BaseType == typeof(System.Object))
                {
                    DisplayObjectFields_Rec(field.FieldType, fieldDepth + 1);
                }
            }
        }
    }

}
