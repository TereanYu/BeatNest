using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Scanlines : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(ScanlinesRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Retro/Scanlines", true)]
    public sealed class Scanlines : PostProcessEffectSettings
    {

        [Range(0f, 1f), Tooltip("Intensity")]
        public FloatParameter intensity = new FloatParameter { value = 0.5f };

        [Range(0f, 2048f), DisplayName("Lines")]
        public FloatParameter amountHorizontal = new FloatParameter { value = 700 };

        [Range(0f, 1f), Tooltip("Animation speed")]
        public FloatParameter speed = new FloatParameter { value = 0f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity.value == 0) return false;
                return true;
            }

            return false;
        }
    }

    internal sealed class ScanlinesRenderer : PostProcessEffectRenderer<Scanlines>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Scanlines");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetVector("_Params", new Vector4(settings.amountHorizontal, settings.intensity / 1000, settings.speed * 8f, 0f));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif