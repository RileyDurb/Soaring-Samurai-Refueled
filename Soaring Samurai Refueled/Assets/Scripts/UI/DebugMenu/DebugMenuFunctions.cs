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
    [SerializeField] Slider CharacterSelectTargetSliderRef;

    int mCurrentPlayerIndexToColorChange = 0;
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

        // Set number of players for character select target slider
        CharacterSelectTargetSliderRef.maxValue = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList.Count - 1;
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
        int targetPlayerIndex = mCurrentPlayerIndexToColorChange;
        List<PlayerCombatController> players = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList;

        PlayerCombatController targetPlayer = players.Find((PlayerCombatController player) => { return player.PlayerIndex == mCurrentPlayerIndexToColorChange; });

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

    public void SetPlayerIndexToCharacterSelect()
    {
        mCurrentPlayerIndexToColorChange = (int)CharacterSelectTargetSliderRef.value;
    }
}
