using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsApplier : MonoBehaviour
{


    // Operator overloads to allow for generic groups
    [System.Serializable]
    
    public abstract class IPhysicsGroup<T>
    {
        protected T mGroupTypeZero;
        public IPhysicsGroup(T groupTypeZero, GameObject parent)
        {
            mGroupTypeZero = groupTypeZero;
            mParent = parent;
        }

        public T mMaxVelocity;
        public T mMaxAcceleration;
        public T mMaxJerk;
        public float DampeningMultiplier = 0.9f;
        public float DragCoeff = 0.3f;
        public bool InputBeingApplied = true;
        private float dampeningZeroThreshold = 0.1f;
        protected GameObject mParent;

        // Handling when velocity is set before initialization
        protected T mPreInitVelocity;
        public T PreVelocity
        {
            get { return mPreInitVelocity; }

        }
        protected bool mVelocityPreInitted;
        public bool PreInitted
        {
            get { return mVelocityPreInitted; }
        }


        // Forces and derivatives
        public T Velocity
        {
            get { return GetVelocity(); }
        }
        protected T mAcceleration;
        public T Acceleration
        {
            get { return mAcceleration; }
        }
        protected T mJerk;
        public T Jerk
        {
            get { return mJerk; }
            set
            {
                mJerk = Clamp(value, mMaxJerk);
            }
        }

        // Modifying behaviour
        Stack<string> mActiveMaxForceUnlocks = new Stack<string>();
        //bool mUnlockMaxForces = false;
        //public bool UnlockMaxForces {
        //    get { return mActiveMaxForceUnlocks.Count > 0; } 
        //    set { mUnlockMaxForces = value; }
        //}

        public void Update(float dt, GameObject parent)
        {
            Rigidbody2D physics = parent.GetComponent<Rigidbody2D>();

            // Apply jerk for this frame
            if (mActiveMaxForceUnlocks.Count <=  0) // If no max force unlocks
            {
                mJerk = Clamp(mJerk, mMaxJerk);
            }
            mAcceleration = Add(mAcceleration, Scale(mJerk, Time.deltaTime));

            // Apply acceleration
            if (mActiveMaxForceUnlocks.Count <= 0) // if no max force unlocks
            {
                mAcceleration = Clamp(mAcceleration, mMaxAcceleration);
            }
            T currVelocity = Add(GetVelocity(), Scale(mAcceleration, Time.deltaTime));

            // Always clamp max velocity, weird stuff if we don't
            currVelocity = Clamp(currVelocity, mMaxVelocity);




            if (physics != null)
            {
                SetVelocity(currVelocity);
            }

            mJerk = mGroupTypeZero; // Cancel out jerk, does not carry over to the next frame

            // Dampening for acceleration

            if (InputBeingApplied == false) // Only apply if not input was made this frame
            {
                // Apply dampening to acceleration
                mAcceleration = Add(mAcceleration, Scale(Scale(Subtract(mGroupTypeZero, mAcceleration), DampeningMultiplier), Time.deltaTime));
                
                // Cut off acceleration at a predefined threshold
                // Do this to prevent infinite drifting, and potential oscillations in the direction of acceleration, which drag can cause at small values
                if (Abs(mAcceleration) <= dampeningZeroThreshold)
                {
                    mAcceleration = mGroupTypeZero; // Cancel acceleration
                }
            }


        }


        public abstract T Add(T left, T right);
        public abstract float Square(T value);
        public abstract T Scale(T baseValue, float scaleValue);
        public abstract T Subtract(T left, T right);
        public abstract float Abs(T value);
        //public abstract void UpdateRigidBody(Rigidbody2D physics);
        //public abstract void UpdateVelocityFromRigidBody(Rigidbody2D physics);

        public abstract void ApplyDrag(GameObject parent);

        public abstract T Clamp(T value, T maxMag);

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
        public void SetParent(GameObject parent)
        {
            mParent = parent;
        }
        public abstract T GetVelocity();
        protected abstract void SetVelocity(T newValue);
        public abstract void SetStartingVelocity(T newVelocity);


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
            return value.magnitude * value.magnitude;
        }

        public override Vector2 Scale(Vector2 baseValue, float scaleValue)
        {
            return baseValue * scaleValue;
        }
        public override Vector2 Subtract(Vector2 left, Vector2 right)
        {
            return left - right;
        }
        public override Vector2 Clamp(Vector2 value, Vector2 max)
        {
            value.x = Mathf.Clamp(value.x, -max.x, max.x);
            value.y = Mathf.Clamp(value.y, -max.y, max.y);
            return value;
        }
        public override float Abs(Vector2 value)
        {
            return Mathf.Abs(value.magnitude);
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
        //public override void UpdateRigidBody(Rigidbody2D physics)
        //{
        //    physics.velocity = mVelocity;
        //}

        //public override void UpdateVelocityFromRigidBody(Rigidbody2D physics)
        //{
        //    mVelocity = physics.velocity;
        //}
        public override void ApplyDrag(GameObject parent)
        {
            float drag = DragCoeff * parent.GetComponent<Rigidbody2D>().mass * (Square(Velocity) / 2) * Time.deltaTime;
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
                //print("SetVelocity(Vector2): Tried to access parent before it was set in initialization");
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
        public override void ApplyForce(float acceleration)
        {
            mAcceleration += acceleration;
            mAcceleration = Clamp(mAcceleration, mMaxAcceleration);
        }

        public override void ApplyJerk(float jerk)
        {
            mJerk += jerk;
        }

        public override void ApplyDrag(GameObject parent)
        {
            float angularDragMag = DragCoeff * parent.GetComponent<Rigidbody2D>().mass * (Mathf.Pow(Velocity, 2) / 2) * Time.deltaTime;
            float angularDrag = Mathf.Sign(Velocity) * -1 * angularDragMag;

            //// If drag would cause object to change directions
            //if (angularDragMag > Mathf.Abs(mAcceleration))
            //{
            //    // Set forces to 0, we've stopped
            //    mAcceleration = 0.0f;
            //    mVelocity = 0.0f;
            //}
            //else
            //{
            float ogSign = Mathf.Sign(mAcceleration);
            mAcceleration += angularDrag;

            //}
            //if (Mathf.Sign(mAcceleration) != Mathf.Sign(mVelocity) && Mathf.Abs(mAcceleration) <= mStaticFrictionThreshold)
            //{
            //    mAcceleration = 0.0f;
            //    mVelocity = 0.0f;
            //}

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
                //print("SetVelocity(float): Tried to access parent before it was set in initialization");
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

        if (mDebugDraw == true && name == "Player")
        {
            TextMeshPro tmp = GameObject.Find("PhysicsDebugPrinter").GetComponent<TextMeshPro>();
            if (tmp != null)
            {
               tmp.text = "Velocity: " + mDirectionalForces.Velocity + "\n" +
                          "Acceleration: " + mDirectionalForces.Acceleration + "\n" +
                          "Jerk: " + mDirectionalForces.Jerk + "\n" +
                          "Velocity: " + mRotationalForces.Velocity + "\n" +
                          "Acceleration: " + mRotationalForces.Acceleration + "\n" +
                          "Jerk: " + mRotationalForces.Jerk;
            }
        }


        mDirectionalForces.ApplyDrag(gameObject);
        mDirectionalForces.Update(Time.deltaTime, gameObject);

        if (mRotationalForces.InputBeingApplied == false) // Only apply when input is not applied
        {
            mRotationalForces.ApplyDrag(gameObject);

        }
        mRotationalForces.Update(Time.deltaTime, gameObject);
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


//[System.Serializable]
//public class PhysicsVectorGroup
//{

//    public PhysicsVectorGroup()
//    {
//    }

//    public Vector2 mMaxVelocity = new Vector2(1, 1);
//    public Vector2 mMaxAcceleration = new Vector2(1, 1);
//    public Vector2 mMaxJerk = new Vector2(1, 1);
//    public float DragMultiplier = 0.9f;
//    public float DragCoeff = 0.3f;

//    Vector2 mVelocity = new Vector2(0, 0);
//    public Vector2 Velocity
//    {
//        get { return mVelocity; }
//    }
//    Vector2 mAcceleration = new Vector2(0, 0);
//    public Vector2 Acceleration
//    {
//        get { return mAcceleration; }
//    }
//    Vector2 mJerk = new Vector2(0, 0);
//    public Vector2 Jerk
//    {
//        get { return mJerk; }
//        set
//        {
//            mJerk = Vector2.ClampMagnitude(value, mMaxJerk.magnitude);
//        }
//    }

//    public void Update(float dt, GameObject parent)
//    {
//        // Applies acceleration over the current frame
//        mAcceleration += mJerk * Time.deltaTime;
//        // Apply drag
//        float drag = DragCoeff * parent.GetComponent<Rigidbody2D>().mass * mVelocity.magnitude * mVelocity.magnitude * Time.deltaTime;
//        Vector2 dragVec = mVelocity * -1;
//        dragVec.Normalize();
//        dragVec *= drag;
//        mAcceleration += dragVec;
//        // Clamps to max
//        mAcceleration.x = Mathf.Clamp(mAcceleration.x, -mMaxAcceleration.x, mMaxAcceleration.x);
//        mAcceleration.y = Mathf.Clamp(mAcceleration.y, -mMaxAcceleration.y, mMaxAcceleration.y);

//        mVelocity += mAcceleration * Time.deltaTime;
//        // Clamps to max
//        mVelocity.x = Mathf.Clamp(mVelocity.x, -mMaxVelocity.x, mMaxVelocity.x);
//        mVelocity.y = Mathf.Clamp(mVelocity.y, -mMaxVelocity.y, mMaxVelocity.y);

//        // if at max velocity
//        if (mVelocity == mMaxVelocity)
//        {
//            // Turn off acceleration and jerk
//            //mAcceleration.x = 0;
//            //mAcceleration.y = 0;

//            //mJerk.x = 0;
//            //mJerk.y = 0;
//        }

//        Rigidbody2D physics = parent.GetComponent<Rigidbody2D>();


//        if (physics != null)
//        {
//            physics.velocity = mVelocity;
//        }

//        mJerk = Vector2.zero; // Clear out jerk, as forces have been applied this frame

//        // Old drag using just the multiplier
//        //mVelocity = mVelocity + (Vector2.zero - mVelocity) * DragMultiplier * Time.deltaTime;
//        //mAcceleration = mAcceleration + (Vector2.zero - mAcceleration) * DragMultiplier * Time.deltaTime;
//        //mJerk = mJerk + (Vector2.zero - mJerk) * DragMultiplier * Time.deltaTime;

//        //mVelocity = mVelocity + (Vector2.zero - mVelocity) * DragMultiplier * Time.deltaTime;
//        //mAcceleration = mAcceleration + (Vector2.zero - mAcceleration) * DragMultiplier * Time.deltaTime;

//        // Notes:
//        /* Using mass instead of volume, so we don't care about how big an object looks
//         * 
//         */
//        //mVelocity += -(DragCoeff * parent.GetComponent<Rigidbody2D>().mass * ((mVelocity * mVelocity) / 2)) * Time.deltaTime;
//        //mAcceleration += -(DragCoeff * parent.GetComponent<Rigidbody2D>().mass * ((mAcceleration * mAcceleration) / 2)) * Time.deltaTime;
//        //mJerk += -(DragCoeff * parent.GetComponent<Rigidbody2D>().mass * ((mJerk * mJerk) / 2)) * Time.deltaTime;

//    }

//    public void DirectionChangeForceModify()
//    {
//        mAcceleration.x = 0;
//        mAcceleration.y = 0;
//        mJerk.x = 0;
//        mJerk.y = 0;
//    }

//    public void ClearAllForces()
//    {
//        mVelocity.x = 0;
//        mVelocity.y = 0;
//        mAcceleration.x = 0;
//        mAcceleration.y = 0;
//        mJerk.x = 0;
//        mJerk.y = 0;
//    }

//    public void ApplyForce(Vector2 jerk)
//    {
//        mJerk += jerk;
//    }
//    //public bool IsAccelerating()
//    //{
//    //    return mAcceleration
//    //}
//}

//[System.Serializable]
//public class PhysicsFloatGroup
//{

//    public PhysicsFloatGroup()
//    {
//    }


//    public float mMaxVelocity = 1;
//    public float mMaxAcceleration = 1;
//    public float mMaxJerk = 1;
//    public float DragMultiplier = 0.9f;
//    public float DragCoeff = 0.3f;

//    float mVelocity = 0;
//    public float Velocity
//    {
//        get { return mVelocity; }
//    }
//    float mAcceleration = 0;
//    public float Acceleration
//    {
//        get { return mAcceleration; }
//    }
//    float mJerk = 0;
//    public float Jerk
//    {
//        get { return mJerk; }
//        set { mJerk = Mathf.Clamp(value, -mMaxJerk, mMaxJerk); }
//    }

//    public void Update(float dt, GameObject parent)
//    {
//        // Applies acceleration over the current frame
//        mAcceleration += mJerk * Time.deltaTime;
//        // Apply drag
//        float drag = DragCoeff * parent.GetComponent<Rigidbody2D>().mass * (mVelocity * mVelocity / 2) * Time.deltaTime;
//        mAcceleration += mAcceleration > 0 ? -1 : 1 * drag;
//        // Clamps to max
//        mAcceleration = Mathf.Clamp(mAcceleration, -mMaxAcceleration, mMaxAcceleration);

//        mVelocity += mAcceleration * Time.deltaTime;
//        // Clamps to max
//        mVelocity = Mathf.Clamp(mVelocity, -mMaxVelocity, mMaxVelocity);

//        // If at max velocity
//        if (mVelocity == mMaxVelocity)
//        {
//            // turn off acceleration and jerk
//            //mAcceleration = 0;
//            //mJerk = 0;
//        }

//        Rigidbody2D physics = parent.GetComponent<Rigidbody2D>();


//        if (physics != null)
//        {
//            physics.angularVelocity = mVelocity;
//        }

//        mJerk = 0.0f; // Clear out jerk, as forces have been applied this frame
//                      //mVelocity = mVelocity + (0 - mVelocity) * DragMultiplier * Time.deltaTime;
//                      //mAcceleration = mAcceleration + (0 - mAcceleration) * DragMultiplier * Time.deltaTime;
//                      //mJerk = mJerk + (0 - mJerk) * DragMultiplier * Time.deltaTime;

//        //mVelocity = mVelocity + (0 - mVelocity) * DragMultiplier * Time.deltaTime;
//        //mAcceleration = mAcceleration + (0 - mAcceleration) * DragMultiplier * Time.deltaTime;
//        //mVelocity += -(DragCoeff * parent.GetComponent<Rigidbody2D>().mass * ((mVelocity * mVelocity) / 2)) * Time.deltaTime;
//        //mAcceleration += -(DragCoeff * parent.GetComponent<Rigidbody2D>().mass * ((mAcceleration * mAcceleration) / 2)) * Time.deltaTime;
//        //mJerk += -(DragCoeff * parent.GetComponent<Rigidbody2D>().mass * ((mJerk * mJerk) / 2)) * Time.deltaTime;

//    }

//    public void DirectionChangeForceModify()
//    {
//        mAcceleration = 0;
//        mJerk = 0;
//    }

//    public void ClearAllForces()
//    {
//        mVelocity = 0;
//        mAcceleration = 0;
//        mJerk = 0;
//    }

//    public void ApplyForce(float jerk)
//    {
//        mJerk += jerk;
//    }

//}