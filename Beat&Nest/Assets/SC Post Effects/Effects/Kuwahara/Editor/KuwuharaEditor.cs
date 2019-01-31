using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if SCPE
using UnityEditor.Rendering.PostProcessing;
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE
    public sealed class KuwuharaEditor : Editor {} }
#else
    [PostProcessEditor(typeof(Kuwahara))]
    public class KuwuharaEditor : PostProcessEffectEditor<Kuwahara>
    {
        SerializedParameterOverride mode;
        SerializedParameterOverride radius;
        SerializedParameterOverride invertFadeDistance;
        SerializedParameterOverride fadeDistance;

        private bool isOrthographic = false;

        public override void OnEnable()
        {
            mode = FindParameterOverride(x => x.mode);
            radius = FindParameterOverride(x => x.radius);
            fadeDistance = FindParameterOverride(x => x.fadeDistance);
            invertFadeDistance = FindParameterOverride(x => x.invertFadeDistance);

            if (Camera.main) isOrthographic = Camera.main.orthographic;
        }

        public override void OnInspectorGUI()
        {
            invertFadeDistance.overrideState.boolValue = fadeDistance.overrideState.boolValue;

            EditorGUI.BeginDisabledGroup(isOrthographic);
            PropertyField(mode);
            EditorGUI.EndDisabledGroup();

            if (isOrthographic)
            {
                mode.value.intValue = 0;
                EditorGUILayout.HelpBox("Depth fade is disabled for orthographic cameras", MessageType.Info);
            }
            PropertyField(radius);
            if (!isOrthographic)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Override checkbox
                    var overrideRect = GUILayoutUtility.GetRect(17f, 17f, GUILayout.ExpandWidth(false));
                    overrideRect.yMin += 4f;
                    EditorUtilities.DrawOverrideCheckbox(overrideRect, fadeDistance.overrideState);

                    EditorGUILayout.PrefixLabel(fadeDistance.displayName);

                    GUILayout.FlexibleSpace();

                    fadeDistance.value.floatValue = EditorGUILayout.FloatField(fadeDistance.value.floatValue);

                    bool enabled = invertFadeDistance.value.boolValue;
                    enabled = GUILayout.Toggle(enabled, "Start", EditorStyles.miniButtonLeft, GUILayout.Width(50f), GUILayout.ExpandWidth(false));
                    enabled = !GUILayout.Toggle(!enabled, "End", EditorStyles.miniButtonRight, GUILayout.Width(50f), GUILayout.ExpandWidth(false));

                    invertFadeDistance.value.boolValue = enabled;
                }
            }
        }
    }
}
#endif
