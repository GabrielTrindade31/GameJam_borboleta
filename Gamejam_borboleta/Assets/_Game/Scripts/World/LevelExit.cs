using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Collider2D))]
    public class LevelExit : PlayerTrigger
    {
        [SerializeField] private Transform spinVisual;
        [SerializeField] private float spinSpeed = 90f;

        protected override void OnPlayerEnter(PlayerController player)
        {
            var ctx = LevelContext.Current;
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Burst(transform.position, new Color(1f, 0.85f, 0.3f), 40);
            if (ctx != null) ctx.Flow.CompleteLevel();
        }

        private void Update()
        {
            if (spinVisual == null) return;
            spinVisual.Rotate(0f, 0f, spinSpeed * UnityEngine.Time.deltaTime);
            float s = 1f + Mathf.Sin(UnityEngine.Time.time * 3f) * 0.08f;
            spinVisual.localScale = new Vector3(s, s, 1f);
        }
    }
}
