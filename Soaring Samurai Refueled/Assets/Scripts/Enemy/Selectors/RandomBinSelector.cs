using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("")]
[MBTNode(name = "RandomBinSelector", order = 100)]
public class RandomBinSelector : Composite
{
    [SerializeField] RandomBinVariableReference RandomBinKeyRef;

    int mCurrPathIndex = 0;

    Dictionary<int, Node> mOriginalPositions = new Dictionary<int, Node>();

    bool mDefaultsInitialized = false;

    public override void OnAllowInterrupt()
    {
        // Saves what posiion each child was originally at, so we can keep selecting new orders (which requires changing the actual child array) while not loosing which child is at which index)
        if (mDefaultsInitialized == false)
        {
            for (int i = 0; i < children.Count; i++)
            {
                mOriginalPositions.Add(i, children[i]);
            }

            mDefaultsInitialized = true;
        }

        mCurrPathIndex = RandomBinKeyRef.Value.PullItemIndex(); // Pulls which branch to update from the random bin

        print("RandomBinSelector:OnEnter: Selected branch " + mCurrPathIndex.ToString());


        // Put the chosen child in front so it executes first

        // Find where our new front node is, for putting the current front node into
        int lastPositionOfTargetNode = -1;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] == mOriginalPositions[mCurrPathIndex])
            {
                lastPositionOfTargetNode = i;
                break;
            }
        }

        Node temp = children[0];
        children[0] = mOriginalPositions[mCurrPathIndex]; // Put node of the chosen branch in the front of the children array, so it is what gets ran
        children[lastPositionOfTargetNode] = temp; // Put old front node where the new front node was

    }
    public override void OnEnter()
    {


    }

    public override NodeResult Execute()
    {
        // If there isn't a child to pick for the chosen index, debug print so we know to add more options
        if (children.Count <= mCurrPathIndex)
        {
            Debug.Log("RamdomBinSelector:Execute: not enough branches to pick selected item number " + mCurrPathIndex.ToString());
            return NodeResult.failure;
        }

        Node child = children[0]; // Update the chosen child
        switch (child.status)
        {
            case Status.Success:
                return NodeResult.success;
            case Status.Failure:
                return NodeResult.failure;
        }
        
        return child.runningNodeResult;
    }
}
