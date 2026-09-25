using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Collider2D))]
    public class DeathZone : PlayerTrigger
    {
        [Tooltip("Se falso, causa apenas dano em vez de morte instantânea.")]
        [SerializeField] private bool instantKill = true;
        [SerializeField] private int damage = 1;

        protected override void OnPlayerEnter(PlayerController player)
        {
            var health = player.GetComponent<PlayerHealth>();
            if (health == null) return;
            if (instantKill) health.Kill();
            else health.TakeDamage(damage, transform.position);
        }
    }
}
