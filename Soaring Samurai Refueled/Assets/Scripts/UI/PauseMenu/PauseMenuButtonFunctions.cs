using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuButtonFunctions : MonoBehaviour
{
    [SerializeField] GameObject DebugMenuPrefab;
    [SerializeField] GameObject MovesListMenuPrefab;
    [SerializeField] GameObject PlayerControllersMenuPrefab;
    [SerializeField] GameObject SettingsMenuPrefab;
    [SerializeField] GameObject ControlsRebindingMenuPrefab;
    [SerializeField] GameObject RuntimeStatsEditingMenuPrefab;
    [SerializeField] GameObject CODAQuitPrefab;
    [SerializeField] string QuitToMainMenuMessage;
    [SerializeField] string QuitGameMessage;
    public void ExitGame()
    {
        GameObject codaQuitPanel = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(CODAQuitPrefab);

        GenericCODAFunctions codaPanelFunctionality = codaQuitPanel.GetComponent<GenericCODAFunctions>();

        codaPanelFunctionality.SetQuitMessage(QuitGameMessage);

        // Make the confirmation of destructive action panel close the CODA on cancelling
        codaPanelFunctionality.OnNoEvent += () => { LevelScopeManagers.Instance.GetComponent<MenuManager>().PopPauseMenu(); };

        // Make the confirmation of destructive action panel quit the game on confirming
        codaPanelFunctionality.OnYesEvent += () => { SimManager.Instance.Exit(); };
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

    // NOTE: Later make this as a submenu of an overall settings menu, along with the audio settings menu, and have the settings button open that
    public void OpenTestControlsRebindingMenu()
    {
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(ControlsRebindingMenuPrefab);
    }

    public void OpenStatsEditingMenu()
    {
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(RuntimeStatsEditingMenuPrefab);
    }

    public void GoBackFromPauseMenu()
    {
        SimManager.Instance.QueuePauseMenuChange();
    }

    public void QuitToMainMenu()
    {
        GameObject codaQuitPanel = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(CODAQuitPrefab);

        GenericCODAFunctions codaPanelFunctionality = codaQuitPanel.GetComponent<GenericCODAFunctions>();

        codaPanelFunctionality.SetQuitMessage(QuitToMainMenuMessage);

        // Make the confirmation of destructive action return to menu options on cancelling
        codaPanelFunctionality.OnNoEvent += ()=>{ LevelScopeManagers.Instance.GetComponent<MenuManager>().PopPauseMenu(); };

        // Make the confirmation of destructive action return to the main menu on confirming
        codaPanelFunctionality.OnYesEvent += ()=>{ SimManager.Instance.ReturnToMainMenu(); };
    }
}
