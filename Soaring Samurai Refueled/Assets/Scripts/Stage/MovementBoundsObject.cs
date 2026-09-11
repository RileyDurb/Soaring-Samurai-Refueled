using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovementBoundsObject : MonoBehaviour
{
    enum BoundDirections
    { 
        Left,
        Right,
        Up,
        Down
    }

    [SerializeField] List<GameObject> mBoundsSides = new List<GameObject>();

    // Public interface

    public void SetBoundDimensions(Vector2 newDimensions)
    {
        // Uses the width of the left and up bound objects, assumes all bound objects have the same thickness
        float xDistance = (newDimensions.x / 2.0f) + (mBoundsSides[(int)BoundDirections.Left].transform.localScale.x / 2.0f);
        float yDistance = (newDimensions.y / 2.0f) + (mBoundsSides[(int)BoundDirections.Up].transform.localScale.x / 2.0f);

        GameObject currBoundObject = mBoundsSides[(int)BoundDirections.Left];

        // For left side
        // Set local position to be half the dimension width
        Vector3 currLocalPosition = currBoundObject.transform.localPosition;

        currBoundObject.transform.localPosition = new Vector3(-xDistance, currLocalPosition.y, currLocalPosition.z);


        currBoundObject.transform.localScale = new Vector2(currBoundObject.transform.localScale.x, newDimensions.y);

        // For right side
        currBoundObject = mBoundsSides[(int)BoundDirections.Right];
        currLocalPosition = currBoundObject.transform.localPosition;

        currBoundObject.transform.localPosition = new Vector3(xDistance, currLocalPosition.y, currLocalPosition.z);

        currBoundObject.transform.localScale = new Vector2(currBoundObject.transform.localScale.x, newDimensions.y);

        // For top side
        currBoundObject = mBoundsSides[(int)BoundDirections.Up];
        currLocalPosition = currBoundObject.transform.localPosition;

        currBoundObject.transform.localPosition = new Vector3(currLocalPosition.x, yDistance, currLocalPosition.z);

        currBoundObject.transform.localScale = new Vector2(currBoundObject.transform.localScale.x, newDimensions.x);

        // For bottom side
        currBoundObject = mBoundsSides[(int)BoundDirections.Down];
        currLocalPosition = currBoundObject.transform.localPosition;

        currBoundObject.transform.localPosition = new Vector3(currLocalPosition.x, -yDistance, currLocalPosition.z);

        currBoundObject.transform.localScale = new Vector2(currBoundObject.transform.localScale.x, newDimensions.x);

    }

    public Vector2 GetBoundsWidth()
    {
        return new Vector2(mBoundsSides[(int)BoundDirections.Left].GetComponent<SpriteRenderer>().bounds.size.x, mBoundsSides[(int)BoundDirections.Up].GetComponent<SpriteRenderer>().bounds.size.y);
    }

    public Vector2 GetBoundDimensions()
    {
        Vector2 boundsWidth = GetBoundsWidth();
        return new Vector2((mBoundsSides[(int)BoundDirections.Right].transform.position - mBoundsSides[(int)BoundDirections.Left].transform.position).magnitude - boundsWidth.x
            , (mBoundsSides[(int)BoundDirections.Up].transform.position - mBoundsSides[(int)BoundDirections.Down].transform.position).magnitude - boundsWidth.x);
    }

    public Vector2 GetBoundDimensionsLocalScale()
    {
        return new Vector2(mBoundsSides[(int)BoundDirections.Left].transform.localScale.y, mBoundsSides[(int)BoundDirections.Up].transform.localScale.y);
    }
}
