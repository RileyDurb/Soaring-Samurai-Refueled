using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using MBT;

public class AIBehaviour : MonoBehaviour
{
    // Enum and struct definitions //////////////////////////////////////////////////////
    public enum AIMode
    {
        PlayerInput,
        MirrorOpponent,
        AttackOnTimer,
        BehaviourTree
    }

    public enum AttackDirection
    { 
        UpRight,
        UpLeft,
        DownRight,
        DownLeft
    }

    public static List<Vector2> AttackDirectionVectors = new List<Vector2>
    {
        Vector2.up + Vector2.right,
        Vector2.up + Vector2.left,
        Vector2.down + Vector2.right,
        Vector2.down + Vector2.left
    };



    // serialized variables ////////////////////////////////////////////////////////////////

    [SerializeField]
    private AIMode mCurrAIMode = AIMode.PlayerInput;
    [SerializeField]
    private AIMode mNextAIMode = AIMode.PlayerInput;

    [SerializeField]
    private float mAttackOnTimerInterval = 5.0f;

    [SerializeField]
    private AttackDirection mAttackOnTimerAttackDirection = AttackDirection.UpLeft;

    // Private variables /////////////////////////////////////////////////////////////////////
    // References

    PlayerCombatController mCombatControllerRef;

    MonoBehaviourTree mBehaviourTreeRef = null;
    [SerializeField] GameObject BehaviourTreePrefab;

    ActionList mAIActionList = new ActionList();

    // Attack on timer variables

    bool mAttackOnTimerActive = false;

    float mLastTimerInterval = 0.0f;

    // Getters and setters //////////////////////////////////////////////////////////////////////
    public AIMode CurrAIMode { get { return mCurrAIMode; } }

    // Start is called before the first frame update
    void Start()
    {
        mCombatControllerRef = GetComponent<PlayerCombatController>();

        // Creates the set behaviour tree if any

        if (BehaviourTreePrefab != null)
        {
            mBehaviourTreeRef = Instantiate(BehaviourTreePrefab, transform).GetComponent<MonoBehaviourTree>();
            BotBehaviourStatsVariable mainBotStats = mBehaviourTreeRef.GetComponent<Blackboard>().GetVariable<BotBehaviourStatsVariable>("MainBotStats");
            mainBotStats.Value.InitializeVariablesOntoBehaviourTree(mBehaviourTreeRef, mBehaviourTreeRef.GetComponent<Blackboard>());
        }
        else
        {
            print("AIBehaviour:Start: No behaviour tree prefab set");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If we have a new AI mode to switch to, do any cleanup for last mode, then switch to the new mode
        if (mNextAIMode != mCurrAIMode)
        {
            switch (mCurrAIMode)
            {
                // Call any on exit functionality that each mode may need
                case AIMode.AttackOnTimer:
                    {
                        mAIActionList.Clear(); // Clears action list so we dom't continue auto attacking
                        mAttackOnTimerActive = false;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            // Set new mode as the current one
            mCurrAIMode = mNextAIMode;
        }


        switch (mCurrAIMode)
        {
            case AIMode.MirrorOpponent: // Case for just mirroring opponent's input
            {
                if (mCombatControllerRef.OpponentRef == null)
                {
                    break;
                }
                Vector2 opponentMoveInput = mCombatControllerRef.OpponentRef.CurrMoveInput;
                mCombatControllerRef.CurrMoveInput = opponentMoveInput;

                break;
            }
            case AIMode.AttackOnTimer:
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
            case AIMode.BehaviourTree:
            {
                mBehaviourTreeRef.Tick();
                break;
            }


            default: // In the case of player input mode, do nothing, just let combat controller handle it
            break;

        }

        // Update action list
        mAIActionList.Update(Time.deltaTime);
    }

    // Public interface
    public void TriggerNormalSlashAttack(AttackDirection directionToSlash)
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

    public void SetAIMode(AIMode newMode)
    {
        if (newMode != mCurrAIMode)
        {
            mNextAIMode = newMode;
        }
    }
    // Private helper functions
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
