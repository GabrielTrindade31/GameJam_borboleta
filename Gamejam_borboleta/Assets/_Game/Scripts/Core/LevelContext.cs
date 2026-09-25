using UnityEngine;

namespace ButterflyStep
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(TimeManager), typeof(WorldState), typeof(LevelFlow))]
    public class LevelContext : MonoBehaviour
    {
        private static LevelContext current;

        [SerializeField] private PlayerController player;
        [SerializeField] private GameHUD hud;
        [SerializeField] private string playerLayerName = "Player";
        [SerializeField] private string enemyLayerName = "Enemy";

        public static LevelContext Current
        {
            get
            {
                if (current == null) current = FindFirstObjectByType<LevelContext>();
                return current;
            }
        }

        public TimeManager Time { get; private set; }
        public WorldState World { get; private set; }
        public LevelFlow Flow { get; private set; }
        public TimeStasis Stasis { get; private set; }
        public PlayerController Player => player;
        public GameHUD Hud => hud;

        private void Awake()
        {
            current = this;
            Time = GetComponent<TimeManager>();
            World = GetComponent<WorldState>();
            Flow = GetComponent<LevelFlow>();
            Stasis = GetComponent<TimeStasis>();
            if (player == null) player = FindFirstObjectByType<PlayerController>();
            if (hud == null) hud = FindFirstObjectByType<GameHUD>();

            int playerLayer = LayerMask.NameToLayer(playerLayerName);
            int enemyLayer = LayerMask.NameToLayer(enemyLayerName);
            if (playerLayer >= 0 && enemyLayer >= 0) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            if (player != null) Time.SetOccupant(player.BodyCollider);
        }

        private void OnDestroy()
        {
            if (current == this) current = null;
        }

        public void Wire(PlayerController playerController, GameHUD gameHud)
        {
            player = playerController;
            hud = gameHud;
        }

        public TimeState Now => Time.State;
    }
}
