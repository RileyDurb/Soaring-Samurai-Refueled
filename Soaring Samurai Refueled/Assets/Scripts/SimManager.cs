using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class SimManager : MonoBehaviour
{
    // Editor Accessible variables ////////////////////////////////////////////////////////////////////////
    public float TestSlapStrength = 30.0f;
    public float TestKnockbackEqualizationPercent = 1.0f;
    public float TestKnockbackDuration = 0.3f;

    // Events /////////////////////////////////////////////////////////////////////////////////////////////
    public Action GameEnd;

    // Instance ///////////////////////////////////////////////////////////////////////////////////////////
    public static SimManager Instance;

    public AttackDataObject mDebugSlapStats;

    // Private Variables //////////////////////////////////////////////////////////////////////////////////
    Dictionary<string, GameObject> mPrefabs = new Dictionary<string, GameObject>();

    // Debug related
    bool mDebugMode = false;
    bool mAllowDebug = true;
    Vector2 mTestSlapDirection = Vector2.left;

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
        // DEBUG KEY: Toggle debug mode
        if (Input.GetKeyUp(KeyCode.D))
        {
            DebugMode = !mDebugMode;
        }

        if (mAllowDebug == true)
        {
            if (Input.GetKeyUp(KeyCode.S))
            {
                GameObject player = GameObject.Find("Player");
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
