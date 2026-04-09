using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MatchStateManager : MonoBehaviour
{
    enum MatchState
    {
        PreRound,
        InProgress,
        PostRound,
        GameEnd
    }

    public Action<int> PlayerDefeated;


    // Private variables
    private List<PlayerCombatController> mPlayers = new List<PlayerCombatController>();
    [SerializeField] MatchTuningStats mMatchStats;
    ActionList mActionList = new ActionList();
    MatchState mCurrMatchState = MatchState.PreRound;

    [SerializeField]GameObject mMatchStartMessagePrefab;
    GameObject mMatchStartMessageObject;

    // Getters and setters
    public List<PlayerCombatController> PlayerList {  get { return mPlayers; } }

    // Start is called before the first frame update
    void Start()
    {
        mPlayers.Clear(); // Don't need to clear list if scence is always reloading, so can remove this if we only reload, but otherwise, want to clear the list and readd all players

        // Makes a list of all current players
        PlayerCombatController[] players = FindObjectsOfType<PlayerCombatController>();
        foreach (PlayerCombatController player in players)
        {
            mPlayers.Add(player);
        }


        // Subscribe player defeated function to player defeated event
        PlayerDefeated += HandlePlayerDefeated;

        RestartMatch();
    }

    // Update is called once per frame
    void Update()
    {
        mActionList.Update(Time.deltaTime);
    }

    // Helper functions
    void RestartMatch()
    {
        int numPlayers = mPlayers.Count;
        Vector2 currSpawnVec = Vector2.left * mMatchStats.PlayerStartOffsetDistance;

        float playerOffsetAngle = 360.0f / numPlayers;

        for (int i = 0; i < numPlayers; i++)
        {
            PlayerCombatController currPlayer = mPlayers[i];

            if (mMatchStats.ResetPositions)
            {
                // Reset player position
                currPlayer.GetComponent<Rigidbody2D>().MovePosition(currSpawnVec);
            }

            if (mMatchStats.ClearForcesOnRestart)
            {
                PhysicsApplier physicsApplier = currPlayer.GetComponent<PhysicsApplier>();
                physicsApplier.mDirectionalForces.ClearAllForces();
                physicsApplier.mUncappedDirectionalForces.ClearAllForces();
                physicsApplier.mRotationalForces.ClearAllForces();
            }

            currPlayer.GetComponent<PoolContainer>().GetPool("Health").ResetPool();

            currPlayer.GetComponent<StateManagerPlayer>().EnterState(PlayerStates.Ready);

            // Do other restarting stuff as needed

            // Rotate spawn vec
            currSpawnVec = Quaternion.Euler(0, 0, playerOffsetAngle) * currSpawnVec;
        }

        StartPreRound();
    }

    public void StartPreRound()
    {
        mCurrMatchState = MatchState.PreRound;
        
        // Block all combat actions
        LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().BlockInputType(InputBlockingManager.InputType.CombatAction);

        if (mMatchStartMessageObject == null)
        {
            mMatchStartMessageObject = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushHUDItem(mMatchStartMessagePrefab);

            // Animation plays automatically on spawn, so don't need to trigger state
        }
        else
        {
            mMatchStartMessageObject.GetComponent<AnimationController>().SetAnimationState("MatchStartSequence");
        }

        // Begin the round after a delay
        mActionList.AddActionCallback(() => { BeginRound(); }, mMatchStats.PreRoundLength);
    }

    public void BeginRound()
    {
        // Unblock combat actions
        LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().UnblockInputType(InputBlockingManager.InputType.CombatAction);
    }

    // Event subscriptions
    public void HandlePlayerDefeated(int playerID)
    {
        // TODO: make this handle more than 2 players if we want that

        // TODO: Make the restart of the match require some sort of confirming, and also make it round based
        mActionList.AddActionCallback(() => { RestartMatch(); }, mMatchStats.MatchEndRestartDelay);
    }


}
