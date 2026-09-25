using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class WindZone2D : MonoBehaviour
    {
        [Tooltip("Aceleração aplicada ao Eco enquanto estiver dentro da área.")]
        [SerializeField] private Vector2 force = new Vector2(18f, 0f);

        public void Setup(Vector2 windForce) => force = windForce;

        private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;

        private void OnTriggerStay2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || other != player.BodyCollider) return;
            player.AddWind(force);
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null) return;
            Gizmos.color = new Color(0.8f, 0.9f, 1f, 0.3f);
            Gizmos.DrawCube(box.bounds.center, box.bounds.size);
            Gizmos.DrawLine(box.bounds.center, box.bounds.center + (Vector3)force.normalized * 2f);
        }
    }
}
