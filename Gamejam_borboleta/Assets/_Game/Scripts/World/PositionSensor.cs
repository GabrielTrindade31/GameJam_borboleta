using UnityEngine;

namespace ButterflyStep
{
    public class PositionSensor : MonoBehaviour, IFlagProvider
    {
        [Tooltip("Flag ativa em todo dia em que a caixa estiver dentro da área.")]
        [SerializeField] private string flag = "CaixaNaArea";
        [SerializeField] private string description = "Caixa dentro da área";
        [SerializeField] private PushableBox target;
        [SerializeField] private Vector2 size = new Vector2(2f, 2f);

        public string FlagKey => flag;
        public string Description => description;

        public void Setup(string key, string text, PushableBox box, Vector2 areaSize)
        {
            flag = key;
            description = text;
            target = box;
            size = areaSize;
        }

        private void Awake()
        {
            var ctx = LevelContext.Current;
            if (ctx != null) ctx.World.RegisterProvider(this);
        }

        private void OnDestroy()
        {
            var ctx = LevelContext.Current;
            if (ctx != null) ctx.World.UnregisterProvider(this);
        }

        public bool IsActiveAt(WorldState world, int day)
        {
            if (target == null) return false;
            Vector2 p = target.GetPositionAt(day);
            Vector2 c = transform.position;
            return Mathf.Abs(p.x - c.x) <= size.x * 0.5f && Mathf.Abs(p.y - c.y) <= size.y * 0.5f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.6f);
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
