using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInputPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI PlayerNumberText;
    [SerializeField] TextMeshProUGUI PlayerInputTypeText;
    [SerializeField] Image PlayerInputTypeIcon;

    [SerializeField] InputTypeData InputTypeDataObject;

    int mPlayerIndex = 0;

    public void SetPlayerIndexNumber(int playerIndex)
    {
        mPlayerIndex = playerIndex;

        if (mPlayerIndex >= 0)
        {
            PlayerNumberText.text = "Player " + (mPlayerIndex + 1);
        }
        else // If no player
        {
            PlayerNumberText.text = "";
        }
    }

    public void SetInputTypeName(string inputTypeName)
    {
        PlayerInputTypeText.text = inputTypeName;

        // Set icon sprite if one is defined
        InputTypeData.InputTypeVisuals typeVisualInfo = InputTypeDataObject.GetInputTypeVisuals(inputTypeName);
        if (typeVisualInfo != null)
        {
            PlayerInputTypeIcon.sprite = typeVisualInfo.IconSprite;
            PlayerInputTypeIcon.enabled = true;
        }
        else
        {
            PlayerInputTypeIcon.enabled = false;
        }
    }
}
