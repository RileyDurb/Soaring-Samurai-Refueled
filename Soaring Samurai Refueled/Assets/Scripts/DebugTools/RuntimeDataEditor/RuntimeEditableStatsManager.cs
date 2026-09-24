using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// Runtime data manager, and command class for setting the data
// may want to move commands, or at least the defined commands, into a separate file
// Reflection data seting created with inspiration from Michael Bitzos' article on creating a persistent in-game debug settings menu. Link here: https://michaelbitzos.com/devblog/debug-settings-menu
// Code from that article was referenced under the MIT Liscence, full copywrite notice of that code here:
/*
MIT License

Copyright (c) 2022 mbitzos

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

// NOTE: This copywrite notice does not apply to this full project, just the code referenced from Michel Bitzos' article




public class RuntimeEditableDataManager : MonoBehaviour
{
    [SerializeField] PlayerBaseDataObject mPlayerStats;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TestChangeScriptableObject(float newMoveSpeed)
    {

        mPlayerStats.mMovementStats.DirectVelocityNormal = newMoveSpeed;

    }
}

public interface ICommand_
{
    // Execute the command. Returns true if the action should be saved in the undo/ redo queue
    public bool Execute();
    //void Undo();
    //void Redo();
}


public class SetStatCommand : ICommand_
{
    protected float mNewStatValue;
    protected float mPreviousStatValue;
    protected string mPropertyNameToSet;
    protected object mObjectToSetOn;

    protected FieldInfo mFieldToSet;
    protected PropertyInfo mPropertyToSet;

    protected List<string> mPropertyChainToAccessValue = new List<string>();

    protected object mSettingValue;

    public void Initalize(object scriptableObjectToUse, string propertyName)
    {
        mObjectToSetOn = scriptableObjectToUse;

        mPropertyNameToSet = propertyName;
        mFieldToSet = GetField();
        mPropertyToSet = GetProperty();

        mSettingValue = GetValue();
    }

    public bool Execute()
    {
        // Gets the reflection type to use, either the field type if it's a field, otherwise the property type
        System.Type reflectionTypeToSetOn = mFieldToSet == null ? mPropertyToSet.PropertyType : mFieldToSet.FieldType;


        var targetType = IsNullableType(reflectionTypeToSetOn) ? Nullable.GetUnderlyingType(reflectionTypeToSetOn) : reflectionTypeToSetOn;


        mSettingValue = Convert.ChangeType(mSettingValue, targetType);

        // Sets the setting value
        if (mFieldToSet != null)
        {
            object recursiveObjectToSetOn = mObjectToSetOn;

            // If there are any subfields we need to go through to get to the field
            if (mPropertyChainToAccessValue.Count > 0)
            {
                FieldInfo currField = null;

                // For each subfield
                for (int i = 0; i < mPropertyChainToAccessValue.Count; i++)
                {
                   // Find the next field to go to
                   currField = recursiveObjectToSetOn.GetType().GetField(mPropertyChainToAccessValue[i]);
                   // Get the object at that field
                   recursiveObjectToSetOn = currField.GetValue(recursiveObjectToSetOn);
                }
            }

            mFieldToSet.SetValue(recursiveObjectToSetOn, mSettingValue);

//#if UNITY_EDITOR
//            EditorUtility.SetDirty(mObjectToSetOn);

//            AssetDatabase.SaveAssets();
//#endif
        }
        else
        {
            mPropertyToSet.SetValue(mObjectToSetOn, mSettingValue);
        }

        return true;
    }

    public void SetValue(object newValue)
    {
        mSettingValue = newValue;
    }

    public object GetValue()
    {
        if (mFieldToSet != null)
        {
            object recursiveObjectToSetOn = mObjectToSetOn;

            // If there are any subfields we need to go through to get to the field
            if (mPropertyChainToAccessValue.Count > 0)
            {
                FieldInfo currField = null;

                // For each subfield
                for (int i = 0; i < mPropertyChainToAccessValue.Count; i++)
                {
                    // Find the next field to go to
                    currField = recursiveObjectToSetOn.GetType().GetField(mPropertyChainToAccessValue[i]);
                    // Get the object at that field
                    recursiveObjectToSetOn = currField.GetValue(recursiveObjectToSetOn);
                }
            }

            return mFieldToSet.GetValue(recursiveObjectToSetOn);

        }
        else
        {
            return mPropertyToSet.GetValue(mObjectToSetOn);
        }
    }

    // helper functions
    protected FieldInfo GetField()
    {

        System.Type scriptableObjectType = mObjectToSetOn.GetType();

        int depthCount = 0; ;
        int maxDepthCount = 1;
        
        FieldInfo foundField = FindField_Rec(scriptableObjectType, ref mPropertyNameToSet, depthCount, ref mPropertyChainToAccessValue, maxDepthCount);

        return foundField;
    }


    protected PropertyInfo GetProperty()
    {
        System.Type scriptableObjectType = mObjectToSetOn.GetType();
        return scriptableObjectType.GetProperty(mPropertyNameToSet);
    }

    protected bool IsNullableType(System.Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
    }

    private FieldInfo FindField_Rec(Type typeToLookIn, ref string fieldName, int depthCount, ref List<string> propertyChain, int maxDepthCount = 1)
    {
        FieldInfo foundField = typeToLookIn.GetField(fieldName);

        if (foundField != null) // if field found
        {
            return foundField; // Return it
        }

        // Search in each field, and see if they contain the field
        FieldInfo[] typeFields = typeToLookIn.GetFields();

        if (typeFields.Length == 0 || typeToLookIn == typeof(System.Single) || depthCount > maxDepthCount)
        {
            return null;
        }

        foreach (FieldInfo field in typeFields)
        {
            propertyChain.Add(field.Name);
            foundField = FindField_Rec(field.FieldType, ref fieldName, depthCount + 1, ref propertyChain, maxDepthCount);


            if (foundField != null)
            {
                return foundField;
            }

            propertyChain.RemoveAt(propertyChain.Count - 1);
        }

        // If no fields were found, return null
        return null;
    }
}
