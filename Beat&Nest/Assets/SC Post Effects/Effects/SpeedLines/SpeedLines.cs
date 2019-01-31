using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class SpeedLines : ScriptableObject{}
}
#else
    [Serializable]
    [PostProcess(typeof(SpeedLinesRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Screen/Speed Lines", true)]
    public sealed class SpeedLines : PostProcessEffectSettings
    {

        [Range(0f, 1f)]
        public FloatParameter intensity = new FloatParameter { value = 1f };
        [Range(0f, 1f)]

        [Tooltip("Determines the radial tiling of the noise texture")]
        public FloatParameter size = new FloatParameter { value = 0.5f };

        [Range(0f, 1f)]
        public FloatParameter falloff = new FloatParameter { value = 0.25f };

        [Tooltip("Assign any grayscale texture with a horizontally repeating pattern")]
        public TextureParameter noise = new TextureParameter { value = null };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0 || noise.value == null) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class SpeedLinesRenderer : PostProcessEffectRenderer<SpeedLines>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/SpeedLines");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            float falloff = 2f + (settings.falloff - 0.0f) * (16.0f - 2f) / (1.0f - 0.0f);
            sheet.properties.SetVector("_Params", new Vector4(settings.intensity, falloff, settings.size * 2,0));
            if (settings.noise.value) sheet.properties.SetTexture("_NoiseTex", settings.noise);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif