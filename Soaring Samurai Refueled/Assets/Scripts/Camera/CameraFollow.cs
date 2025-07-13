using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class CameraFollow : MonoBehaviour
{
    enum FollowMode
    {
        AllPlayers,
        TargetFirstPlayer,
        NoFollow,
        ManualSetTarget
    }

    // Editor accessible values
    [Header("Tunable Vlues")]
    public float CameraMoveSpeed = 1.0f;
    public float CameraZoomSpeed = 5.0f;
    public float MinZoomMarginSpace = 10.0f; // gives at least this much space on the sides of players
    public float MaxZoomMarginSpace = 50.0f; // Maximum space on the sides of players
    [SerializeField] FollowMode mCurrFollowMode = FollowMode.AllPlayers;


    [Header("In Game Set Values")]
    public Vector3 CurrFollowTarget = new Vector3();
    public float CurrTargetVelocityMag = 0.0f;

    // Private variables

    GameObject mDebugFirstPlayer;
    GameObject mManualFollowObject = null;
    List<GameObject> mFollowObjects = new List<GameObject>();

    FollowMode mFollowModeBeforeManual = FollowMode.NoFollow; // temp value for what the mode was before manual follow was set
    float mBaseOrthographicSize = 0.0f;
    float mMinOrthographicSize = 0.0f; // Calculated min camera zoom based on distance between players
    float mMaxOrthographicSize = float.MaxValue; // Calculated max camera zoom based on distance between players

    // Getters and setters

    // Start is called before the first frame update
    void Start()
    {
        mDebugFirstPlayer = GameObject.Find("Player");
        mBaseOrthographicSize = GetComponent<Camera>().orthographicSize;

        // Add each player as a follow object
        List<PlayerCombatController> players = GameObject.Find("MatchManager").GetComponent<MatchStateManager>().PlayerList;

        foreach (PlayerCombatController player in players)
        {
            mFollowObjects.Add(player.gameObject);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (mDebugFirstPlayer == null)
        {
            mDebugFirstPlayer = GameObject.Find("Player");
            if (mDebugFirstPlayer == null )
            {
                return;
            }
        }

        // Calculate target position
        // If mode set to no follow, just return, don't do follow behaviour (likely wanting something else to take over
        switch (mCurrFollowMode)
        {
            case FollowMode.NoFollow:
                {
                    return;
                }
            case FollowMode.AllPlayers:
                {
                    CurrFollowTarget = CalculateFollowTargetAllPlayers();
                    break;
                }
            case FollowMode.TargetFirstPlayer:
                {
                    CurrFollowTarget = mDebugFirstPlayer.transform.position; // Target the single player object, intened just for debug functionality
                    break;
                }
            case FollowMode.ManualSetTarget:
                {
                    CurrFollowTarget = mManualFollowObject.transform.position; // Use manually set target
                    break;
                }
        }
        
        // Lerp position closer to target position
        CurrFollowTarget = new Vector3(CurrFollowTarget.x, CurrFollowTarget.y, transform.position.z);
        transform.position = transform.position + (CurrFollowTarget - transform.position) * CameraMoveSpeed * Time.deltaTime;
        //transform.position = targetPos;

        // Calculate zoom

        // Find target velocity based on follow mode
        switch (mCurrFollowMode)
        {
            case FollowMode.NoFollow:
                {
                    return;
                }
            case FollowMode.AllPlayers:
                {
                    CurrTargetVelocityMag = CalculateFollowVelocityAllPlayers();

                    // Calculate min zoom

                    // find largest distance between players
                    float maxPlayerDistance = 0.0f;
                    Vector2 maxDistanceVec = Vector2.zero;
                    for (int i = 0; i < mFollowObjects.Count; i++)
                    {
                        GameObject currTarget = mFollowObjects[i];
                        for (int j = i + 1;  j < mFollowObjects.Count; j++)
                        {
                            Vector2 vecBetweenPlayers = currTarget.transform.position - mFollowObjects[j].transform.position;
                            float distance = vecBetweenPlayers.magnitude;

                            if (distance > maxPlayerDistance)
                            {
                                maxPlayerDistance = distance;
                                maxDistanceVec = vecBetweenPlayers;
                            }
                        }
                    }

                    // Calculate camera zoom

                    // Calculate target zoom for if players are perfectly horizontal, and vertical
                    float verticalFittingZoom = maxPlayerDistance;
                    float horizontalFittingZoom = maxPlayerDistance * Screen.height / Screen.width;

                    float closenessToBeingVertical = Mathf.Abs(Vector2.Dot(Vector2.up, maxDistanceVec.normalized));

                    // Lerp between the horizontal and vertical zooms to match the current player orientation, between those targets
                    float cameraZoomToFitPlayersX2 = Mathf.Lerp(horizontalFittingZoom, verticalFittingZoom, closenessToBeingVertical);

                    // Apply margin space, and convert to half height, as that's what the orthogonal size we use this value for is
                    mMinOrthographicSize = (cameraZoomToFitPlayersX2 + MinZoomMarginSpace) * 0.5f; // Sets min target zoom to be the max gap between players plus the given margin
                    mMaxOrthographicSize = (cameraZoomToFitPlayersX2 + MaxZoomMarginSpace) * 0.5f; // Sets max target zoom to be the max gap between players plus the given margin
                    break;
                }
            case FollowMode.TargetFirstPlayer:
                {
                    CurrTargetVelocityMag = mDebugFirstPlayer.GetComponent<Rigidbody2D>().velocity.magnitude; // Target the single player object, intened just for debug functionality
                    mMinOrthographicSize = 0.0f; // No minimum
                    mMaxOrthographicSize = float.MaxValue; // No set max
                    break;
                }
            case FollowMode.ManualSetTarget:
                {
                    CurrTargetVelocityMag = mManualFollowObject.GetComponent<Rigidbody2D>().velocity.magnitude;  // Use manually set target

                    mMinOrthographicSize = 0.0f; // No minimum
                    mMaxOrthographicSize = float.MaxValue; // No set max
                    break;
                }
        }
        // Set the zoom
        //SetZoom(CurrTargetVelocityMag);
        //SetZoomImmediate();
        SetZoomOnlyCamSpeedBased();
    }

    // Helpers //////////////////////////////////////////////////////////////////////////////

    // Main zoom setting function
    void SetZoomOnlyCamSpeedBased()
    {
        float targetValue = mMinOrthographicSize;
        GetComponent<Camera>().orthographicSize = GetComponent<Camera>().orthographicSize + (targetValue - GetComponent<Camera>().orthographicSize) * CameraZoomSpeed * Time.deltaTime;
    }

    // Older zoom setting, scaling up as player speeds were higher
    void SetZoomPlayerSpeedBased(float playerSpeedMagnitude)
    {
        //playerSpeedMagnitude = 0.0f;
        // Figure out whether zooming in or out
        bool isZoomingIn = false;

        float tempTargetValue = Mathf.Clamp(GetComponent<Camera>().orthographicSize, mMinOrthographicSize, mMaxOrthographicSize);

        if (mMinOrthographicSize < GetComponent<Camera>().orthographicSize && playerSpeedMagnitude > 1.0f )
        {
            isZoomingIn = true;
        }


        if (isZoomingIn == true)
        {
            playerSpeedMagnitude = 0.0f; // Zoom in faster instead of zooming out faster
        }

        // Set new zoom based on whether we're zooming in or out, and the zoom bounds
        float targetValue = mBaseOrthographicSize * (1 + playerSpeedMagnitude / mBaseOrthographicSize);
        //if (isZoomingIn)
        //{
           targetValue = Mathf.Clamp(targetValue, mMinOrthographicSize, mMaxOrthographicSize); // Clamp zoom to match distance between players
        //}
        //else
        //{
        //    targetValue = Mathf.Clamp(targetValue, mMinOrthographicSize, mMaxOrthographicSize); // Clamp zoom to match distance between players
        //}

        //if (SimManager.Instance.DebugMode)
        //{
        //    print("SetZoom: orthoSize: " + GetComponent<Camera>().orthographicSize.ToString() + ", targetValue: " + targetValue.ToString() + ", tempTargetValue: " + tempTargetValue.ToString() + ", isZoomingIn: " + isZoomingIn.ToString());
        //}

        GetComponent<Camera>().orthographicSize = GetComponent<Camera>().orthographicSize + (targetValue - GetComponent<Camera>().orthographicSize) * Time.deltaTime;
    }

    // Zoom setting option that sets zoom immediately, without lerping. Probably just find to use this, but the added lerping with a high speed looks nice, and has the same function
    void SetZoomImmediate()
    {
        float targetValue = mMinOrthographicSize;
        GetComponent<Camera>().orthographicSize = targetValue;
    }



    // Calculate the position to lerp towards, when following all players
    Vector3 CalculateFollowTargetAllPlayers()
    {
        // Should always ahave a follow target if calling this, print error if not
        if (mFollowObjects.Count <= 0)
        {
            print("CameraFollow:CalculateFollowTarget: In follow mode but no follow objects exist");
            return new Vector3();
        }
            
        // Get the midpoint of all follow objects
        float averageX = 0;
        float averageY = 0;
        float followZ = transform.position.z;

        foreach (GameObject followPoint in mFollowObjects)
        {
            averageX += followPoint.transform.position.x;
            averageY += followPoint.transform.position.y;
        }

        averageX /= mFollowObjects.Count;
        averageY /= mFollowObjects.Count;

        Vector3 followTarget = new Vector3(averageX, averageY, followZ);
        return followTarget;
    }

    // For the older velocity based zoom, calculates the total velocity to use to scale the zoom
    float CalculateFollowVelocityAllPlayers()
    {
        if (mFollowObjects.Count <= 0)
        {
            print("CameraFollow:CalculateFollowZoom: In follow mode but no follow objects exist");
            return 0.0f;
        }

        float totalVelocityMag = 0.0f;

        for (int i = 0; i < mFollowObjects.Count; i++)
        {
            totalVelocityMag += mDebugFirstPlayer.GetComponent<Rigidbody2D>().velocity.magnitude;
        }


        return totalVelocityMag;
    }


    // //////////////////////////////////////////////////////////////////////////////
    // Public Interface /////////////////////////////////////////////////////////////
    public void InstantMoveToTarget()
    {
        Vector3 targetPos = CurrFollowTarget;
        targetPos = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        transform.position = targetPos;
    }

    // Turns to following the given object, instead of calculating it's own target
    public void ActivateManualFollow(GameObject followTarget)
    {
        mManualFollowObject = followTarget;
        mFollowModeBeforeManual = mCurrFollowMode; // Saves current follow mode for restoring later
        mCurrFollowMode = FollowMode.ManualSetTarget;
    }

    // Deactivates manual follow mode, returning to whatever the previous follow mode was
    public void DeactivateManualFollow()
    {
        mManualFollowObject = null;
        mCurrFollowMode = mFollowModeBeforeManual;
    }

}
