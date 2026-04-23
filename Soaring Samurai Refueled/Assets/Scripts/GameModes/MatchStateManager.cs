using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MatchStateManager : MonoBehaviour
{
    // Private class and enum definitions ////////////////////////////////////////////////////////
    public enum MatchState
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


    // Sudden death variables
    [SerializeField] GameObject mSuddenDeathMessagePrefab;
    GameObject mSuddenDeathMessageObject;
    bool mInSuddenDeath = false;

    // Round Win message variables
    [SerializeField] GameObject mRoundWinMessagePrefab;
    GameObject mRoundWinMessageObject;

    // Round timer variables
    [SerializeField] GameObject mRoundTimerPrefab;
    float mCurrMatchTimer = -1.0f;
    Dictionary<int, int> mCurrRoundWins = new Dictionary<int, int>(); // Number of round wins for the current match, keyed by which player won them
    Dictionary<int, int> mTotalRoundWins = new Dictionary<int, int>(); // Number of total round wins for all consecutive matches played in this matchup, keyed by which player won them

    int mCurrRoundNumber = 0;

    // Getters and setters
    public List<PlayerCombatController> PlayerList {  get { return mPlayers; } }

    public int CurrRoundTimeTrimmed {  get { return (int)mCurrMatchTimer; } }

    public MatchState CurrMatchState { get { return mCurrMatchState; } }

    public float RoundTimeUntrimmed { set { mCurrMatchTimer = value; } }

    public MatchTuningStats MatchStats { get { return mMatchStats; } }  

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

        if (mCurrMatchState == MatchState.InProgress && mInSuddenDeath == false)
        {
            mCurrMatchTimer -= Time.deltaTime;

            if (mCurrMatchTimer <= 0.0f) // If time is up
            {
                // Handle timeout win
                HandleTimerUp();   
            }
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

        mInSuddenDeath = false;

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
        mRoundWinMessageObject = LevelScopeManagers.Instance.GetComponent<HUDManager>().AddInfoItem(mRoundWinMessagePrefab); // Add round win message
        mActionList.AddActionCallback(() => { RestartRound(); }, mMatchStats.MatchEndRestartDelay);
    }

    void TriggerMatchEndSequence(int winningPlayerID)
    {
        print("Match be won");

        mMatchWinMenuObject = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushMenu(mMatchWinMenuPrefab);
        mMatchWinMenuObject.GetComponent<MatchEndMenuFeatures>().SetWinnerNameMessage("Player " + (winningPlayerID + 1).ToString() + " Has Won This Fight");
    }

    void HandleTimerUp()
    {
        // Find highest health remaining, or all highest healths if it's a tie
        List<Tuple<int, float>> mCurrentHighestHealths = new List<Tuple<int, float>>();

        foreach (PlayerCombatController player in mPlayers)
        {
            float currHealth = player.GetComponent<PoolContainer>().GetPool("Health").PoolValue;

            if (mCurrentHighestHealths.Count <= 0) // If no current highest health
            {
                // Save this as current highest
                mCurrentHighestHealths.Add(new Tuple<int, float>(player.PlayerIndex, currHealth));
            }
            else
            {
                if (mCurrentHighestHealths[0].Item2 < currHealth) // If this player has the new highest health
                {
                    mCurrentHighestHealths.Clear(); // Remove all highest healths

                    mCurrentHighestHealths.Add(new Tuple<int, float>(player.PlayerIndex, currHealth)); // Add this as the new highest health
                }
                else if (mCurrentHighestHealths[0].Item2 == currHealth) // If this player's health is the same as current highestr
                {
                    mCurrentHighestHealths.Add(new Tuple<int, float>(player.PlayerIndex, currHealth)); // Add this to the current highest count, for a potential tie or sudden death
                }

                // If it wasn't added to the highest list by here, not the current highest health, move on
            }
        }

        if (mCurrentHighestHealths.Count == 0)
        {
            print("MatchStateManager:Update: Timer ended, but no player with the highest health could be found. Investigate");
        }
        else if (mCurrentHighestHealths.Count == 1) // We have 1 winner
        {
            HandlePlayerWin(mCurrentHighestHealths[0].Item1); // Handle player win, giving the player with the current highest health the win
        }
        else // If there is a tie for the highest healths
        {
            // Make a list of all players that tied for the win
            int[] tieingPlayerIndicies = new int[mCurrentHighestHealths.Count];
            for (int i = 0; i < mCurrentHighestHealths.Count; i++)
            {
                tieingPlayerIndicies[i] = mCurrentHighestHealths[i].Item1;
            }

            // Call handling of the tie for the winning players
            HandleRoundTie(tieingPlayerIndicies);
        }
    }
    void HandlePlayerWin(int winningPlayerID)
    {
        // Inrement number of player wins

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

        if (mInSuddenDeath)
        {
            LevelScopeManagers.Instance.GetComponent<MenuManager>().PopHudItem();
            mSuddenDeathMessageObject = null;
        }


        // Call player event for things like round win indicators to respond
        if (PlayerRoundWin != null)
        {
            PlayerRoundWin.Invoke(winningPlayerID, mCurrRoundWins[winningPlayerID]);
        }

        // Increment number of rounds
        mCurrRoundNumber++;

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

    void HandleRoundTie(int[] winningPlayerIndices)
    {
        // If all players are 1 win away, go into sudden death
        bool allPlayersOneWinAway = true;
        for (int i = 0; i < winningPlayerIndices.Length; i++)
        {
            // If index is less than 0, it's an unassigned player, likely a debug situation, skip
            if (winningPlayerIndices[i] < 0)
            {
                continue;
            }

            if (mCurrRoundWins.ContainsKey(winningPlayerIndices[i]) == false ||
                mCurrRoundWins[winningPlayerIndices[i]] < mMatchStats.NumRoundsToWin - 1) // If more than one round win away from winning the match
            {
                allPlayersOneWinAway = false;
                break;
            }
        }

        if (allPlayersOneWinAway)
        {
            TriggerSuddenDeath(winningPlayerIndices);
            return;
        }


        // Give each player a win unless they are only 1 win away from a match win
        // This means a win can't happen off of a tie, but each player will get closer to the match win, or a sudden death
        for (int i = 0; i < winningPlayerIndices.Length; i++)
        {
            // If index is less than 0, it's an unassigned player, likely a debug situation, skip
            if (winningPlayerIndices[i] < 0)
            {
                continue;
            }
            if (mCurrRoundWins.ContainsKey(winningPlayerIndices[i]) == false ||
            mCurrRoundWins[winningPlayerIndices[i]] < mMatchStats.NumRoundsToWin - 1) // If more than one round win away from winning the match
            {
                if (mCurrRoundWins.ContainsKey(winningPlayerIndices[i]))
                {
                    mCurrRoundWins[winningPlayerIndices[i]]++;
                    mTotalRoundWins[winningPlayerIndices[i]]++;
                }
                else
                {
                    mCurrRoundWins.Add(winningPlayerIndices[i], 1);
                    mTotalRoundWins.Add(winningPlayerIndices[i], 1);
                }
            }
        }


        // Call player event for things like round win indicators to respond
        if (PlayerRoundWin != null)
        {
            for (int i = 0; i < winningPlayerIndices.Length; i++)
            {
                // If index is less than 0, it's an unassigned player, likely a debug situation, skip
                if (winningPlayerIndices[i] < 0)
                {
                    continue;
                }
                PlayerRoundWin.Invoke(winningPlayerIndices[i], mCurrRoundWins[winningPlayerIndices[i]]);
            }
        }

        // Increment number of rounds
        mCurrRoundNumber++;

        // Move to next round
        mCurrMatchState = MatchState.PostRound;
        TriggerRoundAdvanceSequence();
    }

    void TriggerSuddenDeath(int[] suddenDeathPlayers)
    {
        for (int i = 0; i < mPlayers.Count; i++)
        {
            PoolContainer.Pool currHealthPool = mPlayers[i].GetComponent<PoolContainer>().GetPool("Health");
            if (currHealthPool.PoolValue > 0.0f)
            {
                currHealthPool.PoolValue = mMatchStats.SuddenDeathHealthValue;
            }
        }

        mSuddenDeathMessageObject = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushHUDItem(mSuddenDeathMessagePrefab);

        mInSuddenDeath = true;
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

        HandlePlayerWin(winningPlayer.PlayerIndex);
    }

    // Public interface /////////////////////////////////////////////////////////////////////////////////
    public void RestartMatch()
    {
        InitMatch();

        RestartRound();
    }

}
