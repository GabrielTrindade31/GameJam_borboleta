using UnityEngine;

namespace ButterflyStep
{
    public class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("0 = acompanha o mundo, 1 = fica parado com a câmera.")]
        [Range(0f, 1f)] [SerializeField] private float factor = 0.5f;
        [SerializeField] private bool affectY = false;

        private Transform cam;
        private Vector3 startPos;
        private Vector3 camStart;

        public void Setup(float parallaxFactor, bool followY = false)
        {
            factor = parallaxFactor;
            affectY = followY;
        }

        private void Start()
        {
            if (Camera.main == null) return;
            cam = Camera.main.transform;
            startPos = transform.position;
            camStart = cam.position;
        }

        private void LateUpdate()
        {
            if (cam == null) return;
            Vector3 delta = cam.position - camStart;
            transform.position = new Vector3(startPos.x + delta.x * factor, startPos.y + (affectY ? delta.y * factor : 0f), startPos.z);
        }
    }
}
