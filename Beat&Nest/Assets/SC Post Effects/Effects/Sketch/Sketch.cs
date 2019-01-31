using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class Sketch : ScriptableObject { }
}
#else
    [Serializable]
    [PostProcess(typeof(SketchRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Stylized/Sketch", true)]
    public sealed class Sketch : PostProcessEffectSettings
    {
        [Tooltip("The Red channel is used for darker shades, whereas the Green channel is for lighter.")]
        public TextureParameter strokeTex = new TextureParameter { value = null };

        public enum SketchProjectionMode
        {
            WorldSpace,
            ScreenSpace
        }
        [Serializable]
        public sealed class SketchProjectioParameter : ParameterOverride<SketchProjectionMode> { }

        [Space]
        [Tooltip("Choose the type of UV space being used")]
        public SketchProjectioParameter projectionMode = new SketchProjectioParameter { value = SketchProjectionMode.WorldSpace };

        public enum SketchMode
        {
            EffectOnly,
            Multiply,
            Add
        }

        [Serializable]
        public sealed class SketchModeParameter : ParameterOverride<SketchMode> { }

        [Tooltip("Choose one of the different modes")]
        public SketchModeParameter blendMode = new SketchModeParameter { value = SketchMode.EffectOnly };

        [Space]

        [Range(0f, 1f)]
        public FloatParameter intensity = new FloatParameter { value = 1f };

        public Vector2Parameter brightness = new Vector2Parameter { value = new Vector2(0f, 1f) };

        [Range(1f, 32f)]
        public FloatParameter tiling = new FloatParameter { value = 8f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (intensity == 0 || strokeTex.value == null) return false;
                return true;
            }

            return false;
        }

    }

    internal sealed class SketchRenderer : PostProcessEffectRenderer<Sketch>
    {

        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Sketch");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            var p = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, false);
            p[2, 3] = p[3, 2] = 0.0f;
            p[3, 3] = 1.0f;
            var clipToWorld = Matrix4x4.Inverse(p * context.camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
            sheet.properties.SetMatrix("clipToWorld", clipToWorld);

            if (settings.strokeTex.value) sheet.properties.SetTexture("_Strokes", settings.strokeTex);

            sheet.properties.SetVector("_Params", new Vector4(0, (int)settings.blendMode.value, settings.intensity, ((int)settings.projectionMode.value == 1) ? settings.tiling * 0.25f : settings.tiling));
            sheet.properties.SetVector("_Brightness", settings.brightness);
            sheet.properties.SetFloat("_Tiling", settings.tiling);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.projectionMode.value);
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.DepthNormals;
        }
    }
}
#endif