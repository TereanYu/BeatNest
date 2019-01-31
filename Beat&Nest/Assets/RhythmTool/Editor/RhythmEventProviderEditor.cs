using UnityEditor;

[CustomEditor(typeof(RhythmEventProvider))]
public class RhythmEventProviderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RhythmEventProvider eventProvider = (RhythmEventProvider)target;

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        SerializedProperty _target = serializedObject.FindProperty("_target");
        EditorGUILayout.PropertyField(_target);

        if (EditorGUI.EndChangeCheck())
            eventProvider.target = (RhythmTool)_target.objectReferenceValue;

        if (eventProvider.offset > 0)
            EditorGUILayout.LabelField("Current Frame:", (eventProvider.currentFrame) + "+" + eventProvider.offset);
        else
            EditorGUILayout.LabelField("Current Frame:", eventProvider.currentFrame.ToString());

        SerializedProperty _targetOffset = serializedObject.FindProperty("_targetOffset");
        EditorGUILayout.IntSlider(_targetOffset, 0, eventProvider.maxOffset);

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(eventProvider);
    }
}
