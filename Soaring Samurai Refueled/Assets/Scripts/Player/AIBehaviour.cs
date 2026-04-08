using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class AIBehaviour : MonoBehaviour
{
    // Enum and struct definitions //////////////////////////////////////////////////////
    enum AIMode
    {
        PlayerInput,
        MirrorOpponent,
        AttackOnTimer
    }

    enum AttackDirection
    { 
        UpRight,
        UpLeft,
        DownRight,
        DownLeft
    }



    // serialized variables ////////////////////////////////////////////////////////////////

    [SerializeField]
    private AIMode mCurrAIMode = AIMode.PlayerInput;

    [SerializeField]
    private float mAttackOnTimerInterval = 5.0f;

    [SerializeField]
    private AttackDirection mAttackOnTimerAttackDirection = AttackDirection.UpLeft;

    // Private variables /////////////////////////////////////////////////////////////////////
    // References

    PlayerCombatController mCombatControllerRef;



    ActionList mAIActionList = new ActionList();

    // Attack on timer variables

    bool mAttackOnTimerActive = false;

    float mLastTimerInterval = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        mCombatControllerRef = GetComponent<PlayerCombatController>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (mCurrAIMode)
        {
            case (AIMode.MirrorOpponent): // Case for just mirroring opponent's input
            {
                if (mCombatControllerRef.OpponentRef == null)
                {
                    break;
                }
                Vector2 opponentMoveInput = mCombatControllerRef.OpponentRef.CurrMoveInput;
                mCombatControllerRef.CurrMoveInput = opponentMoveInput;

                break;
            }
            case (AIMode.AttackOnTimer):
            {
                if (mAttackOnTimerActive == false)
                {
                    mAIActionList.AddActionCallback(()=> { TriggerNormalSlashAttack(mAttackOnTimerAttackDirection); }, mAttackOnTimerInterval, false, true); // Set a looping action to trigger an attack

                        mLastTimerInterval = mAttackOnTimerInterval;

                    mAttackOnTimerActive = true;
                }

                if (mAttackOnTimerInterval != mLastTimerInterval)
                {
                    mAIActionList.Clear(); // Clear actions so we can reset the attack function

                    mAttackOnTimerActive = false; // Set to false so it resets on next loop
                }
                break;
            }


            default: // In the case of player input mode, do nothing, just let combat controller handle it
            break;

        }

        // Handle turning off attack on timer mode
        if (mCurrAIMode != AIMode.AttackOnTimer)
        {
            if (mAttackOnTimerActive == true)
            {
                mCombatControllerRef.mActionList.Clear(); // Clear action list to stop attack timer

                mAttackOnTimerActive = false; // mark as turned off, so we don't clear again
            }
        }

        // Update action list
        mAIActionList.Update(Time.deltaTime);
    }

    void TriggerNormalSlashAttack(AttackDirection directionToSlash)
    {
        switch (directionToSlash)
        {
            case AttackDirection.UpRight:
            {
                TriggerUpRightAttack();
            }
            break;

            case AttackDirection.UpLeft:
            {
                TriggerUpLeftAttack();
            }
            break;

            case AttackDirection.DownRight:
            {
                TriggerDownRightAttack();
            }
            break;

            case AttackDirection.DownLeft:
            {
                TriggerDownLeftAttack();
            }
            break;
        }
    }


    void TriggerUpRightAttack()
    {
        mCombatControllerRef.UpRightAttackInput(InputActionPhase.Started);   
    }

    void TriggerUpLeftAttack()
    {
        mCombatControllerRef.UpLeftAttackInput(InputActionPhase.Started);
    }

    void TriggerDownRightAttack()
    {
        mCombatControllerRef.DownRightAttackInput(InputActionPhase.Started);
    }

    void TriggerDownLeftAttack()
    {
        mCombatControllerRef.DownLeftAttackInput(InputActionPhase.Started);
    }
}
