using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class GenericCODAFunctions : MonoBehaviour
{
    public Action OnYesEvent;
    public Action OnNoEvent;

    [SerializeField] TextMeshProUGUI mTitleText;

    public void TriggerOnYes()
    {
        OnYesEvent();
    }

    public void TriggerOnNo()
    {
        OnNoEvent();
    }

    public void SetQuitMessage(string messageText)
    {
        mTitleText.SetText(messageText);
    }
}
