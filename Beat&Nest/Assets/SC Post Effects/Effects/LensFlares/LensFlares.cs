using System;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class LensFlares : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(LensFlaresRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Rendering/Lens Flares", true)]
    public sealed class LensFlares : PostProcessEffectSettings
    {
        //public BoolParameter debug = new BoolParameter { value = false };
        public bool debug = false;

        [Space]

        [Range(0f, 1f), DisplayName("Intensity")]
        public FloatParameter intensity = new FloatParameter { value = 0.5f };

        [Range(0.01f, 2f), DisplayName("Threshold"), Tooltip("Luminance threshold, pixels above this threshold will contribute to the effect")]
        public FloatParameter luminanceThreshold = new FloatParameter { value = 1f };

        [Space]

        [DisplayName("Mask"), Tooltip("Use a texture to mask out the effect")]
        public TextureParameter maskTex = new TextureParameter { value = null };

        [Range(0f, 20f), DisplayName("Chromatic Abberation"), Tooltip("Refracts the color channels")]
        public FloatParameter chromaticAbberation = new FloatParameter { value = 10f };

        [DisplayName("Gradient"), Tooltip("Color the flares from the center of the screen to the outer edges")]
        public TextureParameter colorTex = new TextureParameter { value = null };

        [Header("Flares")]
        [Range(1, 4), DisplayName("Number")]
        public IntParameter iterations = new IntParameter { value = 2 };

        [Range(1, 2), DisplayName("Distance"), Tooltip("Offsets the Flares towards the edge of the screen")]
        public FloatParameter distance = new FloatParameter { value = 1.5f };

        [Range(1, 10), DisplayName("Falloff"), Tooltip("Fades out the Flares towards the edge of the screen")]
        public FloatParameter falloff = new FloatParameter { value = 10f };

        [Header("Halo"), Tooltip("Creates a halo at the center of the screen when looking directly at a bright spot")]
        [Range(0, 1), DisplayName("Size")]
        public FloatParameter haloSize = new FloatParameter { value = 0f };

        [Range(0f, 100f), DisplayName("Width")]
        public FloatParameter haloWidth = new FloatParameter { value = 70f };

        [Header("Blur")]
        [Range(1, 8), DisplayName("Blur"), Tooltip("The amount of blurring that must be performed")]
        public FloatParameter blur = new FloatParameter { value = 2f };

        [Range(1, 12), DisplayName("Iterations"), Tooltip("The number of times the effect is blurred. More iterations provide a smoother effect but induce more drawcalls.")]
        public IntParameter passes = new IntParameter { value = 3 };
    }

    public sealed class LensFlaresRenderer : PostProcessEffectRenderer<LensFlares>
    {
        Shader shader;
        private int emissionTex;
        private int flaresTex;
        RenderTexture aoRT;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Lensflares");
            emissionTex = Shader.PropertyToID("_BloomTex");
            flaresTex = Shader.PropertyToID("_FlaresTex");
        }

        public override void Release()
        {
            base.Release();
        }

        enum Pass
        {
            LuminanceDiff,
            Ghosting,
            Blur,
            Blend,
            Debug
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            sheet.properties.SetFloat("_Intensity", settings.intensity);
            float luminanceThreshold = Mathf.GammaToLinearSpace(settings.luminanceThreshold.value);
            sheet.properties.SetFloat("_Threshold", luminanceThreshold);
            sheet.properties.SetFloat("_Distance", settings.distance);
            sheet.properties.SetFloat("_Falloff", settings.falloff);
            sheet.properties.SetFloat("_Ghosts", settings.iterations);
            sheet.properties.SetFloat("_HaloSize", settings.haloSize);
            sheet.properties.SetFloat("_HaloWidth", settings.haloWidth);
            sheet.properties.SetFloat("_ChromaticAbberation", settings.chromaticAbberation);


            if (settings.colorTex.value)
            {
                sheet.properties.SetTexture("_ColorTex", settings.colorTex);
            }
            else
            {
                sheet.properties.SetTexture("_ColorTex", Texture2D.whiteTexture);
            }
            if (settings.maskTex.value)
            {
                sheet.properties.SetTexture("_MaskTex", settings.maskTex);
            }
            else
            {
                sheet.properties.SetTexture("_MaskTex", Texture2D.whiteTexture);

            }

            context.command.GetTemporaryRT(emissionTex, context.width, context.height, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
            context.command.BlitFullscreenTriangle(context.source, emissionTex, sheet, (int)Pass.LuminanceDiff);
            context.command.SetGlobalTexture("_BloomTex", emissionTex);

            context.command.GetTemporaryRT(flaresTex, context.width, context.height, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
            context.command.BlitFullscreenTriangle(emissionTex, flaresTex, sheet, (int)Pass.Ghosting);
            context.command.SetGlobalTexture("_FlaresTex", flaresTex);

            // get two smaller RTs
            int blurredID = Shader.PropertyToID("_Temp1");
            int blurredID2 = Shader.PropertyToID("_Temp2");
            cmd.GetTemporaryRT(blurredID, context.width/2, context.height/2, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, context.width/2, context.height/2, 0, FilterMode.Bilinear);

            // downsample screen copy into smaller RT, release screen RT
            cmd.Blit(flaresTex, blurredID);
            cmd.ReleaseTemporaryRT(flaresTex);


            for (int i = 0; i < settings.passes; i++)
            {
                // horizontal blur
                cmd.SetGlobalVector("_Offsets", new Vector4(settings.blur / context.screenWidth, 0, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, (int)Pass.Blur);  // source -> tempRT

                // vertical blur
                cmd.SetGlobalVector("_Offsets", new Vector4(0, settings.blur / context.screenHeight, 0, 0));
                context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, (int)Pass.Blur);  // source -> tempRT       
            }

            context.command.SetGlobalTexture("_FlaresTex", blurredID);

            //Blend tex with image
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (settings.debug) ? (int)Pass.Debug : (int)Pass.Blend);

            // release
            context.command.ReleaseTemporaryRT(emissionTex);
            context.command.ReleaseTemporaryRT(flaresTex);
            context.command.ReleaseTemporaryRT(blurredID);
            context.command.ReleaseTemporaryRT(blurredID2);
        }
    }
}
#endif