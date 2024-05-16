using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

// Container for managing a list of actions that perform some task.
// Actions are given the same dt, which can be manipulated
public class ActionList
{
    // Private members
    List<Action_> mActions = new List<Action_>();

    // Public functions ///////////////////////////////////////////////////////
    public void Update(float dt)
    {
        if (dt == 0.0f) // Don't update if paused (some actions don't depend on dt, but we want those paused too)
        {
            return;
        }


        for (int i = 0; i < mActions.Count; i++)
        {
            Action_ currAction = mActions[i];

            if (currAction.mBlocking == true)
            {
                bool continueAction = UpdateAction(ref currAction, ref i, dt);

                if (continueAction == true) // if action is still blocking
                {
                    break; // Block other actions
                }
            }
            else
            {
                UpdateAction(ref currAction, ref i, dt);
            }
        }
    }

    public void AddAction(Action_ newAction)
    {
        mActions.Add(newAction);
    }

    public void Clear()
    {
        mActions.Clear();
    }

    // Query functions ///////////////////////////////////////////////////////////
    public bool Empty() { return mActions.Count == 0; }

    // Action add helpers

    // Move

    // Moves position to the end position, from the position the object is at once the delay is over
    public void AddActionMove(GameObject parent, Vector3 endPos, float duration, float delay = 0.0f, Action_.EasingTypes easingType = Action_.EasingTypes.None)
    {
        mActions.Add(new Action_Move(parent, endPos, duration, delay, easingType));
    }

    public void AddActionLocalMove(GameObject parent, Vector3 endPos, float duration, float delay = 0.0f, Action_.EasingTypes easingType = Action_.EasingTypes.None)
    {
        mActions.Add(new Action_LocalMove(parent, endPos, duration, delay, easingType));
    }

    // Rotate

    // Spins the given amount of degrees from rotate amount, starting from the current angle
    public void AddActionRotateRelative(GameObject parent, float rotateAmount, float duration, float delay = 0.0f)
    {
        mActions.Add(new Action_Rotate(parent, new Vector3(parent.transform.rotation.eulerAngles.x, parent.transform.rotation.eulerAngles.z, rotateAmount), duration, true, delay));
    }

    // Flips 180 degrees in the y axis (flipping over a card to reveal or conceal it)
    public void AddActionFlip(GameObject parent, float duration, float delay = 0.0f, Action_.EasingTypes easingType = Action_.EasingTypes.None)
    {
        float newRotation = parent.transform.eulerAngles.y + 180.0f;
        if (newRotation == 360.0f)
        {
            newRotation = 0f;
        }
        mActions.Add(new Action_RotateSingleAxis(parent, newRotation, duration, Action_RotateSingleAxis.Dimensions.yAxis, false, delay, easingType));
    }

    // Spins to the new rotation angle, given in degrees
    public void AddActionSpinTo(GameObject parent, float newRotation, float duration, float delay = 0.0f, Action_.EasingTypes easingType = Action_.EasingTypes.None)
    {
        mActions.Add(new Action_RotateSingleAxis(parent, newRotation, duration, Action_RotateSingleAxis.Dimensions.zAxis, false, delay, easingType));
    }
    public void AddActionSpinBy(GameObject parent, float rotation, float duration, float delay = 0.0f, Action_.EasingTypes easingType = Action_.EasingTypes.None)
    {
        mActions.Add(new Action_RotateSingleAxis(parent, parent.transform.eulerAngles.z + rotation, duration, Action_RotateSingleAxis.Dimensions.zAxis, true, delay, easingType));
    }

    // Callback
    public void AddActionCallback(BaseCallback callbackFunc, float delay = 0.0f, bool blocking = false)
    {
        mActions.Add(new Action_Callback(null, callbackFunc, delay, blocking));

    }
    public void AddActionCallback(GameObject parent, BaseCallback callbackFunc, float delay = 0.0f)
    {
        mActions.Add(new Action_Callback(parent, callbackFunc, delay));
    }

    // Fade
    public void AddActionFadeTextTo(GameObject parent, float endAlpha, float duration, float delay = 0.0f, Action_.EasingTypes easingType = Action_.EasingTypes.None)
    {
        mActions.Add(new Action_FadeText(parent, endAlpha, duration, delay, easingType));
    }

    public void AddActionFadeCanvasObject(GameObject parent, float endAlpha, float duration, float delay = 0.0f, Action_.EasingTypes easingType = Action_.EasingTypes.None)
    {
        mActions.Add(new Action_FadeCanvasObject(parent, endAlpha, duration, delay, easingType));
    }

    // Scale
    // Takes a full vector 3 scale
    public void AddActionScale(GameObject parent, Vector3 endScale, float duration, float delay = 0.0f, Action_.EasingTypes easingTypes = Action_.EasingTypes.None)
    {
        mActions.Add(new Action_Scale(parent, endScale, duration, delay, easingTypes));
    }

    // Version that takes a vector 2 scale, and assumes z scale is 1
    public void AddActionScale(GameObject parent, Vector2 endScale, float duration, float delay = 0.0f, Action_.EasingTypes easingTypes = Action_.EasingTypes.None)
    {
        mActions.Add(new Action_Scale(parent, new Vector3(endScale.x, endScale.y, 1), duration, delay, easingTypes));
    }

    // Forces
    public void AddActionKnockback(GameObject parent, Vector2 knockbackForce, float duration, float delay = 0.0f)
    {
        mActions.Add(new Action_Knockback(parent, knockbackForce, duration, delay));
    }

    public void AddActionEqualizedKnockback(GameObject parent, Vector2 knockbackForce, float equalizationPercent, float duration, float delay = 0.0f)
    {
        mActions.Add(new Action_EqualizedKnockback(parent, knockbackForce, equalizationPercent, duration, delay));
    }

    // Effects
    // Does screenshake on the given camera, or on the main camera if given camera is null
    public void AddActionScreenShake(GameObject parentCam, float shakeStrength, float duration, float delay = 0.0f)
    {
        mActions.Add(new Action_ScreenShake(parentCam, shakeStrength, duration, delay));
    }


    // Private Helper functions //////////////////////////////////////////////////
    // Updates the current action in the action list
    // Returns whether the action is over
    private bool UpdateAction(ref Action_ action, ref int currIndex, float dt)
    {
        bool delayOver = action.IncrementTime(dt);
        if (delayOver == false) // If delayed
        {
            return true; // Action delayed, but not finished, return running
        }

        bool continueAction = action.Update(dt);

        if (continueAction == false) // if action is done
        {
            mActions.RemoveAt(currIndex); // Removes current action
            currIndex--; // Updates index to account for deleted action

            return false; // Action is done, return over
        }

        return true; // Action is still going, return running
    }
}
