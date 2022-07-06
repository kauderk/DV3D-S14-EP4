#if UNITY_EDITOR
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PoolsManager))]
public class PoolsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (PoolsManager)target;

        // add white space
        EditorGUILayout.Space();

        if (GUILayout.Button("Release ALL", GUILayout.Height(40)))
        {
            script.ReleaseAll();
        }
        if (GUILayout.Button("GenerateStartPath", GUILayout.Height(40)))
        {
            script.BeforePress();
        }
    }
}
#endif