using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

    // Instance ///////////////////////////////////////////////////////////////////////////////////////////
    public static SimManager Instance;

    public AttackDataObject mDebugSlapStats;

    // Private Variables //////////////////////////////////////////////////////////////////////////////////
    Dictionary<string, GameObject> mPrefabs = new Dictionary<string, GameObject>();
    GameObject mTempPauseMenuSpawnedPrefab;

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
        Instance = this;

        // Singleton, keep around until game shutdown
        DontDestroyOnLoad(gameObject);

        LoadPrefabs();
    }

    // Update is called once per frame
    void Update()
    {
        // DEBUG KEY: Toggle debug mode
        if (Input.GetKeyUp(KeyCode.Slash))
        {
            DebugModeOn = !mDebugMode;
        }

        if (mAllowDebug == true)
        {
            if (Input.GetKeyUp(KeyCode.X))
            {
                GameObject player = GameObject.Find("Player1");
                if (player != null)
                {
                    player.GetComponent<PlayerCombatController>().TakeDamage(new Hitbox.AttackCurrentData(mTestSlapDirection * TestSlapStrength), mDebugSlapStats.mStats);
                    PlayerCombatController combatCont = player.GetComponent<PlayerCombatController>();
                    Vector2 currScale = player.transform.localScale;
                    combatCont.mActionList.AddActionScale(player, new Vector2(currScale.x, currScale.y * 1.2f), .1f);
                    combatCont.mActionList.AddActionScale(player, new Vector2(currScale.x, currScale.y), .1f, .1f);

                    mTestSlapDirection *= -1;
                }
            }

            if (Input.GetKeyUp(KeyCode.Minus) || Input.GetKeyUp(KeyCode.Escape))
            {
                if (mTempPauseMenuSpawnedPrefab == null)
                {
                    mTempPauseMenuSpawnedPrefab = Instantiate(mPrefabs["PauseMenu"], GameObject.Find("Canvas").transform);
                }
                else
                {
                    Destroy(mTempPauseMenuSpawnedPrefab);
                    mTempPauseMenuSpawnedPrefab = null;
                }
            }
            //if (/*Input.GetKeyUp(KeyCode.G)*/true)
            //{
            //    GameObject dashButton = GameObject.Find("DashButton");
            //    if (dashButton != null)
            //    {
            //        print("CurrStickMag" + dashButton.GetComponent<OnScreenButton>().control.EvaluateMagnitude().ToString());
            //    }
            //}
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



}
