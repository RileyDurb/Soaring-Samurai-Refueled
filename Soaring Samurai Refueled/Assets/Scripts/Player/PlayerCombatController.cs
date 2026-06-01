using System;
using System.Xml;
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

    public enum PlayerMoves
    {
        URAttack,
        DRAttack,
        DLAttack,
        ULAttack,
        Dash,
        DashAttack
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
    public PlayerStateAesthetics StateAesthetics;

    public GameObject mTempClashParticlesPrefab;
    GameObject mTempClashParticlesObjectRef;




    [SerializeField] AttackDataObject DirectionalSlashAttackStats;

    [SerializeField] GasMeterStats GasMeterData;


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

    public bool IsSprintOn
    {
        get { return mCurrSprintOn; }
    }
    public void SetIsNonPlayerControlled()
    {
        mNonPlayerControlled = true;
    }

    public bool GetIsNonPlayerControlled()
    {
        return mNonPlayerControlled;
    }

    public GameObject SpriteObject {  get { return mSpriteObject; } }

    public string PlayerName {  get { return mPlayerName; } }

    public CharacterDataManager.Characters CharacterVisualsName {  get { return mCurrentCharacterVisuals; } }

    // Public variables //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public Action<int, int> OnDamageTaken; // Event called when taking damage, 1st parameter is the player index who took the damage, 2nd paramater is who gave the damage

    public Action<CharacterDataManager.Characters> OnCharacterChanged;

    // Private variables //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Vector2 mMoveInput;
    Vector2 mLastDirectionalMoveInput;
    Vector2 mOGScale; // Original scale, for using to base squash and stretch off of, so we don't loose it with overlapping actions and become bigger

    bool mCurrSprintOn = false;
    bool mSprintTurnsOnWithInput = false;
    bool mSprintInToggleMode = true; // If toggle, each press & release only changes the state once, else the button press only changes sprint state until it's released


    [SerializeField]
    private int mPlayerIndex = -1; // Index of player, inits to less than 0 to represent no player assigned

    string mPlayerName = "";

    private bool mNonPlayerControlled = false;

    CharacterDataManager.Characters mCurrentCharacterVisuals;

    // Component references
    AnimationController mAnimationController;

    PlayerCombatController mOpponentRef;

    StateManagerPlayer mStateManager;

    PoolContainer mPoolContainer;

    GameObject mSpriteObject;

    [SerializeField] GameObject mHealthBar;
    [SerializeField] GameObject mRoundWinsIndicator;

    private void Awake()
    {
        // Set this as the parent of this player's round win indicator
        if (mRoundWinsIndicator != null)
        {
            mRoundWinsIndicator.GetComponent<RoundWinIndicator>().OwningPlayer = this;
        }

        mSpriteObject = transform.Find("PlayerSprite").gameObject;
    }

    void Start()
    {
        // Set component references
        mAnimationController = mSpriteObject.GetComponent<AnimationController>();

        mStateManager = GetComponent<StateManagerPlayer>();

        mPoolContainer = GetComponent<PoolContainer>();

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

        // Set player name
        mPlayerName = "Player " + (mPlayerIndex + 1);
        //gameObject.name = mPlayerName;

        // Set up health bar
        if (mHealthBar != null)
        {
            HealthBarController healthBar = mHealthBar.GetComponent<HealthBarController>();
            healthBar.SetPoolToRepresent(GetComponent<PoolContainer>().GetPool("Health")); // Set the health pool to be represented by the health bar
            healthBar.SetPoolToRepresent(GetComponent<PoolContainer>().GetPool("Gas")); // Set gas meter to represent the gas pool
            healthBar.SetPlayerNameText(mPlayerName); // Set the player's name on the health bar (player name uses player index plus 1 to convert the 0 based index into a more expected 1 based player number)


        }


        switch (mPlayerIndex)
        {
            case 0:
                {
                    SetCharacterVisuals(CharacterDataManager.Characters.BluePlayer);
                    break;
                }
            case 1:
                {
                    SetCharacterVisuals(CharacterDataManager.Characters.RedPlayer);
                    break;
                }
            default:
                {
                    SetCharacterVisuals(CharacterDataManager.Characters.BluePlayer);
                    break;
                }
        }

        mCurrSprintOn = !mSprintTurnsOnWithInput;
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



        // Apply gas gain for if we've moving toward our opponent

        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;

        if (velocity.magnitude != 0.0f) // Don't gain meter if velocity is 0, which would pass the angle threshold, but not have any forward movement
        {
            Vector2 vecToOpponent = mOpponentRef.transform.position - transform.position;

            float movementAngle = Mathf.Abs(Vector2.Angle(velocity, vecToOpponent));

            if (movementAngle <= GasMeterData.MovingForwardAngleForgiveness / 2.0f) // If moving forward within the angle of forgiveness
            {
                // Apply moving forward meter gain
                GetComponent<PoolContainer>().GetPool("Gas").DecreasePool(-GasMeterData.GasPerSecondMovingForward * Time.deltaTime);
            }

        }

    }

    // Getters and setters
    public int PlayerIndex
    {
        get { return mPlayerIndex; }
    }

    public void SetPlayerIndex(int newPlayerIndex, bool isPlayerControlled)
    {
        mPlayerIndex = newPlayerIndex;
        mNonPlayerControlled = !isPlayerControlled;
    }


    // Action functions
  
    public void OnMove(InputAction.CallbackContext context)
    {
        OnMove(context.phase, context.ReadValue<Vector2>());
    }
    public void OnMove(InputActionPhase inputPhase, Vector2 inputValue)
    {
        // If combat actions are blocked, don't attack
        if (LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().IsInputTypeBlocked(InputBlockingManager.InputType.MovementAction, mPlayerIndex))
        {
            mMoveInput = Vector2.zero; // Stop any previous movement input
            return;
        }

        switch (inputPhase)
        {
            case InputActionPhase.Performed:
                {
                    mMoveInput = inputValue;
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
            // If any meter gain on use (regardless of if it hits), apply gas meter gain
            if (DirectionalSlashAttackStats.mStats.GasGainOnUse > 0.0f)
            {
                mPoolContainer.GetPool("Gas").DecreasePool(-DirectionalSlashAttackStats.mStats.GasGainOnUse);
            }

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
            // If any meter gain on use (regardless of if it hits), apply gas meter gain
            if (DirectionalSlashAttackStats.mStats.GasGainOnUse > 0.0f)
            {
                mPoolContainer.GetPool("Gas").DecreasePool(-DirectionalSlashAttackStats.mStats.GasGainOnUse);
            }

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
            // If any meter gain on use (regardless of if it hits), apply gas meter gain
            if (DirectionalSlashAttackStats.mStats.GasGainOnUse > 0.0f)
            {
                mPoolContainer.GetPool("Gas").DecreasePool(-DirectionalSlashAttackStats.mStats.GasGainOnUse);
            }

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
            // If any meter gain on use (regardless of if it hits), apply gas meter gain
            if (DirectionalSlashAttackStats.mStats.GasGainOnUse > 0.0f)
            {
                mPoolContainer.GetPool("Gas").DecreasePool(-DirectionalSlashAttackStats.mStats.GasGainOnUse);
            }

            mStateManager.EnterState(PlayerStates.SlashAttack, DirectionalSlashAttackStats.mStats.ActiveTime, PlayerStates.Ready); // Enter State, and set up state done timer

            SpawnDirectionalAttack(new Vector2(1, 1) * DirectionalSlashAttackStats.mStats.AttackOffsetDistance, DirectionalSlashAttackStats.mStats);

            // Set animation and facing direction
            SetFacingDirection(FacingDirection.Right);

            mAnimationController.SetAnimationState("Player_URNormalAttack");
        }
    }


    public void OnDash(InputAction.CallbackContext context)
    {
        DashInput(context.phase);
    }

    public void DashInput(InputActionPhase inputPhase)
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

        if (inputPhase == InputActionPhase.Canceled)
        {
            // Check gas cost
            PoolContainer.Pool gasPool = GetComponent<PoolContainer>().GetPool("Gas");
            if (gasPool.PoolValue < mPlayerBaseStats.mMovementStats.DashGasCost) // If not enough gas
            {
                return; // Can't do move, return
            }
            else
            {
                gasPool.DecreasePool(mPlayerBaseStats.mMovementStats.DashGasCost); // Pay gas cost
            }

            mStateManager.EnterState(PlayerStates.Dash, mPlayerBaseStats.mMovementStats.DashDuration, PlayerStates.Ready);
        }
    }

    public void OnDashAttack(InputAction.CallbackContext context)
    {
        DashAttackInput(context.phase);
    }

    public void DashAttackInput(InputActionPhase inputPhase)
    {
        // If combat actions are blocked, don't attack
        if (LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().IsInputTypeBlocked(InputBlockingManager.InputType.CombatAction))
        {
            return;
        }

        if (inputPhase == InputActionPhase.Performed)
        {
            if (mStateManager.CanEnterState(PlayerStates.DashAttack) == false || mStateManager.CurrStateName == PlayerStates.DashAttack)
            {
                return;
            }

            // Check gas cost
            PoolContainer.Pool gasPool = GetComponent<PoolContainer>().GetPool("Gas");
            if (gasPool.PoolValue < mDashAttackStats.mStats.GasCost) // If not enough gas
            {
                return; // Can't do move, return
            }
            else
            {
                gasPool.DecreasePool(mDashAttackStats.mStats.GasCost); // Pay gas cost
            }

            // Start the move
            mStateManager.EnterState(PlayerStates.DashAttack);
        }
        else if (inputPhase == InputActionPhase.Canceled)
        {
            if (mStateManager.CurrStateName == PlayerStates.DashAttack)
            {
                State_DashAttack dashAttackState = mStateManager.GetState(PlayerStates.DashAttack) as State_DashAttack;
                dashAttackState.DashAttackInputReleased = true;
            }
        }
    }


    public void OnPauseTriggered(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Canceled)
        {
            SimManager.Instance.QueuePauseMenuChange();
        }
    }

    public void SprintInputGiven(InputAction.CallbackContext context)
    {
        SprintInputGiven(context.phase);
    }

    public void SprintInputGiven(InputActionPhase inputPhase)
    {
        if (mSprintInToggleMode)
        {
            if (inputPhase == InputActionPhase.Started)
            {
                mCurrSprintOn = !mCurrSprintOn; // Toggle sprint state
            }
        }
        else // In hold mode
        {
            if (inputPhase == InputActionPhase.Started) // Change on input started
            {
                mCurrSprintOn = mSprintTurnsOnWithInput;
            }
            else if (inputPhase == InputActionPhase.Canceled) // Change back on input ended
            {
                mCurrSprintOn = !mSprintTurnsOnWithInput;
            }
        }

    }



    // Public interface /////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetFacingDirection(FacingDirection newDirection)
    {
        if (newDirection == FacingDirection.Left)
        {
            mAnimationController.GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            mAnimationController.GetComponent<SpriteRenderer>().flipX = false;
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

        // Factor in clashing
        float currentDamage = baseAttackInfo.Damage;
        if (attackData.IsClashing)
        {
            currentDamage = 0.0f;

            // Apply meter gain on clash
            mPoolContainer.GetPool("Gas").DecreasePool(-GasMeterData.GasGainOnClash);
        }

        // Apply damage to health
        bool wasDefeated = GetComponent<PoolContainer>().GetPool("Health").DecreasePool(currentDamage);


        if (SimManager.Instance.DebugModeOn)
        {
            // Debug draw knockback
            Debug.DrawRay(transform.position, attackData.Knockback, Color.yellow, .5f, false);
        }

        // Apply knockback
        if (attackData.Knockback.magnitude > 0.0f)
        {
            mActionList.AddActionEqualizedKnockback(gameObject, attackData.Knockback, baseAttackInfo.KnockbackEqualizationPercent, baseAttackInfo.KnockbackDuration);
        }

        // Apply hit squish
        if (baseAttackInfo.UseCustomHitSquishCurve)
        {
            mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y * 1.2f), .1f, 0.0f, Action_.EasingTypes.Custom, baseAttackInfo.SquishCurve);
        }
        else
        {
            mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y * 1.2f), .1f); // Don't ease, just scale linearly
        }
        mActionList.AddActionScale(gameObject, new Vector2(mOGScale.x, mOGScale.y), .1f, .1f);

        // Spawn hit particles
        // TODO: Spawn normal hit particles (TODO: Change both these systems to come from the attack definition
        if (attackData.IsClashing)
        {
            // Spawn clash particles

            Vector2 knockbackVecNormalized = attackData.Knockback.normalized;
            Quaternion systemRotation = Quaternion.FromToRotation(Vector3.up, new Vector3(knockbackVecNormalized.x, knockbackVecNormalized.y, 0));
            mTempClashParticlesObjectRef = Instantiate(mTempClashParticlesPrefab, transform.position, systemRotation);

            if (SimManager.Instance.DebugModeOn)
            {
                Vector3 particleSystemDirectionVec = systemRotation * Vector3.up;
                Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + particleSystemDirectionVec * 10.0f, Color.blue, 10.0f);
            }

            // Add queued clash, so only one VFX gets spawned for both clashes
            LevelScopeManagers.Instance.GetComponent<MatchStateManager>().mHitsThisFrame.Add(new Tuple<int, Hitbox.AttackCurrentData>(PlayerIndex, attackData));
        }
        else if (baseAttackInfo.HitParticlesPrefab != null) // Normal hit
        {
            // Spawn hit particles

            // Find out which direction to flip the effect
            Quaternion hitParticleRotation = Quaternion.identity;
            float flipX = 0;
            float flipY = 0;

            if (attackData.AttackOffset.magnitude > 0) // If a directional attack, rotate the particle based on the direction of the attack
            {
                float attackDotProductFromUp = Vector2.Dot(Vector2.up, attackData.AttackOffset);

                if (attackDotProductFromUp >= 0) // If upper right or left
                {
                    if (attackData.AttackOffset.x < 0) // If negative, it's an up left attack
                    {
                        // Don't flip, particle is made for up right
                    }
                    else // Upper left
                    {
                        flipX = 1;
                    }
                }
                else // Lower right or left
                {
                    if (attackData.AttackOffset.x < 0) // If negative, it's a down left attack
                    {
                        flipY = 1;
                    }
                    else // Lower right attack
                    {
                        flipX = 1;
                        flipY = 1;
                    }
                }
            }

            GameObject newHitParticlesRef = null;
            if (baseAttackInfo.HitParticlesFollowTarget)
            {
                newHitParticlesRef = Instantiate(baseAttackInfo.HitParticlesPrefab, transform); // Spawn particles as a child of the hit player
            }
            else 
            {
                newHitParticlesRef = Instantiate(baseAttackInfo.HitParticlesPrefab, transform.position, Quaternion.identity); // Just spawn particles at the position of the hit player
            }
            newHitParticlesRef.GetComponent<ParticleSystemRenderer>().flip = new Vector3(flipX, flipY, 0);

        }

        // Play SFX 
        if (attackData.IsClashing == false)
        {
            PersistentScopeManagers.Instance.GetComponent<AudioManager>().PlayEvent(baseAttackInfo.HitSound);
        }

        

        // Notify match of the player being defeated
        if (wasDefeated)
        {
            LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerDefeated.Invoke(mPlayerIndex);

            GetComponent<StateManagerPlayer>().EnterState(PlayerStates.Defeated);
        }
        else if (mStateManager.CanEnterState(PlayerStates.Flinch)) // If can flinch
        {
            mStateManager.EnterState(PlayerStates.Flinch, baseAttackInfo.HitStunTime, PlayerStates.Ready); // Go into flinching
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

        // Play attack sound
        PersistentScopeManagers.Instance.GetComponent<AudioManager>().PlayEvent(attackInfo.AttackStartSoundEvent);

        if (SimManager.Instance.DebugModeOn)
        {
            Debug.DrawLine(transform.position, transform.position + new Vector3(offsetFromPlayer.x, offsetFromPlayer.y), Color.white, 5.0f);
        }
    }

    public void HitOpponentWithAttack(Hitbox.AttackCurrentData attackData, Hitbox.AttackDefinition baseAttackInfo)
    {
        mPoolContainer.GetPool("Gas").DecreasePool(-baseAttackInfo.GasGainOnHit);
    }

    public void SetCharacterVisuals(CharacterDataManager.Characters characterToBe)
    {
        if (mHealthBar != null)
        {
            HealthBarController healthBar = mHealthBar.GetComponent<HealthBarController>();
            healthBar.SetPlayerPortrait(characterToBe);
        }

        // Set color scheme material;
        mAnimationController.GetComponent<SpriteRenderer>().material = PersistentScopeManagers.Instance.GetComponent<CharacterDataManager>().GetCharacterVisualData().GetCharacterVisuals(characterToBe).PlayerColorsMaterial;

        mCurrentCharacterVisuals = characterToBe;

        if (OnCharacterChanged != null)
        {
            OnCharacterChanged.Invoke(mCurrentCharacterVisuals);
        }
    }
}

