using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Posterize : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(PosterizeRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Retro/Posterize", true)]
    public sealed class Posterize : PostProcessEffectSettings
    {

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

    internal sealed class PosterizeRenderer : PostProcessEffectRenderer<Posterize>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Posterize");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetFloat("_Depth", (1-settings.amount) * 8f);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif