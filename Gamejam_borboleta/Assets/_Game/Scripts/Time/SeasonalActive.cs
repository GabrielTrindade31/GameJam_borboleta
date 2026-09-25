using UnityEngine;

namespace ButterflyStep
{
    public class SeasonalActive : TemporalBehaviour
    {
        [Tooltip("Estação em que os objetos filhos aparecem.")]
        [SerializeField] private Season season = Season.Inverno;

        private SpriteRenderer[] renderers = new SpriteRenderer[0];

        public void Setup(Season visibleIn) => season = visibleIn;

        private void Awake() => renderers = GetComponentsInChildren<SpriteRenderer>(true);

        protected override void Refresh(bool instant)
        {
            bool show = Now.Season == season;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = show;
            }
        }
    }
}
