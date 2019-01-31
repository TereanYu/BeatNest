using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Kuwahara : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(KuwaharaRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Stylized/Kuwahara", true)]
    public sealed class Kuwahara : PostProcessEffectSettings
    {
        public enum KuwaharaMode
        {
            Regular = 0,
            DepthFade = 1
        }

        [Serializable]
        public sealed class KuwaharaModeParam : ParameterOverride<KuwaharaMode> { }

        [DisplayName("Method"), Tooltip("Choose to apply the effec to the entire screen, or fade in out over a distance")]
        public KuwaharaModeParam mode = new KuwaharaModeParam { value = KuwaharaMode.Regular };

        [Range(0, 8), DisplayName("Radius")]
        public IntParameter radius = new IntParameter { value = 5 };

        public BoolParameter invertFadeDistance = new BoolParameter { value = false };
        [DisplayName("Fade distance")]
        public FloatParameter fadeDistance = new FloatParameter { value = 1000f };


        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (radius == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class KuwaharaRenderer : PostProcessEffectRenderer<Kuwahara>
    {
        Shader KuwaharaShader;

        public override void Init()
        {
            KuwaharaShader = Shader.Find("Hidden/SC Post Effects/Kuwahara");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            if (context.camera.orthographic) settings.mode.value = Kuwahara.KuwaharaMode.Regular;

            var sheet = context.propertySheets.Get(KuwaharaShader);

            sheet.properties.SetFloat("_Radius", (float)settings.radius);

            sheet.properties.SetFloat("_FadeDistance", settings.fadeDistance);
            sheet.properties.SetVector("_DistanceParams", new Vector4(settings.fadeDistance, (settings.invertFadeDistance) ? 1 : 0, 0, 0));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }

        public override DepthTextureMode GetCameraFlags()
        {
            if ((int)settings.mode.value == 1)
            {
                return DepthTextureMode.Depth;
            }
            else
            {
                return DepthTextureMode.None;
            }
        }
    }
}
#endif