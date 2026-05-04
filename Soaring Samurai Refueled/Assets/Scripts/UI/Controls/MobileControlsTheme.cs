using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileControlsTheme : MonoBehaviour
{
    [SerializeField] List<Image> CharacterColorDependentImages = new List<Image>();

    public void UpdateCharacterBasedVisuals(CharacterDataManager.Characters characterToBe)
    {
        CharacterVisuals.CharacterVisualData characterData = PersistentScopeManagers.Instance.GetComponent<CharacterDataManager>().GetCharactersData(characterToBe);
        foreach (Image imageToRecolor in CharacterColorDependentImages)
        {
            imageToRecolor.material = characterData.PlayerColorsMaterial;

        }
    }
}
