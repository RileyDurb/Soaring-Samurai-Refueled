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
        float xDistance = (newDimensions.x / 2.0f) + (mBoundsSides[(int)BoundDirections.Left].transform.lossyScale.x / 2.0f);
        float yDistance = (newDimensions.y / 2.0f) + (mBoundsSides[(int)BoundDirections.Up].transform.lossyScale.x / 2.0f);

        GameObject currBoundObject = mBoundsSides[(int)BoundDirections.Left];
        Vector3 currLocalPosition = currBoundObject.transform.position;

        currBoundObject.transform.localPosition.Set(-xDistance, currLocalPosition.y, currLocalPosition.z);

        currBoundObject = mBoundsSides[(int)BoundDirections.Right];
        currLocalPosition = currBoundObject.transform.position;

        currBoundObject.transform.localPosition.Set(xDistance, currLocalPosition.y, currLocalPosition.z);

        currBoundObject = mBoundsSides[(int)BoundDirections.Up];
        currLocalPosition = currBoundObject.transform.position;

        currBoundObject.transform.localPosition.Set(currLocalPosition.x, yDistance, currLocalPosition.z);

        currBoundObject = mBoundsSides[(int)BoundDirections.Down];
        currLocalPosition = currBoundObject.transform.position;

        currBoundObject.transform.localPosition.Set(currLocalPosition.x, -yDistance, currLocalPosition.z);



    }

    public Vector2 GetBoundsWidth()
    {
        return new Vector2(mBoundsSides[(int)BoundDirections.Left].GetComponent<SpriteRenderer>().bounds.size.x, mBoundsSides[(int)BoundDirections.Up].GetComponent<SpriteRenderer>().bounds.size.y);
    }

    public Vector2 GetBoundDimensions()
    {
        Vector2 boundsWidth = GetBoundsWidth();
        return new Vector2((mBoundsSides[(int)BoundDirections.Right].transform.position - mBoundsSides[(int)BoundDirections.Left].transform.position).magnitude - boundsWidth.x
            , (mBoundsSides[(int)BoundDirections.Up].transform.position - mBoundsSides[(int)BoundDirections.Down].transform.position).magnitude);
    }
}
