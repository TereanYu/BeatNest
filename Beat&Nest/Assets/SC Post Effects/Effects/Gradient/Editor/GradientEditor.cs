using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;
using SCPE;
#endif

namespace SCPE
{
#if !SCPE
    public sealed class GradientEditor : Editor {} }
#else
    [PostProcessEditor(typeof(Gradient))]
    public class GradientEditor : PostProcessEffectEditor<Gradient>
    {
        SerializedParameterOverride intensity;
        SerializedParameterOverride input;
        SerializedParameterOverride color1;
        SerializedParameterOverride color2;
        SerializedParameterOverride rotation;
        SerializedParameterOverride gradientTex;
        SerializedParameterOverride blendMode;

        public override void OnEnable()
        {
            intensity = FindParameterOverride(x => x.intensity);
            input = FindParameterOverride(x => x.input);
            color1 = FindParameterOverride(x => x.color1);
            color2 = FindParameterOverride(x => x.color2);
            rotation = FindParameterOverride(x => x.rotation);
            gradientTex = FindParameterOverride(x => x.gradientTex);
            blendMode = FindParameterOverride(x => x.blendMode);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(intensity);
            PropertyField(input);

            //If Radial
            if (input.value.intValue == 1)
            {
                PropertyField(gradientTex);

            }
            else
            {
                PropertyField(color1);
                PropertyField(color2);
            }

            PropertyField(blendMode);
            PropertyField(rotation);

        }

    }
}
#endif