#if UNITY_EDITOR
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MainAudioManager))]
public class SoundEffectSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (MainAudioManager)target;

        // add white space
        EditorGUILayout.Space();

        if (GUILayout.Button("Play Preview", GUILayout.Height(40)))
        {
            script.PlayPreview();
        }
        if (GUILayout.Button("Stop Preview", GUILayout.Height(40)))
        {
            script.StopPreview();
        }
    }
}
#endif