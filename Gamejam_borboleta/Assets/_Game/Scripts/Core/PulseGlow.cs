using UnityEngine;

namespace ButterflyStep
{
    public class PulseGlow : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private float speed = 1.6f;
        [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.35f;

        private Color baseColor;
        private float offset;

        private void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            if (target != null) baseColor = target.color;
            offset = transform.position.x * 0.37f;
        }

        private void Update()
        {
            if (target == null) return;
            float k = 0.5f + 0.5f * Mathf.Sin(Time.time * speed + offset);
            var c = baseColor;
            c.a = baseColor.a * Mathf.Lerp(minAlpha, 1f, k);
            target.color = c;
        }
    }
}
