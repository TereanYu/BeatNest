using System;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Gradient : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(GradientRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Screen/Gradient", true)]
    public sealed class Gradient : PostProcessEffectSettings
    {

        [Range(0f, 1f)]
        [DisplayName("Opacity")]
        public FloatParameter intensity = new FloatParameter { value = 1f };

        public enum Mode
        {
            ColorFields,
            Texture
        }

        [Serializable]
        public sealed class GradientModeParameter : ParameterOverride<Mode> { }
        [Space]
        [Tooltip("Set the color either through 2 color fields, or a gradient texture")]
        public GradientModeParameter input = new GradientModeParameter { value = Mode.ColorFields };

        [Tooltip("The color's alpha channel controls its opacity")]
        public ColorParameter color1 = new ColorParameter { value = new Color(0f, 0.8f, 0.56f, 0.5f) };
        [Tooltip("The color's alpha channel controls its opacity")]
        public ColorParameter color2 = new ColorParameter { value = new Color(0.81f, 0.37f, 1f, 0.5f) };

        [Range(0f,1f)]
        [Space]
        [Tooltip("Controls the rotation of the gradient")]
        public FloatParameter rotation = new FloatParameter { value = 0f };

        [DisplayName("Gradient"), Tooltip("")]
        public TextureParameter gradientTex = new TextureParameter { value = null };

        public enum BlendMode
        {
            Linear,
            Additive,
            Multiply,
            Screen
        }

        [Serializable]
        public sealed class BlendModeParameter : ParameterOverride<BlendMode> { }
        [Tooltip("Blends the gradient through various Photoshop-like blending modes")]
        public BlendModeParameter blendMode = new BlendModeParameter { value = BlendMode.Linear };

        private const int RESOLUTION = 64;

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0 || (input.value == Mode.Texture && gradientTex.value == null)) return false;
                return true;
            }

            return false;
        }

        //Converting a gradient to a texture currently breaks volume blending

        /*
        public Texture2D m_gradientTex;

        public Texture2D GenerateGradient(Gradient gradient)
        {
            if (this.gradient.overrideState == false) return null;
            Debug.Log("Converting gradient to texture");

            //Create texture first time
            if (!m_gradientTex)
            {
                m_gradientTex = new Texture2D(RESOLUTION, 1, TextureFormat.ARGB32, false)
                {
                    //Smooth interpolation
                    filterMode = FilterMode.Bilinear
                };
            }

            Color gradientPixel;

            for (int x = 0; x < RESOLUTION; x++)
            {
                gradientPixel = gradient.Evaluate(x / (float)RESOLUTION);
                m_gradientTex.SetPixel(x, 1, gradientPixel);
            }

            m_gradientTex.Apply();

            return m_gradientTex;
        }
        */
    }

    internal sealed class GradientRenderer : PostProcessEffectRenderer<Gradient>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Gradient");
            //settings.gradient.value = new Gradient();
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {

            var sheet = context.propertySheets.Get(shader);

            //This should be editor inspector only, but that's not possible currently
            //Texture2D gradientTexture = settings.GenerateGradient(settings.gradient.value);
            //if(settings.gradient.value.colorKeys.Length > 0) settings.gradientTex.value = settings.GenerateGradient(settings.gradient);

            if (settings.gradientTex.value) sheet.properties.SetTexture("_Gradient", settings.gradientTex);
            sheet.properties.SetColor("_Color1", settings.color1);
            sheet.properties.SetColor("_Color2", settings.color2);
            sheet.properties.SetFloat("_Rotation", settings.rotation *6);
            sheet.properties.SetFloat("_Intensity", settings.intensity);
            sheet.properties.SetFloat("_BlendMode", (int)settings.blendMode.value);


            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.input.value);
        }
    }
}
#endif