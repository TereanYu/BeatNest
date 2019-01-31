using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class HueShift3D : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(HueShift3DRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Image/3D Hue Shift", true)]
    public sealed class HueShift3D : PostProcessEffectSettings
    {
        [Range(0f, 1f), DisplayName("Opacity")]
        public FloatParameter intensity = new FloatParameter { value = 0.33f };

        [Range(0f, 1f), Tooltip("Speed")]
        public FloatParameter speed = new FloatParameter { value = 0.3f };

        [Range(0f, 3f), Tooltip("Size")]
        public FloatParameter size = new FloatParameter { value = 1f };

        [Range(0f, 10f), Tooltip("Bends the effect over the scene's geometry normals\n\nHigh values may induce banding artifacts")]
        public FloatParameter geoInfluence = new FloatParameter { value = 5f };

        public static bool isOrtho = false;

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class HueShift3DRenderer : PostProcessEffectRenderer<HueShift3D>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/3D Hue Shift");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            HueShift3D.isOrtho = context.camera.orthographic;

            sheet.properties.SetVector("_Params", new Vector4(settings.speed, settings.size, settings.geoInfluence, settings.intensity));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.DepthNormals;
        }

    }
}
#endif