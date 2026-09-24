using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RuntimeStatEditorFunctions : MonoBehaviour
{
    //[SerializeField] TMP_InputField TestScriptableObjectEditorButton;

    // Start is called before the first frame update
    void Start()
    {
        //TestScriptableObjectEditorButton.onSubmit.AddListener(ApplyTestSpeedChange);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyTestSpeedChange(string newSpeed)
    {
        float speed = 0;
        bool parseSucceeded = float.TryParse(newSpeed, out speed);

        // Make sure parsing succeded
        if (parseSucceeded == false)
        {
            print("RuntimeStatEditorFunctions:ApplyTestSpeedChange: input value " + newSpeed + "could not be parsed into a float");

            return;
        }

        // Apply stat change directly
        PersistentScopeManagers.Instance.GetComponent<RuntimeEditableDataManager>().TestChangeScriptableObject(speed);
    }


}
