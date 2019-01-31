using System;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class LightStreaks : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(LightStreaksRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Rendering/Light Streaks", true)]
    public sealed class LightStreaks : PostProcessEffectSettings
    {
        public enum Quality
        {
            Performance,
            Appearance
        }

        [Serializable]
        public sealed class BlurMethodParameter : ParameterOverride<Quality> { }

        [DisplayName("Quality"), Tooltip("Choose between Box and Gaussian blurring methods.\n\nBox blurring is more efficient but has a limited blur range")]
        public BlurMethodParameter quality = new BlurMethodParameter { value = Quality.Appearance };

        [Range(0f, 1f), DisplayName("Streaks Only"), Tooltip("Shows only the effect, to alow for finetuning")]
        public BoolParameter debug = new BoolParameter { value = false };

        [Header("Anamorphic Lensfares")]
        [Range(0f, 1f), Tooltip("Intensity")]
        public FloatParameter intensity = new FloatParameter { value = 1f };

        [Range(0.01f, 3f), Tooltip("Luminance threshold, pixels above this threshold will contribute to the effect")]
        public FloatParameter luminanceThreshold = new FloatParameter { value = 1f };

        [Range(-1f, 1f), Tooltip("Direction")]
        public FloatParameter direction = new FloatParameter { value = -1f };

        [Header("Blur")]
        [Range(0f, 10f), DisplayName("Length"), Tooltip("The amount of blurring that must be performed")]
        public FloatParameter blur = new FloatParameter { value = 1f };

        [Range(1, 8), Tooltip("The number of times the effect is blurred. More iterations provide a smoother effect but induce more drawcalls.")]
        public IntParameter iterations = new IntParameter { value = 2 };

        [Range(1, 8), Tooltip("Every step halfs the resolution of the blur effect. Lower resolution provides a smoother blur but may induce flickering")]
        public IntParameter downscaling = new IntParameter { value = 2 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (blur == 0 || intensity == 0 || direction == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    public sealed class LightStreaksRenderer : PostProcessEffectRenderer<LightStreaks>
    {
        Shader shader;
        private int emissionTex;
        RenderTexture aoRT;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Light Streaks");
            emissionTex = Shader.PropertyToID("_BloomTex");
        }

        public override void Release()
        {
            base.Release();
        }

        enum Pass
        {
            LuminanceDiff,
            BlurFast,
            Blur,
            Blend,
            Debug
        }


        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            int blurMode = (settings.quality.value == LightStreaks.Quality.Performance) ? (int)Pass.BlurFast : (int)Pass.Blur;

            //float luminanceThreshold = (context.isSceneView) ? settings.luminanceThreshold / 20f : settings.luminanceThreshold;
            float luminanceThreshold = Mathf.GammaToLinearSpace(settings.luminanceThreshold.value);

            sheet.properties.SetFloat("_Threshold", luminanceThreshold);
            sheet.properties.SetFloat("_Blur", settings.blur);
            sheet.properties.SetFloat("_Intensity", settings.intensity);

            // Create RT for storing edge detection in
            context.command.GetTemporaryRT(emissionTex, context.width, context.height, 0, FilterMode.Bilinear, context.sourceFormat);

            //Luminance difference check on RT
            context.command.BlitFullscreenTriangle(context.source, emissionTex, sheet, (int)Pass.LuminanceDiff);

            int downSamples = settings.downscaling + 1;
            // get two smaller RTs
            int blurredID = Shader.PropertyToID("_Temp1");
            int blurredID2 = Shader.PropertyToID("_Temp2");
            cmd.GetTemporaryRT(blurredID, context.width/ downSamples, context.height/ downSamples, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, context.width/ downSamples, context.height/ downSamples, 0, FilterMode.Bilinear);

            //Pass into blur target texture
            cmd.Blit(emissionTex, blurredID);

            float ratio = Mathf.Clamp(settings.direction, -1, 1);
            float rw = ratio < 0 ? -ratio : 0f;
            float rh = ratio > 0 ? ratio * 8 : 0f;

            for (int i = 0; i < settings.iterations; i++)
            {
                // vertical blur 1
                cmd.SetGlobalVector("_BlurOffsets", new Vector4(rw * settings.blur / context.screenWidth, rh / context.screenHeight, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, blurMode);

                // vertical blur 2
                cmd.SetGlobalVector("_BlurOffsets", new Vector4((rw * settings.blur) * 2f / context.screenWidth, rh * 2f / context.screenHeight, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, blurMode);
            }

            context.command.SetGlobalTexture("_BloomTex", blurredID);

            //Blend AO tex with image
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (settings.debug) ? (int)Pass.Debug : (int)Pass.Blend);

            // release
            context.command.ReleaseTemporaryRT(blurredID);
            context.command.ReleaseTemporaryRT(blurredID2);
            context.command.ReleaseTemporaryRT(emissionTex);
        }
    }
}
#endif