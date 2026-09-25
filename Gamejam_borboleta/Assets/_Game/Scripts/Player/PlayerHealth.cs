using System;
using System.Collections;
using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float invulnerableTime = 1.1f;
        [SerializeField] private Vector2 knockback = new Vector2(7f, 8f);
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private ParticleSystem hurtParticles;

        private PlayerController controller;
        private float invulnerableUntil;
        private bool dead;

        public int Current { get; private set; }
        public int Max => maxHealth;
        public bool IsDead => dead;

        public event Action<int, int> HealthChanged;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            Current = maxHealth;
        }

        private void Start() => HealthChanged?.Invoke(Current, maxHealth);

        public void TakeDamage(int amount, Vector2 source)
        {
            if (dead || UnityEngine.Time.time < invulnerableUntil || amount <= 0) return;

            Current = Mathf.Max(0, Current - amount);
            invulnerableUntil = UnityEngine.Time.time + invulnerableTime;
            HealthChanged?.Invoke(Current, maxHealth);

            float dir = Mathf.Sign(transform.position.x - source.x);
            if (dir == 0f) dir = 1f;
            controller.ApplyKnockback(new Vector2(knockback.x * dir, knockback.y), 0.25f);
            controller.SetAnimTrigger("Hurt");
            GameAudio.Play(Sfx.Hurt);
            if (hurtParticles != null) hurtParticles.Play();
            var ctx = LevelContext.Current;
            if (ctx != null && ctx.Hud != null) ctx.Hud.Shake(0.25f, 0.25f);

            if (Current <= 0) Kill();
            else StartCoroutine(Blink());
        }

        public void Kill()
        {
            if (dead) return;
            dead = true;
            GameAudio.Play(Sfx.PlayerDie);
            Current = 0;
            HealthChanged?.Invoke(Current, maxHealth);
            controller.SetControlEnabled(false);
            if (hurtParticles != null) hurtParticles.Play();
            if (visual != null) visual.enabled = true;
            var ctx = LevelContext.Current;
            if (ctx != null) ctx.Flow.PlayerDied();
        }

        public void Revive()
        {
            dead = false;
            Current = maxHealth;
            invulnerableUntil = UnityEngine.Time.time + 1.5f;
            if (visual != null) visual.enabled = true;
            controller.SetControlEnabled(true);
            HealthChanged?.Invoke(Current, maxHealth);
            StartCoroutine(Blink());
        }

        private IEnumerator Blink()
        {
            if (visual == null) yield break;
            while (UnityEngine.Time.time < invulnerableUntil && !dead)
            {
                visual.enabled = !visual.enabled;
                yield return new WaitForSeconds(0.08f);
            }
            if (!dead) visual.enabled = true;
        }
    }
}
