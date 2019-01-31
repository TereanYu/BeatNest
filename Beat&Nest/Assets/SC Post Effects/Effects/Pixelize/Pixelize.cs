using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Pixelize : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(PixelizeRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Retro/Pixelize", true)]
    public sealed class Pixelize : PostProcessEffectSettings
    {

        [Range(0f, 1f), Tooltip("Amount")]
        public FloatParameter amount = new FloatParameter { value = 0.85f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (amount == 0f) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class PixelizeRenderer : PostProcessEffectRenderer<Pixelize>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Pixelize");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetFloat("_Resolution", 1f / ((1f - settings.amount) * context.screenWidth));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif