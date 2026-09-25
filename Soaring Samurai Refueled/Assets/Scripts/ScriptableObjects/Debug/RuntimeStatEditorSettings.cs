using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

[CreateAssetMenu(fileName = "RuntimeStatEditorSettings", menuName = "Scripts/ScriptableObjects/Debug/RuntimeStatEditorSettings")]
public class RuntimeStatEditorSettings : ScriptableObject
{
    // Public class definitions
    [System.Serializable]
    public class TypeEditorPackage
    {
        public string[] mSupportedTypeNames;
        public GameObject mEditorPrefab;
    }


    // Variables

    [Header("Variable Editor Types")]
    public List<TypeEditorPackage> mSupportedVariableTypesForEditing;

    public GameObject mNestedObjectHeaderPrefab;

    public int mMaxFieldDepthToShow = 1;

    [Header("Data To Edit")]
    public List<ScriptableObject> mEditableScriptableObjectsToChooseFrom;

    [Header("Aesthetics")]
    public RectOffset mListItemPadding = new RectOffset();


    // Public interface

    // Gets the editor UI prefab for the given type. Returns null if type is not supported
    public GameObject GetVariableTypeEditor(System.Type type)
    {
        string typeAsString = type.ToString();
        TypeEditorPackage foundTypePackage = mSupportedVariableTypesForEditing.Find(
            (TypeEditorPackage currType) =>
        {
            return currType.mSupportedTypeNames.Contains(typeAsString);
        }
        );

        // Type is supported
        if (foundTypePackage != null)
        {
            return foundTypePackage.mEditorPrefab; // Return the editor
        }
        else // Type is not supported
        {
            return null;
        }
    }
}
