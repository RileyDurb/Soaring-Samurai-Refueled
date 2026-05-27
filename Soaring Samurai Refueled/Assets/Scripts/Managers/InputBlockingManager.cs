using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For managing input blocking overrides for all players
public class InputBlockingManager : MonoBehaviour
{
    // Public definitions ///////////////////////////////////////////////////////////
    public enum InputType
    {
        CombatAction,
        MovementAction,
        All
    }

    public class PlayerInputBlockingSet
    {
        public Dictionary<InputType, int> mActiveInputBlocks = new Dictionary<InputType, int>();
    }

    // Private variables ///////////////////////////////////////////////////////////

    HashSet<InputType> mActiveInputBlocks = new HashSet<InputType>();

    Dictionary<int, PlayerInputBlockingSet> mPlayerSpecificInputBlocks = new Dictionary<int, PlayerInputBlockingSet>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Public interface /////////////////////////////////////////////////////////////////
    public bool IsInputTypeBlocked(InputType typeToCheck)
    {
        return mActiveInputBlocks.Contains(typeToCheck) || mActiveInputBlocks.Contains(InputType.All);
    }

    public bool IsInputTypeBlocked(InputType typeToCheck, int playerIndex)
    {
        bool inputTypeGloballyBlocked = IsInputTypeBlocked(typeToCheck);

        if (inputTypeGloballyBlocked) // If input type is globally blocked
        {
            return true;
        }

        // If no player specific block list was created for player, or for the input on that player, then no specific blocks exist
        if (mPlayerSpecificInputBlocks.ContainsKey(playerIndex) == false || mPlayerSpecificInputBlocks[playerIndex].mActiveInputBlocks.ContainsKey(typeToCheck) == false)
        {
            return false;
        }

        int numInputBlocksForType = mPlayerSpecificInputBlocks[playerIndex].mActiveInputBlocks[typeToCheck];

        return numInputBlocksForType > 0; // Blocked if any input blocks exist
    }

    public void BlockInputType(InputType typeToBlock)
    {
        mActiveInputBlocks.Add(typeToBlock);
    }

    public void BlockInputType(InputType typeToBlock, int playerIndex)
    {
        // Lazily creates an input blocking set for the player at the index
        if (mPlayerSpecificInputBlocks.ContainsKey(playerIndex) == false)
        {
            mPlayerSpecificInputBlocks.Add(playerIndex, new PlayerInputBlockingSet());
        }

        // Adds or increments current block count
        if (mPlayerSpecificInputBlocks[playerIndex].mActiveInputBlocks.ContainsKey(typeToBlock) == false)
        {
            mPlayerSpecificInputBlocks[playerIndex].mActiveInputBlocks.Add(typeToBlock, 1);
        }
        else
        {
            mPlayerSpecificInputBlocks[playerIndex].mActiveInputBlocks[typeToBlock] += 1;
        }
    }

    public void UnblockInputType(InputType typeToBlock, int playerIndex)
    {
        // Lazily creates an input blocking set for the player at the index
        if (mPlayerSpecificInputBlocks.ContainsKey(playerIndex) == false)
        {
            mPlayerSpecificInputBlocks.Add(playerIndex, new PlayerInputBlockingSet());
        }

        // Adds or increments current block count
        if (mPlayerSpecificInputBlocks[playerIndex].mActiveInputBlocks.ContainsKey(typeToBlock) == false)
        {
            mPlayerSpecificInputBlocks[playerIndex].mActiveInputBlocks.Add(typeToBlock, 0);
        }
        else
        {
            mPlayerSpecificInputBlocks[playerIndex].mActiveInputBlocks[typeToBlock] -= 1;
        }
    }

    public void UnblockInputType(InputType typeToUnblock)
    {
        mActiveInputBlocks.Remove(typeToUnblock);
    }
}
