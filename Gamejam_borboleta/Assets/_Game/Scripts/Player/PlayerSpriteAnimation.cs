using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerSpriteAnimation : MonoBehaviour
    {
        [SerializeField] private SpriteAnimator animator;
        [SerializeField] private string idle = "Idle";
        [SerializeField] private string run = "Run";
        [SerializeField] private string jump = "Jump";
        [SerializeField] private string fall = "Fall";
        [SerializeField] private string attack = "Attack";
        [SerializeField] private string dead = "Dead";
        [SerializeField] private string walk = "Walk";
        [SerializeField] private string blink = "Blink";
        [SerializeField] private string vanish = "Vanish";
        [SerializeField] private float walkThreshold = 3.5f;
        [SerializeField] private float runThreshold = 0.6f;

        private PlayerController controller;
        private PlayerHealth health;
        private PlayerCombat combat;
        private TimeManager time;
        private float blinkTimer = 3f;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();
            combat = GetComponent<PlayerCombat>();
        }

        private void OnEnable()
        {
            if (combat != null) combat.Attacked += OnAttack;
        }

        private void Start()
        {
            var ctx = LevelContext.Current;
            time = ctx != null ? ctx.Time : null;
            if (time != null) time.TimeChanged += OnTimeChanged;
        }

        private void OnDestroy()
        {
            if (time != null) time.TimeChanged -= OnTimeChanged;
        }

        private void OnTimeChanged(TimeState state)
        {
            if (animator != null && animator.Has(vanish) && (health == null || !health.IsDead)) animator.PlayOnce(vanish, idle);
        }

        private void OnDisable()
        {
            if (combat != null) combat.Attacked -= OnAttack;
        }

        private void OnAttack()
        {
            if (animator != null) animator.PlayOnce(attack, idle);
        }

        private void LateUpdate()
        {
            if (animator == null) return;
            if (health != null && health.IsDead)
            {
                animator.Play(dead);
                return;
            }
            if (animator.IsPlayingOnce) return;

            Vector2 v = controller.Velocity;
            if (!controller.IsGrounded) animator.Play(v.y > 0.1f ? jump : fall);
            else if (Mathf.Abs(v.x) > runThreshold) animator.Play(Mathf.Abs(v.x) < walkThreshold && animator.Has(walk) ? walk : run);
            else
            {
                blinkTimer -= Time.deltaTime;
                if (blinkTimer <= 0f && animator.Has(blink))
                {
                    blinkTimer = Random.Range(2.5f, 5f);
                    animator.PlayOnce(blink, idle);
                }
                else animator.Play(idle);
            }
        }
    }
}
