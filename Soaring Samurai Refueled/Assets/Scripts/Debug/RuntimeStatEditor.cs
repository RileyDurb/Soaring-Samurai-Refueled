using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;

public class RuntimeStatEditor : MonoBehaviour
{
    [SerializeField] ScriptableObject scriptableObjectToEdit;

    //Editor currentDataEditor;

    // Start is called before the first frame update
    void Start()
    {
        //Resources.LoadAll<ScriptableObject>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnGUI()
    {
        //    GUILayout.Box("Test");

        //GUILayout.BeginHorizontal();

        //GUILayout.Label("Test");
        ///*scriptableObjectToEdit = */EditorGUILayout.ObjectField(scriptableObjectToEdit, typeof(ScriptableObject), true);

        ////if (whatIsMe != null)
        ////{
        ////    print("Heeheee");
        ////}
        ////scriptableObjectToEdit = whatIsMe as PhysicsTuningStatSet;

        //if (scriptableObjectToEdit != null)
        //{
        //    Editor.CreateCachedEditor(scriptableObjectToEdit, null, ref currentDataEditor);
        //    currentDataEditor.DrawDefaultInspector();
        //    print("NotSad");

        //}
        //else
        //{
        //    print("Sad");
        //}

        //GUILayout.EndHorizontal();
    }


    private void DrawEditorWindow(int windowID)
    {
        GUILayout.Box("Test");
    }
}
