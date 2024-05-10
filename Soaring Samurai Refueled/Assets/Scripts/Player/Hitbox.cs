using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [System.Serializable]
    public class AttackDefinition
    {
        [SerializeField] float mDamage = 0.0f;
        [SerializeField] float mKnockbackStrength = 0.0f;
        [SerializeField] float mActiveTime = 1.0f;

        public float Damage { get { return mDamage; } }
        public float KnockbackStrength { get { return mKnockbackStrength; } }
        public float ActiveTime { get { return mActiveTime; } }
    }

    [System.Serializable]
    public class AttackData
    {
        public AttackData(float damage, Vector2 knockbackVec) 
        {
            mDamage = damage;
            mKnockbackVec = knockbackVec;
        }

        float mDamage = 0.0f;
        Vector2 mKnockbackVec = Vector2.zero;

        public float Damage { get { return mDamage; } }
        public Vector2 Knockback { get { return mKnockbackVec; } }
    }

    // Editor accessible variables


    // Probably just handle destruction with an action, unless lifetime is set based on the animation


    // private variables
    bool mAlreadyHit = false; // Whether this hitbox has already hit an opponent
    float mCurrLifeTimer = 0.0f;
    AttackDefinition mAttackInfo = new AttackDefinition();


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
        GetComponent<SpriteRenderer>().enabled = SimManager.Instance.DebugMode;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (mAlreadyHit == true)
        {
            return;
        }

        if (collision.gameObject.tag.Contains("Player"))
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
            collision.gameObject.GetComponent<PlayerCombatController>().TakeDamage(new AttackData(mAttackInfo.Damage, knockbackVec));

            // Marks hitbox as already hit, so it doesn't trigger again
            mAlreadyHit = true;
        }
    }

    // Getters and setters

    public void InitAttack(AttackDefinition attack)
    {
        mAttackInfo = attack;
        mCurrLifeTimer = 0.0f;
    }
}
