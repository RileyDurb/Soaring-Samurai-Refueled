using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using static FileIOHandler;
using static RuntimeEditableDataManager.StatsVersionData;
using Unity.VisualScripting;
using System.Linq;

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

    [System.Serializable]
    public class StatsVersionData
    {
        [System.Serializable]
        public class StatEditPackage
        {
            public StatEditPackage(ScriptableObject editedObject, string fieldOrPropertyEdited, object newValue)
            {
                mEditedObject = editedObject;
                mFieldOrPropertyEditedName = fieldOrPropertyEdited;
                mNewValue = newValue;

                // Get info from parameters
                mEditedObjectName = editedObject.name;
            }
            public ScriptableObject mEditedObject = null;
            public string mEditedObjectName = "";
            public string mFieldOrPropertyEditedName  = "";
            public object mNewValue;

            public string mValueAsString;
            public string mTypeName;

        }
        public ArraySerializeWrapper<SetStatCommand> mStatSettingInfo = new ArraySerializeWrapper<SetStatCommand>();
        public string Test = "Testing";
        public SetStatCommand mCommand = new SetStatCommand();
        public ArraySerializeWrapper<StatEditPackage> mAllStatEdits = new ArraySerializeWrapper<StatEditPackage>();


        // Public interface
        public void SetUpValuesForSerialization()
        {

            for (int i = 0; i < mAllStatEdits.mItems.Length; i++)
            {
                StatEditPackage currStatEdit = mAllStatEdits.mItems[i];

                currStatEdit.mEditedObjectName = currStatEdit.mEditedObject.name;

                // Save object as string and type name so we can serialize it no matter the type
                currStatEdit.mValueAsString = currStatEdit.mNewValue.ToString();
                currStatEdit.mTypeName = currStatEdit.mNewValue.GetType().Name;
            }
        }

        public void InitializeValuesFromSerialization()
        {
            ScriptableObject[] scriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();

            for (int i = 0; i < mAllStatEdits.mItems.Length; i++)
            {
                StatEditPackage currStatEdit = mAllStatEdits.mItems[i];


                // Find the current scriptable object
                ScriptableObject targetScriptableObject = null;
                for (int j = 0; j < scriptableObjects.Length; j++)
                {
                    // If found, set the scriptable object reference
                    if (scriptableObjects[j].name == currStatEdit.mEditedObjectName)
                    {
                        currStatEdit.mEditedObject = scriptableObjects[j];
                        break;
                    }
                }

                // if scriptable object not found
                if (currStatEdit.mEditedObject == null)
                {
                    Console.Write("RuntimeStatsEditorManager:InitializeValuesFromSerialization: could not find scriptable object of name " + currStatEdit.mEditedObjectName);
                    continue;
                }

                // Convert the value from a string into it's object form so it's usable for setting the field or property it's for
                //currStatEdit.mNewValue = currStatEdit.mValueAsString.ConvertTo(Type.GetType(currStatEdit.mTypeName));

                //Type targetType = IsNullableType(reflectionTypeToSetOn) ? Nullable.GetUnderlyingType(reflectionTypeToSetOn) : reflectionTypeToSetOn;
                //currStatEdit.mNewValue = Convert.ChangeType(currStatEdit.mValueAsString, Type.GetType(currStatEdit.mTypeName));
                currStatEdit.mNewValue = currStatEdit.mValueAsString;

            }
        }
    }

    List<SetStatCommand> mCurrentSavedCommands = new List<SetStatCommand>();

    public List<StatEditPackage> mAllStatEdits = new List<StatEditPackage>();

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


    // Saving functions

    public void TrackStatSetCommand(SetStatCommand command)
    {
        mCurrentSavedCommands.Add(command);
    }

    public void TrackStatEdit(ScriptableObject editedObject, string propertyName, object newValue)
    {
        mAllStatEdits.Add(new StatEditPackage(editedObject, propertyName, newValue));
    }
    public void SaveStatSettingVersion(string statsVersionFileName)
    {
        // Make a package to be saved as JSON
        StatsVersionData statsVersionData = new StatsVersionData();

        //// Store all the stats set commands that are currently tracked, for reapplying when loading the save
        //statsVersionData.mStatSettingInfo.mItems = mCurrentSavedCommands.ToArray();

        // Store all the stats set commands that are currently tracked, for reapplying when loading the save
        statsVersionData.mAllStatEdits.mItems = mAllStatEdits.ToArray();

        statsVersionData.SetUpValuesForSerialization();

        // Turn the stat setting info into a JSON string for saving
        string statSaveJSON = JsonUtility.ToJson(statsVersionData, true);

        string filepath = "";

        // Save the json to a file, using the given filename, and starting path if any
        StreamWriter writer = new StreamWriter(filepath + statsVersionFileName + ".json", false);
        writer.WriteLine(statSaveJSON);
        writer.Close();
    }

    public void LoadStatSettingVersion(string statsVersionFileName)
    {
        StatsVersionData loadedStatsData = new StatsVersionData();

        string filepathPrefix = "";

        string fullFilepath = filepathPrefix + statsVersionFileName + ".json";

        if (File.Exists(fullFilepath) == false)
        {
            Console.Write("RuntimeStatsEditorManager: LoadStatSettingversion: No file of name " + statsVersionFileName + " could be found");
            return;
        }
        string statDataAsString = File.ReadAllText(fullFilepath);


        // Load data from string into the data object
        JsonUtility.FromJsonOverwrite(statDataAsString, loadedStatsData);

        // Get the scriptable object and object values, becuase they couldn't be serialized directly
        loadedStatsData.InitializeValuesFromSerialization();

        mAllStatEdits = loadedStatsData.mAllStatEdits.mItems.ToList();

        // Apply all the loaded stat edits
        for (int i = 0; i < mAllStatEdits.Count; i++)
        {
            StatEditPackage currStatEdit = mAllStatEdits[i];

            /* Apply the stat edit*/

            SetStatCommand statCommand = new SetStatCommand();
            statCommand.Initalize(currStatEdit.mEditedObject, currStatEdit.mFieldOrPropertyEditedName);

            // Give the command the value it will apply when executing it
            statCommand.SetValue(currStatEdit.mNewValue);

            statCommand.Execute(); // Make the stat edit that was read from the save file
        }
    }

}


public class FileIOHandler
{
    public static T[] LoadFromJson<T>(string json)
    {
        ArraySerializeWrapper<T> wrapper = JsonUtility.FromJson<ArraySerializeWrapper<T>>(json);
        return wrapper.mItems;
    }

    public static string ToJson<T>(T[] array, bool prettyPrint = false)
    {
        ArraySerializeWrapper<T> wrapper = new ArraySerializeWrapper<T>();
        wrapper.mItems = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    public class ArraySerializeWrapper<T>
    {
        public T[] mItems;
    }
}

public interface ICommand_
{
    // Execute the command. Returns true if the action should be saved in the undo/ redo queue
    public bool Execute();
    //void Undo();
    //void Redo();
}

[System.Serializable]
public class SetStatCommand : ICommand_
{
    // For saving the path to get to a property, supporting going through multiple nested fields and/ or properties
    [System.Serializable]
    protected class PropertyFieldChainLinkPackage
    {
        public PropertyFieldChainLinkPackage(string name, bool isProperty)
        {
            mName = name;
            mIsProperty = isProperty;
        }

        public string mName;
        public bool mIsProperty = false;
    }


    protected float mNewStatValue;
    protected float mPreviousStatValue;
    protected string mPropertyNameToSet;
    protected object mObjectToSetOn;

    protected FieldInfo mFieldToSet;
    protected PropertyInfo mPropertyToSet;

    protected List<string> mFieldChainToAccessValue = new List<string>();
    protected List<PropertyFieldChainLinkPackage> mPropertyChainToAccessValue = new List<PropertyFieldChainLinkPackage>();

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
            if (mFieldChainToAccessValue.Count > 0)
            {
                FieldInfo currField = null;

                // For each subfield
                for (int i = 0; i < mFieldChainToAccessValue.Count; i++)
                {
                   // Find the next field to go to
                   currField = recursiveObjectToSetOn.GetType().GetField(mFieldChainToAccessValue[i]);
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
            object recursiveObjectToSetOn = mObjectToSetOn;

            // If there are any subfields we need to go through to get to the proprty
            if (mPropertyChainToAccessValue.Count > 0)
            {
                MemberInfo currMember = null;
                PropertyFieldChainLinkPackage currChainLink = null;

                // For each subfield
                for (int i = 0; i < mPropertyChainToAccessValue.Count; i++)
                {
                    currChainLink = mPropertyChainToAccessValue[i];
                    if (currChainLink.mIsProperty)
                    {
                        currMember = recursiveObjectToSetOn.GetType().GetProperty(currChainLink.mName);

                        // Get the object at that field
                        recursiveObjectToSetOn = (currMember as PropertyInfo).GetValue(recursiveObjectToSetOn);
                    }
                    else
                    {
                        currMember = recursiveObjectToSetOn.GetType().GetField(currChainLink.mName);

                        recursiveObjectToSetOn = (currMember as FieldInfo).GetValue(recursiveObjectToSetOn);

                    }
                }
            }

            mPropertyToSet.SetValue(recursiveObjectToSetOn, mSettingValue);
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
            if (mFieldChainToAccessValue.Count > 0)
            {
                FieldInfo currField = null;

                // For each subfield
                for (int i = 0; i < mFieldChainToAccessValue.Count; i++)
                {
                    // Find the next field to go to
                    currField = recursiveObjectToSetOn.GetType().GetField(mFieldChainToAccessValue[i]);
                    // Get the object at that field
                    recursiveObjectToSetOn = currField.GetValue(recursiveObjectToSetOn);
                }
            }

            return mFieldToSet.GetValue(recursiveObjectToSetOn);

        }
        else // Check the property
        {
            object recursiveObjectToSetOn = mObjectToSetOn;

            // If there are any subfields we need to go through to get to the field
            if (mPropertyChainToAccessValue.Count > 0)
            {
                MemberInfo currMember = null;
                PropertyFieldChainLinkPackage currChainLink = null;


                // For each subfield
                for (int i = 0; i < mPropertyChainToAccessValue.Count; i++)
                {
                    currChainLink = mPropertyChainToAccessValue[i];
                    if (currChainLink.mIsProperty)
                    {
                        currMember = recursiveObjectToSetOn.GetType().GetProperty(currChainLink.mName);

                        // Get the object at that field
                        recursiveObjectToSetOn = (currMember as PropertyInfo).GetValue(recursiveObjectToSetOn);
                    }
                    else // Get the field
                    {
                        currMember = recursiveObjectToSetOn.GetType().GetField(currChainLink.mName);

                        recursiveObjectToSetOn = (currMember as FieldInfo).GetValue(recursiveObjectToSetOn);

                    }
                }
            }

            return mPropertyToSet.GetValue(recursiveObjectToSetOn);
        }
    }

    // helper functions
    protected FieldInfo GetField()
    {

        System.Type scriptableObjectType = mObjectToSetOn.GetType();

        int depthCount = 0;
        int maxDepthCount = 1;
        
        FieldInfo foundField = FindField_Rec(scriptableObjectType, ref mPropertyNameToSet, depthCount, ref mFieldChainToAccessValue, maxDepthCount);

        return foundField;
    }


    protected PropertyInfo GetProperty()
    {
        System.Type scriptableObjectType = mObjectToSetOn.GetType();


        int depthCount = 0;
        int maxDepthCount = 1;

        PropertyInfo foundProperty = FindProperty_Rec(scriptableObjectType, ref mPropertyNameToSet, depthCount, ref mPropertyChainToAccessValue, maxDepthCount);
        return foundProperty;
    }

    protected bool IsNullableType(System.Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
    }

    private FieldInfo FindField_Rec(Type typeToLookIn, ref string fieldName, int depthCount, ref List<string> fieldChain, int maxDepthCount = 1)
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
            fieldChain.Add(field.Name);
            foundField = FindField_Rec(field.FieldType, ref fieldName, depthCount + 1, ref fieldChain, maxDepthCount);


            if (foundField != null)
            {
                return foundField;
            }

            fieldChain.RemoveAt(fieldChain.Count - 1);
        }

        // If no fields were found, return null
        return null;
    }

    private PropertyInfo FindProperty_Rec(Type typeToLookIn, ref string fieldName, int depthCount, ref List<PropertyFieldChainLinkPackage> propertyChain, int maxDepthCount = 1)
    {
        PropertyInfo foundProperty = typeToLookIn.GetProperty(fieldName);

        if (foundProperty != null) // if field found
        {
            return foundProperty; // Return it
        }

        // Search in each property, and see if they contain the property
        PropertyInfo[] typeProperties = typeToLookIn.GetProperties();

        if (typeProperties.Length == 0 || typeToLookIn == typeof(System.Single) || depthCount > maxDepthCount)
        {
            return null;
        }

        foreach (PropertyInfo property in typeProperties)
        {
            propertyChain.Add(new PropertyFieldChainLinkPackage(property.Name, true));
            foundProperty = FindProperty_Rec(property.PropertyType, ref fieldName, depthCount + 1, ref propertyChain, maxDepthCount);


            if (foundProperty != null)
            {
                return foundProperty;
            }

            propertyChain.RemoveAt(propertyChain.Count - 1);
        }

        // Search in each field, in case the property is nested within that (like a struct stored as a field that has the value stored as a property), and see if any contain the property
        FieldInfo[] typeFields = typeToLookIn.GetFields();

        if (typeFields.Length == 0 || typeToLookIn == typeof(System.Single) || depthCount > maxDepthCount)
        {
            return null;
        }

        foreach (FieldInfo field in typeFields)
        {
            propertyChain.Add(new PropertyFieldChainLinkPackage(field.Name, false));
            foundProperty = FindProperty_Rec(field.FieldType, ref fieldName, depthCount + 1, ref propertyChain, maxDepthCount);


            if (foundProperty != null)
            {
                return foundProperty;
            }

            propertyChain.RemoveAt(propertyChain.Count - 1);
        }

        // If no properties were found, return null
        return null;
    }
}
