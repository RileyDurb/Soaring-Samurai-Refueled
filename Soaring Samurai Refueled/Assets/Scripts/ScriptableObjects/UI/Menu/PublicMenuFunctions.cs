using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PublicMenuFunctions", menuName = "Scripts/ScriptableObjects/UI/Menu/PublicMenuFunctions")]
public class PublicMenuFunctions : ScriptableObject
{
    public void GoBackInPauseMenu()
    {
        MenuManager currentMenuManager = LevelScopeManagers.Instance.GetComponent<MenuManager>();

        if (currentMenuManager == null)
        {
            Console.Write("PublicMenuFunctions:GoBackInMenu: level scope manager could be found by the static go back in menu function, so no context to go back from. Make sure one exists in the scene, likely by adding the LevelScopeManagersHolder prefab");
            return;
        }

        currentMenuManager.PopPauseMenu();
    }

    public void ExitGame()
    {
        SimManager.Instance.Exit();
    }
};
