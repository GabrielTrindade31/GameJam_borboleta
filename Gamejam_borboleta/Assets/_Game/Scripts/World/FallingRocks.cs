using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    public class FallingRocks : TemporalBehaviour
    {
        [Tooltip("Estação em que as pedras despencam da encosta.")]
        [SerializeField] private Season season = Season.Outono;
        [SerializeField] private float xMin = 0f;
        [SerializeField] private float xMax = 8f;
        [SerializeField] private float spawnY = 9f;
        [SerializeField] private float interval = 0.7f;
        [SerializeField] private float rockSize = 0.7f;
        [SerializeField] private int damage = 1;
        [SerializeField] private Sprite rockSprite;
        [SerializeField] private GameObject warningVisual;

        private readonly List<GameObject> rocks = new List<GameObject>();
        private bool active;
        private float timer;
        private int seed;

        public void Setup(Season hazardSeason, float left, float right, float top, Sprite sprite, GameObject warning)
        {
            season = hazardSeason;
            xMin = left;
            xMax = right;
            spawnY = top;
            rockSprite = sprite;
            warningVisual = warning;
        }

        protected override void Refresh(bool instant)
        {
            active = Now.Season == season;
            if (warningVisual != null) warningVisual.SetActive(active);
            Clear();
            timer = 0.2f;
        }

        private void Clear()
        {
            foreach (var r in rocks)
            {
                if (r != null) Destroy(r);
            }
            rocks.Clear();
        }

        private void Update()
        {
            if (!active || PauseMenu.IsPaused) return;
            bool frozen = TimeStasis.Active;
            foreach (var r in rocks)
            {
                if (r == null) continue;
                var body = r.GetComponent<Rigidbody2D>();
                if (body != null) body.simulated = !frozen;
            }
            if (frozen) return;
            rocks.RemoveAll(r => r == null);
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = interval * Random.Range(0.7f, 1.3f);
            Spawn();
        }

        private void Spawn()
        {
            seed++;
            float x = Mathf.Lerp(xMin, xMax, Mathf.Repeat(seed * 0.618f, 1f));
            var go = new GameObject("Pedra Rolando");
            go.transform.position = new Vector3(x, spawnY, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = rockSprite;
            sr.sortingOrder = 8;
            float s = rockSprite != null ? rockSize / Mathf.Max(0.01f, rockSprite.bounds.size.x) : rockSize;
            go.transform.localScale = new Vector3(s, s, 1f);
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 2.2f;
            body.linearVelocity = new Vector2(Random.Range(0.5f, 2.5f), 0f);
            body.angularVelocity = Random.Range(-360f, -120f);
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = rockSprite != null ? rockSprite.bounds.extents.x * 0.8f : 0.4f;
            var player = LevelContext.Current != null ? LevelContext.Current.Player : null;
            if (player != null && player.BodyCollider != null) Physics2D.IgnoreCollision(col, player.BodyCollider);
            var hurt = go.AddComponent<RockHit>();
            hurt.Setup(damage);
            rocks.Add(go);
            Destroy(go, 4.5f);
        }

        private void OnDisable() => Clear();
    }

    public class RockHit : MonoBehaviour
    {
        private int damage = 1;
        private float armedAt;

        public void Setup(int amount) => damage = amount;

        private void Awake() => armedAt = Time.time + 0.05f;

        private void Update()
        {
            if (Time.time < armedAt) return;
            var player = LevelContext.Current != null ? LevelContext.Current.Player : null;
            if (player == null) return;
            Vector2 d = (Vector2)transform.position - (Vector2)player.transform.position;
            if (Mathf.Abs(d.x) > 0.7f || Mathf.Abs(d.y) > 1f) return;
            var health = player.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(damage, transform.position);
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Burst(transform.position, new Color(0.6f, 0.5f, 0.4f), 12);
            Destroy(gameObject);
        }
    }
}
