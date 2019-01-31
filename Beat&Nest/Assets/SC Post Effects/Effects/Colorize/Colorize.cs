using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Colorize : ScriptableObject { }
}
#else
    [Serializable]
    [PostProcess(typeof(ColorizeRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Image/Colorize", true)]
    public sealed class Colorize : PostProcessEffectSettings
    {
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

        [Range(0f, 1f), Tooltip("Fades the effect in or out")]
        public FloatParameter intensity = new FloatParameter { value = 1f };

        [Tooltip("Supply a gradient texture.\n\nLuminance values are colorized from left to right")]
        public TextureParameter colorRamp = new TextureParameter { value = null };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0 || !colorRamp.value) return false;
                return true;
            }

            return false;
        }

    }

    internal sealed class ColorizeRenderer : PostProcessEffectRenderer<Colorize>
    {

        Shader shader;

        public override void Init()
        {

            shader = Shader.Find("Hidden/SC Post Effects/Colorize");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetFloat("_Intensity", settings.intensity);
            if (settings.colorRamp.value) sheet.properties.SetTexture("_ColorRamp", settings.colorRamp);
            sheet.properties.SetFloat("_BlendMode", (int)settings.blendMode.value);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif