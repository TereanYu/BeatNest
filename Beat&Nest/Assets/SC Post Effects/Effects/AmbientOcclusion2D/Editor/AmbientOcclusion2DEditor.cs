using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;
using SCPE;

[PostProcessEditor(typeof(SCPE.AmbientOcclusion2D))]
public class AmbientOcclusion2DEditor : PostProcessEffectEditor<SCPE.AmbientOcclusion2D>
{
    SerializedParameterOverride aoOnly;
    SerializedParameterOverride intensity;
    SerializedParameterOverride luminanceThreshold;
    SerializedParameterOverride distance;
    SerializedParameterOverride blurAmount;
    SerializedParameterOverride iterations;
    SerializedParameterOverride downscaling;

    public override void OnEnable()
    {

        aoOnly = FindParameterOverride(x => x.aoOnly);
        intensity = FindParameterOverride(x => x.intensity);
        luminanceThreshold = FindParameterOverride(x => x.luminanceThreshold);
        distance = FindParameterOverride(x => x.distance);
        blurAmount = FindParameterOverride(x => x.blurAmount);
        iterations = FindParameterOverride(x => x.iterations);
        downscaling = FindParameterOverride(x => x.downscaling);
    }

    public override void OnInspectorGUI()
    {
        if (RuntimeUtilities.isSinglePassStereoSelected)
        {
            EditorGUILayout.HelpBox("Ambient Occlusion 2D is not supported in Single-Pass Stereo Rendering", MessageType.Warning);
            return;
        }

        PropertyField(aoOnly);
        PropertyField(intensity);
        PropertyField(luminanceThreshold);
        PropertyField(distance);
        PropertyField(blurAmount);
        PropertyField(iterations);
        PropertyField(downscaling);

    }
}
#endif
