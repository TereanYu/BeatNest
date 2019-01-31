using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SCPE
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Light))]
    sealed class SunshaftCaster : MonoBehaviour
    {
#if SCPE
        [Range(0f, 10000f)]
        public float distance = 10000f;

        private Vector3 sunPosition;

        //Light component
        private Light sunLight;
        public static Color color;
        public static float intensity;

        private void OnEnable()
        {
            sunPosition = this.transform.position;

            if (!sunLight)
            {
                sunLight = this.GetComponent<Light>();
                if (sunLight)
                {
                    color = sunLight.color;
                    intensity = sunLight.intensity;
                }
            }
        }

        private void OnDisable()
        {
            sunPosition = Vector3.zero;
            Sunshafts.sunPosition = Vector3.zero;

        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(Sunshafts.sunPosition, "LensFlare Icon", true);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 0, 0.5f);
            Gizmos.DrawRay(transform.position, sunPosition);
        }

        void Update()
        {
            sunPosition = -transform.forward * distance;
            Sunshafts.sunPosition = sunPosition;

            if (sunLight)
            {
                color = sunLight.color;
                intensity = sunLight.intensity;
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SunshaftCaster))]
    public class SunshaftCasterInspector : Editor
    {
#if SCPE
        private new SerializedObject serializedObject;
        private SerializedProperty distance;

        void OnEnable()
        {
            serializedObject = new SerializedObject(target);
            distance = serializedObject.FindProperty("distance");
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(target, "Component");

            EditorGUILayout.PropertyField(distance, new GUIContent("Distance"));

            EditorGUILayout.HelpBox("This object is used as the source sunshaft caster for all sunshaft effect instances", MessageType.None);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
#endif
}

