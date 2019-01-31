using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE 
    public sealed class SunshaftsEditor : Editor {} }
#else
    [PostProcessEditor(typeof(Sunshafts))]
    public sealed class SunshaftsEditor : PostProcessEffectEditor<Sunshafts>
    {
        SerializedParameterOverride useCasterColor;
        SerializedParameterOverride useCasterIntensity;

        SerializedParameterOverride resolution;
        SerializedParameterOverride sunThreshold;
        SerializedParameterOverride blendMode;
        SerializedParameterOverride sunColor;
        SerializedParameterOverride sunShaftIntensity;
        SerializedParameterOverride falloff;

        SerializedParameterOverride length;
        SerializedParameterOverride highQuality;

        public override void OnEnable()
        {
            useCasterColor = FindParameterOverride(x => x.useCasterColor);
            useCasterIntensity = FindParameterOverride(x => x.useCasterIntensity);

            resolution = FindParameterOverride(x => x.resolution);
            sunThreshold = FindParameterOverride(x => x.sunThreshold);
            blendMode = FindParameterOverride(x => x.blendMode);
            sunColor = FindParameterOverride(x => x.sunColor);
            sunShaftIntensity = FindParameterOverride(x => x.sunShaftIntensity);
            falloff = FindParameterOverride(x => x.falloff);
            length = FindParameterOverride(x => x.length);
            highQuality = FindParameterOverride(x => x.highQuality);
        }

        public override void OnInspectorGUI()
        {
            SCPE_GUI.DrawDocumentationHeader("");

            if (RuntimeUtilities.isSinglePassStereoSelected)
            {
                EditorGUILayout.HelpBox("Sunshafts are not supported in Single-Pass Stereo Rendering", MessageType.Warning);
                return;
            }
            if(Sunshafts.sunPosition == Vector3.zero)
            {
                EditorGUILayout.HelpBox("No source Directional Light found!\n\nAdd the \"SunshaftCaster\" script to your main light", MessageType.Warning);

                if (GUILayout.Button("Add")) Sunshafts.AddShaftCaster();
            }

            EditorUtilities.DrawHeaderLabel("Quality");
            PropertyField(resolution);
            PropertyField(highQuality, new GUIContent("High quality"));

            EditorGUILayout.Space();

            EditorUtilities.DrawHeaderLabel("Use values from caster");
            PropertyField(useCasterColor, new GUIContent("Color"));
            PropertyField(useCasterIntensity, new GUIContent("Intensity"));

            EditorGUILayout.Space();

            EditorUtilities.DrawHeaderLabel("Sunshafts");
            PropertyField(blendMode);
            PropertyField(sunThreshold);
            PropertyField(falloff);
            PropertyField(length);
            if (useCasterColor.value.boolValue == false) PropertyField(sunColor);
            if (useCasterIntensity.value.boolValue == false) PropertyField(sunShaftIntensity);

        }

    }
}
#endif
