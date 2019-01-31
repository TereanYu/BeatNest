using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class BlackBars : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(BlackBarsRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Screen/Black Bars", true)]
    public sealed class BlackBars : PostProcessEffectSettings
    {
        public enum Direction
        {
            Horizontal = 0,
            Vertical = 1,
        }

        [Serializable]
        public sealed class DirectionParam : ParameterOverride<Direction> { }

        [DisplayName("Direction"), Tooltip("")]
        public DirectionParam mode = new DirectionParam { value = Direction.Horizontal };

        [Range(0f, 1f), Tooltip("Size")]
        public FloatParameter size = new FloatParameter { value = 1f };

        [Range(0f, 1f), Tooltip("Max Size")]
        public FloatParameter maxSize = new FloatParameter { value = 0.33f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (size == 0 || maxSize == 0) { return false; }
                return true;
            }

            return false;
        }

    }

    internal sealed class BlackBarsRenderer : PostProcessEffectRenderer<BlackBars>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Black Bars");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetVector("_Size", new Vector2(settings.size / 10f, settings.maxSize * 5));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }

    }
}
#endif