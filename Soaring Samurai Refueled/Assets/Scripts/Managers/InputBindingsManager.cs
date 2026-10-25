using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBindingsManager : MonoBehaviour
{
    public class PlayerInputBindingInfo
    {
        public int PlayerIndex;
        public string CurrentInputMappingName;
    }
    // Public events
    public Action<int, string> OnInputMappingSwitch; // Called when input mapping is switched. Param1: Player index whose mapping was switched. param2: new input mapping name

    List<PlayerInputBindingInfo> mPlayerBindingInfo = new List<PlayerInputBindingInfo>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetInputMapping(int playerIndex, string inputMappingName)
    {
        // Makes or gets the binding info for this player index
        PlayerInputBindingInfo targetPlayerBindingInfo = GetPlayerBindingInfo(playerIndex);

        if (targetPlayerBindingInfo == null)
        {
            targetPlayerBindingInfo = new PlayerInputBindingInfo();
            mPlayerBindingInfo.Add(targetPlayerBindingInfo);
        }

        // Tell all listeners about the binding switch, to actually apply the binding map to the input component
        OnInputMappingSwitch.Invoke(playerIndex, inputMappingName);

        // Save binding info in the manager
        targetPlayerBindingInfo.CurrentInputMappingName = inputMappingName;
    }

    public PlayerInputBindingInfo GetPlayerBindingInfo(int playerIndex)
    {
        PlayerInputBindingInfo info = mPlayerBindingInfo.Find((PlayerInputBindingInfo currBindingInfo) => { return currBindingInfo.PlayerIndex == playerIndex; });

        return info;
    }

}
