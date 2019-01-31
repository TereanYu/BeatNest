using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class EdgeDetection : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(EdgeDetectionRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Stylized/Edge Detection", true)]
    public sealed class EdgeDetection : PostProcessEffectSettings
    {
        [Range(0f, 1f), DisplayName("Edges Only"), Tooltip("Shows only the effect, to alow for finetuning")]
        public BoolParameter debug = new BoolParameter { value = false };


        public enum EdgeDetectMode
        {
            DepthNormals = 0,
            RobertsCrossDepthNormals = 1,
            SobelDepth = 2,
            LuminanceBased = 3,
        }

        [Serializable]
        public sealed class EdgeDetectionMode : ParameterOverride<EdgeDetectMode> { }

        [Space]
        [Tooltip("Choose one of the different edge solvers")]
        public EdgeDetectionMode mode = new EdgeDetectionMode { value = EdgeDetectMode.DepthNormals };

        public BoolParameter invertFadeDistance = new BoolParameter { value = false };
        [UnityEngine.Rendering.PostProcessing.Min(0.01f), Range(0.01f, 10000)]
        [Tooltip("Fades out the effect between the cameras near and far clipping plane")]
        public FloatParameter fadeDistance = new FloatParameter { value = 1000f };


        [Header("Sensitivity")]
        [DisplayName("Depth"), Range(0f, 1f), Tooltip("Sets how much difference in depth between pixels contribute to drawing an edge")]
        public FloatParameter sensitivityDepth = new FloatParameter { value = 0f };

        [DisplayName("Normals"), Range(0f, 1f), Tooltip("Sets how much difference in normals between pixels contribute to drawing an edge")]
        public FloatParameter sensitivityNormals = new FloatParameter { value = 1f };

        [Range(0.01f, 1f), DisplayName("Luminance Threshold"), Tooltip("Luminance threshold, pixels above this threshold will contribute to the effect")]
        public FloatParameter lumThreshold = new FloatParameter { value = 0.01f };

        [Header("Edges")]
        [DisplayName("Color"), Tooltip("")]
        public ColorParameter edgeColor = new ColorParameter { value = Color.black };

        [Range(1f, 50f), Tooltip("Edge Exponent")]
        public FloatParameter edgeExp = new FloatParameter { value = 1f };

        [DisplayName("Size"), Range(1, 4), Tooltip("Edge Distance")]
        public IntParameter edgeSize = new IntParameter { value = 1 };

        [DisplayName("Opacity"), Range(0f, 1f), Tooltip("Opacity")]
        public FloatParameter edgeOpacity = new FloatParameter { value = 1f };

        [DisplayName("Thin")]
        public BoolParameter sobelThin = new BoolParameter { value = false };


        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (edgeOpacity > 0)
                    return true;
            }

            return false;
        }
    }

    internal sealed class EdgeDetectionRenderer : PostProcessEffectRenderer<EdgeDetection>
    {

        Shader edgeDetectShader;

        public override void Init()
        {
            edgeDetectShader = Shader.Find("Hidden/SC Post Effects/Edge Detection");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(edgeDetectShader);
            CommandBuffer cmd = context.command;

            Vector2 sensitivity = new Vector2(settings.sensitivityDepth, settings.sensitivityNormals);
            sheet.properties.SetVector("_Sensitivity", sensitivity);
            sheet.properties.SetFloat("_BackgroundFade", (settings.debug) ? 1f : 0f);
            sheet.properties.SetFloat("_EdgeSize", settings.edgeSize);
            sheet.properties.SetFloat("_Exponent", settings.edgeExp);
            sheet.properties.SetFloat("_Threshold", settings.lumThreshold);
            sheet.properties.SetColor("_EdgeColor", settings.edgeColor);

            float fadeDist = (context.camera.orthographic) ? settings.fadeDistance * (float)1e-10 : settings.fadeDistance;
            sheet.properties.SetVector("_DistanceParams", new Vector4(fadeDist, (settings.invertFadeDistance) ? 1 : 0, 0, 0));

            sheet.properties.SetVector("_SobelParams", new Vector4((settings.sobelThin) ? 1 : 0, 0, 0, 0));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.DepthNormals;
        }

    }
}
#endif