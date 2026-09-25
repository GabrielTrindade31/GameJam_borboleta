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
        [SerializeField] private float runThreshold = 0.6f;

        private PlayerController controller;
        private PlayerHealth health;
        private PlayerCombat combat;

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
            else animator.Play(Mathf.Abs(v.x) > runThreshold ? run : idle);
        }
    }
}
