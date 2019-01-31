using UnityEditor;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE
    public sealed class SketchEditor : Editor {} }
#else
    [PostProcessEditor(typeof(Sketch))]
    public sealed class SketchEditor : PostProcessEffectEditor<Sketch>
    {
        SerializedParameterOverride projectionMode;
        SerializedParameterOverride blendMode;
        SerializedParameterOverride strokeTex;
        SerializedParameterOverride intensity;
        SerializedParameterOverride brightness;
        SerializedParameterOverride tiling;

        float minBrightness;
        float maxBrightness;

        public override void OnEnable()
        {
            projectionMode = FindParameterOverride(x => x.projectionMode);
            blendMode = FindParameterOverride(x => x.blendMode);
            strokeTex = FindParameterOverride(x => x.strokeTex);
            intensity = FindParameterOverride(x => x.intensity);
            brightness = FindParameterOverride(x => x.brightness);
            tiling = FindParameterOverride(x => x.tiling);
        }

        public override void OnInspectorGUI()
        {
            if (RuntimeUtilities.isSinglePassStereoSelected)
            {
                EditorGUILayout.HelpBox("Not supported in Single-Pass Stereo Rendering", MessageType.Warning);
                return;
            }

            PropertyField(strokeTex);

            if (strokeTex.value.objectReferenceValue == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Assign a texture to enable the effect.\n\nStrokes for dark shades are sampled from the Red channel. Light shades are sampled from Green.", MessageType.Info);
                return;
            }

            PropertyField(projectionMode);
            PropertyField(blendMode);
            PropertyField(intensity);

            minBrightness = brightness.value.vector2Value.x;
            maxBrightness = brightness.value.vector2Value.y;

            using (new EditorGUILayout.HorizontalScope())
            {
                // Override checkbox
                var overrideRect = GUILayoutUtility.GetRect(17f, 17f, GUILayout.ExpandWidth(false));
                overrideRect.yMin += 4f;
                EditorUtilities.DrawOverrideCheckbox(overrideRect, brightness.overrideState);

                // Property
                using (new EditorGUI.DisabledScope(!brightness.overrideState.boolValue))
                {
                    EditorGUILayout.LabelField(brightness.displayName + " (Min/Max)", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
                    EditorGUILayout.LabelField(minBrightness.ToString(), GUILayout.Width(50f));
                    EditorGUILayout.MinMaxSlider(ref minBrightness, ref maxBrightness, 0f, 2f);
                    EditorGUILayout.LabelField(maxBrightness.ToString(), GUILayout.Width(50f));
                }
            }

            brightness.value.vector2Value = new Vector2(minBrightness, maxBrightness);
            PropertyField(tiling);
        }
    }
}
#endif
