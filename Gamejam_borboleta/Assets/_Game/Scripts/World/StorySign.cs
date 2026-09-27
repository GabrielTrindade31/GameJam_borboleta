using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Collider2D))]
    public class StorySign : PlayerTrigger
    {
        [TextArea(2, 5)] [SerializeField] private string text = "Texto da placa";

        private SpriteRenderer[] renderers = new SpriteRenderer[0];
        private Color[] baseColors = new Color[0];
        private bool reading;

        public static StorySign Nearby { get; private set; }
        public bool IsReading => reading;

        public void Setup(string message) => text = message;

        private void Awake()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) baseColors[i] = renderers[i].color;
        }

        protected override void OnPlayerEnter(PlayerController player)
        {
            Nearby = this;
            SetGlow(true);
        }

        protected override void OnPlayerExit(PlayerController player)
        {
            if (Nearby == this) Nearby = null;
            SetGlow(false);
            Close();
        }

        private void OnDisable()
        {
            if (Nearby == this) Nearby = null;
            Close();
        }

        public void Toggle()
        {
            if (reading) Close();
            else Open();
        }

        private void Open()
        {
            var ctx = LevelContext.Current;
            if (ctx == null || ctx.Hud == null) return;
            reading = true;
            ctx.Hud.ShowSign(text);
            GameAudio.Play(Sfx.UiMove);
        }

        private void Close()
        {
            if (!reading) return;
            reading = false;
            var ctx = LevelContext.Current;
            if (ctx != null && ctx.Hud != null) ctx.Hud.HideSign(text);
        }

        private void SetGlow(bool on)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].color = on ? Color.Lerp(baseColors[i], new Color(1f, 0.92f, 0.55f), 0.45f) : baseColors[i];
            }
        }
    }
}
