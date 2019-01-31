// SC Post Effects
// Staggart Creations
// http://staggart.xyz

using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class VolumeTrigger : MonoBehaviour
    {
#if SCPE

        [Header("Target volume")]
        public PostProcessVolume volume;
        [Space]
        public float decreaseSpeed = 1f;

        private float currentWeight = 0f;

        private void OnEnable()
        {
            //No volume assigned, try to get from current object
            if (volume == null)
            {
                volume = this.GetComponent<PostProcessVolume>();
                if (volume) volume.weight = 0f;
            }
            //Set start initial weight to 0
            else
            {
                volume.weight = 0f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Trigger();
        }

        /// <summary>
        /// Sets the weight of the current volume to 1, after which it will decrease by the set descreaseSpeed value
        /// </summary>
        public void Trigger()
        {
            //Bump up weight to 1
            currentWeight = 1f;
        }

        private void Update()
        {
            if (!volume) return;

            //Decreased weight
            currentWeight = Mathf.Clamp01(currentWeight - Time.deltaTime * decreaseSpeed);
            //Set current weight
            volume.weight = currentWeight;
        }
#endif
    }
}