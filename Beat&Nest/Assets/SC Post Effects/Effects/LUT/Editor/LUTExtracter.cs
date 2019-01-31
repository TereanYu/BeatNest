// SC Post Effects
// Staggart Creations
// http://staggart.xyz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LUTExtracter : Editor
{
    //Cached in- and outputs
    public static string InputPath
    {
        get { return EditorPrefs.GetString("LUT_INPUT_PATH", ""); }
        set { EditorPrefs.SetString("LUT_INPUT_PATH", value); }
    }

    public static string OutputPath
    {
        get { return EditorPrefs.GetString("LUT_OUTPUT_PATH", ""); }
        set { EditorPrefs.SetString("LUT_OUTPUT_PATH", value); }
    }

    public static bool AutoExtract
    {
        get { return SessionState.GetBool("LUT_AUTO_EXTRACT", false); }
        set { SessionState.SetBool("LUT_AUTO_EXTRACT", value); }
    }
    public static string InputName //Use to check if a file by this name has been changed
    {
        get { return SessionState.GetString("LUT_INPUT_NAME", string.Empty); }
        set { SessionState.SetString("LUT_INPUT_NAME", value); }
    }

    private static void ExecuteAutoExtract()
    {
        Texture2D OutputLUT = (Texture2D)AssetDatabase.LoadAssetAtPath(OutputPath, typeof(Texture2D));
        Texture2D inputTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(InputPath, typeof(Texture2D));

        if (OutputLUT == null) return;

#if SCPE_DEV
        Debug.Log("Auto extracting LUT from " + inputTexture.name);
#endif
        ExtractLUT(inputTexture, OutputLUT);
    }

    public static void ExtractLUT(Texture2D screenshot, Texture2D targetLUT)
    {
        if (!screenshot || !targetLUT) return;

        //Save this texture's name for the auto-extraction check
        InputName = screenshot.name;

        Color[] texels = screenshot.GetPixels(0, 0, targetLUT.width, targetLUT.height);

        //Create new LUT
        Texture2D newLUT = new Texture2D(targetLUT.width, targetLUT.height, TextureFormat.RGBA32, false, true);

        newLUT.SetPixels(texels);
        newLUT.Apply();

        byte[] bytes = newLUT.EncodeToPNG();

        //Save new LUT
        string filePath = AssetDatabase.GetAssetPath(targetLUT);
        System.IO.File.WriteAllBytes(filePath, bytes);

        //AssetDatabase.Refresh();
        //AssetDatabase.SaveAssets();
        newLUT = (Texture2D)AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D));
        EditorUtility.CopySerialized(newLUT, targetLUT);
        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
    }



    internal sealed class TextureChangeListener : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (string.IsNullOrEmpty(InputName)) return;

            if (AutoExtract && assetPath.Contains(InputName))
            {
                ExecuteAutoExtract();
            }
        }
    }

}
