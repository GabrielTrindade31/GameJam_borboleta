using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Collider2D))]
    public class HintZone : PlayerTrigger
    {
        [TextArea(1, 3)] [SerializeField] private string text = "Aviso";

        public void Setup(string message) => text = message;

        protected override void OnPlayerEnter(PlayerController player)
        {
            var ctx = LevelContext.Current;
            if (ctx != null && ctx.Hud != null) ctx.Hud.ShowHint(text);
        }

        protected override void OnPlayerExit(PlayerController player)
        {
            var ctx = LevelContext.Current;
            if (ctx != null && ctx.Hud != null) ctx.Hud.HideHint(text);
        }
    }
}
