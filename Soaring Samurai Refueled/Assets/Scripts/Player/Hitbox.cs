using AudioEvents;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [System.Serializable]
    public class AttackDefinition
    {
        public AttackDefinition() { } // Default Constructor
        public AttackDefinition(float damage, float activeTime, float knockbackStrength, float knockbackEqualizationPercent, float knockbackDuration) // Testing costructor
        {
            mDamage = damage;
            mActiveTime = activeTime;
            mKnockbackStrength = knockbackStrength;
            mKnockbackEqualizationPercent = knockbackEqualizationPercent;
            mKnockbackDuration = knockbackDuration;

            // Constructor is only meant for use with making hardcoded attacks for testing, and setting of other variables likely isn't needed
        }
        [Header("Main Effectiveness")]
        [SerializeField] float mDamage = 0.0f;
        [SerializeField] float mActiveTime = 1.0f;
        [SerializeField] float mAttackOffsetDistance = 1.0f;
        [SerializeField] Vector2 mHitboxScaleFromPlayer = new Vector2(1.0f, 1.0f);
        [SerializeField] float mHitStunTime = 1.0f;
        [Header("Resources")]
        [SerializeField] float mGasCost = 0.0f;
        [SerializeField] float mGasGainOnUse = 0.0f;
        [SerializeField] float mGasGainOnHit = 15.0f;
        [Header("Knockback")]
        [SerializeField] float mKnockbackStrength = 0.0f;
        [SerializeField] float mKnockbackEqualizationPercent = 1.0f;
        [SerializeField] float mKnockbackDuration = 0.3f;
        [Header("VFX")]
        [SerializeField] bool mUseCustomCurveHitSquish = false;
        [SerializeField] AnimationCurve mSquishCurve;
        [SerializeField] GameObject mHitParticlesPrefab = null;
        [SerializeField] bool mHitParticlesFollowTarget = false;
        [Header("Audio")]
        [SerializeField] SoundEvent mAttackStartSoundEvent;
        [SerializeField] SoundEvent mHitSound;

        // Getters
        public float Damage { get { return mDamage; } }
        public float KnockbackStrength { get { return mKnockbackStrength; } }
        public float ActiveTime { get { return mActiveTime; } }
        public float HitStunTime { get { return mHitStunTime; } }
        public float GasCost {  get { return mGasCost; } }
        public float GasGainOnHit {  get { return mGasGainOnHit; } }
        public float GasGainOnUse {  get { return mGasGainOnUse; } }
        public float KnockbackEqualizationPercent { get { return mKnockbackEqualizationPercent; } }
        public float KnockbackDuration { get { return mKnockbackDuration; } }
        public AnimationCurve SquishCurve {  get { return mSquishCurve; } }
        public bool UseCustomHitSquishCurve { get { return mUseCustomCurveHitSquish; } }
        public float AttackOffsetDistance {  get { return mAttackOffsetDistance; } }
        public Vector2 HitboxScale { get { return mHitboxScaleFromPlayer; } }
        public GameObject HitParticlesPrefab { get { return mHitParticlesPrefab; } }
        public bool HitParticlesFollowTarget { get { return mHitParticlesFollowTarget; } }
        public SoundEvent AttackStartSoundEvent {  get {  return mAttackStartSoundEvent; } }
        public SoundEvent HitSound {  get {  return mHitSound; } }
    }

    [System.Serializable]
    public class AttackCurrentData
    {
        public AttackCurrentData(Vector2 knockbackVec, int attackingPlayerSourceID, bool isClashing = false, Vector2 attackOffset = default) 
        {
            mKnockbackVec = knockbackVec;
            mAttackingPlayerID = attackingPlayerSourceID;
            mIsClashing = isClashing;
            mAttackOffset = attackOffset;
        }

        Vector2 mKnockbackVec = Vector2.zero;
        int mAttackingPlayerID = -1;
        bool mIsClashing = false;
        Vector2 mAttackOffset = Vector2.zero;
        // Getters
        public Vector2 Knockback { get { return mKnockbackVec; } }
        public int AttackingSourcePlayerID {  get { return mAttackingPlayerID; } }
        public bool IsClashing { get { return mIsClashing; } }
        public Vector2 AttackOffset { get { return mAttackOffset; } }
    }

    // Editor accessible variables


    // private variables
    bool mAlreadyHit = false; // Whether this hitbox has already hit an opponent
    float mCurrLifeTimer = 0.0f;
    AttackDefinition mAttackInfo = new AttackDefinition();
    int mOwningPlayerID = -1;
    

    // Start is called before the first frame update
    void Start()
    {
        mCurrLifeTimer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Timer is active (non-negative)
        if (mCurrLifeTimer >= 0.0f)
        {
            mCurrLifeTimer += Time.deltaTime;

            if (mCurrLifeTimer >= mAttackInfo.ActiveTime) // If timer is up
            {
                Destroy(gameObject);
            }
        }


        // Enables debug draw if in debug mode, disables if not
        // NOTE: Size not accurate at the moment
        GetComponentInChildren<SpriteRenderer>().enabled = SimManager.Instance.DebugModeOn;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (mAlreadyHit == true)
        {
            return;
        }

        if (collision.collider.gameObject.tag.Contains("Player")) // If hit player's body, send a hit
        {
            GameObject parentAttacker = transform.parent.gameObject;
            if (parentAttacker == null)
            {
                print("Hitbox:OnCollisionEnter2D: Hitbox with null parent collided with " + collision.gameObject.name);
                mAlreadyHit = true;
                return;
            }

            // Gets knockback vector
            Vector2 vecToReceiver = collision.transform.position - parentAttacker.transform.position;
            Vector2 knockbackVec = vecToReceiver.normalized * mAttackInfo.KnockbackStrength;


            // Sends attack
            // Passess in specifc attack info like knockback vec, and all the attack's data for other purposes like how it squishes the opponent visually
            AttackCurrentData currAttackData = new AttackCurrentData(knockbackVec, mOwningPlayerID, false, transform.localPosition);
            collision.gameObject.GetComponent<PlayerCombatController>().TakeDamage(currAttackData, mAttackInfo);

            // Marks hitbox as already hit, so it doesn't trigger again
            mAlreadyHit = true;


            // Tell the attacking player it hit an opponent, for things like meter gain on attack hitr
            transform.parent.GetComponent<PlayerCombatController>().HitOpponentWithAttack(currAttackData, mAttackInfo);

        }
        else if (collision.collider.gameObject.tag.Contains("Hitbox")) // If hit only another hitbox, trigger a clash
        {
            ContactPoint2D[] collisions = new ContactPoint2D[collision.contactCount];
            collision.GetContacts(collisions);

            // Check if we only hit hitboxes
            bool onlyHitboxCollision = true;
            foreach (ContactPoint2D contact in collisions)
            {
                if (contact.collider.gameObject.tag.Contains("Hitbox") == false)
                {
                    onlyHitboxCollision = false;
                    break;
                }
            }

            if (onlyHitboxCollision == true) // If only hit another hitbox, trigger a clash
            {
                GameObject parentAttacker = transform.parent.gameObject;
                if (parentAttacker == null)
                {
                    print("Hitbox:OnCollisionEnter2D: Hitbox with null parent collided with " + collision.gameObject.name);
                    mAlreadyHit = true;
                    return;
                }

                // Gets knockback vector
                Vector2 vecToReceiver = collision.transform.position - parentAttacker.transform.position;
                Vector2 knockbackVec = vecToReceiver.normalized * mAttackInfo.KnockbackStrength;

                // Sends attack
                // Passess in specifc attack info like knockback vec, and all the attack's data for other purposes like how it squishes the opponent visually
                collision.gameObject.GetComponent<PlayerCombatController>().TakeDamage(new AttackCurrentData(knockbackVec, mOwningPlayerID, true, transform.localPosition), mAttackInfo);

                // Marks hitbox as already hit, so it doesn't trigger again
                mAlreadyHit = true;
            }
        }
    }

    // Getters and setters

    public void InitAttack(AttackDefinition attack, int owningPlayerID)
    {
        mAttackInfo = attack;
        mCurrLifeTimer = 0.0f;
        mOwningPlayerID = owningPlayerID;
    }
}
