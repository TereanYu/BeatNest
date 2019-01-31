using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Ripples : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(RipplesRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Screen/Ripples", true)]
    public sealed class Ripples : PostProcessEffectSettings
    {
        public enum RipplesMode
        {
            Radial = 0,
            OmniDirectional = 1,
        }

        [Serializable]
        public sealed class RipplesModeParam : ParameterOverride<RipplesMode> { }

        [DisplayName("Method")]
        public RipplesModeParam mode = new RipplesModeParam { value = RipplesMode.Radial };

        [Range(0f, 10), DisplayName("Intensity")]
        public FloatParameter strength = new FloatParameter { value = 2f };

        [Range(1f, 10), Tooltip("The frequency of the waves")]
        public FloatParameter distance = new FloatParameter { value = 5f };

        [Range(0f, 10), Tooltip("Speed")]
        public FloatParameter speed = new FloatParameter { value = 3f };

        [Range(0f, 5), Tooltip("Width")]
        public FloatParameter width = new FloatParameter { value = 1.5f };

        [Range(0f, 5), Tooltip("Height")]
        public FloatParameter height = new FloatParameter { value = 1f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (strength == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class RipplesRenderer : PostProcessEffectRenderer<Ripples>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Ripples");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetFloat("_Strength", (settings.strength * 0.01f));
            sheet.properties.SetFloat("_Distance", (settings.distance * 0.01f));
            sheet.properties.SetFloat("_Speed", settings.speed);
            sheet.properties.SetVector("_Size", new Vector2(settings.width, settings.height));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }

    }
}
#endif