using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;


[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsApplier : MonoBehaviour
{
    public ActionList mActionList = new ActionList();

    // Operator overloads to allow for generic groups
    [System.Serializable]
    
    public abstract class IPhysicsGroup<T>
    {
        enum DampeningType
        {
            Percentage, // Percentage dampened each frame
            Interpolation
        }
        protected T mGroupTypeZero; // The zero for the generic type used, to make setting values to 0 work with generics
        public IPhysicsGroup(T groupTypeZero, GameObject parent)
        {
            mGroupTypeZero = groupTypeZero;
            mParent = parent;
        }

        public float mMaxVelocity;
        public float mMaxAcceleration;
        public float mMaxJerk;
        public float DampeningMultiplier = 0.9f;
        public float DragCoeff = 0.3f;
        public bool InputBeingApplied = true;

        // Private variables
        [SerializeField] float mDampeningZeroThreshold = 0.1f; // NOT used, may revisit zeroing out a force once it hits a certain low threshold after applying drag
        [SerializeField] DampeningType mDampeningType = DampeningType.Percentage;
        bool mInputAppliedLastFrame = true;
        protected GameObject mParent;

        // Interpolated dampening variables
        // TODO: probably make this interp velocity instead, bnecause interpint acceleration doesn't really do anything if it doesn't make the acceleration go in the opposite direction as the velocity
        [SerializeField] float mMaxDampeningTime = 1.0f; // The time it takes to dampen when at max velocity. Lower velocities will take less time

        // Handling when velocity is set before initialization
        protected T mPreInitVelocity;

        protected bool mVelocityPreInitted;


        // Forces and derivatives

        protected T mAcceleration;

        protected T mJerk;


        // Modifying behaviour
        Stack<string> mActiveMaxForceUnlocks = new Stack<string>();



        public int Update(float dt)
        {            
            Rigidbody2D physics = mParent.GetComponent<Rigidbody2D>();

            // Apply jerk for this frame
            if (mActiveMaxForceUnlocks.Count <=  0) // If no max force unlocks
            {
                mJerk = Clamp(mJerk, mMaxJerk);
            }
            mAcceleration = Add(mAcceleration, Scale(mJerk, dt));

            // Apply acceleration
            if (mActiveMaxForceUnlocks.Count <= 0) // if no max force unlocks
            {
                mAcceleration = Clamp(mAcceleration, mMaxAcceleration);
            }
            T currVelocity = Add(GetVelocity(), Scale(mAcceleration, dt));

            // Always allow for clamping clamp max velocity, weird stuff if we don't
            currVelocity = Clamp(currVelocity, mMaxVelocity);

            // Apply new velocity
            if (physics != null)
            {
                SetVelocity(currVelocity);
            }

            mJerk = mGroupTypeZero; // Cancel out jerk, does not carry over to the next frame



            // Dampening for acceleration
            int cancelVelocity = 0;
            if (InputBeingApplied == false) // Only apply if not input was made this frame
            {
                cancelVelocity = ApplyDampening(dt);
            }
            else
            {
                HandleDampeningInputChange();
            }


            // Saves current input state as the last, for next frame
            mInputAppliedLastFrame = InputBeingApplied;

            return cancelVelocity;

        }

        int ApplyDampening(float dt)
        {
            if (mDampeningType == DampeningType.Percentage)
            {
                // Apply dampening to acceleration
                mAcceleration = Add(mAcceleration, Scale(Scale(Subtract(mGroupTypeZero, mAcceleration), DampeningMultiplier), dt));

                // Cut off acceleration at a predefined threshold
                // Do this to prevent infinite drifting, and potential oscillations in the direction of acceleration, which drag can cause at small values
                // TODO: Probably switch this to instead of hard stopping everything, switch to linear dampening, with a constant, or adjustable amount, so that it will actually hit 0, but isn't so abrupt. Could just lerp, or decay depending on the amount
                if (Abs(mAcceleration) <= mDampeningZeroThreshold)
                {
                    mAcceleration = mGroupTypeZero; // Cancel acceleration
                    return 1;
                }
                return 0;
            }
            else // Interpolated
            {
                // NOTE: Maybe make the timer for dampening tick back up when not applying dampening

                // If newly starting 
                if (mInputAppliedLastFrame == true && InputBeingApplied == false)
                {
                    mParent.GetComponent<PhysicsApplier>().mActionList.AddAction(new Action_DampenDirectional(mParent, 0.0f, mMaxDampeningTime * (Abs(Velocity) / mMaxVelocity)));
                }
                return 0;
            }
        }

        void HandleDampeningInputChange()
        {
            if (mDampeningType == DampeningType.Interpolation)
            {
                if (mInputAppliedLastFrame == false && InputBeingApplied == true)
                {
                    mParent.GetComponent<PhysicsApplier>().mActionList.Clear();
                }
            }
        }
            


        public abstract void ApplyDrag(float dt);

        // Operators

        public abstract T Add(T left, T right);
        public abstract float Square(T value);
        public abstract T Scale(T baseValue, float scaleValue);
        public abstract T Subtract(T left, T right);
        public abstract float Abs(T value);
        public abstract float Mag(T value);

        public abstract T Clamp(T value, float maxMag);


        // Force manipulation
        public abstract void ApplyForce(T acceleration);

        public abstract void ApplyJerk(T jerk);
        public void DirectionChangeForceModify()
        {
            mAcceleration = mGroupTypeZero;
            mJerk = mGroupTypeZero;
        }

        public void ClearAllForces()
        {
            SetVelocity(mGroupTypeZero);
            mAcceleration = mGroupTypeZero;
            mJerk = mGroupTypeZero;
        }

        public void TransferForceModifiers(IPhysicsGroup<T> other)
        {
            mMaxVelocity = other.mMaxVelocity;
            mMaxAcceleration = other.mMaxAcceleration;
            mMaxJerk = other.mMaxJerk;
            DampeningMultiplier = other.DampeningMultiplier;
            DragCoeff = other.DragCoeff;
        }

        // Getters and setters

        // Pre-initting
        public T PreVelocity
        {
            get { return mPreInitVelocity; }

        }
        public bool PreInitted
        {
            get { return mVelocityPreInitted; }
        }

        // Forces and derivatives
        public T Velocity
        {
            get { return GetVelocity(); }
        }
        public T Acceleration
        {
            get { return mAcceleration; }
        }
        public T Jerk
        {
            get { return mJerk; }
            set
            {
                mJerk = Clamp(value, mMaxJerk);
            }
        }
        public void SetParent(GameObject parent)
        {
            mParent = parent;
        }
        public abstract T GetVelocity();
        protected abstract void SetVelocity(T newValue);
        public abstract void SetStartingVelocity(T newVelocity);

        // For interpolation mode
        public abstract void SetAccelerationDampening(float magnitude);


        // Max Force Unlocking/ locking
        // NOTE: Maybe have the release function throw an error if the reason is not present (change data structure to a list so you can search)
        public void UnlockMaxForces(string unlockReason)
        {
            mActiveMaxForceUnlocks.Push(unlockReason);
        }

        public void ReleaseMaxForceUnlock(string unlockReason)
        {
            if (mActiveMaxForceUnlocks.Count <= 0)
            {
                print("ReleaseMaxForceUnlock: Tried to lock max forces that were unlocked because of " + unlockReason + ", but forces are currently locked");
                return;
            }

            mActiveMaxForceUnlocks.Pop();
        }
    }

    [System.Serializable]
    public class PhysicsVectorGroup : IPhysicsGroup<Vector2>
    {
        //float dragZeroThreshold = 0.05f;
        public PhysicsVectorGroup(Vector2 zeroVec, GameObject parent = null) : base(zeroVec, parent)
        {
        }

        public override Vector2 Add(Vector2 left, Vector2 right)
        {
            return left + right;
        }

        public override float Square(Vector2 value)
        {
            return value.sqrMagnitude;
        }

        public override Vector2 Scale(Vector2 baseValue, float scaleValue)
        {
            return baseValue * scaleValue;
        }
        public override Vector2 Subtract(Vector2 left, Vector2 right)
        {
            return left - right;
        }
        public override Vector2 Clamp(Vector2 value, float maxMag)
        {
            
            //value.x = Mathf.Clamp(value.x, -max.x, max.x);
            //value.y = Mathf.Clamp(value.y, -max.y, max.y);
            return Vector2.ClampMagnitude(value, maxMag);
        }

        public override float Abs(Vector2 value)
        {
            return Mathf.Abs(value.magnitude);
        }

        public override float Mag(Vector2 value)
        {
            return value.magnitude;
        }

        public override void SetAccelerationDampening(float magnitude)
        {
            mAcceleration = mAcceleration.normalized * magnitude;
        }

        // temp function to test proper force application
        public void ApplyUncappedForce(Vector2 acceleration)
        {
            mAcceleration += acceleration;
        }
        public override void ApplyForce(Vector2 acceleration)
        {
            mAcceleration += acceleration;
            //mAcceleration = Clamp(mAcceleration, mMaxAcceleration);
        }

        public override void ApplyJerk(Vector2 jerk)
        {
            mJerk += jerk;
        }

        public override void ApplyDrag(float dt)
        {
            float drag = DragCoeff * mParent.GetComponent<Rigidbody2D>().mass * (Square(Velocity) / 2) * dt;
            Vector2 dragVec = Velocity.normalized * -1 * drag;

            //// Set to 0 if drag would make the object change directions
            //if (/*Vector2.Dot(mAcceleration, dragVec) < 0 && *//*mAcceleration.magnitude < dragVec.magnitude*/false)
            //{
            //    // Set forces to 0, we've stopped
            //    mAcceleration = mGroupTypeZero;
            //    mVelocity = mGroupTypeZero;
            //}
            //else // Won't flip directions
            //{
                mAcceleration += dragVec; // Apply drag normally
            //}

            //if (Abs(mAcceleration) <= dragZeroThreshold)
            //{
            //    mVelocity = mGroupTypeZero;
            //    mAcceleration = mGroupTypeZero;
            //}
        }

        // Sets velocity, to only be used when initializing an object's velocity
        public override void SetStartingVelocity(Vector2 velocity)
        {
            SetVelocity(velocity);
        }

        public override Vector2 GetVelocity()
        {
            if (mParent == null)
            {
                print("GetVelocity(Vector2): Tried to access parent before it was set in initialization");
                return Vector2.zero;
            }
            return mParent.GetComponent<Rigidbody2D>().velocity;
        }

        protected override void SetVelocity(Vector2 newValue)
        {
            if (mParent == null)
            {
                mPreInitVelocity = newValue;
                mVelocityPreInitted = true;
                return;
            }
            mParent.GetComponent<Rigidbody2D>().velocity = newValue;
        }
    }

    [System.Serializable]
    public class PhysicsFloatGroup : IPhysicsGroup<float>
    {
        float dragZeroThreshold = 7.0f;
        public float mStaticFrictionThreshold = 0.5f;
        public PhysicsFloatGroup(float zero, GameObject parent = null) : base(zero, parent)
        {
        }

        public override float Add(float left, float right)
        {
            return left + right;
        }

        public override float Square(float value)
        {
            return value * value;
        }

        public override float Subtract(float left, float right)
        {
            return left - right;
        }

        public override float Scale(float baseValue, float scaleValue)
        {
            return baseValue * scaleValue;
        }

        public override float Clamp(float value, float maxMag)
        {
            return Mathf.Clamp(value, -maxMag, maxMag);
        }

        public override float Abs(float value)
        {
            return Mathf.Abs(value);
        }

        public override float Mag(float value)
        {
            return Mathf.Abs(value);
        }

        public override void SetAccelerationDampening(float magnitude)
        {
            mAcceleration = Mathf.Sign(mAcceleration) * magnitude;
        }

        public override void ApplyForce(float acceleration)
        {
            mAcceleration += acceleration;
            mAcceleration = Clamp(mAcceleration, mMaxAcceleration);
        }

        public override void ApplyJerk(float jerk)
        {
            mJerk += jerk;
        }

        public override void ApplyDrag(float dt)
        {
            float angularDragMag = DragCoeff * mParent.GetComponent<Rigidbody2D>().mass * (Mathf.Pow(Velocity, 2) / 2) * dt;
            float angularDrag = Mathf.Sign(Velocity) * -1 * angularDragMag;

            float ogSign = Mathf.Sign(mAcceleration);
            mAcceleration += angularDrag;

            if (Mathf.Sign(mAcceleration) != ogSign && Abs(mAcceleration) <= dragZeroThreshold)
            {
                SetVelocity(mGroupTypeZero);
                mAcceleration = mGroupTypeZero;
            }
        }

        public override float GetVelocity()
        {
            if (mParent == null)
            {
                print("GetVelocity(float): Tried to access parent before it was set in initialization");
                return 0.0f;
            }
            return mParent.GetComponent<Rigidbody2D>().angularVelocity;
        }
        
        protected override void SetVelocity(float newValue)
        {
            if (mParent == null)
            {
                mVelocityPreInitted = true;
                mPreInitVelocity = newValue;
                return;
            }
            mParent.GetComponent<Rigidbody2D>().angularVelocity = newValue;
        }

        public override void SetStartingVelocity(float newVelocity)
        {
            SetVelocity(newVelocity);
        }

    }

    public PhysicsVectorGroup mDirectionalForces = new PhysicsVectorGroup(Vector2.zero);
    public PhysicsVectorGroup mUncappedDirectionalForces = new PhysicsVectorGroup(Vector2.zero);

    public PhysicsFloatGroup mRotationalForces = new PhysicsFloatGroup(0.0f);

    // Debug stuff
    public bool mDebugDraw = true;

    // Start is called before the first frame update
    void Start()
    {
        mDirectionalForces.SetParent(gameObject);
        if (mDirectionalForces.PreInitted == true)
        {
            mDirectionalForces.SetStartingVelocity(mDirectionalForces.PreVelocity);
        }
        mUncappedDirectionalForces.SetParent(gameObject);
        if (mUncappedDirectionalForces.PreInitted == true)
        {
            mUncappedDirectionalForces.SetStartingVelocity(mUncappedDirectionalForces.PreVelocity);
        }
        mUncappedDirectionalForces.UnlockMaxForces("Default");
        mUncappedDirectionalForces.InputBeingApplied = false;

        mRotationalForces.SetParent(gameObject);
        if (mRotationalForces.PreInitted == true)
        {
            mRotationalForces.SetStartingVelocity(mRotationalForces.PreVelocity);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        mActionList.Update(Time.fixedDeltaTime);

        if (mDebugDraw == true)
        {

            
            if (name == "Player")
            {
                // Draw debug lines for each force and derivative
                Debug.DrawRay(transform.position, new Vector3(mDirectionalForces.GetVelocity().x, mDirectionalForces.GetVelocity().y, 0), Color.green, 0, false);
                Debug.DrawRay(transform.position, new Vector3(mDirectionalForces.Acceleration.x, mDirectionalForces.Acceleration.y, 0), Color.red, 0, false);
                Debug.DrawRay(transform.position, new Vector3(mDirectionalForces.Jerk.x, mDirectionalForces.Jerk.y, 0), Color.blue, 0, false);

                Debug.DrawRay(transform.position, new Vector3(mUncappedDirectionalForces.GetVelocity().x, mUncappedDirectionalForces.GetVelocity().y, 0), Color.black, 0, false);
                Debug.DrawRay(transform.position, new Vector3(mUncappedDirectionalForces.Acceleration.x, mUncappedDirectionalForces.Acceleration.y, 0), Color.grey, 0, false);
                Debug.DrawRay(transform.position, new Vector3(mUncappedDirectionalForces.Jerk.x, mUncappedDirectionalForces.Jerk.y, 0), Color.magenta, 0, false);

                TextMeshProUGUI tmp = GameObject.Find("PhysicsDebugPrinter").GetComponent<TextMeshProUGUI>();

                if (tmp != null)
                {
                    tmp.text = "Velocity: \n" + mDirectionalForces.Velocity + "\n" +
                               "Acceleration: \n" + mDirectionalForces.Acceleration + "\n" +
                               "Jerk: \n" + mDirectionalForces.Jerk + "\n" +
                               "Velocity: " + mRotationalForces.Velocity + "\n" +
                               "Acceleration: " + mRotationalForces.Acceleration + "\n" +
                               "Jerk: " + mRotationalForces.Jerk;
                }
            }
        }




        // Update directional forces

        mDirectionalForces.ApplyDrag(Time.fixedDeltaTime);

        // First update capped
        int cancelVelocity = mDirectionalForces.Update(Time.fixedDeltaTime);

        // Then uncapped
        cancelVelocity += mUncappedDirectionalForces.Update(Time.fixedDeltaTime);

        if (cancelVelocity >= 2)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        // Update rotational forces

        if (mRotationalForces.InputBeingApplied == false) // Only apply when input is not applied
        {
            mRotationalForces.ApplyDrag(Time.fixedDeltaTime);

        }
        cancelVelocity = mRotationalForces.Update(Time.fixedDeltaTime);
        if (cancelVelocity >= 1)
        {
            GetComponent<Rigidbody2D>().angularVelocity =  0.0f;
        }
        if (name == "Player")
        {
            //print("Player Physics Update");
            TelemetryTracker.Instance.TrackPlayerLinearAcceleration(mDirectionalForces.Acceleration);
            TelemetryTracker.Instance.TrackPlayerLinearVelocity(mDirectionalForces.Velocity);

            TelemetryTracker.Instance.TrackPlayerAngularAcceleration(mRotationalForces.Acceleration);
            TelemetryTracker.Instance.TrackPlayerAngularVelocity(mRotationalForces.Velocity);
        }

    }

    public void ResetForces()
    {
        mDirectionalForces.ClearAllForces();
        mRotationalForces.ClearAllForces();
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0.0f;
    }

    public void TransferForceModifiers(PhysicsApplier physics)
    {
        mDirectionalForces.TransferForceModifiers(physics.mDirectionalForces);

        mRotationalForces.TransferForceModifiers(physics.mRotationalForces);

        mDebugDraw = physics.mDebugDraw;
    }
}





// Dampening action
class Action_DampenDirectional : Action_
{
    // Private members
    GameObject mParentObj;
    float mStartAcceleration = 0.0f;
    float mEndAcceleration = 0.0f;
    bool mInitted = false;

    public Action_DampenDirectional(GameObject parent, float endAcceleration, float duration, float delay = 0.0f)
    {
        mParentObj = parent;
        //if (parent != null)
        //{
        //    mStartRotation = parent.GetComponent<Transform>().rotation.eulerAngles;
        //}

        mEndAcceleration = endAcceleration;

        mDuration = duration;
        mDelay = delay;
    }

    public override bool Update(float dt)
    {
        if (mParentObj == null)
        {
            return false; // Action cannot continue with null object, return false to stop
        }

        if (mInitted == false)
        {
            if (mParentObj != null)
            {
                // Set start thing
                mStartAcceleration = mParentObj.GetComponent<PhysicsApplier>().mDirectionalForces.Acceleration.magnitude;
            }

            mInitted = true;
        }

        float newAccelerationMag = mStartAcceleration + ((mEndAcceleration - mStartAcceleration) * mPercentDone); // Lerp

        // Apply lerp
        PhysicsApplier physics = mParentObj.GetComponent<PhysicsApplier>();
        physics.mDirectionalForces.SetAccelerationDampening(newAccelerationMag);


        // If interpolation is complete
        if (mPercentDone == 1)
        {
            physics.mDirectionalForces.SetStartingVelocity(Vector2.zero);
            return false; // Action done, return false to stop
        }

        return true; // Action not done, return true to continue
    }
}