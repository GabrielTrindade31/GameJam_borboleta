using UnityEngine;

namespace ButterflyStep
{
    public class Butterfly : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private float size = 0.6f;
        [SerializeField] private float flapSpeed = 14f;
        [Tooltip("Raio do voo em oito em volta do ponto inicial.")]
        [SerializeField] private Vector2 hoverRadius = new Vector2(0.35f, 0.18f);
        [SerializeField] private float hoverSpeed = 1.3f;
        [SerializeField] private bool affectedByStasis = true;

        private Vector3 anchor;
        private float phase;
        private float baseScale = 1f;

        public void Setup(SpriteRenderer renderer, float worldSize, Vector2 radius, float speed)
        {
            target = renderer;
            size = worldSize;
            hoverRadius = radius;
            hoverSpeed = speed;
        }

        private void Awake()
        {
            if (target == null) target = GetComponentInChildren<SpriteRenderer>();
            anchor = transform.localPosition;
            phase = Random.value * 10f;
            if (target != null && target.sprite != null) baseScale = size / Mathf.Max(0.01f, target.sprite.bounds.size.x);
        }

        private void Update()
        {
            if (target == null) return;
            if (affectedByStasis && TimeStasis.Active) return;
            phase += Time.deltaTime;
            float flap = 0.2f + 0.8f * Mathf.Abs(Mathf.Cos(phase * flapSpeed));
            target.transform.localScale = new Vector3(baseScale * flap, baseScale, 1f);
            float t = phase * hoverSpeed;
            transform.localPosition = anchor + new Vector3(Mathf.Sin(t) * hoverRadius.x, Mathf.Sin(t * 2f) * hoverRadius.y, 0f);
            target.transform.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Cos(t) * 18f);
        }
    }
}
