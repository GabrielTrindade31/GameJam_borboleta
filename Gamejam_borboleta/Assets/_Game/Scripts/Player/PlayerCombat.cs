using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float cooldown = 0.35f;
        [SerializeField] private Vector2 hitboxSize = new Vector2(2.3f, 1.7f);
        [SerializeField] private float hitboxDistance = 1.3f;
        [SerializeField] private float stompBounce = 12f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private SpriteRenderer slashVisual;

        private readonly Collider2D[] hits = new Collider2D[8];
        private readonly HashSet<TemporalEnemy> struck = new HashSet<TemporalEnemy>();
        private readonly HashSet<TemporalBoss> struckBosses = new HashSet<TemporalBoss>();
        private PlayerController controller;
        private float nextAttackTime;

        public float StompBounce => stompBounce;
        public event System.Action Attacked;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            if (slashVisual != null) slashVisual.enabled = false;
        }

        private void Update()
        {
            if (PauseMenu.IsPaused || !controller.ControlEnabled || UnityEngine.Time.time < nextAttackTime) return;
            if (controller.Input.AttackPressed) Attack();
        }

        private void Attack()
        {
            nextAttackTime = UnityEngine.Time.time + cooldown;
            controller.SetAnimTrigger("Attack");
            Attacked?.Invoke();
            GameAudio.Play(Sfx.Swing);

            Vector2 center = (Vector2)transform.position + new Vector2(hitboxDistance * controller.Facing, 0f);
            var filter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = hitMask };
            int count = Physics2D.OverlapBox(center, hitboxSize, 0f, filter, hits);
            struck.Clear();
            struckBosses.Clear();
            for (int i = 0; i < count; i++)
            {
                var enemy = hits[i].GetComponentInParent<TemporalEnemy>();
                if (enemy != null && struck.Add(enemy))
                {
                    enemy.TakeDamage(damage, transform.position);
                    ImpactAt(hits[i]);
                }
                var boss = hits[i].GetComponentInParent<TemporalBoss>();
                if (boss != null && struckBosses.Add(boss))
                {
                    boss.TakeDamage(damage, transform.position);
                    ImpactAt(hits[i]);
                }
            }
            if (struck.Count + struckBosses.Count > 0)
            {
                GameAudio.Play(Sfx.Hit);
                var fx = FeedbackFX.Instance;
                if (fx != null) fx.HitStop(0.06f);
            }

            var slashFx = FeedbackFX.Instance;
            if (slashFx != null && slashFx.Library.slash.Length > 0) slashFx.Slash(transform, new Vector3(hitboxDistance * 0.8f * controller.Facing, 0.1f, 0f), controller.Facing, new Color(0.85f, 0.95f, 1f), 1.15f);
            else if (slashVisual != null) StartCoroutine(ShowSlash());
        }

        private void ImpactAt(Collider2D target)
        {
            var fx = FeedbackFX.Instance;
            if (fx == null) return;
            Vector2 point = target.ClosestPoint(transform.position + Vector3.right * controller.Facing * 0.5f);
            fx.Impact(point, new Color(1f, 0.95f, 0.7f), 1.1f);
        }

        private IEnumerator ShowSlash()
        {
            slashVisual.transform.localPosition = new Vector3(hitboxDistance * controller.Facing, 0f, 0f);
            slashVisual.enabled = true;
            yield return new WaitForSeconds(0.1f);
            slashVisual.enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            int facing = controller != null ? controller.Facing : 1;
            Gizmos.DrawWireCube(transform.position + new Vector3(hitboxDistance * facing, 0f), hitboxSize);
        }
    }
}
