using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Empty Menu attribute prevents Node to show up in "Add Component" menu.
[AddComponentMenu("")]
// Register node in visual editor node finder
[MBTNode(name = "Tasks/SetRandomBinItem")]
public class Task_SetRandomBinItem : Leaf
{
    [SerializeField] RandomBinVariableReference RandomBinKey;

    [SerializeField] string ItemNameToSet = "";
    [SerializeField] int MinValueToAdd = 10;
    [SerializeField] int MaxValueToAdd = 10;

    [SerializeField] bool SetInsteadOfAdd = false;


    public override NodeResult Execute()
    {

        // Sets or adds to item
        if (SetInsteadOfAdd)
        {
            RandomBinKey.Value.SetItem(ItemNameToSet, MyRandom.RandomRange(MinValueToAdd, MaxValueToAdd));
        }
        else
        {
            RandomBinKey.Value.AddToItem(ItemNameToSet, MyRandom.RandomRange(MinValueToAdd, MaxValueToAdd));
        }

        return NodeResult.success;
    }


}

