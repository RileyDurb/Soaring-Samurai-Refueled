using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuButtonFunctions : MonoBehaviour
{
    [SerializeField] GameObject DebugMenuPrefab;
    public void ExitGame()
    {
        SimManager.Instance.Exit();
    }

    public void OpenDebugMenu()
    {
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PushMenu(DebugMenuPrefab);
    }

    public void GoBackFromPauseMenu()
    {
        MenuManager menuMan = LevelScopeManagers.Instance.GetComponent<MenuManager>();

        menuMan.PopMenu();

        SimManager.Instance.SetPaused(false);
    }
}
