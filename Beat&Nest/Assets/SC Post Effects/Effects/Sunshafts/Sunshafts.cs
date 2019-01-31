using System;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Sunshafts : ScriptableObject {}
}
#else
    [Serializable]
    [PostProcess(typeof(SunshaftsRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Environment/Sun Shafts", true)]
    public sealed class Sunshafts : PostProcessEffectSettings
    {
        //SunshaftCaster
        [Tooltip("Use the color of the Directional Light that's set as the caster")]
        public BoolParameter useCasterColor = new BoolParameter { value = true };
        [Tooltip("Use the intensity of the Directional Light that's set as the caster")]
        public BoolParameter useCasterIntensity = new BoolParameter { value = false };

        public enum BlendMode
        {
            Additive,
            Screen
        }

        [Serializable]
        public sealed class SunShaftsSourceParameter : ParameterOverride<BlendMode> { }
        [Tooltip("Additive mode adds the sunshaft color to the image, while Screen mode perserves color values")]
        public SunShaftsSourceParameter blendMode = new SunShaftsSourceParameter { value = BlendMode.Screen };

        public enum SunShaftsResolution
        {
            High = 1,
            Normal = 2,
            Low = 3,
        }

        [Serializable]
        public sealed class SunShaftsResolutionParameter : ParameterOverride<SunShaftsResolution> { }
        [DisplayName("Resolution"), Tooltip("")]
        public SunShaftsResolutionParameter resolution = new SunShaftsResolutionParameter { value = SunShaftsResolution.Normal };

        public static Vector3 sunPosition = Vector3.zero;

        [Tooltip("Any color values over this threshold will contribute to the sunshafts effect")]
        [DisplayName("Sky color threshold")]
        public ColorParameter sunThreshold = new ColorParameter { value = Color.black};

        [DisplayName("Color")]
        public ColorParameter sunColor = new ColorParameter { value = new Color(1f, 1f, 1f) };
        [DisplayName("Intensity")]
        public FloatParameter sunShaftIntensity = new FloatParameter { value = 1f };
        [Range(0.1f, 1f)]
        [Tooltip("The degree to which the shafts’ brightness diminishes with distance from the caster")]
        public FloatParameter falloff = new FloatParameter { value = 0.5f };

        [Tooltip("The length of the sunrays from the caster's position to the camera")]
        [UnityEngine.Rendering.PostProcessing.Min(0f)]
        public FloatParameter length = new FloatParameter { value = 10f };
        public BoolParameter highQuality = new BoolParameter { value = false };
        //[Range(1f, 3f)]
        //public IntParameter blurItterations = new IntParameter { value = 2 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (sunShaftIntensity == 0 || length == 0) { return false; }
                return true;
            }

            return false;
        }

        public static void AddShaftCaster()
        {
            GameObject directionalLight = null;

            if (GameObject.Find("Directional Light"))
            {
                directionalLight = GameObject.Find("Directional Light");
            }

            if (!directionalLight)
            {
                if (GameObject.Find("Directional light"))
                {
                    directionalLight = GameObject.Find("Directional light");
                }
            }

            if (!directionalLight)
            {
                Debug.LogError("<b>Sunshafts:</b> No object with the name 'Directional Light' or 'Directional light' could be found");
                return;
            }

            SunshaftCaster caster = directionalLight.GetComponent<SunshaftCaster>();

            if (!caster)
            {
                caster = directionalLight.AddComponent<SunshaftCaster>();
                Debug.Log("\"SunshaftCaster\" component was added to the <b>" + caster.gameObject.name + "</b> GameObject");
            }

            if (caster.enabled == false)
            {
                caster.enabled = true;
            }
        }
    }

    internal sealed class SunshaftsRenderer : PostProcessEffectRenderer<Sunshafts>
    {
        Shader shader;
        private int skyboxBufferID;

        enum Pass
        {
            SkySource,
            RadialBlur,
            Blend
        }

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Sun Shafts");
            skyboxBufferID = Shader.PropertyToID("_SkyboxBuffer");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            #region Parameters
            float sunIntensity = (settings.useCasterIntensity) ? SunshaftCaster.intensity : settings.sunShaftIntensity.value;

            //Screen-space sun position
            Vector3 v = Vector3.one * 0.5f;
            if (Sunshafts.sunPosition != Vector3.zero)
                v = context.camera.WorldToViewportPoint(Sunshafts.sunPosition);
            else
                v = new Vector3(0.5f, 0.5f, 0.0f);
            sheet.properties.SetVector("_SunPosition", new Vector4(v.x, v.y, sunIntensity, settings.falloff));

            Color sunColor = (settings.useCasterColor) ? SunshaftCaster.color : settings.sunColor.value;
            sheet.properties.SetFloat("_BlendMode", (int)settings.blendMode.value);
            sheet.properties.SetColor("_SunColor", (v.z >= 0.0f) ? sunColor : new Color(0, 0, 0, 0));
            sheet.properties.SetColor("_SunThreshold", settings.sunThreshold);
            #endregion

            int res = (int)settings.resolution.value;

            //Create skybox mask
            context.command.GetTemporaryRT(skyboxBufferID, context.width/2, context.height/2, 0, FilterMode.Bilinear, context.sourceFormat);
            context.command.BlitFullscreenTriangle(context.source, skyboxBufferID, sheet, (int)Pass.SkySource);
            cmd.SetGlobalTexture("_SunshaftBuffer", skyboxBufferID);

            //Blur buffer
            #region Blur
            cmd.BeginSample("Sunshafts blur");
            int blurredID = Shader.PropertyToID("_Temp1");
            int blurredID2 = Shader.PropertyToID("_Temp2");
            cmd.GetTemporaryRT(blurredID, context.width/res, context.height/res, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, context.width/res, context.height /res, 0, FilterMode.Bilinear);

            cmd.Blit(skyboxBufferID, blurredID);

            float offset = settings.length * (1.0f / 768.0f);

            int iterations = (settings.highQuality) ? 2 : 1;
            float blurAmount = (settings.highQuality) ? settings.length / 2.5f : settings.length;

            for (int i = 0; i < iterations; i++)
            {
                context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, (int)Pass.RadialBlur);
                offset = blurAmount * (((i * 2.0f + 1.0f) * 6.0f)) / context.screenWidth;
                sheet.properties.SetFloat("_BlurRadius", offset);

                context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, (int)Pass.RadialBlur);
                offset = blurAmount * (((i * 2.0f + 2.0f) * 6.0f)) / context.screenWidth;
                sheet.properties.SetFloat("_BlurRadius", offset);

            }
            cmd.EndSample("Sunshafts blur");

            cmd.SetGlobalTexture("_SunshaftBuffer", blurredID);
            #endregion

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)Pass.Blend);

            cmd.ReleaseTemporaryRT(blurredID);
            cmd.ReleaseTemporaryRT(blurredID2);
            cmd.ReleaseTemporaryRT(skyboxBufferID);
        }
    }
}
#endif