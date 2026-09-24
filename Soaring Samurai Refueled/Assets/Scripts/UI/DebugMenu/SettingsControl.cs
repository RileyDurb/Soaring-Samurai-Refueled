using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//
public class SettingsControl : MonoBehaviour
{
    protected SetStatCommand mStatSetCommandObject;

    protected ICommand_ mCommandToExecute;
    [SerializeField] protected object mScriptableObjectToSetOn;
    [SerializeField] protected string mPropertyNameToSet;
    public object mInitialValue;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        print("Here");
    }


    public virtual void AssignPropertyOrFieldToSet(ScriptableObject objectToSetOn, string fieldOrPropertyName)
    {
        mScriptableObjectToSetOn = objectToSetOn;
        mPropertyNameToSet = fieldOrPropertyName;

        mStatSetCommandObject = new SetStatCommand();
        mStatSetCommandObject.Initalize(mScriptableObjectToSetOn, mPropertyNameToSet);

        mCommandToExecute = mStatSetCommandObject;

        // Sets initial value
        mInitialValue = mStatSetCommandObject.GetValue();
    }
}

