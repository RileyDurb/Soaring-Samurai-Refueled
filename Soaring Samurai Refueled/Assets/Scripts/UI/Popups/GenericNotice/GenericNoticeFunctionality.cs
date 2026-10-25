using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GenericNoticeFunctionality : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI MessageText;
    MenuManager mMenuMan;

    public void Start()
    {
        mMenuMan = LevelScopeManagers.Instance.GetComponent<MenuManager>();
    }

    
    // Public interface
    public void SetMessageText(string message)
    {
        MessageText.text = message;
    }
}
