using UnityEditor;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE
    public sealed class RipplesEditor : Editor {} }
#else
    [PostProcessEditor(typeof(Ripples))]
    public sealed class RipplesEditor : PostProcessEffectEditor<Ripples>
    {

        SerializedParameterOverride m_Mode;

        SerializedParameterOverride m_Strength;
        SerializedParameterOverride m_Distance;
        SerializedParameterOverride m_Speed;
        SerializedParameterOverride m_Width;
        SerializedParameterOverride m_Height;

        public override void OnEnable()
        {
            m_Strength = FindParameterOverride(x => x.strength);
            m_Mode = FindParameterOverride(x => x.mode);
            m_Distance = FindParameterOverride(x => x.distance);
            m_Speed = FindParameterOverride(x => x.speed);
            m_Width = FindParameterOverride(x => x.width);
            m_Height = FindParameterOverride(x => x.height);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(m_Mode);
            PropertyField(m_Strength);
            PropertyField(m_Distance);
            PropertyField(m_Speed);

            //If Radial
            if (m_Mode.value.intValue == 0)
            {
                EditorGUILayout.Space();
                PropertyField(m_Width);
                PropertyField(m_Height);

            }
        }
    }
}
#endif