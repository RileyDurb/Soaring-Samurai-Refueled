using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterVisuals", menuName = "Scripts/ScriptableObjects/Player/CharacterVisuals")]

public class CharacterVisuals : ScriptableObject
{
    [System.Serializable]
    public class CharacterVisualData
    {
        public CharacterDataManager.Characters Name = default;
        public Sprite HealthBarPortrait;
        public Material PlayerColorsMaterial;
    }



    [SerializeField] List<CharacterVisualData> mCharacterVisuals = new List<CharacterVisualData>();

    public CharacterVisualData GetCharacterVisuals(CharacterDataManager.Characters characterToGet)
    {
        CharacterVisualData characterData = mCharacterVisuals.Find((CharacterVisualData currData) => { return currData.Name == characterToGet; });
        if (characterData == null)
        {
            Console.Write("CharacterVisuals:GetCharacterVisuals: no visuals have been defined in this scriptable object for character of name " + characterToGet.ToString());
            return null;
        }
        else
        {
            return characterData;
        }
    }
}
