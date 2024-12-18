using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Filename: AnimationController.cs
 * Description: Component for facillitating controlling animation directly through code, rather than through the parameters state machine
 * * Copyright:  
 * Author(s): Riley Durbin
 */

public class AnimationController : MonoBehaviour
{
    // Public variables
    public Animator mAnimator;

    public string CurrAnimationName
    {
        get { return mCurrAnimation; }
    }

    // private variables
    string mCurrAnimation;

    // Start is called before the first frame update
    void Start()
    {
        if (mAnimator == null)
        {
            mAnimator = GetComponent<Animator>();
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAnimationState(string animationName)
    {
        mAnimator.Play(animationName);

        mCurrAnimation = animationName;
    }

}
