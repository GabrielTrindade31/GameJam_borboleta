using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    public class PlayerBolt : MonoBehaviour
    {
        [SerializeField] private float speed = 13f;
        [SerializeField] private float lifetime = 1.1f;
        [SerializeField] private float radius = 0.45f;
        [SerializeField] private int damage = 1;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private LayerMask solidMask = 1;
        [SerializeField] private Transform visual;

        private readonly Collider2D[] hits = new Collider2D[8];
        private readonly RaycastHit2D[] rayHits = new RaycastHit2D[2];
        private readonly HashSet<Object> struck = new HashSet<Object>();
        private int direction = 1;
        private float age;

        public void Launch(int dir)
        {
            direction = dir;
            if (visual != null) visual.localScale = new Vector3(Mathf.Abs(visual.localScale.x) * dir, visual.localScale.y, 1f);
        }

        private void Update()
        {
            if (PauseMenu.IsPaused) return;
            float dt = Time.deltaTime;
            age += dt;
            if (visual != null) visual.Rotate(0f, 0f, -540f * dt * direction);
            Vector2 step = Vector2.right * direction * speed * dt;
            var solid = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = solidMask };
            if (Physics2D.Raycast(transform.position, Vector2.right * direction, solid, rayHits, step.magnitude + 0.1f) > 0)
            {
                Burst();
                return;
            }
            transform.position += (Vector3)step;
            var filter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = hitMask };
            int count = Physics2D.OverlapCircle(transform.position, radius, filter, hits);
            for (int i = 0; i < count; i++)
            {
                var enemy = hits[i].GetComponentInParent<TemporalEnemy>();
                if (enemy != null && enemy.IsAlive && struck.Add(enemy))
                {
                    enemy.TakeDamage(damage, transform.position - Vector3.right * direction);
                    Burst();
                    return;
                }
                var boss = hits[i].GetComponentInParent<TemporalBoss>();
                if (boss != null && boss.IsAlive && struck.Add(boss))
                {
                    boss.TakeDamage(damage, transform.position - Vector3.right * direction);
                    Burst();
                    return;
                }
            }
            if (age >= lifetime) Burst();
        }

        private void Burst()
        {
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Impact(transform.position, new Color(0.65f, 0.8f, 1f), 0.8f);
            Destroy(gameObject);
        }
    }
}
