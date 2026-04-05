using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for attaching manager scripts to, for easy access, and making their lifetime scoped to the current level
public class LevelScopeManagers : MonoBehaviour
{
    public static LevelScopeManagers Instance;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

}
