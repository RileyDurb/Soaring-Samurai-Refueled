using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferableSettingsMenu : MonoBehaviour
{
    public List<GameObject> SettingsObjectsToBufferChanges = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConfirmSettingChanges()
    {
        foreach (GameObject settingsObject in SettingsObjectsToBufferChanges)
        {
            Component[] settings = settingsObject.GetComponents(typeof(IBufferableSetting));

            foreach (Component component in settings)
            {
                IBufferableSetting settingInterface = component as IBufferableSetting;
                if (settingInterface != null)
                {
                    settingInterface.ConfirmSettingChanges(); // Saves new changes and applies to the relevant in-game functionality
                }
            }

        }
    }

    public void UndoTemporaryChanges()
    {
        foreach (GameObject settingsObject in SettingsObjectsToBufferChanges)
        {
            Component[] settings = settingsObject.GetComponents(typeof(IBufferableSetting));

            foreach (Component component in settings)
            {
                IBufferableSetting settingInterface = component as IBufferableSetting;
                if (settingInterface != null)
                {
                    settingInterface.UndoTemporarySettingsChanges(); // Reverts settings to the last saved values
                }
            }

        }
    }
   
}

public interface IBufferableSetting
{
    // Apply the buffered setting changes now that the user has confirmed them
    public void ConfirmSettingChanges();

    // Undo whatever the setting was changed to temporarily, usually reverting back to the last saved value
    public void UndoTemporarySettingsChanges();
}
