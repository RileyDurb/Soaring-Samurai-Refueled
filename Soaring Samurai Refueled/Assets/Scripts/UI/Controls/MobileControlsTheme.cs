using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileControlsTheme : MonoBehaviour
{
    [SerializeField] Image MoveJoystickImage;

    public void UpdateCharacterBasedVisuals(CharacterDataManager.Characters characterToBe)
    {
        MoveJoystickImage.material = PersistentScopeManagers.Instance.GetComponent<CharacterDataManager>().GetCharactersData(characterToBe).PlayerColorsMaterial;
    }
}
