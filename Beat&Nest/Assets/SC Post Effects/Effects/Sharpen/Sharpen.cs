using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Sharpen : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(SharpenRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Image/Sharpen", true)]
    public sealed class Sharpen : PostProcessEffectSettings
    {

        [Range(0f, 1f), Tooltip("Amount")]
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

    internal sealed class SharpenRenderer : PostProcessEffectRenderer<Sharpen>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Sharpen");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetFloat("_Amount", settings.amount);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif