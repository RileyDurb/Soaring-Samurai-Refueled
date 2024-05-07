using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [System.Serializable]
    public class AttackData
    {
        public float damage = 0.0f;
        public Vector2 knockback = Vector2.zero;
    }

    // Editor accessible variables


    // Probably just handle destruction with an action, unless lifetime is set based on the animation


    // private variables
    bool mAlreadyHit = false;
    float mLifetime = 1.0f;
    float mCurrLifeTimer = 0.0f;
    AttackData mAttackData = new AttackData();


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

            if (mCurrLifeTimer >= mLifetime) // If timer is up
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
            collision.gameObject.GetComponent<PlayerCombatController>().TakeDamage(mAttackData);
            mAlreadyHit = true;
        }
    }

    // Getters and setters
    private void SetLifeTimer(float newLifeTime)
    {
        mLifetime = newLifeTime;
        mCurrLifeTimer = 0.0f;
    }

    public void InitAttack(AttackData attackData, float lifeTime)
    {
        mAttackData = attackData;
        SetLifeTimer(lifeTime);
    }
}
