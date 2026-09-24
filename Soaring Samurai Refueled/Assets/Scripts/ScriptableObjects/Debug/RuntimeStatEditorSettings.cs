using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

[CreateAssetMenu(fileName = "RuntimeStatEditorSettings", menuName = "Scripts/ScriptableObjects/Debug/RuntimeStatEditorSettings")]
public class RuntimeStatEditorSettings : ScriptableObject
{
    [System.Serializable]
    public class TypeEditorPackage
    {
        public string[] mSupportedTypeNames;
        public GameObject mEditorPrefab;
    }

    public List<TypeEditorPackage> mSupportedVariableTypesForEditing;
    
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
