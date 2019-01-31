using System;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class RadialBlur : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(RadialBlurRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Blurring/Radial Blur", true)]
    public sealed class RadialBlur : PostProcessEffectSettings
    {
        [Range(0f, 1f)]
        public FloatParameter amount = new FloatParameter { value = 0.5f };
        [Range(3, 12)]
        public IntParameter iterations = new IntParameter { value = 6 };

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

    internal sealed class RadialBlurRenderer : PostProcessEffectRenderer<RadialBlur>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Radial Blur");

        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            sheet.properties.SetFloat("_Amount", settings.amount / 50);
            sheet.properties.SetFloat("_Iterations", settings.iterations);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif