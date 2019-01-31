using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
#if SCPE
using UnityEditor.Rendering.PostProcessing;
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE
    public sealed class HueShift3DEditor : Editor {} }
#else
    [PostProcessEditor(typeof(HueShift3D))]
    public sealed class HueShift3DEditor : PostProcessEffectEditor<HueShift3D>
    {
        SerializedParameterOverride intensity;
        SerializedParameterOverride speed;
        SerializedParameterOverride size;
        SerializedParameterOverride geoInfluence;

        public override void OnEnable()
        {
            intensity = FindParameterOverride(x => x.intensity);
            speed = FindParameterOverride(x => x.speed);
            size = FindParameterOverride(x => x.size);
            geoInfluence = FindParameterOverride(x => x.geoInfluence);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(intensity);
            PropertyField(speed);
            PropertyField(size);

            EditorGUI.BeginDisabledGroup(HueShift3D.isOrtho || GraphicsSettings.renderPipelineAsset != null);
            {
                PropertyField(geoInfluence);
                if (HueShift3D.isOrtho) EditorGUILayout.HelpBox("Not available for orthographic cameras", MessageType.None);
                if (GraphicsSettings.renderPipelineAsset != null) EditorGUILayout.HelpBox("Not available when using a custom render pipeline", MessageType.None);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif