using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuButtonFunctions : MonoBehaviour
{
    [SerializeField] GameObject DebugMenuPrefab;
    [SerializeField] GameObject MovesListMenuPrefab;
    [SerializeField] GameObject PlayerControllersMenuPrefab;
    [SerializeField] GameObject SettingsMenuPrefab;
    public void ExitGame()
    {
        SimManager.Instance.Exit();
    }

    public void OpenDebugMenu()
    {
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(DebugMenuPrefab);
    }

    public void OpenMovesListMenu()
    {
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(MovesListMenuPrefab);
    }

    public void OpenCheckPlayersMenu()
    {
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(PlayerControllersMenuPrefab);
    }

    public void OpenSettingsMenu()
    {
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(SettingsMenuPrefab);
    }

    public void GoBackFromPauseMenu()
    {
        SimManager.Instance.QueuePauseMenuChange();
    }
}
