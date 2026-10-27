using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputRebindingOptionSettings", menuName = "Scripts/ScriptableObjects/Player/Input/InputRebindingOptionSettings")]
public class InputRebindingOptionSettings : ScriptableObject
{
    [System.Serializable]
    public class InputMethodBindingAesthetics
    {
        public ActiveInputTypeManager.ControlMethodTypes InputMethodType;
        public string RebindNoticeMainMessage = "Press any button to rebind control";
    }

    [Header("InputMappingPresets")]
    public List<string> mPresetInputMappingOptionNames = new List<string>();

    public string mDefaultInputMapName = "Combat";

    public string ControlFilenamePrefix;

    [SerializeField] List<InputMethodBindingAesthetics> mInputMethodBindingAesthetics = new List<InputMethodBindingAesthetics>();

    [Header("FeelTuning")]
    public float InputRebindDelayBeforeAcceptance = 0.5f;



    public InputMethodBindingAesthetics GetAestheticsForBindingMethod(ActiveInputTypeManager.ControlMethodTypes controlMethod)
    {
        InputMethodBindingAesthetics targetBindingAesthetics = mInputMethodBindingAesthetics.Find((InputMethodBindingAesthetics currAestheticSet) => { return currAestheticSet.InputMethodType == controlMethod; });

        return targetBindingAesthetics;
    }
}
