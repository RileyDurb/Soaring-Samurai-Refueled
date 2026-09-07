using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourMovementBounds : MonoBehaviour
{
    // NOTE: Uses some calculations from camera follow, so ideally, camera follow is before this in the script execution order

    // Public Variables

    // Private Variables //////////////////////////////////////////////////
    [SerializeField] private GameObject mMovementBoundsObject = null;
    [SerializeField] private GameObject mBackgroundImage = null; // Either remove this ref, or change to background container class ref when that system is created. For now it's just an image
    private GameObject mCamRef = null; // Camera to use for positiining
    StageStats mStats;

    DebugSpriteView mMovementBoundsDebugSpriteView;



    // Tracked target values
    Vector3 mCurrPlayersCenter = new Vector3();
    Vector2 mMaxDistFromCenter = new Vector2();
    Vector3 mCurrPos = new Vector3();

    Vector2 mCurrPlayerGapBounds = Vector2.zero;

    // Getters and setters
    public Vector2 MovementBoundsObjectScale { get { return mMovementBoundsObject.GetComponent<MovementBoundsObject>().GetBoundDimensions(); } }

    
    // Start is called before the first frame update
    void Start()
    {
        mCamRef = GameObject.Find("Main Camera");
        mStats = GetComponent<StageDataManager>().mStageStats;
        GetComponent<StageDataManager>().mOnStageChanged += UpdateInfoFromStageChange;
        mMovementBoundsDebugSpriteView = mMovementBoundsObject.GetComponent<DebugSpriteView>();
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate follow target


        // Get the player's average center position
        mCurrPlayersCenter = mCamRef.GetComponent<CameraFollow>().CurrFollowTarget;

        // Update Scale
        List<PlayerCombatController> mPlayers = LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerList; // Get player list

        //mCamRef.GetComponent<Camera>().orthographicSize

        // Get max x and y separations between players
        Vector2 maxPlayerCenterDistances = new Vector2();
        //Vector2 maxDistanceVec = Vector2.zero;
        for (int i = 0; i < mPlayers.Count; i++)
        {
            PlayerCombatController currPlayer = mPlayers[i];
            
            Vector2 vectorFromCenter = mCurrPlayersCenter - currPlayer.transform.position;

            // Update max distance from center if we've found a greater one
            if (Mathf.Abs(vectorFromCenter.x) > maxPlayerCenterDistances.x)
            {
                maxPlayerCenterDistances.Set(Mathf.Abs(vectorFromCenter.x), maxPlayerCenterDistances.y);
            }

            if (Mathf.Abs(vectorFromCenter.y) > maxPlayerCenterDistances.y)
            {
                maxPlayerCenterDistances.Set(maxPlayerCenterDistances.x, Mathf.Abs(vectorFromCenter.y));
            }
        }


        // Update Position
        UpdateMaxCenterDistance();

        mCurrPos = new Vector3(Mathf.Clamp(mCurrPlayersCenter.x, -Mathf.Abs(mMaxDistFromCenter.x), Mathf.Abs(mMaxDistFromCenter.x))
                              , Mathf.Clamp(mCurrPlayersCenter.y, -Mathf.Abs(mMaxDistFromCenter.y), Mathf.Abs(mMaxDistFromCenter.y))
                              , mCurrPlayersCenter.z);
        mMovementBoundsObject.transform.position = mCurrPos;


        // Detecting stat changes


        if (mCurrPlayerGapBounds != mStats.MaxPlayerSpacingBounds)
        {
            // Set size for how much space can be between players before they are blocked by boundaries
            mMovementBoundsObject.GetComponent<MovementBoundsObject>().SetBoundDimensions(mStats.MaxPlayerSpacingBounds);
        }

        if (mStats.ShowMaxMoveBoundLines)
        {
            Debug.DrawLine(new Vector3(), new Vector3(-mMaxDistFromCenter.x, 0.0f, 0.0f), Color.blue);
            Debug.DrawLine(new Vector3(), new Vector3(0.0f, -mMaxDistFromCenter.y, 0.0f), Color.blue);
        }

        mMovementBoundsDebugSpriteView.SetShowInGame = mStats.ShowPlayerSpacingBounds;
    }
    
    void UpdateMaxCenterDistance()
    {
        //Debug.DrawLine(new Vector3(), new Vector3(-mBackgroundImage.GetComponent<SpriteRenderer>().bounds.size.x / 2.0f, 0.0f, 0.0f));
        //Debug.DrawLine(new Vector3(), new Vector3(0.0f, -mBackgroundImage.GetComponent<SpriteRenderer>().bounds.size.y / 2.0f, 0.0f), Color.red);

        // Calculate max distance bounds can be from center on x axis
        float xBackgroundExtent = mStats.MaxMoveBounds.x / 2.0f;
        //float minDistanceFromEdgeX = xBackgroundExtent - mMovementBoundsObject.transform.lossyScale.x / 2.0f;
        mMaxDistFromCenter.x = /*Mathf.Abs(minDistanceFromEdgeX) - Mathf.Abs(mCurrPlayersCenter.x);*/ xBackgroundExtent;

        // Calculate max distance bounds can be from center on y axis
        float yBackgroundExtent = mStats.MaxMoveBounds.y / 2.0f;
        //float minDistanceFromEdgeY = yBackgroundExtent - mMovementBoundsObject.transform.lossyScale.y / 2.0f;
        mMaxDistFromCenter.y = /*Mathf.Abs(minDistanceFromEdgeY) - Mathf.Abs(mCurrPlayersCenter.y);*/ yBackgroundExtent;

        //Debug.DrawLine(new Vector3(), new Vector3(-mMaxDistFromCenter.x, 0.0f, 0.0f), Color.blue);
        //Debug.DrawLine(new Vector3(), new Vector3(0.0f, -mMaxDistFromCenter.y, 0.0f), Color.blue);

    }

    void UpdateInfoFromStageChange(StageDataManager.StageInfo newStage)
    {
        mStats = newStage.StatsObject;
    }
}
