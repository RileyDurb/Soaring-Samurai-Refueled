using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtonFunctions : MonoBehaviour
{
    [SerializeField] GameObject mSettingsMenuPrefab;
    [SerializeField] GameObject mCODAQuitPrefab;
    [SerializeField] string mQuitGameMessage;

    public void StartGame()
    {
        SceneManager.LoadScene("TestArena");
    }

    public void OpenSettingsMenu()
    {
        MenuManager menuManager = LevelScopeManagers.Instance.GetComponent<MenuManager>();
        menuManager.PushPauseMenu(mSettingsMenuPrefab);
    }

    public void ExitGame()
    {
        GameObject codaQuitPanel = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(mCODAQuitPrefab);

        GenericCODAFunctions codaPanelFunctionality = codaQuitPanel.GetComponent<GenericCODAFunctions>();

        codaPanelFunctionality.SetQuitMessage(mQuitGameMessage);

        // Make the confirmation of destructive action panel close the CODA on cancelling
        codaPanelFunctionality.OnNoEvent += () => { LevelScopeManagers.Instance.GetComponent<MenuManager>().PopPauseMenu(); };

        // Make the confirmation of destructive action panel quit the game on confirming
        codaPanelFunctionality.OnYesEvent += () => { SimManager.Instance.Exit(); };
        
    }
}
