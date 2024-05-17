using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Action_
{
    public enum EasingTypes
    {
        None,
        EaseInSmall,
        EaseOutSmall,
        EaseInMedium,
        EaseOutMedium,
        EaseInHeavy,
        EaseOutHeavy,
        EaseInShakey,
        EaseInBounce
    }

    // Public variables
    public bool mBlocking = false;

    // Private variables
    protected float mTime = 0.0f;
    protected float mDuration = 0.0f;
    protected float mPercentDone = 0.0f; // 0-1 value representing current progress towards goal (0-1 = 0-100%)
    protected float mDelay = 0.0f;
    protected EasingTypes mEasingType = EasingTypes.None;

    // Updates time and percent done variables based on given dt
    // Returns whether to keep update the action
    public virtual bool IncrementTime(float dt)
    {
        if (mDelay >= 0.0f)
        {
            mDelay -= dt;

            if (mDelay <= 0.0f)
            {
                dt = (-1 * mDelay); // dt becomes time leftover from delay
                mDelay = 0.0f; // Resets delay
            }
            else // Still delayed
            {
                return false; 
            }
        }

        // If not done, updates dt
        if (mTime < mDuration)
        {
            mTime += dt;

            // Clamps time to duration, to ensure exact end result
            if (mTime > mDuration)
            {
                mTime = mDuration;
            }
        }

        // Updates percent done
        mPercentDone = Easing(mTime / mDuration, mEasingType);

        return true;
    }

    // The action's logic, should always be
    public abstract bool Update(float dt);

    // Private functions

    float Easing(float percentDone, EasingTypes type)
    {
        switch (type)
        {
            case EasingTypes.None:
                return percentDone;
            case EasingTypes.EaseInSmall:
                return 1 - Mathf.Cos((percentDone * Mathf.PI) / 2);
            case EasingTypes.EaseOutSmall:
                return Mathf.Sin((percentDone * Mathf.PI) / 2);
            case EasingTypes.EaseInMedium:
                return Mathf.Pow(percentDone, 2);
            case EasingTypes.EaseOutMedium:
                return Mathf.Sqrt(percentDone);
            case EasingTypes.EaseInHeavy:
                return Mathf.Pow(percentDone, 10);
            case EasingTypes.EaseOutHeavy:
                return Mathf.Sqrt(Mathf.Sqrt(percentDone));
            case EasingTypes.EaseInShakey:
                const float c4 = (2 * Mathf.PI) / 3;

                return percentDone == 0
                  ? 0
                  : percentDone == 1
                  ? 1
                  : -Mathf.Pow(2, 10 * percentDone - 10) * Mathf.Sin((float)(percentDone * 10 - 10.75) * c4);
            case EasingTypes.EaseInBounce:
                const float c1 = 1.70158f;
                const float c3 = c1 + 1;

                return c3 * percentDone * percentDone * percentDone - c1 * percentDone * percentDone;
            default:
                //("Easing: Tried to eas an action, but no valid easing type provided." + " \"" + type.ToString() + "\" " + "Either fix input, or add an easing function for the given type")
                return percentDone;
        }

    }

}

// Derived actions
class Action_Move : Action_
{
    // Private members
    GameObject mParentObj;
    Vector3 mStartPos;
    Vector3 mEndPos;
    bool mPositionInitted = false;

    //public Action_Move(GameObject parent, Vector3 startPos, Vector3 endPos, float duration)
    //{
    //    mParentObj = parent;
    //    mStartPos = startPos;
    //    mEndPos = endPos;
    //    mDuration = duration;
    //}

    public Action_Move(GameObject parent, Vector3 endPos, float duration, float delay = 0.0f, EasingTypes easingType = EasingTypes.None)
    {
        mParentObj = parent;
        if (parent != null)
        {
            mStartPos = parent.GetComponent<Transform>().position;
        }

        mEndPos = endPos;
        mDuration = duration;
        mDelay = delay;
        mEasingType = easingType;
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mPositionInitted == false)
        {
            // Inits start position here so that it starts at the current position, which could have changed if the action was delayed
            mStartPos = mParentObj.GetComponent<Transform>().position;
            mPositionInitted = true;
        }

        mParentObj.GetComponent<Transform>().position = mStartPos + ((mEndPos - mStartPos) * mPercentDone);

        // If interpolation is complete
        if (mPercentDone == 1)
        {
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }

    
}

// Derived actions
class Action_LocalMove : Action_
{
    // Private members
    GameObject mParentObj;
    Vector3 mStartPos;
    Vector3 mEndPos;
    bool mPositionInitted = false;

    //public Action_Move(GameObject parent, Vector3 startPos, Vector3 endPos, float duration)
    //{
    //    mParentObj = parent;
    //    mStartPos = startPos;
    //    mEndPos = endPos;
    //    mDuration = duration;
    //}

    public Action_LocalMove(GameObject parent, Vector3 endPos, float duration, float delay = 0.0f, EasingTypes easingType = EasingTypes.None)
    {
        mParentObj = parent;
        if (parent != null)
        {
            mStartPos = parent.GetComponent<Transform>().localPosition;
        }

        mEndPos = endPos;
        mDuration = duration;
        mDelay = delay;
        mEasingType = easingType;
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mPositionInitted == false)
        {
            // Inits start position here so that it starts at the current position, which could have changed if the action was delayed
            mStartPos = mParentObj.GetComponent<Transform>().localPosition;
            mPositionInitted = true;
        }

        mParentObj.GetComponent<Transform>().localPosition = (mStartPos + ((mEndPos - mStartPos) * mPercentDone));/* - GameObject.Find("Canvas").transform.position;*/

        // If interpolation is complete
        if (mPercentDone == 1)
        {
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }


}


class Action_Rotate : Action_
{
    // Private members
    GameObject mParentObj;
    Vector3 mStartRotation = new Vector3();
    Vector3 mEndRotation = new Vector3();
    bool mRotationInitted = false;
    bool mRotateFromCurrent = false;

    public Action_Rotate(GameObject parent, Vector3 endRotation, float duration, bool rotateFromCurrent = true, float delay = 0.0f)
    {
        mParentObj = parent;
        if (parent != null)
        {
            mStartRotation = parent.GetComponent<Transform>().rotation.eulerAngles;
        }

        mEndRotation = endRotation;

        mDuration = duration;
        mDelay = delay;
        mRotateFromCurrent = rotateFromCurrent;
        //if (mStartRotation == mEndRotation) // If the same
        //{
        //    // Expects that the user wants a 180 rotation
        //    mEndRotation += 180; // Sets end rotation to make this p
        //}
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mRotationInitted == false)
        {
            if (mParentObj != null)
            {
                mStartRotation = mParentObj.GetComponent<Transform>().rotation.eulerAngles;
            }

            if (mRotateFromCurrent == true)
            {
                mEndRotation = mStartRotation + mEndRotation;
            }

            mRotationInitted = true;
        }

        Vector3 newRotation = mStartRotation + ((mEndRotation - mStartRotation) * mPercentDone);
        //mParentObj.GetComponent<Transform>().rotation.eulerAngles.Set(newXRotation, 0.0f, 0.0f);
        mParentObj.GetComponent<Transform>().rotation = Quaternion.Euler(newRotation);


        // If interpolation is complete
        if (mPercentDone == 1)
        {
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }

    private void InitRotation()
    {

    }
}


class Action_RotateSingleAxis : Action_
{
    public enum Dimensions
    {
        xAxis,
        yAxis,
        zAxis
    }
    // Private members
    GameObject mParentObj;
    float mStartRotation = 0.0f;
    float mEndRotation = 0.0f;
    bool mRotationInitted = false;
    bool mRotateFromCurrent = false;
    Dimensions mRotateAxis = Dimensions.xAxis;

    public Action_RotateSingleAxis(GameObject parent, float endRotation, float duration, Dimensions rotateAxis, bool rotateFromCurrent = true, float delay = 0.0f, EasingTypes easingType = EasingTypes.None)
    {
        mParentObj = parent;
        //if (parent != null)
        //{
        //    mStartRotation = parent.GetComponent<Transform>().rotation.eulerAngles;
        //}

        mEndRotation = endRotation;

        mDuration = duration;
        mDelay = delay;
        mRotateFromCurrent = rotateFromCurrent;
        mRotateAxis = rotateAxis;
        mEasingType = easingType;
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mRotationInitted == false)
        {
            if (mParentObj != null)
            {
                if (mRotateAxis == Dimensions.xAxis)
                {
                    mStartRotation = mParentObj.GetComponent<Transform>().rotation.eulerAngles.x;
                }
                else if (mRotateAxis == Dimensions.yAxis)
                {
                    mStartRotation = mParentObj.GetComponent<Transform>().rotation.eulerAngles.y;
                }
                else
                {
                    mStartRotation = mParentObj.GetComponent<Transform>().rotation.eulerAngles.z;
                }
            }

            if (mRotateFromCurrent == true)
            {
                mEndRotation = mStartRotation + mEndRotation;
            }

            mRotationInitted = true;
        }

        float newRotation = mStartRotation + ((mEndRotation - mStartRotation) * mPercentDone);

        Vector3 currRotation = mParentObj.transform.rotation.eulerAngles;
        if (mRotateAxis == Dimensions.xAxis)
        {
            mParentObj.GetComponent<Transform>().rotation = Quaternion.Euler(newRotation, currRotation.y, currRotation.z);
        }
        else if (mRotateAxis == Dimensions.yAxis)
        {
            mParentObj.GetComponent<Transform>().rotation = Quaternion.Euler(currRotation.x, newRotation, currRotation.z);
        }
        else
        {

            mParentObj.GetComponent<Transform>().rotation = Quaternion.Euler(currRotation.x, currRotation.y, newRotation);
        }



        // If interpolation is complete
        if (mPercentDone == 1)
        {
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }
}

// NOTE: Can maybe make it variadic if we need parameters
public delegate void BaseCallback(); // Callback with 0 arguments
class Action_Callback : Action_
{
    // Private members
    GameObject mParentObj;
    BaseCallback mCallback;
    bool mDependsOnObject = false;

    public Action_Callback(GameObject parent, BaseCallback callback, float delay = 0.0f, bool blocking = false)
    {
        mParentObj = parent;
        if (parent == null)
        {
            mDependsOnObject = false;
        }
        else
        {
            mDependsOnObject = true;
        }

        mCallback = callback;

        mDelay = delay;

        mBlocking = blocking;
    }

    public override bool Update(float dt)
    {
        if (mDependsOnObject == true && mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        mCallback(); // Calls the callback given

        return false; // Only call once, return false for done
    }
}

class Action_FadeText : Action_
{
    // Private members
    GameObject mParentObj;
    float mStartAlpha = 0.0f;
    float mEndAlpha = 0.0f;
    bool mAlphaInitted = false;

    public Action_FadeText(GameObject parent, float endAlpha, float duration, float delay = 0.0f, EasingTypes easingType = EasingTypes.None)
    {
        mParentObj = parent;

        mEndAlpha = endAlpha;

        mDuration = duration;
        mDelay = delay;

        mEasingType = easingType;
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mAlphaInitted == false)
        {
            if (mParentObj != null)
            {
                // Tries starting from parent alpha
                TextMeshPro parentTMP;
                if (mParentObj.TryGetComponent<TextMeshPro>(out parentTMP) == true)
                {
                    mStartAlpha = parentTMP.alpha;

                }
                else
                {
                    // Gets starting alpha from children instead
                    for (int i = 0; i < mParentObj.transform.childCount; i++)
                    {
                        CanvasRenderer renderer = mParentObj.transform.GetChild(0).GetComponent<CanvasRenderer>();
                        if (renderer != null)
                        {
                            mStartAlpha = renderer.GetAlpha();
                        }
                    }
                }
            }

            mAlphaInitted = true;
        }

        float newAlpha = mStartAlpha + ((mEndAlpha - mStartAlpha) * mPercentDone); // Lerp

        // Apply lerp
        // To parent
        TextMeshPro parentTMPSet;
        if (mParentObj.TryGetComponent<TextMeshPro>(out parentTMPSet) == true)
        {
            parentTMPSet.alpha = newAlpha;
        }
        
        // To children
        for (int i = 0; i < mParentObj.transform.childCount; i++)
        {
            CanvasRenderer renderer = mParentObj.transform.GetChild(0).GetComponent<CanvasRenderer>();
            if (renderer != null)
            {
                renderer.SetAlpha(newAlpha);
            }
        }


        // If interpolation is complete
        if (mPercentDone == 1)
        {
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }
}

class Action_FadeCanvasObject : Action_
{
    // Private members
    GameObject mParentObj;
    float mStartAlpha = 0.0f;
    float mEndAlpha = 0.0f;
    bool mAlphaInitted = false;

    public Action_FadeCanvasObject(GameObject parent, float endAlpha, float duration, float delay = 0.0f, EasingTypes easingType = EasingTypes.None)
    {
        mParentObj = parent;

        mEndAlpha = endAlpha;

        mDuration = duration;
        mDelay = delay;

        mEasingType = easingType;
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mAlphaInitted == false)
        {
            if (mParentObj != null)
            {
                // Set start alpha
                mStartAlpha = mParentObj.GetComponent<CanvasRenderer>().GetAlpha();
            }

            mAlphaInitted = true;
        }

        float newAlpha = mStartAlpha + ((mEndAlpha - mStartAlpha) * mPercentDone); // Lerp

        // Apply lerp

        mParentObj.GetComponent<CanvasRenderer>().SetAlpha(newAlpha);

        // If interpolation is complete
        if (mPercentDone == 1)
        {
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }
}

class Action_Scale : Action_
{
    // Private members
    GameObject mParentObj;
    Vector3 mStartScale;
    Vector3 mEndScale;
    bool mScaleInitted = false;

    public Action_Scale(GameObject parent, Vector3 endScale, float duration, float delay = 0.0f, EasingTypes easingType = EasingTypes.None)
    {
        mParentObj = parent;

        mEndScale = endScale;

        mDuration = duration;
        mDelay = delay;
        
        mEasingType = easingType;
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mScaleInitted == false)
        {
            if (mParentObj != null)
            {
                // Set start scale
                mStartScale = mParentObj.transform.localScale;
            }

            mScaleInitted = true;
        }

        Vector3 newScale = mStartScale + ((mEndScale - mStartScale) * mPercentDone); // Lerp

        // Apply lerp
        mParentObj.transform.localScale = newScale;


        // If interpolation is complete
        if (mPercentDone == 1)
        {
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }
}

class Action_Knockback : Action_
{
    // Private members
    GameObject mParentObj;
    Vector2 mKnockbackForce;
    bool mKnockbackInitted = false;

    // Adds knockback to the given object, of the strength of the knockbackForce vector, and then negates the given percentage of that force over the duration
    // negationPercent is from 0-1
    public Action_Knockback(GameObject parent, Vector2 knockbackForce, float duration, float delay = 0.0f)
    {
        mParentObj = parent;
        //if (parent != null)
        //{
        //    mStartRotation = parent.GetComponent<Transform>().rotation.eulerAngles;
        //}
        mKnockbackForce = knockbackForce;
        mDuration = duration;
        mDelay = delay;
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mKnockbackInitted == false)
        {
            if (mParentObj != null)
            {
                mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.UnlockMaxForces("Knockback");
                // Apply knockback
                mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.ApplyUncappedForce(mKnockbackForce);
                ////mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.SetStartingVelocity(mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.Velocity + mKnockbackForce);
            }

            mKnockbackInitted = true;
            return true; // Don't start bleeding force the frame we apply it
        }

        //float currentStep = mPercentDone - mLastPercentDone; // Get time difference from last update

        //Vector2 currentStepForce = -mKnockbackForce * currentStep; // Lerp for the current time step

        ////mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.ApplyUncappedForce(currentStepForce); // Apply force of current time step, but currently not using, because I already bleed off forces with drag
        ////mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.SetStartingVelocity(mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.Velocity + currentStepForce);


        //mLastPercentDone = mPercentDone; // Save current percent done for next frame


        // If interpolation is complete
        if (mPercentDone == 1)
        {
            mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.ReleaseMaxForceUnlock("Knockback");
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }
}

class Action_EqualizedKnockback : Action_
{
    // Private members
    GameObject mParentObj;
    Vector2 mKnockbackForce;
    Vector2 mEquilazationForce;
    float mLastPercentDone = 0.0f;
    bool mKnockbackInitted = false;

    public Action_EqualizedKnockback(GameObject parent, Vector2 knockbackForce, float equalizationPercent, float duration, float delay = 0.0f, EasingTypes easingType = EasingTypes.None)
    {
        mParentObj = parent;
        //if (parent != null)
        //{
        //    mStartRotation = parent.GetComponent<Transform>().rotation.eulerAngles;
        //}
        mKnockbackForce = knockbackForce;
        mEquilazationForce = -knockbackForce * equalizationPercent;
        mDuration = duration;
        mDelay = delay;

        mEasingType = easingType;
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mKnockbackInitted == false)
        {
            if (mParentObj != null)
            {
                mParentObj.GetComponent<PhysicsApplier>().mUncappedDirectionalForces.UnlockMaxForces("EqualizedKnockback");
                // Apply knockback
                mParentObj.GetComponent<PhysicsApplier>().mUncappedDirectionalForces.ApplyUncappedForce(mKnockbackForce);
                mParentObj.GetComponent<PhysicsApplier>().mUncappedDirectionalForces.InputBeingApplied = true; // TODO: Change to a stack if I continue to use input being applied here, because there could be multiple instances of knockback
                ////mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.SetStartingVelocity(mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.Velocity + mKnockbackForce);
            }

            mKnockbackInitted = true;
            return true; // Don't start bleeding force the frame we apply it
        }

        float currentStep = mPercentDone - mLastPercentDone; // Get time difference from last update

        Vector2 currentStepForce = mEquilazationForce * currentStep; // Lerp for the current time step

        mParentObj.GetComponent<PhysicsApplier>().mUncappedDirectionalForces.ApplyUncappedForce(currentStepForce); // Apply force of current time step, but currently not using, because I already bleed off forces with drag
        //mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.SetStartingVelocity(mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.Velocity + currentStepForce);


        mLastPercentDone = mPercentDone; // Save current percent done for next frame


        // If interpolation is complete
        if (mPercentDone == 1)
        {
            mParentObj.GetComponent<PhysicsApplier>().mUncappedDirectionalForces.ReleaseMaxForceUnlock("EqualizedKnockback");
            mParentObj.GetComponent<PhysicsApplier>().mUncappedDirectionalForces.InputBeingApplied = false;
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }
}

class Action_ScreenShake : Action_
{
    // Private members
    GameObject mParentObj;
    float mShakeStrength = 0.0f;
    //bool mInitialPositionInitted = false;

    public Action_ScreenShake(GameObject parent, float shakeStrength, float duration, float delay = 0.0f)
    {
        mParentObj = parent;

        mShakeStrength = shakeStrength;

        mDuration = duration;
        mDelay = delay;
    }

    public override bool Update(float dt)
    {
        Camera shakeCam = Camera.main;
        if (mParentObj != null)
        {
            shakeCam = mParentObj.GetComponent<Camera>();
        }

        //if (mInitialPositionInitted == false)
        //{
        //    if (mParentObj != null)
        //    {
        //    }

        //    mInitialPositionInitted = true;
        //}

        // Apply shake
        Vector3 currCamPos = shakeCam.transform.position;
        float shakeMag = MyRandom.RandomRange(-mShakeStrength, mShakeStrength);
        shakeCam.transform.position = new Vector3(currCamPos.x + shakeMag, currCamPos.y + shakeMag, currCamPos.z);


        // If interpolation is complete
        if (mPercentDone == 1)
        {
            return false; // Action done, return false to stop

        }

        return true; // Action not done, return true to continue
    }
}

//class Action_Stub : Action_
//{
//    // Private members
//    GameObject mParentObj;
//    float mStartThing = 0.0f;
//    float mEndThing = 0.0f;
//    bool mThingInitted = false;

//    public Action_Stub(GameObject parent, float endThing, float duration, float delay = 0.0f)
//    {
//        mParentObj = parent;
//        //if (parent != null)
//        //{
//        //    mStartRotation = parent.GetComponent<Transform>().rotation.eulerAngles;
//        //}

//        mEndThing = endThing;

//        mDuration = duration;
//        mDelay = delay;
//    }

//    public override bool Update(float dt)
//    {
//        if (mParentObj == null)
//        {
//            return false; // Action cannot continue with null object, return false to stop
//        }

//        if (mThingInitted == false)
//        {
//            if (mParentObj != null)
//            {
//                // Set start thing
//            }

//            mThingInitted = true;
//        }

//        float newThing = mStartThing + ((mEndThing - mStartThing) * mPercentDone); // Lerp

//        // Apply lerp



//        // If interpolation is complete
//        if (mPercentDone == 1)
//        {
//            return false; // Action done, return false to stop
//        }

//        return true; // Action not done, return true to continue
//    }
//}