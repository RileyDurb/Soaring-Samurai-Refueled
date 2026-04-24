using System;
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
    public PlayerBaseDataObject mPlayerBaseStats;
    public DashAttackDataObject mDashAttackStats;


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

    public Vector2 LastDirectionalMoveInput
    {
        get { return mLastDirectionalMoveInput; }
    }
    
    public Vector2 OGScale
    {
        get { return mOGScale; }
    }

    public void SetIsNonPlayerControlled()
    {
        mNonPlayerControlled = true;
    }


    // Public variables //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public Action<int, int> OnDamageTaken; // Event called when taking damage, 1st parameter is the player index who took the damage, 2nd paramater is who gave the damage

    // Private variables //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Vector2 mMoveInput;
    Vector2 mLastDirectionalMoveInput;
    Vector2 mOGScale; // Original scale, for using to base squash and stretch off of, so we don't loose it with overlapping actions and become bigger


    [SerializeField]
    private int playerIndex = -1; // Index of player, inits to less than 0 to represent no player assigned

    private bool mNonPlayerControlled = false;

    // Component references
    AnimationController mAnimationController;

    PlayerCombatController mOpponentRef;

    StateManagerPlayer mStateManager;

    [SerializeField] GameObject mHealthBar;
    [SerializeField] GameObject mRoundWinsIndicator;

    private void Awake()
    {
        // Set this as the parent of this player's round win indicator
        if (mRoundWinsIndicator != null)
        {
            mRoundWinsIndicator.GetComponent<RoundWinIndicator>().OwningPlayer = this;
        }
    }

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
        //mStateManager.AddOnEnter(PlayerStates.Ready, StartIdle);

        //mStateManager.AddOnEnter(PlayerStates.Dash, StartDash);

        //mStateManager.AddOnEnter(PlayerStates.DashAttack, StartDashAttackCharge);

        // Set up health bar
        if (mHealthBar != null)
        {
            mHealthBar.GetComponent<HealthBarController>().SetPoolToRepresent(GetComponent<PoolContainer>().GetPool("Health"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        mActionList.Update(Time.deltaTime);



        // Apply movement from current input value
        PhysicsApplier physics = GetComponent<PhysicsApplier>();


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
    }

    public void SetPlayerIndex(int newPlayerIndex, bool isPlayerControlled)
    {
        playerIndex = newPlayerIndex;
        mNonPlayerControlled = !isPlayerControlled;
    }


    // Action functions
    public void OnMove(InputAction.CallbackContext context)
    {
        // If combat actions are blocked, don't attack
        if (LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().IsInputTypeBlocked(InputBlockingManager.InputType.MovementAction))
        {
            mMoveInput = Vector2.zero; // Stop any previous movement input
            return;
        }

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

        // If combat actions are blocked, don't attack
        if (LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().IsInputTypeBlocked(InputBlockingManager.InputType.CombatAction))
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

        // If combat actions are blocked, don't attack
        if (LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().IsInputTypeBlocked(InputBlockingManager.InputType.CombatAction))
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

        // If combat actions are blocked, don't attack
        if (LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().IsInputTypeBlocked(InputBlockingManager.InputType.CombatAction))
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

        // If combat actions are blocked, don't attack
        if (LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().IsInputTypeBlocked(InputBlockingManager.InputType.CombatAction))
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

        // If combat actions are blocked, don't attack
        if (LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().IsInputTypeBlocked(InputBlockingManager.InputType.MovementAction))
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
        // If combat actions are blocked, don't attack
        if (LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().IsInputTypeBlocked(InputBlockingManager.InputType.CombatAction))
        {
            return;
        }

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
                State_DashAttack dashAttackState = mStateManager.GetState(PlayerStates.DashAttack) as State_DashAttack;
                dashAttackState.DashAttackInputReleased = true;
            }
        }
    }


    // Public interface /////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

    public void ApplyCappedMovementJerk(Vector2 moveVec, float dt)
    {
        PhysicsApplier physics = GetComponent<PhysicsApplier>();
        physics.mDirectionalForces.ApplyJerk(moveVec * dt);
    }

    public void ApplyUncappedMovementJerk(Vector2 moveVec, float dt)
    {
        PhysicsApplier physics = GetComponent<PhysicsApplier>();
        physics.mUncappedDirectionalForces.ApplyJerk(moveVec * dt);
    }

    // Combat related functions //////////////////////////////////////////////////////////////////////////////////////////////////////
    public void TakeDamage(Hitbox.AttackCurrentData attackData, Hitbox.AttackDefinition baseAttackInfo)
    {
        // Only allow damage while the match is in progress
        if (LevelScopeManagers.Instance.GetComponent<MatchStateManager>().CurrMatchState != MatchStateManager.MatchState.InProgress)
        {
            return;
        }

        bool wasDefeated = GetComponent<PoolContainer>().GetPool("Health").DecreasePool(baseAttackInfo.Damage);


        if (SimManager.Instance.DebugModeOn)
        {
            Debug.DrawRay(transform.position, attackData.Knockback, Color.yellow, .5f, false);
        }

        if (attackData.Knockback.magnitude > 0.0f)
        {
            mActionList.AddActionEqualizedKnockback(gameObject, attackData.Knockback, baseAttackInfo.KnockbackEqualizationPercent, baseAttackInfo.KnockbackDuration);
        }

        if (baseAttackInfo.UseCustomHitSquishCurve)
        {
            mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y * 1.2f), .1f, 0.0f, Action_.EasingTypes.Custom, baseAttackInfo.SquishCurve);
        }
        else
        {
            mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y * 1.2f), .1f); // Don't ease, just scale linearly
        }
        mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y), .1f, .1f);


        // Notify match of the player being defeated
        if (wasDefeated)
        {
            LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerDefeated.Invoke(playerIndex);

            GetComponent<StateManagerPlayer>().EnterState(PlayerStates.Defeated);
        }

        if (OnDamageTaken != null)
        {
            OnDamageTaken(PlayerIndex, attackData.AttackingSourcePlayerID);
        }
    }

    public void SpawnDirectionalAttack(Vector2 offsetFromPlayer, Hitbox.AttackDefinition attackInfo)
    {
        GameObject newHitbox = Instantiate(SimManager.Instance.GetPrefab("BaseHitbox_V2"), transform); // Spawn a hitbox


        newHitbox.transform.localScale = new Vector3(newHitbox.transform.localScale.x * attackInfo.HitboxScale.x, newHitbox.transform.localScale.y * attackInfo.HitboxScale.y, 1.0f); // Sets scale equal to a multiplier of the player's scale
        newHitbox.transform.localPosition += new Vector3(offsetFromPlayer.x, offsetFromPlayer.y); // Adds the given offset
        newHitbox.GetComponent<Hitbox>().InitAttack(attackInfo, PlayerIndex);

        Debug.DrawLine(transform.position, transform.position + new Vector3(offsetFromPlayer.x, offsetFromPlayer.y), Color.white, 5.0f);
    }
}

