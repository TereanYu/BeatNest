using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
#if SCPE
using UnityEditor.Rendering.PostProcessing;
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE
    public sealed class FogEditor : Editor {}
    }
#else
    [PostProcessEditor(typeof(Fog))]
    public sealed class FogEditor : PostProcessEffectEditor<Fog>
    {
        //string docUrl = SCPE.DOC_URL + "#fog-gradient";

        SerializedParameterOverride useSceneSettings;
        SerializedParameterOverride fogMode;
        SerializedParameterOverride fogDensity;
        SerializedParameterOverride fogStartDistance;
        SerializedParameterOverride fogEndDistance;

        SerializedParameterOverride colorMode;
        SerializedParameterOverride fogColor;
        SerializedParameterOverride fogColorGradient;
        SerializedParameterOverride gradientDistance;
        SerializedParameterOverride gradientUseFarClipPlane;

        SerializedParameterOverride distanceFog;
        SerializedParameterOverride distanceDensity;
        SerializedParameterOverride useRadialDistance;

        SerializedParameterOverride excludeSkybox;

        SerializedParameterOverride heightFog;
        SerializedParameterOverride height;
        SerializedParameterOverride heightDensity;

        SerializedParameterOverride heightFogNoise;
        SerializedParameterOverride heightNoiseTex;
        SerializedParameterOverride heightNoiseSize;
        SerializedParameterOverride heightNoiseStrength;
        SerializedParameterOverride heightNoiseSpeed;

        SerializedParameterOverride lightScattering;
        SerializedParameterOverride scatterIntensity;
        SerializedParameterOverride scatterDiffusion;
        SerializedParameterOverride scatterThreshold;
        SerializedParameterOverride scatterSoftKnee;

        private float animSpeed = 4f;
        AnimBool m_showControls;
        AnimBool m_showHeight;
        AnimBool m_showScattering;

        public override void OnEnable()
        {
            useSceneSettings = FindParameterOverride(x => x.useSceneSettings);
            fogMode = FindParameterOverride(x => x.fogMode);
            fogDensity = FindParameterOverride(x => x.globalDensity);
            fogStartDistance = FindParameterOverride(x => x.fogStartDistance);
            fogDensity = FindParameterOverride(x => x.globalDensity);
            fogEndDistance = FindParameterOverride(x => x.fogEndDistance);
            colorMode = FindParameterOverride(x => x.colorSource);
            fogColor = FindParameterOverride(x => x.fogColor);
            fogColorGradient = FindParameterOverride(x => x.fogColorGradient);
            gradientDistance = FindParameterOverride(x => x.gradientDistance);
            gradientUseFarClipPlane = FindParameterOverride(x => x.gradientUseFarClipPlane);
            distanceFog = FindParameterOverride(x => x.distanceFog);
            distanceDensity = FindParameterOverride(x => x.distanceDensity);
            useRadialDistance = FindParameterOverride(x => x.useRadialDistance);
            excludeSkybox = FindParameterOverride(x => x.excludeSkybox);
            heightFog = FindParameterOverride(x => x.heightFog);
            height = FindParameterOverride(x => x.height);
            heightDensity = FindParameterOverride(x => x.heightDensity);
            heightFogNoise = FindParameterOverride(x => x.heightFogNoise);
            heightNoiseTex = FindParameterOverride(x => x.heightNoiseTex);
            heightNoiseSize = FindParameterOverride(x => x.heightNoiseSize);
            heightNoiseStrength = FindParameterOverride(x => x.heightNoiseStrength);
            heightNoiseSpeed = FindParameterOverride(x => x.heightNoiseSpeed);

            lightScattering = FindParameterOverride(x => x.lightScattering);
            scatterIntensity = FindParameterOverride(x => x.scatterIntensity);
            scatterDiffusion = FindParameterOverride(x => x.scatterDiffusion);
            scatterThreshold = FindParameterOverride(x => x.scatterThreshold);
            scatterSoftKnee = FindParameterOverride(x => x.scatterSoftKnee);


            m_showControls = new AnimBool(true);
            m_showControls.valueChanged.AddListener(Repaint);
            m_showControls.speed = animSpeed;

            m_showHeight = new AnimBool(true);
            m_showHeight.valueChanged.AddListener(Repaint);
            m_showHeight.speed = animSpeed;

            m_showScattering = new AnimBool(true);
            m_showScattering.valueChanged.AddListener(Repaint);
            m_showScattering.speed = animSpeed;
        }

        public override void OnInspectorGUI()
        {
            //SCPE_GUI.DrawDocumentationHeader(docUrl);

            if (RenderSettings.fog)
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.HelpBox("Fog is currently enabled in the active scene, resulting in an overlapping fog effect", MessageType.Warning);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Disable scene fog"))
                        {
                            RenderSettings.fog = false;
                        }
                        GUILayout.FlexibleSpace();
                    }


                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
            }
            PropertyField(useSceneSettings);
            EditorGUILayout.Space();

            m_showControls.target = !useSceneSettings.value.boolValue;
            if (EditorGUILayout.BeginFadeGroup(m_showControls.faded))
            {

                PropertyField(fogMode);
                PropertyField(fogStartDistance);
                if (fogMode.value.intValue == 1)
                {
                    PropertyField(fogEndDistance);
                }
                else
                {
                    PropertyField(fogDensity);
                }

                PropertyField(colorMode);
                if (colorMode.value.intValue == 0)
                {
                    PropertyField(fogColor);
                }
                else if (colorMode.value.intValue == 1)
                {
                    PropertyField(fogColorGradient);
                    if (fogColorGradient.value.objectReferenceValue)
                    {
                        //Gradient preview
                        //Rect rect = GUILayoutUtility.GetRect(1, 12, "TextField");
                        //EditorGUI.DrawPreviewTexture(rect, fogColorGradient.value.objectReferenceValue as Texture2D);

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUI.BeginDisabledGroup(gradientUseFarClipPlane.value.boolValue);
                            {
                                PropertyField(gradientDistance, new GUIContent("Distance"));
                            }
                            EditorGUI.EndDisabledGroup();
                            gradientUseFarClipPlane.overrideState.boolValue = true;
                            gradientUseFarClipPlane.value.boolValue = GUILayout.Toggle(gradientUseFarClipPlane.value.boolValue, "Automatic", EditorStyles.miniButton);
                        }
                        EditorGUILayout.EndHorizontal();

                        //gradientUseFarClipPlane.value.boolValue = (gradientDistance.overrideState.boolValue) ? false : true;

                        //EditorGUILayout.HelpBox("In scene view, the gradient will appear to look different due to the camera's different far clipping value", MessageType.None);
                    }
                }
            }
            EditorGUILayout.EndFadeGroup();

            PropertyField(distanceFog);
            if (distanceFog.value.boolValue)
            {
                PropertyField(useRadialDistance, new GUIContent("Radial"));
                PropertyField(distanceDensity);
            }

            //Always exclude skybox for skybox color mode
            if (colorMode.value.intValue != 2) PropertyField(excludeSkybox, new GUIContent("Exclude"));

            PropertyField(heightFog);
            m_showHeight.target = heightFog.value.boolValue;
            if (EditorGUILayout.BeginFadeGroup(m_showHeight.faded))
            {
                if(RuntimeUtilities.isSinglePassStereoSelected)
                {
                    EditorGUILayout.HelpBox("Currently bugged in VR", MessageType.Warning);
                }
                PropertyField(height);
                PropertyField(heightDensity);

                PropertyField(heightFogNoise);
                if (heightFogNoise.value.boolValue)
                {
#if UNITY_2018_1_OR_NEWER
                //EditorGUILayout.HelpBox("Fog noise is currently bugged when using the Post Processing installed through the Package Manager.", MessageType.Warning);
#endif
                    PropertyField(heightNoiseTex);
                    if (heightNoiseTex.value.objectReferenceValue)
                    {
                        PropertyField(heightNoiseSize);
                        PropertyField(heightNoiseStrength);
                        PropertyField(heightNoiseSpeed);
                    }

                }
            }
            EditorGUILayout.EndFadeGroup();

            PropertyField(lightScattering);
            m_showScattering.target = lightScattering.value.boolValue;
            if (EditorGUILayout.BeginFadeGroup(m_showScattering.faded))
            {
                PropertyField(scatterIntensity);
                PropertyField(scatterThreshold);
                PropertyField(scatterDiffusion);
                PropertyField(scatterSoftKnee);
            }
            EditorGUILayout.EndFadeGroup();

            }
    }
}
#endif