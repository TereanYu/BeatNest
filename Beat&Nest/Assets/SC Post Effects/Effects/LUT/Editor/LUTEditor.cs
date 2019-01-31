using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if SCPE
using UnityEditor.Rendering.PostProcessing;
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE
    public sealed class LUTEditor : Editor {} }
#else
    [PostProcessEditor(typeof(LUT))]
    public sealed class LUTEditor : PostProcessEffectEditor<LUT>
    {
        SerializedParameterOverride mode;
        SerializedParameterOverride intensity;
        SerializedParameterOverride lutNear;
        SerializedParameterOverride lutFar;
        SerializedParameterOverride distance;

        public override void OnEnable()
        {
            mode = FindParameterOverride(x => x.mode);
            intensity = FindParameterOverride(x => x.intensity);
            lutNear = FindParameterOverride(x => x.lutNear);
            lutFar = FindParameterOverride(x => x.lutFar);
            distance = FindParameterOverride(x => x.distance);
        }

        public override void OnInspectorGUI()
        {
            //SCPE_GUI.DrawDocumentationHeader("");

            //GUILayout.Space(-5f);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent("Open LUT Extracter", EditorGUIUtility.IconContent("d_PreTextureRGB").image, 
                    "Extract a LUT from the bottom-left corner of a screenshot"), 
                    EditorStyles.miniButton, GUILayout.Height(20f), GUILayout.Width(150f)))
                {
                    LUTExtracterWindow.ShowWindow();
                }
                
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space();

            CheckLUTImportSettings(lutNear);
            if (mode.value.intValue == 1) CheckLUTImportSettings(lutFar);

            PropertyField(mode);
            PropertyField(intensity);
            PropertyField(lutNear, new GUIContent(mode.value.intValue == 0 ? "Look up Texture" : "Near"));
            if (mode.value.intValue == 1)
            {
                PropertyField(lutFar);
                PropertyField(distance);
            }
        }

        // Checks import settings on the lut, offers to fix them if invalid
        void CheckLUTImportSettings(SerializedParameterOverride tex)
        {
            if (tex != null)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex.value.objectReferenceValue));

                if (importer != null) // Fails when using a non-persistent texture
                {
                    bool valid = importer.anisoLevel == 0
                        && importer.mipmapEnabled == false
                        && importer.sRGBTexture == false
                        && (importer.textureCompression == TextureImporterCompression.Uncompressed)
                        && importer.filterMode == FilterMode.Bilinear;

                    if (!valid)
                    {
                        EditorGUILayout.HelpBox("\"" + tex.value.objectReferenceValue.name + "\" has invalid LUT import settings.", MessageType.Warning);

                        GUILayout.Space(-32);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Fix", GUILayout.Width(60)))
                            {
                                SetLUTImportSettings(importer);
                                AssetDatabase.Refresh();
                            }
                            GUILayout.Space(8);
                        }
                        GUILayout.Space(11);
                    }
                }
                else
                {
                    tex.value.objectReferenceValue = null;
                }
            }
        }

        void SetLUTImportSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Bilinear;
            importer.sRGBTexture = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.anisoLevel = 0;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

    }
}
#endif