using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PushableBox : TemporalBehaviour
    {
        [Tooltip("Identificador único na fase. Deixe vazio para usar o nome do objeto.")]
        [SerializeField] private string timelineId = "";
        [Tooltip("Distância mínima para considerar que a caixa foi movida.")]
        [SerializeField] private float moveThreshold = 0.15f;
        [Tooltip("Abaixo desta altura a caixa volta para a última posição registrada.")]
        [SerializeField] private float fallLimitY = -30f;
        [Tooltip("Velocidade horizontal máxima ao ser empurrada.")]
        [SerializeField] private float maxPushSpeed = 2.5f;

        private Rigidbody2D body;
        private Vector2 origin;
        private Vector2 lastApplied;

        public string Id => string.IsNullOrEmpty(timelineId) ? name : timelineId;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.freezeRotation = true;
            origin = transform.position;
            lastApplied = origin;
        }

        public Vector2 GetPositionAt(int day)
        {
            var ctx = LevelContext.Current;
            if (ctx != null && ctx.World.TryGetTimelineValue(Id, day, out var pos)) return pos;
            return origin;
        }

        protected override void OnBeforeTimeChange(TimeState state) => Commit();

        protected override void Refresh(bool instant)
        {
            Vector2 target = GetPositionAt(Now.Day);
            if ((target - lastApplied).sqrMagnitude < 0.0001f && !instant) return;
            PlaceAt(target);
        }

        private void PlaceAt(Vector2 target)
        {
            lastApplied = target;
            body.position = target;
            transform.position = target;
            body.linearVelocity = Vector2.zero;
        }

        private void Commit()
        {
            Vector2 current = body.position;
            if ((current - lastApplied).magnitude < moveThreshold) return;
            lastApplied = current;
            Context.World.SetTimelineValue(Id, Now.Day, current, true);
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Burst(current, new Color(1f, 0.9f, 0.3f), 8);
        }

        private void FixedUpdate()
        {
            if (Context == null) return;
            if (body.position.y < fallLimitY) PlaceAt(GetPositionAt(Now.Day));
            Vector2 v = body.linearVelocity;
            if (Mathf.Abs(v.x) > maxPushSpeed) body.linearVelocity = new Vector2(Mathf.Sign(v.x) * maxPushSpeed, v.y);
        }
    }
}
