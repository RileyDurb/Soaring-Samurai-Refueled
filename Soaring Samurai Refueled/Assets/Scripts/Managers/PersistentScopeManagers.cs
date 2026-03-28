using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for making a persistent singleton that stays between levels. For attaching persistent manager scripts on the same prefab this script is on, so they can be accessed in one place
public class PersistentScopeManagers : MonoBehaviour
{
    public static PersistentScopeManagers Instance;
    // Start is called before the first frame update
    void Awake()
    {
        // If this is the first awake of the manager
        if (Instance == null)
        {
            // Set this as the signgleton instance
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            // Instance already exists, destroy this new one
            Destroy(this);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
