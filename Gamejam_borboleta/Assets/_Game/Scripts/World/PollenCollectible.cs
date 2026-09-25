using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ButterflyStep
{
    [RequireComponent(typeof(Collider2D))]
    public class PollenCollectible : TemporalBehaviour
    {
        [Tooltip("Número do pólen nesta fase (0 a 4).")]
        [Range(0, 4)] [SerializeField] private int pollenId;
        [Tooltip("Quando o pólen aparece (estação, dias, flags). Vazio = sempre.")]
        [SerializeField] private List<TemporalCondition> existsWhen = new List<TemporalCondition>();
        [SerializeField] private Transform visual;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Transform ring;

        private bool collected;
        private bool visible;
        private Vector3 baseLocal;

        public void Setup(int id, Transform visualRoot, SpriteRenderer renderer, params TemporalCondition[] conditions)
        {
            pollenId = id;
            visual = visualRoot;
            sprite = renderer;
            existsWhen = new List<TemporalCondition>(conditions);
        }

        private string SceneName => SceneManager.GetActiveScene().name;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (visual == null) return;
            baseLocal = visual.localPosition;
        }

        protected override void Start()
        {
            collected = GameProgress.HasPollen(SceneName, pollenId);
            base.Start();
            UpdateHud();
        }

        protected override void Refresh(bool instant)
        {
            visible = Check(existsWhen) && !collected;
            if (visual != null) visual.gameObject.SetActive(visible);
        }

        private void Update()
        {
            if (!visible || visual == null) return;
            visual.localPosition = baseLocal + Vector3.up * Mathf.Sin(UnityEngine.Time.time * 2.5f + pollenId) * 0.15f;
            if (ring == null) return;
            ring.Rotate(0f, 0f, -40f * UnityEngine.Time.deltaTime);
            float pulse = 0.75f + Mathf.Sin(UnityEngine.Time.time * 3f + pollenId) * 0.06f;
            ring.localScale = new Vector3(pulse, pulse, 1f);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!visible || collected) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;
            collected = true;
            GameProgress.CollectPollen(SceneName, pollenId);
            GameAudio.Play(Sfx.Pollen, 0f);
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.PollenCollect(transform.position);
            Refresh(false);
            UpdateHud();
        }

        private void UpdateHud()
        {
            var hud = Context != null ? Context.Hud : null;
            if (hud != null) hud.SetPollen(GameProgress.PollenCount(SceneName), GameProgress.PollenPerLevel);
        }
    }
}
