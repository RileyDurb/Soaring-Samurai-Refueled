using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MatchStateManager : MonoBehaviour
{
    private List<PlayerCombatController> mPlayers = new List<PlayerCombatController>();

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
