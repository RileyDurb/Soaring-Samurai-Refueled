using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{
    // Class and other Definitions ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public enum FacingDirection
    { 
        Left,
        Right
    }

    enum DashAttackStates
    {
        Charge,
        Ready,
        Active,
        Recovery
    }

    // TODO: make player use a scriptable object with base stats instead of the straight variables


    [System.Serializable]
    public class ActionAesthetics
    {
        [Header("Dash")]
        public float DashStretchMin = 0.8f;
        public float DashStretchMax = 1.6f;
        public Action_.EasingTypes DashStrechEasing = Action_.EasingTypes.None;
    }



    // Editor Accessible variables  ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] PlayerBaseDataObject mPlayerBaseStats;
    [SerializeField] DashAttackDataObject mDashAttackStats;


    [SerializeField] AttackDataObject DirectionalSlashAttackStats;

    [Header("Aesthetics")]
    public ActionAesthetics mActionAesthetics = new ActionAesthetics();

    public ActionList mActionList = new ActionList();

    // Getters and setters ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public PlayerCombatController OpponentRef { get { return mOpponentRef; } }

    public Vector2 CurrMoveInput
    {
        get { return mMoveInput; }
        set { mMoveInput = value; }
    }

    // Private variables //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Vector2 mMoveInput;
    Vector2 mLastDirectionalMoveInput;
    Vector2 mOGScale; // Original scale, for using to base squash and stretch off of, so we don't loose it with overlapping actions and become bigger


    [SerializeField]
    private int playerIndex = -1; // Index of player, inits to less than 0 to represent no player assigned

    // Component references
    AnimationController mAnimationController;

    PlayerCombatController mOpponentRef;

    StateManagerPlayer mStateManager;

    // Dash attack variables
    DashAttackStates mCurrDashAttackState = DashAttackStates.Charge;
    bool mDashAttackInputReleased = false;

    void Start()
    {
        // Set component references
        mAnimationController = GetComponent<AnimationController>();

        mStateManager = GetComponent<StateManagerPlayer>();

        // gets reference to the opponent
        PlayerCombatController[] mPlayers = FindObjectsByType<PlayerCombatController>(FindObjectsSortMode.None);
        foreach (PlayerCombatController player in mPlayers )
        {
            if (player != this)
            {
                mOpponentRef = player;
            }
        }

        mOGScale = transform.localScale;

        // Subscribe state change functions
        mStateManager.AddOnEnter(PlayerStates.Ready, StartIdle);

        mStateManager.AddOnEnter(PlayerStates.Dash, StartDash);

        mStateManager.AddOnEnter(PlayerStates.DashAttack, StartDashAttackCharge);
    }

    // Update is called once per frame
    void Update()
    {
        mActionList.Update(Time.deltaTime);

        // Face opponent while idling
        if (mStateManager.CurrStateName == PlayerStates.Ready || mStateManager.CurrStateName == PlayerStates.DashAttack)
        {
            if (mOpponentRef.transform.position.x < transform.position.x)
            {
                SetFacingDirection(FacingDirection.Left);
            }
            else
            {
                SetFacingDirection(FacingDirection.Right);

            }
        }


        // Dash attack state update
        if (mStateManager.CurrStateName == PlayerStates.DashAttack)
        {
            if (mCurrDashAttackState == DashAttackStates.Ready && mDashAttackInputReleased == true)
            {
                mCurrDashAttackState = DashAttackStates.Active;

                mAnimationController.SetAnimationState("Player_DashAttackActive"); // Play animation

                // Spawns attack hitbox right around the player
                SpawnDirectionalAttack(new Vector2(0, 0), mDashAttackStats.mStats);

                // Set to go into recovery after active time is done
                mActionList.AddActionCallback(() => StartDashAttackRecovery(), mDashAttackStats.mStats.ActiveTime);
            }
        }


        // Apply movement from current input value
        float currSpeed = mPlayerBaseStats.mMovementStats.MoveJerk;

        PhysicsApplier physics = GetComponent<PhysicsApplier>();
        if (mStateManager.CurrStateName == PlayerStates.Dash)
        {
            currSpeed = mPlayerBaseStats.mMovementStats.DashingJerk;
        }
        else if (mStateManager.CurrStateName == PlayerStates.DashAttack)
        {
            currSpeed = mDashAttackStats.DashingJerk;

            // Modify ability to move based on state of the attack

            // If not attackng, can't move
            if (mCurrDashAttackState == DashAttackStates.Charge || mCurrDashAttackState == DashAttackStates.Ready || mCurrDashAttackState == DashAttackStates.Recovery)
            {
                currSpeed = 0.0f;

                physics.mUncappedDirectionalForces.ClearAllForces();
                physics.mDirectionalForces.ClearAllForces();
            }
            else if (mCurrDashAttackState == DashAttackStates.Active) // If attacking, force movement
            {
                if (mMoveInput == Vector2.zero) // if not inputting a direction
                {
                    if (mLastDirectionalMoveInput == Vector2.zero) // if last move input isn't anything
                    {
                        // Use facing direction for movement
                        if (transform.localScale.x > 0) // if facing right
                        {
                            mMoveInput = Vector2.right; // Go right
                        }
                        else // If facing left
                        {
                            mMoveInput = Vector2.left; // go left
                        }
                    }
                    else // Last directional move input is valid
                    {
                        mMoveInput = mLastDirectionalMoveInput.normalized; // use last directional input
                    }
                }

            }
        }

        Vector2 moveVec = mMoveInput * currSpeed;

        //if (mMoveInput.magnitude > 0)
        //{
        //    print(mMoveInput.magnitude.ToString()); 
        //}

        if (mStateManager.CurrStateName == PlayerStates.Dash || mStateManager.CurrStateName == PlayerStates.DashAttack)
        {
            // Applies jerk
            physics.mUncappedDirectionalForces.ApplyJerk(moveVec * Time.deltaTime);
        }
        else
        {
            physics.mDirectionalForces.ApplyJerk(moveVec * Time.deltaTime);
        }

        // Since things like dampening can be applied differently based in if input is being given, tell the physics the current state
        if (mMoveInput == Vector2.zero)
        {
            physics.mDirectionalForces.InputBeingApplied = false;
            //physics.mUncappedDirectionalForces.InputBeingApplied = false;
        }
        else
        {
            physics.mDirectionalForces.InputBeingApplied = true;
            //physics.mUncappedDirectionalForces.InputBeingApplied = true;

            mLastDirectionalMoveInput = mMoveInput; // Saves as last nonzero move onput
        }
    }

    // Getters and setters
    public int PlayerIndex
    {
        get { return playerIndex; }
        set { playerIndex = value; }
    }



    // Action functions
    public void OnMove(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                {
                    mMoveInput = context.ReadValue<Vector2>();
                }
                break;

            case InputActionPhase.Canceled:
                {
                    mMoveInput = Vector2.zero;
                }
                break;

            default:
                break;
        }
    }

    //public void OnTestAttack(InputAction.CallbackContext context)
    //{
    //    if (context.phase == InputActionPhase.Started)
    //    {
    //        GameObject newHitbox = Instantiate(SimManager.Instance.GetPrefab("BaseHitbox"), transform);
    //        newHitbox.transform.localScale = new Vector3(transform.lossyScale.x, transform.lossyScale.y, newHitbox.transform.lossyScale.z); // Sets scale equal to the player's

    //        newHitbox.GetComponent<Hitbox>().InitAttack(TestAttackInfo);
    //    }

    //}

    public void OnDownLeftAttack(InputAction.CallbackContext context)
    {

        DownLeftAttackInput(context.phase);
    }

    // Takes the current input phase for the attack, letting the attack be triggered by info from input, or manually calling it
    public void DownLeftAttackInput(InputActionPhase inputPhase)
    {
        if (mStateManager.CanEnterState(PlayerStates.SlashAttack) == false)
        {
            return;
        }
        if (inputPhase == InputActionPhase.Started)
        {
            mStateManager.EnterState(PlayerStates.SlashAttack, DirectionalSlashAttackStats.mStats.ActiveTime, PlayerStates.Ready); // Enter State, and set up state done timer

            SpawnDirectionalAttack(new Vector2(-1, -1) * DirectionalSlashAttackStats.mStats.AttackOffsetDistance, DirectionalSlashAttackStats.mStats);

            // Set animation and facting direction
            SetFacingDirection(FacingDirection.Left);

            mAnimationController.SetAnimationState("Player_DRNormalAttack");
        }
    }

    public void OnUpLeftAttack(InputAction.CallbackContext context)
    {
        UpLeftAttackInput(context.phase);   
    }
    // Takes the current input phase for the attack, letting the attack be triggered by info from input, or manually calling it
    public void UpLeftAttackInput(InputActionPhase inputPhase)
    {
        if (mStateManager.CanEnterState(PlayerStates.SlashAttack) == false)
        {
            return;
        }
        if (inputPhase == InputActionPhase.Started)
        {
            mStateManager.EnterState(PlayerStates.SlashAttack, DirectionalSlashAttackStats.mStats.ActiveTime, PlayerStates.Ready); // Enter State, and set up state done timer

            SpawnDirectionalAttack(new Vector2(-1, 1) * DirectionalSlashAttackStats.mStats.AttackOffsetDistance, DirectionalSlashAttackStats.mStats);

            // Set animation and facing direction

            SetFacingDirection(FacingDirection.Left);

            mAnimationController.SetAnimationState("Player_URNormalAttack");
        }
    }


    public void OnDownRightAttack(InputAction.CallbackContext context)
    {
        DownRightAttackInput(context.phase);
    }

    // Takes the current input phase for the attack, letting the attack be triggered by info from input, or manually calling it
    public void DownRightAttackInput(InputActionPhase inputPhase)
    {
        if (mStateManager.CanEnterState(PlayerStates.SlashAttack) == false)
        {
            return;
        }
        if (inputPhase == InputActionPhase.Started)
        {
            mStateManager.EnterState(PlayerStates.SlashAttack, DirectionalSlashAttackStats.mStats.ActiveTime, PlayerStates.Ready); // Enter State, and set up state done timer

            SpawnDirectionalAttack(new Vector2(1, -1) * DirectionalSlashAttackStats.mStats.AttackOffsetDistance, DirectionalSlashAttackStats.mStats);

            // Set animation and facing direction

            SetFacingDirection(FacingDirection.Right);

            mAnimationController.SetAnimationState("Player_DRNormalAttack");
        }
    }

    public void OnUpRightAttack(InputAction.CallbackContext context)
    {
        UpRightAttackInput(context.phase);
    }

    // Takes the current input phase for the attack, letting the attack be triggered by info from input, or manually calling it
    public void UpRightAttackInput(InputActionPhase inputPhase)
    {
        if (mStateManager.CanEnterState(PlayerStates.SlashAttack) == false)
        {
            return;
        }

        if (inputPhase == InputActionPhase.Started)
        {
            mStateManager.EnterState(PlayerStates.SlashAttack, DirectionalSlashAttackStats.mStats.ActiveTime, PlayerStates.Ready); // Enter State, and set up state done timer

            SpawnDirectionalAttack(new Vector2(1, 1) * DirectionalSlashAttackStats.mStats.AttackOffsetDistance, DirectionalSlashAttackStats.mStats);

            // Set animation and facing direction

            SetFacingDirection(FacingDirection.Right);

            mAnimationController.SetAnimationState("Player_URNormalAttack");
        }
    }


    public void OnDash(InputAction.CallbackContext context)
    {
        if (mStateManager.CanEnterState(PlayerStates.Dash) == false)
        {
            return;
        }

        if (context.phase == InputActionPhase.Canceled)
        {
            mStateManager.EnterState(PlayerStates.Dash, mPlayerBaseStats.mMovementStats.DashDuration, PlayerStates.Ready);
        }
    }


    public void OnDashAttack(InputAction.CallbackContext context)
    {

        if (context.phase == InputActionPhase.Performed)
        {
            if (mStateManager.CanEnterState(PlayerStates.DashAttack) == false || mStateManager.CurrStateName == PlayerStates.DashAttack)
            {
                return;
            }

            mStateManager.EnterState(PlayerStates.DashAttack);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            if (mStateManager.CurrStateName == PlayerStates.DashAttack)
            {
                mDashAttackInputReleased = true;
            }
        }
    }


    // Public interface
    public void SetFacingDirection(FacingDirection newDirection)
    {
        if (newDirection == FacingDirection.Left)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
    }


    // Combat related functions
    public void TakeDamage(Hitbox.AttackCurrentData attackData, Hitbox.AttackDefinition baseAttackInfo)
    {
        GetComponent<PoolContainer>().GetPool("Health").DecreasePool(baseAttackInfo.Damage);

        if (SimManager.Instance.DebugMode)
        {
            Debug.DrawRay(transform.position, attackData.Knockback, Color.yellow, .5f, false);
        }
        mActionList.AddActionEqualizedKnockback(gameObject, attackData.Knockback, baseAttackInfo.KnockbackEqualizationPercent, baseAttackInfo.KnockbackDuration);

        if (baseAttackInfo.UseCustomHitSquishCurve)
        {
            mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y * 1.2f), .1f, 0.0f, Action_.EasingTypes.Custom, baseAttackInfo.SquishCurve);
        }
        else
        {
            mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y * 1.2f), .1f); // Don't ease, just scale linearly
        }
        mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y), .1f, .1f);
    }


    // Helper functions
    void SpawnDirectionalAttack(Vector2 offsetFromPlayer, Hitbox.AttackDefinition attackInfo)
    {
        GameObject newHitbox = Instantiate(SimManager.Instance.GetPrefab("BaseHitbox_V2"), transform); // Spawn a hitbox


        newHitbox.transform.localScale = new Vector3(newHitbox.transform.localScale.x * attackInfo.HitboxScale.x, newHitbox.transform.localScale.y * attackInfo.HitboxScale.y, 1.0f); // Sets scale equal to a multiplier of the player's scale
        newHitbox.transform.localPosition += new Vector3(offsetFromPlayer.x, offsetFromPlayer.y); // Adds the given offset
        newHitbox.GetComponent<Hitbox>().InitAttack(attackInfo);

        Debug.DrawLine(transform.position, transform.position + new Vector3(offsetFromPlayer.x, offsetFromPlayer.y), Color.white, 5.0f);
    }


    // State functions

    void StartIdle(PlayerStates prevState)
    {
        mAnimationController.SetAnimationState("Player_Idle");
    }

    void StartDash(PlayerStates prevState)
    {
        // Do squash and stretch
        float timeElapsed = 0.0f;

        // Scale down to min
        float currDashStageTime = mPlayerBaseStats.mMovementStats.DashDuration / 4;
        mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x * 1.2f, mOGScale.y * mActionAesthetics.DashStretchMin), currDashStageTime, 0.0f, Action_.EasingTypes.EaseInSmall);
        timeElapsed += currDashStageTime;

        // up to max
        currDashStageTime = mPlayerBaseStats.mMovementStats.DashDuration / 2;
        mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x * mActionAesthetics.DashStretchMin, mOGScale.y * mActionAesthetics.DashStretchMax), currDashStageTime,  timeElapsed, Action_.EasingTypes.EaseInBounce);
        timeElapsed += currDashStageTime;

        // Scale back to normal
        currDashStageTime = mPlayerBaseStats.mMovementStats.DashDuration / 4;
        mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y), currDashStageTime, timeElapsed, Action_.EasingTypes.EaseOutMedium);

        // Nothing needed gameplay wise, movememnt update speeds up while in dash state
    }

    void StartDashAttackCharge(PlayerStates prevState)
    {
        mAnimationController.SetAnimationState("Player_DashAttackCharge");

        // Initialize variables
        mCurrDashAttackState = DashAttackStates.Charge;
        mDashAttackInputReleased = false;

        mActionList.AddActionCallback(() => mCurrDashAttackState = DashAttackStates.Ready, mDashAttackStats.ChargeTime); // Set timer for charge to be ready
    }

    void StartDashAttackRecovery()
    {
        mCurrDashAttackState = DashAttackStates.Recovery;

        mAnimationController.SetAnimationState("Player_DashAttackRecoverySheathed");

        mActionList.AddActionCallback(() => EndDashAttackRecovery(), mDashAttackStats.RecoveryTime);
    }

    void EndDashAttackRecovery()
    {
        if (mStateManager.CanEnterState(PlayerStates.Ready))
        {
            mStateManager.EnterState(PlayerStates.Ready);
        }
    }
}

