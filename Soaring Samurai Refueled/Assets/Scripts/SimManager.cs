using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class SimManager : MonoBehaviour
{
    // Events /////////////////////////////////////////////////////////////////////////////////////////////
    public Action GameEnd;

    // Instance ///////////////////////////////////////////////////////////////////////////////////////////
    public static SimManager Instance;

    // Private Variables //////////////////////////////////////////////////////////////////////////////////
    Dictionary<string, GameObject> mPrefabs = new Dictionary<string, GameObject>();
    bool mDebugMode = false;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // Singleton, keep around until game shutdown
        DontDestroyOnLoad(gameObject);

        LoadPrefabs();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            DebugMode = !mDebugMode;
        }
    }

    // Getters and setters //////////////////////////////////////////////////////////////////////////////////
    public GameObject GetPrefab(string name)
    {
        return mPrefabs[name];
    }
    public bool DebugMode
    { get { return mDebugMode; }
      set
        {
            // NOTE: when functionality needs to respond to debug mode being turned on/ off, can add an event call here to handle that
            mDebugMode = value;
        } 
    }
    public void SetDebugMode(bool debugOn)
    {

        mDebugMode = debugOn;
    }

    public bool IsDebugMode()
    {
        return mDebugMode;
    }
    // Helper functions /////////////////////////////////////////////////////////////////////////////////////
    void LoadPrefabs()
    {
        UnityEngine.Object[] prefabs = Resources.LoadAll("Prefabs", typeof(GameObject));

        foreach (GameObject prefab in prefabs)
        {
            mPrefabs.Add(prefab.name, prefab);
        }
    }

    // Misc usage functions /////////////////////////////////////////////////////////////////////////////////
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
