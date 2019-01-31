using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class DoubleVision : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(DoubleVisionRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Blurring/Double Vision", true)]
    public sealed class DoubleVision : PostProcessEffectSettings
    {

        public enum Mode
        {
            FullScreen = 0,
            Edges = 1,
        }

        [Serializable]
        public sealed class EdgeDetectionMode : ParameterOverride<Mode> { }

        [DisplayName("Method"), Tooltip("Choose to apply the effect over the entire screen or just the edges")]
        public EdgeDetectionMode mode = new EdgeDetectionMode { value = Mode.FullScreen };

        [Range(0f, 1f), Tooltip("Intensity")]
        public FloatParameter intensity = new FloatParameter { value = 0.1f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class DoubleVisionRenderer : PostProcessEffectRenderer<DoubleVision>
    {
        Shader DoubleVisionShader;

        public override void Init()
        {
            DoubleVisionShader = Shader.Find("Hidden/SC Post Effects/Double Vision");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(DoubleVisionShader);

            sheet.properties.SetFloat("_Amount", settings.intensity / 10);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }

    }
}
#endif