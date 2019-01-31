using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Refraction : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(RefractionRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Screen/Refraction", true)]
    public sealed class Refraction : PostProcessEffectSettings
    {

        [Range(0f, 1f), Tooltip("Amount")]
        public FloatParameter amount = new FloatParameter { value = 1f };

        [Tooltip("Takes a DUDV map (normal map without a blue channel) to perturb the image")]
        public TextureParameter refractionTex = new TextureParameter { value = null };

        [Tooltip("In the absense of a DUDV map, the supplied normal map can be converted in the shader")]
        public BoolParameter convertNormalMap = new BoolParameter { value = false };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (amount == 0 || refractionTex.value == null) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class RefractionRenderer : PostProcessEffectRenderer<Refraction>
    {
        Shader RefractionShader;

        public override void Init()
        {
            RefractionShader = Shader.Find("Hidden/SC Post Effects/Refraction");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(RefractionShader);

            sheet.properties.SetFloat("_Amount", settings.amount);
            sheet.properties.SetFloat("_DUDV", (settings.convertNormalMap) ? 1 : 0);
            if (settings.refractionTex.value) sheet.properties.SetTexture("_RefractionTex", settings.refractionTex);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }

    }
}
#endif