using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : PlayerTrigger
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private ParticleSystem glow;
        [SerializeField] private Color idleColor = new Color(0.55f, 0.6f, 0.7f);
        [SerializeField] private Color activeColor = Color.white;

        public Vector2 RespawnPoint => (Vector2)transform.position + Vector2.up * 1f;

        public void Setup(SpriteRenderer sprite, ParticleSystem particles)
        {
            visual = sprite;
            glow = particles;
        }

        private void Start() => SetActive(false);

        public void SetActive(bool active)
        {
            if (visual != null) visual.color = active ? activeColor : idleColor;
            if (glow == null) return;
            if (active) glow.Play();
            else glow.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        protected override void OnPlayerEnter(PlayerController player)
        {
            var ctx = LevelContext.Current;
            if (ctx != null) ctx.Flow.SetCheckpoint(this);
        }
    }
}
