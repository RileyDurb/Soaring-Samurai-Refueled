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
    [SerializeField] float mNonMaxDistanceBoundsMargin = 0.0f; // When bounds are not at max, the margin it gives on each side
   [SerializeField] Vector2 mMaxMoveBounds = new Vector2(50, 50); // Max xy bounds the move bounds will stretch to


    // Tracked target values
    Vector3 mCurrPlayersCenter = new Vector3();
    Vector2 mMaxDistFromCenter = new Vector2();
    Vector3 mCurrPos = new Vector3();


    // Start is called before the first frame update
    void Start()
    {
        mCamRef = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        // Get the player's average center position
        mCurrPlayersCenter = mCamRef.GetComponent<CameraFollow>().CurrFollowTarget;

        // Update Scale
        List<PlayerCombatController> mPlayers = GameObject.Find("MatchManager").GetComponent<MatchStateManager>().PlayerList; // Get player list

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

        // Add margins
        Vector2 currBoundsHalfLength = maxPlayerCenterDistances;

        currBoundsHalfLength += new Vector2(mNonMaxDistanceBoundsMargin, mNonMaxDistanceBoundsMargin) / 2.0f;

        // Clamp to max bounds
        currBoundsHalfLength.Set(Mathf.Clamp(currBoundsHalfLength.x, 0.0f, mMaxMoveBounds.x), Mathf.Clamp(currBoundsHalfLength.y, 0.0f, mMaxMoveBounds.y));

        // Set the scale
        //mMovementBoundsObject.transform.lossyScale.Set(currBoundsHalfLength.x, currBoundsHalfLength.y, mMovementBoundsObject.transform.lossyScale.z);
        mMovementBoundsObject.transform.localScale.Set(1, 1, mMovementBoundsObject.transform.lossyScale.z);

        // Update Position
        UpdateMaxCenterDistance();

        mCurrPos = new Vector3(Mathf.Clamp(mCurrPlayersCenter.x, -Mathf.Abs(mMaxDistFromCenter.x), Mathf.Abs(mMaxDistFromCenter.x))
                              , Mathf.Clamp(mCurrPlayersCenter.y, -Mathf.Abs(mMaxDistFromCenter.y), Mathf.Abs(mMaxDistFromCenter.y))
                              , mCurrPlayersCenter.z);
        mMovementBoundsObject.transform.position = mCurrPos;

        //Transform[] childTransforms = mMovementBoundsObject.transform.GetComponentsInChildren<Transform>();

        //foreach (Transform childTransform in childTransforms)
        //{
        //    childTransform.lossyScale.Set(currBoundsHalfLength.x, currBoundsHalfLength.y, childTransform.transform.lossyScale.z);
        //}

    }
    
    void UpdateMaxCenterDistance()
    {
        Debug.DrawLine(new Vector3(), new Vector3(-mBackgroundImage.GetComponent<SpriteRenderer>().bounds.size.x / 2.0f, 0.0f, 0.0f));
        Debug.DrawLine(new Vector3(), new Vector3(0.0f, -mBackgroundImage.GetComponent<SpriteRenderer>().bounds.size.y / 2.0f, 0.0f), Color.red);

        // Calculate max distance bounds can be from center on x axis
        float xBackgroundExtent = mBackgroundImage.GetComponent<SpriteRenderer>().bounds.size.x / 2.0f;
        float minDistanceFromEdgeX = xBackgroundExtent - mMovementBoundsObject.transform.lossyScale.x / 2.0f;
        mMaxDistFromCenter.x = Mathf.Abs(minDistanceFromEdgeX) - Mathf.Abs(mCurrPlayersCenter.x);

        // Calculate max distance bounds can be from center on y axis
        float yBackgroundExtent = mBackgroundImage.GetComponent<SpriteRenderer>().bounds.size.y / 2.0f;
        float minDistanceFromEdgeY = yBackgroundExtent - mMovementBoundsObject.transform.lossyScale.y / 2.0f;
        mMaxDistFromCenter.y = Mathf.Abs(minDistanceFromEdgeY) - Mathf.Abs(mCurrPlayersCenter.y);

        Debug.DrawLine(new Vector3(), new Vector3(-mMaxDistFromCenter.x, 0.0f, 0.0f), Color.blue);
        Debug.DrawLine(new Vector3(), new Vector3(0.0f, -mMaxDistFromCenter.y, 0.0f), Color.blue);

    }

}
