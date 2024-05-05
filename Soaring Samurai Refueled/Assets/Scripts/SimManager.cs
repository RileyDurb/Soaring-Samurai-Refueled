using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class SimManager : MonoBehaviour
{
    public Action GameEnd;

    public static SimManager Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // Singleton, keep around until game shutdown
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void Exit()
    {

        // Calls game end event
        if (GameEnd != null)
        {
            GameEnd.Invoke();
        }

        // Does proper exit, based on if in editor or a build
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }
}
