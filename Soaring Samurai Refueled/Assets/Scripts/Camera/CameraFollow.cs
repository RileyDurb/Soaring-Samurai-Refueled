using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraFollow : MonoBehaviour
{
    enum FollowMode
    {
        SingleTarget,
        AllPlayers
    }

    // Editor accessible values
    public float CameraSpeed = 1.0f;
    public Vector3 CurrFollowTarget = new Vector3();
    public float CurrTargetVelocityMag = 0.0f;
    // Private variables

    GameObject mFollowObject;
    GameObject mFollowObjects;
    [SerializeField]FollowMode mCurrFollowMode = FollowMode.AllPlayers;
    float mBaseOrthographicSize = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        mFollowObject = GameObject.Find("Player");
        mBaseOrthographicSize = GetComponent<Camera>().orthographicSize;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (mFollowObject == null)
        {
            mFollowObject = GameObject.Find("Player");
            if (mFollowObject == null )
            {
                return;
            }
        }

        // Lerp position closer to target position
        CurrFollowTarget = mFollowObject.transform.position;
        CurrFollowTarget = new Vector3(CurrFollowTarget.x, CurrFollowTarget.y, transform.position.z);
        transform.position = transform.position + (CurrFollowTarget - transform.position) * CameraSpeed * Time.deltaTime;
        //transform.position = targetPos;

        // Set the zoom based on the follow target's velocity
        CurrTargetVelocityMag = mFollowObject.GetComponent<Rigidbody2D>().velocity.magnitude;
        SetZoom(CurrTargetVelocityMag);
    }


    void SetZoom(float playerSpeedMagnitude)
    {
        float targetValue = mBaseOrthographicSize * (1 + playerSpeedMagnitude / mBaseOrthographicSize);
        GetComponent<Camera>().orthographicSize = GetComponent<Camera>().orthographicSize + (targetValue - GetComponent<Camera>().orthographicSize) * Time.deltaTime;
    }
    public void InstantMove()
    {
        Vector3 targetPos = mFollowObject.transform.position;
        targetPos = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        transform.position = targetPos;
    }
}
