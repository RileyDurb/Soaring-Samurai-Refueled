using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.DeviceSimulation;
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

    // serialized variables ////////////////////////////////////////////////////////////////

    [SerializeField]
    private AIMode mCurrAIMode = AIMode.PlayerInput;

    [SerializeField]
    private float mAttackOnTimerInterval = 5.0f;

    // Private variables /////////////////////////////////////////////////////////////////////
    // References

    PlayerCombatController mCombatControllerRef;



    ActionList mAIActionList = new ActionList();

    // Attack on timer variables

    bool mAttackOnTimerActive = false;

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
                mCombatControllerRef.OpponentRef.CurrMoveInput = opponentMoveInput;

                break;
            }
            case (AIMode.AttackOnTimer):
            {
                if (mAttackOnTimerActive == false)
                {
                    mAIActionList.AddActionCallback(TriggerUpLeftAttack, 5.0f, false, true); // Set a looping action to trigger an attack

                    mAttackOnTimerActive = true;
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
