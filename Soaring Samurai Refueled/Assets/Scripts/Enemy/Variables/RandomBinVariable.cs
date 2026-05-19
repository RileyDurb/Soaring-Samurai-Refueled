using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class RandomBinVariable : Variable<RandomBin>
{
    protected override bool ValueEquals(RandomBin val1, RandomBin val2)
    {
        return val1 == val2;
    }
}

[System.Serializable]
public class RandomBinVariableReference : VariableReference<RandomBinVariable, RandomBin>
{
    // You can create additional constructors and Value getter/setter
    // See FloatVariable.cs as example

    // If your variable is reference type you might want constant validation
    protected override bool isConstantValid
    {
        get { return constantValue != null; }
    }

    public RandomBin Value
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
