using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Overlay : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(OverlayRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Screen/Overlay", true)]
    public sealed class Overlay : PostProcessEffectSettings
    {
        [Tooltip("The texture's alpha channel controls its opacity")]
        public TextureParameter overlayTex = new TextureParameter { value = null };

        public enum BlendMode
        {
            Linear,
            Additive,
            Multiply,
            Screen
        }

        [Serializable]
        public sealed class BlendModeParameter : ParameterOverride<BlendMode> { }

        [Tooltip("Blends the gradient through various Photoshop-like blending modes")]
        public BlendModeParameter blendMode = new BlendModeParameter { value = BlendMode.Linear };

        [Range(0f, 1f)]
        public FloatParameter intensity = new FloatParameter { value = 1f };

        [Range(0f, 1f)]
        public FloatParameter tiling = new FloatParameter { value = 0f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (overlayTex.value == null) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class OverlayRenderer : PostProcessEffectRenderer<Overlay>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Overlay");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(shader);

            if (settings.overlayTex.value) sheet.properties.SetTexture("_OverlayTex", settings.overlayTex);
            sheet.properties.SetFloat("_BlendMode", (int)settings.blendMode.value);
            sheet.properties.SetFloat("_Intensity", settings.intensity);
            sheet.properties.SetFloat("_Tiling", Mathf.Pow(settings.tiling + 1, 2));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }

    }
}
#endif