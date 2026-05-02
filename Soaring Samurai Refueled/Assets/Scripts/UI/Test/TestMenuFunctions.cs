using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMenuFunctions : MonoBehaviour
{
    [SerializeField] GameObject TestNextMenu;
    public void CreateNextPauseMenu()
    {
        MenuManager menuManager = LevelScopeManagers.Instance.GetComponent<MenuManager>();

        if (TestNextMenu == null)
        {
            print("TestMenuFunctions:CreateNextMenu: No menu prefab set, cannot create next menu. Try setting a prefab for the TestNextMenu variable");
            return;
        }
        menuManager.PushPauseMenu(TestNextMenu);
    }

    public void GoBackInPauseMenu()
    {
        MenuManager menuManager = LevelScopeManagers.Instance.GetComponent<MenuManager>();

        menuManager.PopPauseMenu();
    }
}
