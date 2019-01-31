using System;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif
namespace SCPE
{
#if !SCPE
    public class CloudShadows : ScriptableObject
    {

    }
}
#else
    [Serializable]
    [PostProcess(typeof(CloudShadowsRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Environment/Cloud Shadows")]
    public sealed class CloudShadows : PostProcessEffectSettings
    {
        [DisplayName("Texture (R)"), Tooltip("The red channel of this texture is used to sample the clouds")]
        public TextureParameter texture = new TextureParameter { value = null };

        [Space]

        [Range(0f, 1f)]
        [DisplayName("Size")]
        public FloatParameter size = new FloatParameter { value = 0.5f };
        [Range(0f, 1f)]
        [DisplayName("Density")]
        public FloatParameter density = new FloatParameter { value = 0.5f };
        [Range(0f, 1f)]
        [DisplayName("Speed")]
        public FloatParameter speed = new FloatParameter { value = 0.5f };

        [DisplayName("Direction"), Tooltip("Set the X and Z world-space direction the clouds should move in")]
        public Vector2Parameter direction = new Vector2Parameter { value = new Vector2(0f, 1f) };

        public static bool isOrtho = false;

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (density == 0 || texture.value == null) return false;
                return true;
            }

            return false;
        }

    }

    internal sealed class CloudShadowsRenderer : PostProcessEffectRenderer<CloudShadows>
    {

        Shader shader;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Cloud Shadows");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            Camera cam = context.camera;
            CloudShadows.isOrtho = context.camera.orthographic;

            sheet.properties.SetTexture("_NoiseTex", (settings.texture.value) ? settings.texture : Texture2D.whiteTexture as Texture);

            var p = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
            p[2, 3] = p[3, 2] = 0.0f;
            p[3, 3] = 1.0f;
            var clipToWorld = Matrix4x4.Inverse(p * cam.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
            sheet.properties.SetMatrix("clipToWorld", clipToWorld);

            float cloudsSpeed = settings.speed * 0.1f;
            sheet.properties.SetVector("_CloudParams", new Vector4(settings.size * 0.01f, settings.direction.value.x * cloudsSpeed, settings.direction.value.y * cloudsSpeed, settings.density));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.Depth;
        }


    }
}
#endif