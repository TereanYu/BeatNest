using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SCPE
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class RenderScreenSpaceSkybox : MonoBehaviour
    {
        private Camera currentCam;
        private bool isSceneView;
        private Camera skyboxCam;

        private CommandBuffer cmd;
        private int skyboxTexID;

        private const string texName = "_SkyboxTex";
        private const int downsamples = 2;

        //Blurring is disabled, works, but causes jittering
        public static bool enableBlur = false;
        private RenderTexture skyboxRT;

        public bool manuallyAdded = true;

        private void OnEnable()
        {
            if (!currentCam) currentCam = GetComponent<Camera>();

            isSceneView = (currentCam.name == "SceneCamera") ? true : false;

            cmd = new CommandBuffer();
            cmd.name = "[SCPE] Render skybox to texture";

            // Scene-view, AfterSkybox works correctly
            if (isSceneView)
            {
                currentCam.AddCommandBuffer(CameraEvent.AfterSkybox, cmd);
            }
            // Game-view, AfterSkybox event also renders geometry (Unity bug?)
            // Use secondary camera instead
            else
            {
                if (!skyboxCam)
                {
                    CreateSkyboxCamera();
                }
            }

            //Use traditional RenderTexture, which has mipmaps
            if (enableBlur)
            {
                this.skyboxRT = new RenderTexture(currentCam.pixelHeight / downsamples, currentCam.pixelWidth / downsamples, 0, RenderTextureFormat.ARGB32);
                this.skyboxRT.filterMode = FilterMode.Trilinear;
                this.skyboxRT.wrapMode = TextureWrapMode.Clamp;
                this.skyboxRT.useMipMap = true;
                this.skyboxRT.Create();
                cmd.Blit(BuiltinRenderTextureType.CurrentActive, skyboxRT);
            }
            else
            {
                skyboxTexID = Shader.PropertyToID(texName);
                cmd.GetTemporaryRT(skyboxTexID, currentCam.pixelHeight / downsamples, currentCam.pixelWidth / downsamples, 0, FilterMode.Bilinear);
                cmd.Blit(BuiltinRenderTextureType.CurrentActive, skyboxTexID);
            }
        }

        private void CreateSkyboxCamera()
        {
            GameObject camObj = new GameObject("Skybox renderer for " + currentCam.name);

            camObj.hideFlags = HideFlags.HideAndDontSave;

            skyboxCam = camObj.AddComponent<Camera>();
            skyboxCam.hideFlags = HideFlags.NotEditable;
            skyboxCam.useOcclusionCulling = false;
            skyboxCam.depth = -100;
            skyboxCam.allowMSAA = false;
            skyboxCam.cullingMask = 0;
            skyboxCam.clearFlags = CameraClearFlags.Skybox;
            skyboxCam.nearClipPlane = 0.01f;
            skyboxCam.farClipPlane = 1;

            skyboxCam.AddCommandBuffer(CameraEvent.AfterSkybox, cmd);
        }

        public void Destroy()
        {
            if (isSceneView)
            {
                currentCam.RemoveCommandBuffer(CameraEvent.AfterSkybox, cmd);
            }
            else
            {
                if (skyboxCam)
                {
                    skyboxCam.RemoveCommandBuffer(CameraEvent.AfterSkybox, cmd);
                    DestroyImmediate(skyboxCam.gameObject);
                }
            }
        }

        private static void CopyCameraSettings(Camera src, Camera dest)
        {
            if (dest == null) return;

            dest.transform.position = src.transform.position;
            dest.transform.rotation = src.transform.rotation;

            dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;

            dest.orthographic = src.orthographic;
            dest.orthographicSize = src.orthographicSize;

            dest.renderingPath = src.renderingPath;
            dest.targetDisplay = src.targetDisplay;
        }

        private void Update()
        {
            if (isSceneView == false) CopyCameraSettings(currentCam, skyboxCam);

            if (enableBlur)
            {
                cmd.SetGlobalTexture(texName, skyboxRT);
            }
            else
            {
                cmd.SetGlobalTexture(texName, skyboxTexID);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RenderScreenSpaceSkybox))]
    public class RenderScreenSpaceSkyboxInspector : Editor
    {
        RenderScreenSpaceSkybox script;

        private void OnEnable()
        {
            script = (RenderScreenSpaceSkybox)target;
        }
        override public void OnInspectorGUI()
        {
            if (script.manuallyAdded)
            {
                EditorGUILayout.HelpBox("\nThis script should not be manually added to a camera!\n\nIt is automatically managed by the Fog effect.\n", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("\nThis script was automatically added by the SCPE Fog effect.", MessageType.Info);

            }
        }
    }
#endif

}
