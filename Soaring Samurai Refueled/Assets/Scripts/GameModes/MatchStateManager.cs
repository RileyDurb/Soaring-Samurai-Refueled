using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchStateManager : MonoBehaviour
{
    // Private class and enum definitions ////////////////////////////////////////////////////////
    enum MatchState
    {
        PreRound,
        InProgress,
        PostRound,
        GameEnd
    }

    // Events //////////////////////////////////////////////////////////////////////////////////////
    public Action<int> PlayerDefeated;
    public Action<int, int> PlayerRoundWin; // Event for saying a player has won, sends the player index as the 1st parameter, and the new number of round wins in this match as the second parameter
    public Action OnInitMatch;


    // Private variables ///////////////////////////////////////////////////////////////////////////
    private List<PlayerCombatController> mPlayers = new List<PlayerCombatController>();
    [SerializeField] MatchTuningStats mMatchStats;
    ActionList mActionList = new ActionList();
    MatchState mCurrMatchState = MatchState.PreRound;

    // Round Start Message Variables
    [SerializeField]GameObject mMatchStartMessagePrefab;
    GameObject mMatchStartMessageObject;

    // Match win menu variables
    [SerializeField] GameObject mMatchWinMenuPrefab;
    GameObject mMatchWinMenuObject;
    
    // Round timer variables
    [SerializeField] GameObject mRoundTimerPrefab;
    float mCurrMatchTimer = -1.0f;
    Dictionary<int, int> mCurrRoundWins = new Dictionary<int, int>(); // Number of round wins for the current match, keyed by which player won them
    Dictionary<int, int> mTotalRoundWins = new Dictionary<int, int>(); // Number of total round wins for all consecutive matches played in this matchup, keyed by which player won them

    int mCurrRoundNumber = 0;

    // Getters and setters
    public List<PlayerCombatController> PlayerList {  get { return mPlayers; } }

    public int CurrRoundTimeTrimmed {  get { return (int)mCurrMatchTimer; } }

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

        // Create round timer
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PushHUDItem(mRoundTimerPrefab);

        RestartMatch();
    }

    // Update is called once per frame
    void Update()
    {
        mActionList.Update(Time.deltaTime);

        if (mCurrMatchState == MatchState.InProgress)
        {
            mCurrMatchTimer -= Time.deltaTime;
        }
    }

    // Helper functions
    void RestartRound()
    {
        int numPlayers = mPlayers.Count;
        Vector2 currSpawnVec = Vector2.left * mMatchStats.PlayerStartOffsetDistance;

        float playerOffsetAngle = 360.0f / numPlayers;

        for (int i = 0; i < numPlayers; i++)
        {
            PlayerCombatController currPlayer = mPlayers[i];

            if (mCurrRoundNumber == 0) // If on the first round
            {
                if (mMatchStats.ResetPositionsOnMatchStart)
                {
                    // Reset player position
                    currPlayer.GetComponent<Rigidbody2D>().MovePosition(currSpawnVec);
                }
            }
            else // For subsequent rounds
            {
                if (mMatchStats.ResetPositionsOnRoundStart)
                {
                    // Reset player position
                    currPlayer.GetComponent<Rigidbody2D>().MovePosition(currSpawnVec);
                }
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

        InitRoundState(); // Inits state info like the timer

        StartPreRound();
    }


    void StartPreRound()
    {
        mCurrMatchState = MatchState.PreRound;
        
        // Block all combat actions
        LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().BlockInputType(InputBlockingManager.InputType.CombatAction);

        if (mMatchStartMessageObject == null)
        {
            mMatchStartMessageObject = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPopup(mMatchStartMessagePrefab);

            // Animation plays automatically on spawn, so don't need to trigger state
        }
        else
        {
            if (mCurrRoundNumber == 0)
            {
                mMatchStartMessageObject.GetComponent<AnimationController>().SetAnimationState("MatchStartSequence");
            }
            else
            {
                mMatchStartMessageObject.GetComponent<AnimationController>().SetAnimationState("MatchRound2StartSequence");
            }
        }

        // Begin the round after a delay
        float currPreRoundLength = mCurrRoundNumber == 0 ? mMatchStats.FirstPreRoundLength : mMatchStats.SubsequentPreRoundsLength;
        mActionList.AddActionCallback(() => { BeginRound(); }, currPreRoundLength);
    }



    void InitMatch()
    {
        // Reset current round wins
        int[] playersWithRoundWins = mCurrRoundWins.Keys.ToArray();
        foreach (int playerID in playersWithRoundWins)
        {
            mCurrRoundWins[playerID] = 0;
        }

        mCurrRoundNumber = 0;

        if (OnInitMatch != null)
        {
            OnInitMatch.Invoke();
        }
    }
    void InitRoundState()
    {
        mCurrMatchTimer = mMatchStats.MaxRoundTime;
    }

    void BeginRound()
    {
        // Unblock combat actions
        LevelScopeManagers.Instance.GetComponent<InputBlockingManager>().UnblockInputType(InputBlockingManager.InputType.CombatAction);

        mCurrMatchState = MatchState.InProgress;
    }
    void TriggerRoundAdvanceSequence()
    {
        // TODO: Make the restart of the match require some sort of confirming, and also make it round based
        mActionList.AddActionCallback(() => { RestartRound(); }, mMatchStats.MatchEndRestartDelay);
    }

    void TriggerMatchEndSequence(int winningPlayerID)
    {
        print("Match be won");

        mMatchWinMenuObject = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushMenu(mMatchWinMenuPrefab);
        mMatchWinMenuObject.GetComponent<MatchEndMenuFeatures>().SetWinnerNameMessage("Player " + (winningPlayerID + 1).ToString() + " Has Won This Fight");
    }

    // Event subscriptions //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void HandlePlayerDefeated(int playerID)
    {
        // TODO: make this handle more than 2 players if we want that

        PlayerCombatController winningPlayer = null; 
        // Find winning player
        foreach (PlayerCombatController player in mPlayers)
        {
            if (player.GetComponent<PoolContainer>().GetPool("Health").PoolValue > 0.0f) // If player is still alove
            {
                winningPlayer = player;
                break;
            }
        }

        if (winningPlayer == null)
        {
            print("MatchStateManager: HandlePlayerDefeated: No live player could be found to count as winner");
            return;
        }

        // Inrement number of player wins

        int winningPlayerID = winningPlayer.PlayerIndex;

        if (mCurrRoundWins.ContainsKey(winningPlayerID))
        {
            mCurrRoundWins[winningPlayerID]++;
            mTotalRoundWins[winningPlayerID]++;
        }
        else
        {
            mCurrRoundWins.Add(winningPlayerID, 1);
            mTotalRoundWins.Add(winningPlayerID, 1);
        }

        // Increment number of rounds
        mCurrRoundNumber++;

        // Call player event for things like round win indicators to respond
        if (PlayerRoundWin != null)
        {
            PlayerRoundWin.Invoke(winningPlayerID, mCurrRoundWins[winningPlayerID]);
        }

        // handle round advancing
        if (mCurrRoundWins[winningPlayerID] >= mMatchStats.NumRoundsToWin) // If match has been won
        {
            // trigger match end
            mCurrMatchState = MatchState.GameEnd;

            // TODO: Handle ending the round instead of transitioning to post round and restarting
            TriggerMatchEndSequence(winningPlayerID);
        }
        else
        {
            mCurrMatchState = MatchState.PostRound;
            TriggerRoundAdvanceSequence();
        }



    }

    // Public interface /////////////////////////////////////////////////////////////////////////////////
    public void RestartMatch()
    {
        InitMatch();

        RestartRound();
    }

}
