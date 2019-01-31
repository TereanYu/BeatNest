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
    public class Dithering : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(DitheringRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Retro/Dithering", true)]
    public sealed class Dithering : PostProcessEffectSettings
    {
        public enum SizeMode
        {
            Small = 0,
            Big = 1,
        }

        [Serializable]
        public sealed class SizeModeParam : ParameterOverride<SizeMode> { }

        [DisplayName("Size")]
        public SizeModeParam size = new SizeModeParam { value = SizeMode.Small };

        [Range(0f, 1f), Tooltip("Fades the effect in or out")]
        public FloatParameter intensity = new FloatParameter { value = 1f };

        [Range(0f, 1f), Tooltip("The screen's luminance values control the density of the dithering matrix")]
        public FloatParameter luminanceThreshold = new FloatParameter { value = 1f };


        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0) return false;
                return true;
            }

            return false;
        }

    }

    internal sealed class DitheringRenderer : PostProcessEffectRenderer<Dithering>
    {

        Shader shader;

        public override void Init()
        {

            shader = Shader.Find("Hidden/SC Post Effects/Dithering");

        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            //context.camera.depthTextureMode = DepthTextureMode.DepthNormals;
            var sheet = context.propertySheets.Get(shader);

            float size = (settings.size.value == 0) ? 1f : 0.5f;
            Vector4 ditherParams = new Vector4(size, size, settings.luminanceThreshold, settings.intensity);
            sheet.properties.SetVector("_Dithering_Coords", ditherParams);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }

    }
}
#endif