using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Invert : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(InvertRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Misc/Invert", true)]
    public sealed class Invert : PostProcessEffectSettings
    {

        [Range(0f, 1f)]
        public FloatParameter amount = new FloatParameter { value = 1f };

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

    internal sealed class InvertRenderer : PostProcessEffectRenderer<Invert>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Invert");
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