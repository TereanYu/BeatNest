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
    public class LUT : ScriptableObject { }
}
#else
    [Serializable]
    [PostProcess(typeof(LUTRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Image/Color Grading LUT", true)]
    public sealed class LUT : PostProcessEffectSettings
    {
        public enum Mode
        {
            Single = 0,
            DistanceBased = 1,
        }

        [Serializable]
        public sealed class ModeParam : ParameterOverride<Mode> { }

        [DisplayName("Mode"), Tooltip("Distance-based mode blends two LUTs over a distance")]
        public ModeParam mode = new ModeParam { value = Mode.Single };

        [Range(1f,3000f)]
        public FloatParameter distance = new FloatParameter { value = 1000f };

        [Range(0f, 1f), Tooltip("Fades the effect in or out")]
        public FloatParameter intensity = new FloatParameter { value = 1f };

        [Tooltip("Supply a LUT strip texture.")]
        public TextureParameter lutNear = new TextureParameter { value = null };
        [DisplayName("Far")]
        public TextureParameter lutFar = new TextureParameter { value = null };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0 || !lutNear.value) return false;
                return true;
            }

            return false;
        }

    }

    internal sealed class LUTRenderer : PostProcessEffectRenderer<LUT>
    {

        Shader shader;

        public override void Init()
        {

            shader = Shader.Find("Hidden/SC Post Effects/LUT");

        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            if (settings.lutNear.value)
            {
                sheet.properties.SetTexture("_LUT_Near", settings.lutNear);
                sheet.properties.SetVector("_LUT_Params", new Vector4(1f / settings.lutNear.value.width, 1f / settings.lutNear.value.height, settings.lutNear.value.height - 1f, settings.intensity));
            }

            if((int)settings.mode.value == 1)
            {
                sheet.properties.SetFloat("_Distance", settings.distance);
                if (settings.lutFar.value) sheet.properties.SetTexture("_LUT_Far", settings.lutFar);
            }

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }

    }
}
#endif