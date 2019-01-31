using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class ColorSplit : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(ColorSplitRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Retro/Color Split", true)]
    public sealed class ColorSplit : PostProcessEffectSettings
    {
        public enum SplitMode
        {
            Single = 0,
            SingleBoxFiltered = 1,
            Double = 2,
            DoubleBoxFiltered = 3
        }

        [Serializable]
        public sealed class SplitModeParam : ParameterOverride<SplitMode> { }

        [DisplayName("Method"), Tooltip("Box filtered methods provide a subtle blur effect and are less efficient")]
        public SplitModeParam mode = new SplitModeParam { value = SplitMode.Single };

        [Range(0f, 1f), Tooltip("The amount by which the color channels offset")]
        public FloatParameter offset = new FloatParameter { value = 1f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (offset == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class ColorSplitRenderer : PostProcessEffectRenderer<ColorSplit>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Color Split");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetFloat("_Offset", settings.offset/100);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }

    }
}
#endif
