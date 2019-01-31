using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class TubeDistortion : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(TubeDistortionRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Screen/Tube Distortion", true)]
    public sealed class TubeDistortion : PostProcessEffectSettings
    {
        public enum DistortionMode
        {
            Buldged = 0,
            Pinched = 1,
            Beveled = 2
        }

        [Serializable]
        public sealed class DistortionModeParam : ParameterOverride<DistortionMode> { }

        public DistortionModeParam mode = new DistortionModeParam { value = DistortionMode.Buldged };

        [Range(0f, 1f)]
        public FloatParameter amount = new FloatParameter { value = 0.5f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (amount == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class TubeDistortionRenderer : PostProcessEffectRenderer<TubeDistortion>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Tube Distortion");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetFloat("_Amount", settings.amount);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }
    }
}
#endif