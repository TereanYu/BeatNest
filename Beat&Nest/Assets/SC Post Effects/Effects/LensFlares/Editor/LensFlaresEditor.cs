using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;
using SCPE;

[PostProcessEditor(typeof(SCPE.LensFlares))]
public class LensFlaresEditor : PostProcessEffectEditor<SCPE.LensFlares>
{
    SerializedParameterOverride intensity;
    SerializedParameterOverride luminanceThreshold;
    SerializedParameterOverride maskTex;
    SerializedParameterOverride chromaticAbberation;
    SerializedParameterOverride colorTex;

    //Flares
    SerializedParameterOverride iterations;
    SerializedParameterOverride distance;
    SerializedParameterOverride falloff;

    //Halo
    SerializedParameterOverride haloSize;
    SerializedParameterOverride haloWidth;

    //Blur
    SerializedParameterOverride blur;
    SerializedParameterOverride passes;

    public override void OnEnable()
    {
        intensity = FindParameterOverride(x => x.intensity);
        luminanceThreshold = FindParameterOverride(x => x.luminanceThreshold);
        maskTex = FindParameterOverride(x => x.maskTex);
        chromaticAbberation = FindParameterOverride(x => x.chromaticAbberation);
        colorTex = FindParameterOverride(x => x.colorTex);

        //Flares
        iterations = FindParameterOverride(x => x.iterations);
        distance = FindParameterOverride(x => x.distance);
        falloff = FindParameterOverride(x => x.falloff);

        //Halo
        haloSize = FindParameterOverride(x => x.haloSize);
        haloWidth = FindParameterOverride(x => x.haloWidth);

        //Blur
        blur = FindParameterOverride(x => x.blur);
        passes = FindParameterOverride(x => x.passes);
    }

    public override void OnInspectorGUI()
    {
        if (RuntimeUtilities.isSinglePassStereoSelected)
        {
            EditorGUILayout.HelpBox("Lens Flares is not supported in Single-Pass Stereo Rendering", MessageType.Warning);
            return;
        }

        PropertyField(intensity);
        PropertyField(luminanceThreshold);
        PropertyField(maskTex);
        PropertyField(chromaticAbberation);
        PropertyField(colorTex);

        //Flares
        PropertyField(iterations);
        PropertyField(distance);
        PropertyField(falloff);

        //Halo
        PropertyField(haloSize);
        PropertyField(haloWidth);

        //Blur
        PropertyField(blur);
        PropertyField(passes);
    }
}
#endif
