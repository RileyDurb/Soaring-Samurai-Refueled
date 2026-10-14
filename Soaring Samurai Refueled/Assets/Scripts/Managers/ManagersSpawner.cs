using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagersSpawner : MonoBehaviour
{
    [SerializeField] ManagersSetupSettings mManagerSetupSettings;

    // Start is called before the first frame update
    void Awake()
    {
        if (mManagerSetupSettings.UsePersistentScopedManagers)
        {
            if (PersistentScopeManagers.Instance == null) // if not already spawned
            {
                Instantiate(mManagerSetupSettings.PersistentScopedManagersPrefab);
            }
        }

        if (mManagerSetupSettings.UseLevelScopedManagers)
        {
            if (LevelScopeManagers.Instance == null) // if not already spawned
            {
                Instantiate(mManagerSetupSettings.LevelScopedManagersPrefab);
            }
        }
    }
}
