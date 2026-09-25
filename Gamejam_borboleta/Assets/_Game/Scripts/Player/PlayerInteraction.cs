using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float radius = 1.3f;
        [SerializeField] private LayerMask interactableMask = ~0;

        private readonly Collider2D[] hits = new Collider2D[16];
        private PlayerInputReader input;
        private PlayerController controller;
        private Interactable focused;

        public Interactable Focused => focused;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            controller = GetComponent<PlayerController>();
        }

        private void Update()
        {
            Interactable best = FindBest();
            if (best != focused)
            {
                if (focused != null) focused.SetHighlighted(false);
                focused = best;
                if (focused != null) focused.SetHighlighted(true);
            }

            var ctx = LevelContext.Current;
            var hud = ctx != null ? ctx.Hud : null;
            if (hud != null) hud.SetPrompt(focused != null ? $"[{input.BindingName("Interact")}] {focused.CurrentPrompt}" : null);

            if (focused != null && !PauseMenu.IsPaused && input.InteractPressed && (controller == null || controller.ControlEnabled)) focused.Interact();
        }

        private Interactable FindBest()
        {
            var filter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = interactableMask };
            int count = Physics2D.OverlapCircle(transform.position, radius, filter, hits);
            Interactable best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var candidate = hits[i].GetComponentInParent<Interactable>();
                if (candidate == null || (!candidate.CanInteract && !candidate.ShowsHint)) continue;
                float d = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = candidate;
                }
            }
            return best;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
