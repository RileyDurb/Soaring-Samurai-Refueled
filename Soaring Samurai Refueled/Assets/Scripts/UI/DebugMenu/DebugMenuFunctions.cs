using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuFunctions : MonoBehaviour
{
    [SerializeField] GameObject DebugMenuToggleRef;
    // Start is called before the first frame update
    void Start()
    {
        DebugMenuToggleRef.GetComponent<Toggle>().SetIsOnWithoutNotify(SimManager.Instance.DebugModeOn);
    }

    public void ToggleDebugMode()
    {
        // Toggle debug mode state
        SimManager.Instance.DebugModeOn = !SimManager.Instance.DebugModeOn;
    }
}
