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


    // Private variables ///////////////////////////////////////////////////////////

    HashSet<InputType> mActiveInputBlocks = new HashSet<InputType>();
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

    public void BlockInputType(InputType typeToBlock)
    {
        mActiveInputBlocks.Add(typeToBlock);
    }

    public void UnblockInputType(InputType typeToUnblock)
    {
        mActiveInputBlocks.Remove(typeToUnblock);
    }
}
