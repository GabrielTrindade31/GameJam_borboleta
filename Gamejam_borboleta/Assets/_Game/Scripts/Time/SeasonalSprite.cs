using UnityEngine;

namespace ButterflyStep
{
    public class SeasonalSprite : TemporalBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [Header("Sprite por estação (vazio = mantém o atual)")]
        [SerializeField] private Sprite spring;
        [SerializeField] private Sprite summer;
        [SerializeField] private Sprite autumn;
        [SerializeField] private Sprite winter;
        [Header("Cor por estação")]
        [SerializeField] private Color springTint = Color.white;
        [SerializeField] private Color summerTint = Color.white;
        [SerializeField] private Color autumnTint = Color.white;
        [SerializeField] private Color winterTint = new Color(0.85f, 0.92f, 1f);

        public void Setup(SpriteRenderer renderer, Sprite springSprite, Sprite summerSprite, Sprite autumnSprite, Sprite winterSprite)
        {
            target = renderer;
            spring = springSprite;
            summer = summerSprite;
            autumn = autumnSprite;
            winter = winterSprite;
        }

        private void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
        }

        protected override void Refresh(bool instant)
        {
            if (target == null) return;
            Sprite sprite;
            Color tint;
            switch (Now.Season)
            {
                case Season.Primavera: sprite = spring; tint = springTint; break;
                case Season.Verao: sprite = summer; tint = summerTint; break;
                case Season.Outono: sprite = autumn; tint = autumnTint; break;
                default: sprite = winter; tint = winterTint; break;
            }
            if (sprite != null) target.sprite = sprite;
            target.color = tint;
        }
    }
}
