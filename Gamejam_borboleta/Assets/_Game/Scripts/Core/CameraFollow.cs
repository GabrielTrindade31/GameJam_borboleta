using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset = new Vector2(0f, 2f);
        [SerializeField] private float smoothTime = 0.18f;
        [SerializeField] private float lookAhead = 1.5f;
        [Tooltip("Limites da câmera no mundo (mínimo X/Y e máximo X/Y).")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(50f, 20f);

        private Camera cam;
        private Vector3 velocity;
        private float shakeTime;
        private float shakeStrength;
        private float lookAheadCurrent;

        public void Setup(Transform followTarget, Vector2 min, Vector2 max)
        {
            target = followTarget;
            minBounds = min;
            maxBounds = max;
        }

        private void Awake() => cam = GetComponent<Camera>();

        private void Start()
        {
            if (target != null) transform.position = ClampToBounds(Desired(0f));
        }

        public void Shake(float duration, float strength)
        {
            shakeTime = Mathf.Max(shakeTime, duration);
            shakeStrength = Mathf.Max(shakeStrength, strength);
        }

        private Vector3 Desired(float ahead)
        {
            return new Vector3(target.position.x + offset.x + ahead, target.position.y + offset.y, transform.position.z);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            var body = target.GetComponent<Rigidbody2D>();
            float desiredAhead = body != null ? Mathf.Clamp(body.linearVelocity.x * 0.25f, -lookAhead, lookAhead) : 0f;
            lookAheadCurrent = Mathf.Lerp(lookAheadCurrent, desiredAhead, UnityEngine.Time.deltaTime * 3f);

            Vector3 desired = ClampToBounds(Desired(lookAheadCurrent));
            Vector3 pos = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

            if (shakeTime > 0f)
            {
                shakeTime -= UnityEngine.Time.deltaTime;
                pos += (Vector3)(Random.insideUnitCircle * shakeStrength);
                if (shakeTime <= 0f) shakeStrength = 0f;
            }
            transform.position = pos;
        }

        private Vector3 ClampToBounds(Vector3 p)
        {
            if (!useBounds || cam == null) return p;
            float h = cam.orthographicSize;
            float w = h * cam.aspect;
            p.x = maxBounds.x - minBounds.x < w * 2f ? (minBounds.x + maxBounds.x) * 0.5f : Mathf.Clamp(p.x, minBounds.x + w, maxBounds.x - w);
            p.y = maxBounds.y - minBounds.y < h * 2f ? (minBounds.y + maxBounds.y) * 0.5f : Mathf.Clamp(p.y, minBounds.y + h, maxBounds.y - h);
            return p;
        }

        private void OnDrawGizmosSelected()
        {
            if (!useBounds) return;
            Gizmos.color = Color.cyan;
            Vector3 center = (minBounds + maxBounds) * 0.5f;
            Gizmos.DrawWireCube(center, maxBounds - minBounds);
        }
    }
}
