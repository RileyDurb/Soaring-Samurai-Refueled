using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RuntimeStatEditorFunctions : MonoBehaviour
{
    //[SerializeField] TMP_InputField TestScriptableObjectEditorButton;
    [SerializeField] TMP_InputField SaveFileNameBox;

    [SerializeField] string mSaveLoadStatVersionName = "Test Stat Version";

    [SerializeField] TMP_InputField SaveFilenameInputBox;
    [SerializeField] TMP_Dropdown ExistingSaveFilenamesDropdown;

    string mSaveDirectory = "";
    string mSaveFilenamePrefix = "StatData_";

    // Start is called before the first frame update
    void Start()
    {
        //TestScriptableObjectEditorButton.onSubmit.AddListener(ApplyTestSpeedChange);



        RefreshExistingSaveFileOptions();
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
        PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>().SaveStatSettingVersion(mSaveLoadStatVersionName);

        RefreshExistingSaveFileOptions();
    }

    public void TestLoadEditedStats()
    {
        PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>().LoadStatSettingVersion(mSaveLoadStatVersionName);
    }

    public void SetCurrentStatFilename(string newFilename)
    {
        mSaveLoadStatVersionName = newFilename;
    }

    public void SelectExistingFilenameFromDropdown(int filenameIndex)
    {
        mSaveLoadStatVersionName = ExistingSaveFilenamesDropdown.options[filenameIndex].text;
    }

    // Private helpers
    void RefreshExistingSaveFileOptions()
    {
        // Initialize items for stat filename selector
        ExistingSaveFilenamesDropdown.ClearOptions();

        // Get all filename options
        string[] allFilesInSaveDirectory = Directory.GetFiles(mSaveDirectory, "*.json", SearchOption.TopDirectoryOnly);
        List<string> allStatSaveFiles = new List<string>();

        for (int i = 0; i < allFilesInSaveDirectory.Length; i++)
        {
            if (allFilesInSaveDirectory[i].StartsWith(mSaveFilenamePrefix))
            {
                allStatSaveFiles.Add(allFilesInSaveDirectory[i]); // Add file name to list
            }
        }

        List<TMP_Dropdown.OptionData> existingStatFileOptions = new List<TMP_Dropdown.OptionData>(); // Clear old options


        // Make all existing filenames dropdown optios
        foreach (string saveFileName in allStatSaveFiles)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = saveFileName;

            existingStatFileOptions.Add(option);
        }
        ExistingSaveFilenamesDropdown.AddOptions(existingStatFileOptions);

        ExistingSaveFilenamesDropdown.onValueChanged.AddListener(SelectExistingFilenameFromDropdown); // Add select event to occur when value is changed
    }
}
