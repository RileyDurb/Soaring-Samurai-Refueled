using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
[MBTNode("Services/SetBinBasedOnDistance")]
public class Service_SetBinBasedOnDistance : Service
{

    [SerializeField] RandomBinVariableReference BinToUpdateKey;
    [SerializeField] GameObjectReference FromTargetKey;
    [SerializeField] GameObjectReference ToTargetKey;
    [SerializeField] string ItemToUpdateName = "";
    [SerializeField] float DistanceThresholdToAddToBin = 10.0f;
    [SerializeField] bool AddIfPastThreshold = true; // By default, adds to bin if past the distance threshold. If false, adds to bin if targets are within the threshold instead
    [SerializeField] Vector2Int ValueToAddRange = Vector2Int.zero;
    [SerializeField] bool SetInsteadOfAdd = false;


    public override void Task()
    {
        Vector2 distanceBetweenTargets = ToTargetKey.Value.transform.position - FromTargetKey.Value.transform.position;

        bool shouldAddToBin = false;
        if (AddIfPastThreshold)
        {
            if (distanceBetweenTargets.magnitude > DistanceThresholdToAddToBin)
            {
                shouldAddToBin = true;
            }
        }
        else
        {
            if (distanceBetweenTargets.magnitude > DistanceThresholdToAddToBin)
            {
                shouldAddToBin = true;
            }
        }

        if (shouldAddToBin)
        {
            if (SetInsteadOfAdd)
            {
                BinToUpdateKey.Value.SetItem(ItemToUpdateName, MyRandom.RandomRange(ValueToAddRange.x, ValueToAddRange.y + 1));
            }
            else
            {
                BinToUpdateKey.Value.AddToItem(ItemToUpdateName, MyRandom.RandomRange(ValueToAddRange.x, ValueToAddRange.y + 1));
            }
        }
    }
}