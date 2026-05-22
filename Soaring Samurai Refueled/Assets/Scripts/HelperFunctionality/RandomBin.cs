using System.Collections.Generic;
using UnityEngine;

// Class for getting a weighted random selection from a list of values. Each has their own weight for how likely they are compared to the others
[System.Serializable]
public class RandomBin
{
    [System.Serializable]
    public class RandomBinItem
    {
        public RandomBinItem(string name, int weight)
        {
            Name = name;
            Weight = weight;
        }
        public string Name = "";
        public int Weight = 0;
    }

    [SerializeField] List<RandomBinItem> mItems = new List<RandomBinItem>();

    public virtual void AddToItem(string itemName, int amountToAdd)
    {
        RandomBinItem item = mItems.Find((RandomBinItem otherItem) => { return otherItem.Name == itemName; });

        if (item != null)
        {
            item.Weight += amountToAdd;
        }
        else
        {
            Debug.Log("RandomBin:AddToItem: item of name \"" + itemName + "\" does not exist");
            return;
        }

        if (item.Weight < 0)
        {
            item.Weight = 0;
        }
    }

    public virtual void AddItem(string itemName, int amountToAdd)
    {
        RandomBinItem newItem = new RandomBinItem(itemName, amountToAdd);

        if (newItem.Weight < 0)
        {
            newItem.Weight = 0;
        }

        mItems.Add(newItem);
    }

    public virtual void SetItem(string itemName, int newWeight)
    {
        RandomBinItem item = mItems.Find((RandomBinItem otherItem) => { return otherItem.Name == itemName; });

        if (item != null)
        {
            item.Weight = newWeight;
        }
        else
        {
            Debug.Log("RandomBin:SetItem: item of name \"" + itemName + "\" does not exist");
            return;
        }

        if (item.Weight < 0)
        {
            item.Weight = 0;
        }
    }

    // Helper functions
    protected int PullItem()
    {
        // Total Up all randoms
        int totalRandomValue = 0;
        for (int i  = 0; i < mItems.Count; i++)
        {
            totalRandomValue += mItems[i].Weight;
        }
         
        // Pick a random value, and the item who's weight range that value is within will get chosen
        int randomChoice = MyRandom.RandomRange(0, totalRandomValue);

        // From the list of weights, find the range that the random value falls under, and select that item
        int currentCountPassed = 0;
        for (int i = 0; i < mItems.Count; i++)
        {
            int currItemWeight = mItems[i].Weight;

            // If the chosen random value is within the window of the current itwm's weight
            if (randomChoice >= currentCountPassed && randomChoice < currentCountPassed + currItemWeight)
            {
                return i; // Return choosing this item
            }
            else // This item was not chosen
            {
                currentCountPassed += currItemWeight; // Move past this item's range
            }
        }

        // Should not reach here because we should have found an option, print warning
        Debug.Log("RandomBin:PullItem: went past the list of items when selecting an option, investigate ");
        return -1; // Return -1 as error

    }

    // Public interface
    public string PullItemName()
    {
        if (mItems.Count <= 0)
        {
            Debug.Log("RandomBin:PullItem: No items in bin, need at least one item to choose ");
            return "";
        }

        return mItems[PullItem()].Name;
    }   
    
    public int PullItemIndex()
    {
        if (mItems.Count <= 0)
        {
            Debug.Log("RandomBin:PullItem: No items in bin, need at least one item to choose ");
            return 0;
        }

        return PullItem();
    }
}
