using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraFollow : MonoBehaviour
{
    // Editor accessible values
    public float CameraSpeed = 1.0f;
    // Private variables

    GameObject mFollowObject;
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
        Vector3 targetPos = mFollowObject.transform.position;
        targetPos = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        transform.position = transform.position + (targetPos - transform.position) * CameraSpeed * Time.deltaTime;
        //transform.position = targetPos;

        SetZoom(mFollowObject.GetComponent<Rigidbody2D>().velocity.magnitude);
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
