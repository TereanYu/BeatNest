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
    public sealed class CloudShadowsEditor : Editor {}
    }
#else
    [PostProcessEditor(typeof(CloudShadows))]
    public sealed class CloudShadowsEditor : PostProcessEffectEditor<CloudShadows>
    {
        SerializedParameterOverride texture;
        SerializedParameterOverride size;
        SerializedParameterOverride density;
        SerializedParameterOverride speed;
        SerializedParameterOverride direction;

        public override void OnEnable()
        {
            texture = FindParameterOverride(x => x.texture);
            size = FindParameterOverride(x => x.size);
            density = FindParameterOverride(x => x.density);
            speed = FindParameterOverride(x => x.speed);
            direction = FindParameterOverride(x => x.direction);
        }

        public override void OnInspectorGUI()
        {
            if (RuntimeUtilities.isSinglePassStereoSelected)
            {
                EditorGUILayout.HelpBox("Cloud Shadows are not supported in Single-Pass Stereo Rendering", MessageType.Warning);
                return;
            }
            if (CloudShadows.isOrtho) EditorGUILayout.HelpBox("Not available for orthographic cameras", MessageType.Warning);

            EditorGUI.BeginDisabledGroup(CloudShadows.isOrtho);
            {
                PropertyField(texture);
                if (texture.value.objectReferenceValue)
                {
                    PropertyField(size);
                    PropertyField(density);
                    PropertyField(speed);
                    PropertyField(direction);
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif