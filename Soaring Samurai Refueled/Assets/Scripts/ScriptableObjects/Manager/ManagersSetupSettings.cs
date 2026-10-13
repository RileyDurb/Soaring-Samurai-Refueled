using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ManagerSetupSettings", menuName = "Scripts/ScriptableObjects/Manager/ManagersSetupSettings")]
public class ManagersSetupSettings : ScriptableObject
{
    public GameObject LevelScopedManagersPrefab;
    public bool UseLevelScopedManagers = false;
    public GameObject PersistentScopedManagersPrefab;
    public bool UsePersistentScopedManagers = true;

}
