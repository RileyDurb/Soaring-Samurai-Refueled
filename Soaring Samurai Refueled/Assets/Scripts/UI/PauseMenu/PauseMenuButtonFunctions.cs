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
}
