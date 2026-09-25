using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Collider2D))]
    public class StorySign : PlayerTrigger
    {
        [TextArea(2, 5)] [SerializeField] private string text = "Texto da placa";

        public void Setup(string message) => text = message;

        protected override void OnPlayerEnter(PlayerController player)
        {
            var ctx = LevelContext.Current;
            if (ctx != null && ctx.Hud != null) ctx.Hud.ShowSign(text);
        }

        protected override void OnPlayerExit(PlayerController player)
        {
            var ctx = LevelContext.Current;
            if (ctx != null && ctx.Hud != null) ctx.Hud.HideSign(text);
        }
    }
}
