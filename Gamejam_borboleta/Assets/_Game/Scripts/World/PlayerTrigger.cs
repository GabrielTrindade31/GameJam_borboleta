using UnityEngine;

namespace ButterflyStep
{
    public abstract class PlayerTrigger : MonoBehaviour
    {
        protected virtual void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null) OnPlayerEnter(player);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null) OnPlayerExit(player);
        }

        protected abstract void OnPlayerEnter(PlayerController player);
        protected virtual void OnPlayerExit(PlayerController player) { }
    }
}
