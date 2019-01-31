using System;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Blur : ScriptableObject
    {

    }
}
#else
    [System.Serializable]
    [PostProcess(typeof(BlurRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Blurring/Blur")]
    public sealed class Blur : PostProcessEffectSettings
    {
        public enum BlurMethod
        {
            Gaussian,
            Box
        }

        [Serializable]
        public sealed class BlurMethodParameter : ParameterOverride<BlurMethod> { }

        [DisplayName("Method"), Tooltip("Box blurring uses fewer texture samples but has a limited blur range")]
        public BlurMethodParameter method = new BlurMethodParameter { value = BlurMethod.Gaussian };

        [Tooltip("When enabled, the amount of blur passes is doubled")]
        public BoolParameter highQuality = new BoolParameter { value = false };

        [Range(0f, 5f), Tooltip("The amount of blurring that must be performed")]
        public FloatParameter amount = new FloatParameter { value = 3f };

        [Range(1, 12), Tooltip("The number of times the effect is blurred. More iterations provide a smoother effect but induce more drawcalls.")]
        public IntParameter iterations = new IntParameter { value = 6 };

        [Range(1, 8), Tooltip("Every step halfs the resolution of the blur effect. Lower resolution provides a smoother blur but may induce flickering.")]
        public IntParameter downscaling = new IntParameter { value = 2 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (amount == 0) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class BlurRenderer : PostProcessEffectRenderer<Blur>
    {
        Shader _shader;
        int screenCopyID;

        enum Pass
        {
            Blend,
            Gaussian,
            Box
        }

        public override void Init()
        {
            _shader = Shader.Find("Hidden/SC Post Effects/Blur");
            screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
        }


        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(_shader);
            CommandBuffer cmd = context.command;

            context.command.GetTemporaryRT(screenCopyID, context.width, context.height, 0, FilterMode.Bilinear, context.sourceFormat);
            context.command.BlitFullscreenTriangle(context.source, screenCopyID, sheet, 0);

            //Gaussian
            if (settings.method == Blur.BlurMethod.Gaussian)
            {
                RenderGaussian(context, cmd, sheet);
            }
            //Box
            else
            {
                RenderBox(context, cmd, sheet);
            }
        }

        public void RenderBox(PostProcessRenderContext context, CommandBuffer cmd, PropertySheet sheet)
        {
            // get two smaller RTs
            int blurredID = Shader.PropertyToID("_Temp1");
            int blurredID2 = Shader.PropertyToID("_Temp2");
            cmd.GetTemporaryRT(blurredID, context.screenWidth / settings.downscaling, context.screenHeight / settings.downscaling, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, context.screenWidth / settings.downscaling, context.screenHeight / settings.downscaling, 0, FilterMode.Bilinear);

            // downsample screen copy into smaller RT, release screen RT
            cmd.Blit(screenCopyID, blurredID);
            cmd.ReleaseTemporaryRT(screenCopyID);

            for (int i = 0; i < settings.iterations; i++)
            {
                //Safeguard for exploding GPUs
                if (settings.iterations > 12) return;

                // horizontal blur
                cmd.SetGlobalVector("_Offsets", new Vector4((settings.amount * 1000) / context.screenWidth, 0, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, (int)Pass.Box);  // source -> tempRT

                // vertical blur
                cmd.SetGlobalVector("_Offsets", new Vector4(0, (settings.amount * 1000) / context.screenHeight, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, (int)Pass.Box);  // source -> tempRT

                //Double blur
                if (settings.highQuality)
                {
                    // horizontal blur
                    cmd.SetGlobalVector("_Offsets", new Vector4((settings.amount * 1000) / context.screenWidth, 0, 0, 0));
                    context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, (int)Pass.Box);  // source -> tempRT

                    // vertical blur
                    cmd.SetGlobalVector("_Offsets", new Vector4(0, (settings.amount * 1000) / context.screenHeight, 0, 0));
                    context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, (int)Pass.Box);  // source -> tempRT
                }
            }

            // Render blurred texture in blend pass
            cmd.BlitFullscreenTriangle(blurredID, context.destination, sheet, (int)Pass.Blend);

            // release
            context.command.ReleaseTemporaryRT(blurredID);
            context.command.ReleaseTemporaryRT(blurredID2);
        }

        public void RenderGaussian(PostProcessRenderContext context, CommandBuffer cmd, PropertySheet sheet)
        {
            // get two smaller RTs
            int blurredID = Shader.PropertyToID("_Temp1");
            int blurredID2 = Shader.PropertyToID("_Temp2");
            cmd.GetTemporaryRT(blurredID, context.screenWidth / settings.downscaling, context.screenHeight / settings.downscaling, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, context.screenWidth / settings.downscaling, context.screenHeight / settings.downscaling, 0, FilterMode.Bilinear);

            // downsample screen copy into smaller RT, release screen RT
            cmd.Blit(screenCopyID, blurredID);
            cmd.ReleaseTemporaryRT(screenCopyID);

            for (int i = 0; i < settings.iterations; i++)
            {
                //Safeguard for exploding GPUs
                if (settings.iterations > 12) return;

                // horizontal blur
                cmd.SetGlobalVector("_Offsets", new Vector4(settings.amount / context.screenWidth, 0, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, (int)Pass.Gaussian);  // source -> tempRT

                // vertical blur
                cmd.SetGlobalVector("_Offsets", new Vector4(0, settings.amount / context.screenHeight, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, (int)Pass.Gaussian);  // source -> tempRT

                //Double blur
                if (settings.highQuality)
                {
                    // horizontal blur
                    cmd.SetGlobalVector("_Offsets", new Vector4(settings.amount / context.screenWidth, 0, 0, 0));
                    context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, (int)Pass.Gaussian);  // source -> tempRT

                    // vertical blur
                    cmd.SetGlobalVector("_Offsets", new Vector4(0, settings.amount / context.screenHeight, 0, 0));
                    context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, (int)Pass.Gaussian);  // source -> tempRT
                }
            }

            // Render blurred texture in blend pass
            cmd.BlitFullscreenTriangle(blurredID, context.destination, sheet, (int)Pass.Blend);

            // release
            context.command.ReleaseTemporaryRT(blurredID);
            context.command.ReleaseTemporaryRT(blurredID2);
        }
    }
}
#endif