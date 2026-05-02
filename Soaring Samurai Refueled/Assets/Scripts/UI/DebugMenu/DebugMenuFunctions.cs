using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuFunctions : MonoBehaviour
{
    [SerializeField] GameObject DebugMenuToggleRef;
    [SerializeField] TMP_Dropdown CharacterSelectDropdownRef;
    [SerializeField] Slider PlayerToChangeSlider;
    [SerializeField] TextMeshProUGUI SelectedPlayerText;
    [SerializeField] TMP_Dropdown CPUModeDropdown;

    int mSelectedPlayerIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        DebugMenuToggleRef.GetComponent<Toggle>().SetIsOnWithoutNotify(SimManager.Instance.DebugModeOn);

        CharacterSelectDropdownRef.ClearOptions(); // Clears any default options
        CharacterVisuals characters = PersistentScopeManagers.Instance.GetComponent<CharacterDataManager>().GetCharacterVisualData();

        // Adds am option for each character
        List<TMP_Dropdown.OptionData> characterSelectOptions = new List<TMP_Dropdown.OptionData>();
        foreach (CharacterDataManager.Characters character in Enum.GetValues(typeof(CharacterDataManager.Characters)))
        {
            CharacterVisuals.CharacterVisualData currCharacter = characters.GetCharacterVisuals(character);
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = character.ToString().Replace("Player", "");

            characterSelectOptions.Add(option);
        }

        CharacterSelectDropdownRef.AddOptions(characterSelectOptions);

        // Initialize items for CPU Mode selector
        CPUModeDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> cpuModeSelectOptions = new List<TMP_Dropdown.OptionData>();
        foreach (AIBehaviour.AIMode mode in Enum.GetValues(typeof(AIBehaviour.AIMode)))
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = mode.ToString();

            cpuModeSelectOptions.Add(option);
        }
        CPUModeDropdown.AddOptions(cpuModeSelectOptions);

        CPUModeDropdown.onValueChanged.AddListener(SelectCPUMode); // Add select event to occur when value is changed

        // Set number of players for player edit target slider
        PlayerToChangeSlider.maxValue = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList.Count - 1;
    }

    public void ToggleDebugMode()
    {
        // Toggle debug mode state
        SimManager.Instance.DebugModeOn = !SimManager.Instance.DebugModeOn;
    }

    public void SetAllPlayersToLowHealth()
    {
        LevelScopeManagers.Instance.GetComponent<DebugHotkeyManager>().DebugSetPlayerHealthToLow();
    }

    public void SetRoundTimeToLow()
    {
        LevelScopeManagers.Instance.GetComponent<DebugHotkeyManager>().SetRoundTime(3.0f);
    }

    public void ResetRoundTime()
    {
        MatchStateManager matchStateMan = LevelScopeManagers.Instance.GetComponent<MatchStateManager>();
        LevelScopeManagers.Instance.GetComponent<DebugHotkeyManager>().SetRoundTime(matchStateMan.MatchStats.MaxRoundTime);
    }

    public void SelectCharacter(int optionIndex)
    {
        int targetPlayerIndex = mSelectedPlayerIndex;
        List<PlayerCombatController> players = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList;

        PlayerCombatController targetPlayer = players.Find((PlayerCombatController player) => { return player.PlayerIndex == mSelectedPlayerIndex; });

        if (targetPlayer == null)
        {
            print("DebugMenuFunctions:SelectCharacter: no player of index " + targetPlayerIndex.ToString() + " could be found.");
            return;
        }
        CharacterDataManager.Characters characterToBe = CharacterDataManager.Characters.BluePlayer;
        bool optionWasValid = Enum.TryParse(CharacterSelectDropdownRef.options[optionIndex].text + "Player", out characterToBe);

        if (optionWasValid == false)
        {
            print("DebugMenuFunctions:SelectCharacter: no character enum matched player option " + CharacterSelectDropdownRef.options[optionIndex].text);
            return;
        }

        targetPlayer.SetCharacterVisuals(characterToBe);
    }

    public void SelectCPUMode(int optionIndex)
    {

        // Get selected player
        int targetPlayerIndex = mSelectedPlayerIndex;
        List<PlayerCombatController> players = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList;
        PlayerCombatController targetPlayer = players.Find((PlayerCombatController player) => { return player.PlayerIndex == mSelectedPlayerIndex; });
        if (targetPlayer == null)
        {
            print("DebugMenuFunctions:SelectCPUMode: no player of index " + targetPlayerIndex.ToString() + " could be found.");
            return;
        }

        // Get selectied option as an AI mode enum
        AIBehaviour.AIMode modeToChangeTo = AIBehaviour.AIMode.PlayerInput;
        bool modeWasValid = Enum.TryParse(CPUModeDropdown.options[optionIndex].text, out modeToChangeTo);

        if (modeWasValid == false)
        {
            print("DebugModeFunctions:SelectCPUMode: no AI mode matched player option " + CPUModeDropdown.options[optionIndex].text);
            return;
        }

        targetPlayer.GetComponent<AIBehaviour>().SetAIMode(modeToChangeTo);

    }
    
    public void SetSelectedPlayer()
    {
        mSelectedPlayerIndex = (int)PlayerToChangeSlider.value; // Set the player to target for player specific menu items

        // Find target player controller to get name
        List<PlayerCombatController> players = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList;
        PlayerCombatController targetPlayer = players.Find((PlayerCombatController player) => { return player.PlayerIndex == mSelectedPlayerIndex; });

        SelectedPlayerText.text = "Selected Player: " + targetPlayer.PlayerName; // Set player name

        // Update dropdowns to show current value
        CharacterSelectDropdownRef.SetValueWithoutNotify((int)targetPlayer.CharacterVisualsName);
        CPUModeDropdown.SetValueWithoutNotify((int)targetPlayer.GetComponent<AIBehaviour>().CurrAIMode);

    }

    public void ToggleTimePaused()
    {
        LevelScopeManagers.Instance.GetComponent<DebugHotkeyManager>().ToggleTimerPaused();
    }
}
