using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuButtonFunctions : MonoBehaviour
{
    [SerializeField] GameObject DebugMenuPrefab;
    [SerializeField] GameObject MovesListMenuPrefab;
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

    public void GoBackFromPauseMenu()
    {
        MenuManager menuMan = LevelScopeManagers.Instance.GetComponent<MenuManager>();

        menuMan.PopPauseMenu();

        SimManager.Instance.SetPaused(false);
    }
}
