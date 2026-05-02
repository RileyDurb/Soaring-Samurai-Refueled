using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDataManager : MonoBehaviour
{
    public enum Characters
    {
        BluePlayer,
        RedPlayer,
        YellowPlayer,
        GreenPlayer
    }

    [SerializeField] CharacterVisuals mCharacterVisualData;


    public CharacterVisuals GetCharacterVisualData()
    {
        return mCharacterVisualData;
    }

    public CharacterVisuals.CharacterVisualData GetCharactersData(Characters character)
    {
        return mCharacterVisualData.GetCharacterVisuals(character);
    }
}
