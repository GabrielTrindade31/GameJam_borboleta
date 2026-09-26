using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerShooter : MonoBehaviour
    {
        [Tooltip("Tempo de recarga do Disparo do Tempo, em segundos.")]
        [SerializeField] private float cooldown = 2.5f;
        [SerializeField] private PlayerBolt boltPrefab;

        private PlayerController controller;
        private float readyAt;

        public bool Unlocked => GameProgress.HasBolt;
        public float Charge => cooldown <= 0f ? 1f : Mathf.Clamp01(1f - (readyAt - Time.time) / cooldown);

        private void Awake() => controller = GetComponent<PlayerController>();

        private void Update()
        {
            if (PauseMenu.IsPaused || !controller.ControlEnabled || boltPrefab == null) return;
            if (!controller.Input.ShootPressed) return;
            var hud = LevelContext.Current != null ? LevelContext.Current.Hud : null;
            if (!Unlocked)
            {
                if (hud != null) hud.ShowMessage("Eco ainda não tem o Disparo do Tempo.", 1.5f);
                return;
            }
            if (Time.time < readyAt)
            {
                GameAudio.Play(Sfx.Blocked);
                return;
            }
            readyAt = Time.time + cooldown;
            var bolt = Instantiate(boltPrefab, transform.position + new Vector3(0.7f * controller.Facing, 0.15f, 0f), Quaternion.identity);
            bolt.Launch(controller.Facing);
            controller.SetAnimTrigger("Attack");
            GameAudio.Play(Sfx.Stasis);
        }
    }
}
