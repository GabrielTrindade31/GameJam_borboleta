using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Collider2D))]
    public class BouncePad : MonoBehaviour
    {
        [Tooltip("Velocidade vertical dada ao Eco ao pular no cogumelo.")]
        [SerializeField] private float bounceVelocity = 19f;
        [SerializeField] private Transform visual;

        private Vector3 baseScale = Vector3.one;
        private float squash;

        public void Setup(float velocity, Transform visualRoot)
        {
            bounceVelocity = velocity;
            visual = visualRoot;
        }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (visual != null) baseScale = visual.localScale;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || other != player.BodyCollider || player.Velocity.y > 0.5f) return;
            player.Bounce(bounceVelocity, true);
            squash = 1f;
            GameAudio.Play(Sfx.Stomp);
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Burst(transform.position, new Color(1f, 0.55f, 0.4f), 10);
        }

        private void Update()
        {
            if (visual == null) return;
            squash = Mathf.MoveTowards(squash, 0f, Time.deltaTime * 4f);
            float k = Mathf.Sin(squash * Mathf.PI) * 0.35f;
            visual.localScale = new Vector3(baseScale.x * (1f + k), baseScale.y * (1f - k), baseScale.z);
        }
    }
}
