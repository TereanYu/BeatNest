using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LUTUtilities : Editor {

    public const int width = 1024;
    public const int height = 32;

    //Base64 encoded bytes
    private static readonly string neutralLUT64 = "iVBORw0KGgoAAAANSUhEUgAABAAAAAAgCAIAAAADnJ3xAAACY0lEQVR4Ae3bXQsBURSG0TPlgv//Y7nznUQzuZnQsyQRM9lrv0O705nGcWzH5b67Pr4/X3jr+ZC5j01j+37OTw58OeHLy8cZ1j//7s+/P/+lhK+fn3/3X/v7u76+m0/+/O8Cc3+yC/+Afj9vOHN003B9ub5+9/rajMNwI0CAAAECBAgQIEAgImAAiDRamQQIECBAgAABAgQuApuxB0GAAAECBAgQIECAQEXACkCl0+okQIAAAQIECBAgcBYwAIgBAQIECBAgQIAAgZCAASDUbKUSIECAAAECBAgQsAdABggQIECAAAECBAiEBKwAhJqtVAIECBAgQIAAAQIGABkgQIAAAQIECBAgEBIwAISarVQCBAgQIECAAAECBgAZIECAAAECBAgQIBASsAk41GylEiBAgAABAgQIELACIAMECBAgQIAAAQIEQgIGgFCzlUqAAAECBAgQIEDAACADBAgQIECAAAECBEIC9gCEmq1UAgQIECBAgAABAlYAZIAAAQIECBAgQIBASMAAEGq2UgkQIECAAAECBAgYAGSAAAECBAgQIECAQEjAHoBQs5VKgAABAgQIECBAwAqADBAgQIAAAQIECBAICRgAQs1WKgECBAgQIECAAAEDgAwQIECAAAECBAgQCAkYAELNVioBAgQIECBAgAABm4BlgAABAgQIECBAgEBIwApAqNlKJUCAAAECBAgQIGAAkAECBAgQIECAAAECIQEDQKjZSiVAgAABAgQIECBgD4AMECBAgAABAgQIEAgJWAEINVupBAgQIECAAAECBAwAMkCAAAECBAgQIEAgJHACW5RAfHLgP0sAAAAASUVORK5CYII=";

    public static Texture2D _NeutralLUT;
    public static Texture2D NeutralLUT
    {
        get
        {
            if (_NeutralLUT == null)
            {
                _NeutralLUT = NewNeutralLut();
            }
            return _NeutralLUT;
        }
    }

    private static Texture2D NewNeutralLut()
    {
        byte[] bytes = System.Convert.FromBase64String(neutralLUT64);

        Texture2D LUT = new Texture2D(width, height, TextureFormat.RGBA32, false);
        if (LUT.LoadImage(bytes, false))
        {
            return LUT;
        }

        return null;
    }

    public static bool IsValidScreenshot(TextureImporter importer)
    {
        return importer.isReadable && importer.npotScale == TextureImporterNPOTScale.None && importer.textureCompression == TextureImporterCompression.Uncompressed;
    }

    public static void SetSSImportSettings(TextureImporter importer)
    {
        importer.isReadable = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    public static bool IsValidLUT(TextureImporter importer)
    {
        return importer.anisoLevel == 0
                && importer.mipmapEnabled == false
                && importer.sRGBTexture == false
                && (importer.textureCompression == TextureImporterCompression.Uncompressed)
                && importer.filterMode == FilterMode.Bilinear;
    }

    public static void SetLUTImportSettings(TextureImporter importer)
    {
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Bilinear;
        importer.sRGBTexture = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.anisoLevel = 0;
        importer.mipmapEnabled = false;
        importer.isReadable = true;
        importer.SaveAndReimport();
    }

    public static void CaptureScreenshotWithLUT()
    {
        int ssWidth = 1280;
        int ssHeight = 720;

        //Set focus to scene view
#if UNITY_2018_1_OR_NEWER
        EditorApplication.ExecuteMenuItem("Window/General/Scene");
#else
        EditorApplication.ExecuteMenuItem("Window/Scene");
#endif

        if (SceneView.lastActiveSceneView)
        {
            Camera cam = SceneView.lastActiveSceneView.camera;

            RenderTexture rt = new RenderTexture(ssWidth, ssHeight, (cam.depthTextureMode == DepthTextureMode.None) ? 0 : 24);
            cam.targetTexture = rt;

            Texture2D screenShot = new Texture2D(ssWidth, ssHeight, TextureFormat.RGBA32, false);
            cam.Render();

            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, ssWidth, ssHeight), 0, 0);
            cam.targetTexture = null;
            RenderTexture.active = null;
            
            //Insert LUT into image
            for (int x = 0; x < NeutralLUT.width; x++)
            {
                for (int y = 0; y < NeutralLUT.height; y++)
                {
                    screenShot.SetPixel(x, y, NeutralLUT.GetPixel(x, y));
                }
            }
            screenShot.Apply();
            

            byte[] screenshotBytes = screenShot.EncodeToPNG();

            string filePath = "Assets/ColorGradingScreenshot.png";

            System.IO.File.WriteAllBytes(filePath, screenshotBytes);
            AssetDatabase.ImportAsset(filePath);

            //Application.OpenURL(filePath);

            Texture2D result = (Texture2D)AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D));
            LUTExtracterWindow.inputTexture = result;
        }
    }
}
