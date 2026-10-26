using System.Collections;
using System.Collections.Generic;
using System.Security;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindMenuButtonFunctions : MonoBehaviour
{
    // Editor accessible variables
    [SerializeField] TMP_Dropdown mInputMapPresetDropdown;
    [SerializeField] InputRebindingOptionSettings mSettings;
    [SerializeField] ControlRebindList mControlsRebindingListObject;
    [SerializeField] Slider mPlayerSelectorSlider;
    [SerializeField] TextMeshProUGUI mSelectedPlayerText;

    // private variables
    InputBindingsManager mBindingsManagerRef;
    int mSelectedPlayerIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        mBindingsManagerRef = PersistentScopeManagers.Instance.GetComponent<InputBindingsManager>();


        mPlayerSelectorSlider.maxValue = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList.Count - 1;

        SetSelectedPlayer(0);


        PopulateInputMappingDropdown();

        //PlayerInputManager.instance.onPlayerJoined += RecievePlayerJoinedOrLeft; // Subscribe to player joining event so we can reset menu if we have the joining player selected
        //PlayerInputManager.instance.onPlayerLeft += RecievePlayerJoinedOrLeft; // Subscribe to player leaving event so we can reset menu if we have the leaving player selected
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopulateInputMappingDropdown()
    {
        mInputMapPresetDropdown.ClearOptions();

        List<string> mInputMapOptions = mSettings.mPresetInputMappingOptionNames;

        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();

        // Make an option for each input map
        for (int i = 0; i < mInputMapOptions.Count; i++)
        {
            TMP_Dropdown.OptionData currOption = new TMP_Dropdown.OptionData();
            currOption.text = mInputMapOptions[i];

            dropdownOptions.Add(currOption);
        }

        mInputMapPresetDropdown.AddOptions(dropdownOptions);
    }

    public void SelectInputMap(int dropdownIndex)
    {
        string selectedMapOption = mInputMapPresetDropdown.options[mInputMapPresetDropdown.value].text;

        mBindingsManagerRef.SetInputMapping(0, selectedMapOption);

        // Refresh the current control list to show the new input mapping
        mControlsRebindingListObject.PopulateControlEditingMenu();
    }

    public void SetSelectedPlayer(int newSelectedPlayer)
    {
        mSelectedPlayerIndex = newSelectedPlayer; // Set the player to target for player specific menu items

        // Find target player controller to get name
        List<PlayerCombatController> players = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList;
        PlayerCombatController targetPlayer = players.Find((PlayerCombatController player) => { return player.PlayerIndex == mSelectedPlayerIndex; });

        mSelectedPlayerText.text = "Selected Player: " + targetPlayer.PlayerName; // Set player name


        string currentInputMap = mSettings.mDefaultInputMapName;
        InputBindingsManager.PlayerInputBindingInfo selectedPlayerBindingsInfo = mBindingsManagerRef.GetPlayerBindingInfo(mSelectedPlayerIndex);

        if (selectedPlayerBindingsInfo != null)
        {
            currentInputMap = selectedPlayerBindingsInfo.CurrentInputMappingName;
        }



        // Make control list display right control type
        PlayerInput[] playerInputComponents = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);

        PlayerInput targetPlayerInput = null;
        for (int i = 0; i < playerInputComponents.Length; i++)
        {
            if (playerInputComponents[i].playerIndex == mSelectedPlayerIndex)
            {
                targetPlayerInput = playerInputComponents[i];
                break;
            }
        }

        if (targetPlayerInput == null)
        {
            print($"RebindMenuButtonFunctions:SetSelectedPlayer: No player input of selected player index {mSelectedPlayerIndex} exists");

            ActiveInputTypeManager.ControlMethodTypes errorInputMethodType = ActiveInputTypeManager.ControlMethodTypes.Invalid;
            mControlsRebindingListObject.CurrentControlMethod = errorInputMethodType;

            mControlsRebindingListObject.PopulateControlEditingMenu();
            return;
        }

        // Make rebinding menu show the selected action map of the new player
        mControlsRebindingListObject.ActionMapToShow = targetPlayerInput.currentActionMap;


        ActiveInputTypeManager.ControlMethodTypes targetPlayerInputMethodType = ActiveInputTypeManager.GetControlMethodTypeFromInputDevice(targetPlayerInput.devices[0]);
        mControlsRebindingListObject.CurrentControlMethod = targetPlayerInputMethodType;


        // Refesh controls rebinding list now that the map is set
        mControlsRebindingListObject.PopulateControlEditingMenu();

        // Update map dropdown to have the player's map selected
        int currInputMapOptionIndex = mInputMapPresetDropdown.options.FindIndex((TMP_Dropdown.OptionData currOption) => { return currOption.text == currentInputMap; });

        if (currInputMapOptionIndex >= 0)
        {
            mInputMapPresetDropdown.SetValueWithoutNotify(currInputMapOptionIndex);
        }

    }

    public void SetSelectedPlayerThroughSlider()
    {
        SetSelectedPlayer((int)mPlayerSelectorSlider.value);
    }

    public void TriggerInputBindingsSave()
    {
        mControlsRebindingListObject.SaveInputBindings(mSelectedPlayerIndex);
    }

    public void TriggerInputBindingsLoad()
    {
        mControlsRebindingListObject.LoadInputBindings(mSelectedPlayerIndex);
    }

    // Event subscriptions
    //void RecievePlayerJoinedOrLeft(PlayerInput newPlayerInputComp)
    //{
    //    if (mSelectedPlayerIndex == newPlayerInputComp.playerIndex)
    //    {
    //        SetSelectedPlayer(newPlayerInputComp.playerIndex);
    //    }
    //}
}
