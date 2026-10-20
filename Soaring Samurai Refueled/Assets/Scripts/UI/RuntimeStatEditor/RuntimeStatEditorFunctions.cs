using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RuntimeStatEditorFunctions : MonoBehaviour
{
    // In-editor set references
    [SerializeField] TMP_InputField SaveFileNameBox;
    [SerializeField] TMP_InputField SaveFilenameInputBox;
    [SerializeField] TMP_Dropdown ExistingSaveFilenamesDropdown;
    [SerializeField] ScriptableObjectSelector ObjectSelectorObject; // Object selector reference, for refreshing after loading stats
    [SerializeField] RuntimeStatEditorSettings StatEditorSettings;

    // In-game set  references
    RuntimeEditableDataManager RuntimeDataManager;

    // Defined filepath variables
    [SerializeField] string mSaveLoadStatVersionName = "Test Stat Version";
    string mSaveDirectory = "";
    string mSaveFilenamePrefix = "StatData_";

    // Start is called before the first frame update
    void Start()
    {
        //TestScriptableObjectEditorButton.onSubmit.AddListener(ApplyTestSpeedChange);

        mSaveDirectory = Application.persistentDataPath;

        RefreshExistingSaveFileOptions();


        ExistingSaveFilenamesDropdown.onValueChanged.AddListener(SelectExistingFilenameFromDropdown); // Add select event to occur when value is changed

        RuntimeDataManager = PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>();

        // get starting filename, first trying to use the last set stat filename
        string startingStatFilename = RuntimeDataManager.LastSetFilenameInStatEditor;

        if (startingStatFilename == "")
        {
            // if no stat filename has been set yet, use the default defined one in the settings
            startingStatFilename = StatEditorSettings.DefaultInitialStatVersion;
        }

        // if there is a stat version that was found to use as the default
        if (startingStatFilename != "")
        {
            // Search for the dropdown option that matches the default filename
            int desiredFilenameOptionIndex = ExistingSaveFilenamesDropdown.options.FindIndex((TMP_Dropdown.OptionData currOption) => { return currOption.text == startingStatFilename; });


            if (desiredFilenameOptionIndex >= 0) // if option is found
            {
                // Make sure dropdown has that option selected, with no notify, because it's possibly we'll select the currently selected option, and that won't trigger the notify
                ExistingSaveFilenamesDropdown.SetValueWithoutNotify(desiredFilenameOptionIndex);

                // Set the filename in the editor manually
                SelectExistingFilenameFromDropdown(desiredFilenameOptionIndex);
            }
        }
        else
        {
            // Set current filename as the first one in the dropdown
            if (ExistingSaveFilenamesDropdown.options.Count > 0)
            {
                SelectExistingFilenameFromDropdown(ExistingSaveFilenamesDropdown.value);
            }
        }




        //ExistingSaveFilenamesDropdown.SetValueWithoutNotify

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyTestSpeedChange(string newSpeed)
    {
        float speed = 0;
        bool parseSucceeded = float.TryParse(newSpeed, out speed);

        // Make sure parsing succeded
        if (parseSucceeded == false)
        {
            print("RuntimeStatEditorFunctions:ApplyTestSpeedChange: input value " + newSpeed + "could not be parsed into a float");

            return;
        }

        // Apply stat change directly
        PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>().TestChangeScriptableObject(speed);
    }

    public void TestSaveEditedStats()
    {
        PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>().SaveStatSettingVersion(mSaveFilenamePrefix + mSaveLoadStatVersionName);

        RefreshExistingSaveFileOptions();
    }

    public void TestLoadEditedStats()
    {
        PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>().LoadStatSettingVersion(mSaveFilenamePrefix + mSaveLoadStatVersionName);

        ObjectSelectorObject.RefreshSelectedScriptableObject();
    }

    public void SetCurrentStatFilename(string newFilename)
    {
        mSaveLoadStatVersionName = newFilename;

        RuntimeDataManager.LastSetFilenameInStatEditor = newFilename; // Save this as the current last filename set, for restoring if we leave the editor and come back
    }

    public void SelectExistingFilenameFromDropdown(int filenameIndex)
    {
        string newStatFilename = ExistingSaveFilenamesDropdown.options[filenameIndex].text;

        mSaveLoadStatVersionName = newStatFilename; // Set filename variable to use for saving, loading, ect

        SaveFilenameInputBox.SetTextWithoutNotify(newStatFilename); // Set text of input box to show the new filename

        RuntimeDataManager.LastSetFilenameInStatEditor = newStatFilename; // Save this as the current last filename set, for restoring if we leave the editor and come back
    }

    // Private helpers
    void RefreshExistingSaveFileOptions()
    {
        // Initialize items for stat filename selector
        ExistingSaveFilenamesDropdown.ClearOptions();

        string saveDirectory = PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>().StatDataSaveDirectory;
        // Get all filename options
        string[] allFilesInSaveDirectory = Directory.GetFiles(saveDirectory, "*.*", SearchOption.TopDirectoryOnly);

        List<string> allStatSaveFiles = new List<string>();

        for (int i = 0; i < allFilesInSaveDirectory.Length; i++)
        {
            if (allFilesInSaveDirectory[i].Contains(mSaveFilenamePrefix))
            {
                string[] splitName = allFilesInSaveDirectory[i].Split(mSaveFilenamePrefix); // Split string to isolate just the filename from the rest of the path
                allStatSaveFiles.Add(splitName[1].Remove(splitName[1].Length - 5)); // Add file name to list, with the .json extension removed
            }
        }

        // Disable and exit early if no files found
        if (allStatSaveFiles.Count == 0)
        {
            ExistingSaveFilenamesDropdown.enabled = false;
            return;
        }
        else
        {
            ExistingSaveFilenamesDropdown.enabled = true;
        }

        List<TMP_Dropdown.OptionData> existingStatFileOptions = new List<TMP_Dropdown.OptionData>(); // Clear old options


        // Make all existing filenames dropdown options
        foreach (string saveFileName in allStatSaveFiles)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = saveFileName;

            existingStatFileOptions.Add(option);
        }

        ExistingSaveFilenamesDropdown.AddOptions(existingStatFileOptions);
    }
}
