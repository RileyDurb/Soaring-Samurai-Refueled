using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class BotBehaviourStatsVariable : Variable<BotBehaviourStats>
{
    protected override bool ValueEquals(BotBehaviourStats val1, BotBehaviourStats val2)
    {
        return val1 == val2;
    }
}

[System.Serializable]
public class BotBehaviourStatsReference : VariableReference<BotBehaviourStatsVariable, BotBehaviourStats>
{
    // You can create additional constructors and Value getter/setter
    // See FloatVariable.cs as example

    // If your variable is reference type you might want constant validation
    protected override bool isConstantValid
    {
        get { return constantValue != null; }
    }

    public BotBehaviourStats Value
    {
        get
        {
            return (useConstant) ? constantValue : this.GetVariable().Value;
        }
        set
        {
            if (useConstant)
            {
                constantValue = value;
            }
            else
            {
                this.GetVariable().Value = value;
            }
        }
    }
}
