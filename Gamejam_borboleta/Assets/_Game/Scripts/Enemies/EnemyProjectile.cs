using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private LayerMask solidMask = 1;
        [Tooltip("Se falso, atravessa o chão (ondas de choque).")]
        [SerializeField] private bool destroyOnSolid = true;
        [SerializeField] private Transform visual;
        [SerializeField] private float spinSpeed = 540f;

        private Rigidbody2D body;
        private Vector2 velocity;
        private int damage = 1;
        private int spawnDay;
        private float life;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;
        }

        public void Launch(Vector2 launchVelocity, int hitDamage, int day)
        {
            velocity = launchVelocity;
            body.linearVelocity = velocity;
            damage = hitDamage;
            spawnDay = day;
            life = lifetime;
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            if (visual != null && spinSpeed == 0f) visual.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void SetPassThroughSolids(bool value) => destroyOnSolid = !value;

        private void Update()
        {
            var ctx = LevelContext.Current;
            if (ctx == null || ctx.Now.Day != spawnDay)
            {
                Destroy(gameObject);
                return;
            }

            bool frozen = TimeStasis.Active;
            body.linearVelocity = frozen ? Vector2.zero : velocity;
            if (frozen) return;

            life -= UnityEngine.Time.deltaTime;
            if (life <= 0f) Destroy(gameObject);
            if (visual != null && spinSpeed != 0f) visual.Rotate(0f, 0f, spinSpeed * UnityEngine.Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                var health = player.GetComponent<PlayerHealth>();
                if (health != null) health.TakeDamage(damage, transform.position);
                Destroy(gameObject);
                return;
            }
            if (destroyOnSolid && !other.isTrigger && (solidMask.value & (1 << other.gameObject.layer)) != 0) Destroy(gameObject);
        }
    }
}
