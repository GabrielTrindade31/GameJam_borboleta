using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ClimbZone : MonoBehaviour
    {
        private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null && other == player.BodyCollider) player.SetClimbZone(this, true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null && other == player.BodyCollider) player.SetClimbZone(this, false);
        }

        private void OnDisable()
        {
            var ctx = LevelContext.Current;
            if (ctx != null && ctx.Player != null) ctx.Player.SetClimbZone(this, false);
        }
    }
}
