using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScriptableObjectSelector : MonoBehaviour
{
    [SerializeField] RuntimeStatEditorSettings mSettings;
    [SerializeField] TMP_Dropdown mObjectDropdown;
    [SerializeField] ScriptableObjectDebugList mObjectListToSetOn;
    RuntimeEditableDataManager RuntimeDataManager;

    List<ScriptableObject> mDropdownItems = new List<ScriptableObject>();

    // Start is called before the first frame update
    void Start()
    {
        mObjectDropdown.ClearOptions(); // Remove any editor only example options

        // Make all objects to show into dropdown options
        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();

        foreach (ScriptableObject scriptableObject in mSettings.mEditableScriptableObjectsToChooseFrom)
        {
            TMP_Dropdown.OptionData currOptionData = new TMP_Dropdown.OptionData();
            currOptionData.text = scriptableObject.name;
            dropdownOptions.Add(currOptionData);

            mDropdownItems.Add(scriptableObject); // Saves the item to use for access when selecting a dropdown option
        }

        mObjectDropdown.AddOptions(dropdownOptions); // Adds all the options to the dropdown


        RuntimeDataManager = PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>();
        // Set initial selected valie in dropdown
        string initialScriptableObjectName = RuntimeDataManager.LastSelectedScriptableObjectInEditorName; // Prioritize using the last selected object from any time the editor was used
        if (initialScriptableObjectName == "") // if no object has been set yet
        {
            // Use default set object, which is either set in the inspector, or the editor settings scriptable object
            initialScriptableObjectName = mObjectListToSetOn.ScriptableObjectToShow.name;
        }


        // Set the initial object in the dropdown
        int initialObjectIndex = mDropdownItems.FindIndex((ScriptableObject checkObject) => { return checkObject.name == initialScriptableObjectName; });
        if (initialObjectIndex >= 0) // If initial object on the editor was found in the dropdown
        {
            mObjectDropdown.value = initialObjectIndex; // Set the dropdown to have the currently shown item selected
            mObjectListToSetOn.PopulateValueList(mDropdownItems[initialObjectIndex]);
        }

        // Subscribe set function to be called when the value changes
        mObjectDropdown.onValueChanged.AddListener(SelectScriptableObject); 
    }
    // Update is called once per frame

    void Update()
    {
        
    }

    public void SelectScriptableObject(int optionIndex)
    {
        mObjectListToSetOn.PopulateValueList(mDropdownItems[optionIndex]);

        RuntimeDataManager.LastSelectedScriptableObjectInEditorName = mObjectDropdown.options[optionIndex].text; // Save this as the new last selected object
    }

    public void RefreshSelectedScriptableObject()
    {
        mObjectListToSetOn.PopulateValueList(mDropdownItems[mObjectDropdown.value]); // Populate scriptable object editor with currently selected value again

    }
}
