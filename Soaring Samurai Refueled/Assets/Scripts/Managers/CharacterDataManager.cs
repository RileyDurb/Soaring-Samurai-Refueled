using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDataManager : MonoBehaviour
{
    public enum Characters
    {
        BluePlayer,
        RedPlayer
    }

    [SerializeField] CharacterVisuals mCharacterVisualData;


    public CharacterVisuals GetCharacterVisualData()
    {
        return mCharacterVisualData;
    }
}
