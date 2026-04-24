using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

public class SimManager : MonoBehaviour
{
    // Editor Accessible variables ////////////////////////////////////////////////////////////////////////
    public float TestSlapStrength = 30.0f;
    public float TestKnockbackEqualizationPercent = 1.0f;
    public float TestKnockbackDuration = 0.3f;

    // Events /////////////////////////////////////////////////////////////////////////////////////////////
    public Action GameEnd;
    public Action<bool> DebugModeStateChanged;
    public Action<bool> OnPausedChange;

    // Instance ///////////////////////////////////////////////////////////////////////////////////////////
    public static SimManager Instance;

    public AttackDataObject mDebugSlapStats;

    // Private Variables //////////////////////////////////////////////////////////////////////////////////
    Dictionary<string, GameObject> mPrefabs = new Dictionary<string, GameObject>();
    GameObject mTempPauseMenuSpawnedPrefab;

    bool mInPause = false;
    private bool IsPaused { 
        get { return mInPause; } 
        set { mInPause = value; if (OnPausedChange != null) { OnPausedChange(mInPause); } }
    }
    // Debug related
    bool mDebugMode = false;
    bool mAllowDebug = true;

    Vector2 mTestSlapDirection = Vector2.left;

    /* Debug hotkeys
    // / - debug mode
    // X - test slap your player with side to side knockback
    */

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        // Singleton, keep around until game shutdown
        DontDestroyOnLoad(gameObject);

        LoadPrefabs();
    }

    // Update is called once per frame
    void Update()
    {

        //if (/*Input.GetKeyUp(KeyCode.G)*/true)
        //{
        //    GameObject dashButton = GameObject.Find("DashButton");
        //    if (dashButton != null)
        //    {
        //        print("CurrStickMag" + dashButton.GetComponent<OnScreenButton>().control.EvaluateMagnitude().ToString());
        //    }
        //}
        if (DebugModeOn == true)
        {
            if (Input.GetKeyUp(KeyCode.X))
            {
                GameObject player = GameObject.Find("Player1");
                if (player != null)
                {
                    player.GetComponent<PlayerCombatController>().TakeDamage(new Hitbox.AttackCurrentData(mTestSlapDirection * TestSlapStrength, 1), mDebugSlapStats.mStats);
                    PlayerCombatController combatCont = player.GetComponent<PlayerCombatController>();
                    Vector2 currScale = player.transform.localScale;
                    combatCont.mActionList.AddActionScale(player, new Vector2(currScale.x, currScale.y * 1.2f), .1f);
                    combatCont.mActionList.AddActionScale(player, new Vector2(currScale.x, currScale.y), .1f, .1f);

                    mTestSlapDirection *= -1;
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P) || Input.GetKey(KeyCode.Menu))
        {
            MenuManager menuManager = LevelScopeManagers.Instance.GetComponent<MenuManager>();
            if (mInPause == false)
            {
                menuManager.PushMenu(mPrefabs["PauseMenu"]);
                IsPaused = true;
            }
            else
            {
                menuManager.PopMenu();
                IsPaused = false;
            }
        }
    }

    //private void OnApplicationQuit()
    //{
    //    CallGameEnd();
    //}

    // Getters and setters //////////////////////////////////////////////////////////////////////////////////
    public GameObject GetPrefab(string name)
    {
        return mPrefabs[name];
    }
    public bool DebugModeOn
    { get { return mDebugMode; }
      set
        {
            bool lastDebugModeState = mDebugMode;
            mDebugMode = value;

            if (lastDebugModeState != mDebugMode)
            {
                if (DebugModeStateChanged != null)
                {
                    DebugModeStateChanged.Invoke(mDebugMode);
                }
            }
        } 
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

    void CallGameEnd()
    {
        // Calls game end event
        if (GameEnd != null)
        {
            GameEnd.Invoke();
        }
    }


    // Misc usage functions /////////////////////////////////////////////////////////////////////////////////
    public void Exit()
    {
        CallGameEnd();
        // Does proper exit, based on if in editor or a build
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    // Public getter for changing what happens when wantimg to set paused publicly, vs within the sim manager
    public void SetPaused(bool newPaused)
    {
        IsPaused = newPaused;
    }

}
