using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RhythmTool))]
public class RhythmToolEditor : Editor
{

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RhythmTool rhythmTool = (RhythmTool)target;

        EditorGUILayout.LabelField("Total frames:", rhythmTool.totalFrames.ToString());
        EditorGUILayout.LabelField("Last Frame:", rhythmTool.lastFrame.ToString());
        EditorGUILayout.LabelField("Current Frame:", rhythmTool.currentFrame.ToString());
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("BPM:", rhythmTool.bpm.ToString());
        EditorGUILayout.LabelField("Beat Length:", rhythmTool.beatLength.ToString() + " frames");
        EditorGUILayout.Separator();
        
        //Note: Uncomment and make _audioClip field serializable for editor support
        //EditorGUI.BeginChangeCheck();
        //SerializedProperty audioClip = serializedObject.FindProperty("_audioClip");
        //EditorGUILayout.PropertyField(audioClip);
        //if (EditorGUI.EndChangeCheck())        
        //    rhythmTool.audioClip = (AudioClip)audioClip.objectReferenceValue;
        //EditorGUILayout.Separator();

        SerializedProperty _trackBeat = serializedObject.FindProperty("_trackBeat");
        EditorGUILayout.PropertyField(_trackBeat);

        SerializedProperty _preAnalyze = serializedObject.FindProperty("_preAnalyze");
        EditorGUILayout.PropertyField(_preAnalyze);

        if (_preAnalyze.boolValue)
        {
            SerializedProperty _cacheAnalysis = serializedObject.FindProperty("_cacheAnalysis");
            EditorGUILayout.PropertyField(_cacheAnalysis);
        }
        else
        {
            SerializedProperty _lead = serializedObject.FindProperty("_lead");
            EditorGUILayout.IntSlider(_lead, 1, 1800);
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}
