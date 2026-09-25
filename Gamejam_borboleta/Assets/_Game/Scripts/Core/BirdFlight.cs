using UnityEngine;

namespace ButterflyStep
{
    public class BirdFlight : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [Tooltip("Voa em volta do ponto inicial. Desligado = fica pulando no lugar (preso).")]
        [SerializeField] private bool flying = true;
        [SerializeField] private Vector2 radius = new Vector2(2.5f, 0.8f);
        [SerializeField] private float speed = 0.9f;

        private Vector3 anchor;
        private float phase;
        private float lastX;

        public void Setup(SpriteRenderer renderer, bool fly, Vector2 flightRadius, float flightSpeed)
        {
            target = renderer;
            flying = fly;
            radius = flightRadius;
            speed = flightSpeed;
        }

        private void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            anchor = transform.localPosition;
            phase = Random.value * Mathf.PI * 2f;
            lastX = anchor.x;
        }

        private void Update()
        {
            if (TimeStasis.Active) return;
            phase += Time.deltaTime * speed;
            Vector3 p;
            if (flying) p = anchor + new Vector3(Mathf.Sin(phase) * radius.x, Mathf.Sin(phase * 2f) * radius.y, 0f);
            else p = anchor + new Vector3(0f, Mathf.Abs(Mathf.Sin(phase * 5f)) * 0.12f, 0f);
            transform.localPosition = p;
            if (target == null) return;
            if (flying)
            {
                if (Mathf.Abs(p.x - lastX) > 0.0005f) target.flipX = p.x < lastX;
            }
            else if (Mathf.Sin(phase * 0.7f) > 0.95f) target.flipX = !target.flipX;
            lastX = p.x;
        }
    }
}
