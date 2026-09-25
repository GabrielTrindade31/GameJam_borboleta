using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class WaterBody : MonoBehaviour
    {
        [Tooltip("Correnteza em unidades por segundo. X negativo empurra para a esquerda.")]
        [SerializeField] private Vector2 flow = Vector2.zero;
        [Tooltip("Força de empuxo aplicada em objetos com Rigidbody2D (inimigos, caixas).")]
        [SerializeField] private float buoyancy = 1.4f;
        [Tooltip("Resistência da água para objetos que não são o jogador.")]
        [SerializeField] private float drag = 3f;
        [SerializeField] private WaterSurface surface;
        [SerializeField] private ParticleSystem splash;
        [SerializeField] private string enterMessage = "";
        [Tooltip("Água funda: o Eco se afoga ao cair nela.")]
        [SerializeField] private bool deadly = true;

        private readonly HashSet<Rigidbody2D> floating = new HashSet<Rigidbody2D>();
        private BoxCollider2D area;
        private PlayerController swimmer;
        private bool messageShown;

        public Vector2 Flow => flow;
        public bool Deadly => deadly;
        public float SurfaceY => area.bounds.max.y;
        public float BottomY => area.bounds.min.y;

        public void Setup(Vector2 current, WaterSurface waterSurface, ParticleSystem splashParticles, string message, bool drowns)
        {
            deadly = drowns;
            flow = current;
            surface = waterSurface;
            splash = splashParticles;
            enterMessage = message;
        }

        private void Awake()
        {
            area = GetComponent<BoxCollider2D>();
            area.isTrigger = true;
        }

        private void OnDisable()
        {
            if (swimmer != null) swimmer.ExitWater(this);
            swimmer = null;
            floating.Clear();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var body = other.attachedRigidbody;
            if (body == null) return;
            var player = body.GetComponent<PlayerController>();
            if (player != null)
            {
                if (other != player.BodyCollider) return;
                swimmer = player;
                player.EnterWater(this);
                Splash(other.bounds.center.x, body.linearVelocity.y);
                if (!messageShown && !string.IsNullOrEmpty(enterMessage))
                {
                    messageShown = true;
                    var hud = LevelContext.Current != null ? LevelContext.Current.Hud : null;
                    if (hud != null) hud.ShowMessage(enterMessage, 3f);
                }
                return;
            }
            if (body.bodyType == RigidbodyType2D.Dynamic && floating.Add(body)) Splash(other.bounds.center.x, body.linearVelocity.y);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var body = other.attachedRigidbody;
            if (body == null) return;
            var player = body.GetComponent<PlayerController>();
            if (player != null)
            {
                if (other != player.BodyCollider) return;
                player.ExitWater(this);
                if (swimmer == player) swimmer = null;
                if (body.linearVelocity.y > 1f) Splash(other.bounds.center.x, -body.linearVelocity.y * 0.5f);
                return;
            }
            floating.Remove(body);
        }

        private void FixedUpdate()
        {
            if (floating.Count == 0) return;
            float g = Mathf.Abs(Physics2D.gravity.y);
            floating.RemoveWhere(b => b == null || !b.simulated);
            foreach (var body in floating)
            {
                if (body.bodyType != RigidbodyType2D.Dynamic || body.constraints == RigidbodyConstraints2D.FreezeAll) continue;
                float depth = Mathf.Clamp01((SurfaceY - body.position.y) / 1f);
                body.AddForce(Vector2.up * body.mass * g * body.gravityScale * buoyancy * depth);
                var v = body.linearVelocity;
                v = Vector2.MoveTowards(v, flow, drag * Time.fixedDeltaTime * Mathf.Max(1f, v.magnitude));
                body.linearVelocity = v;
            }
        }

        public void Splash(float x, float velocityY)
        {
            float strength = Mathf.Clamp(Mathf.Abs(velocityY), 1f, 14f);
            if (surface != null) surface.Disturb(x, -Mathf.Sign(velocityY == 0f ? -1f : velocityY) * strength * 0.035f);
            if (splash != null)
            {
                splash.transform.position = new Vector3(x, SurfaceY, 0f);
                splash.Emit(Mathf.RoundToInt(4 + strength * 1.5f));
            }
            if (strength > 4f) GameAudio.Play(Sfx.Splash);
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null) return;
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f);
            Gizmos.DrawCube(box.bounds.center, box.bounds.size);
            if (flow.sqrMagnitude < 0.01f) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(box.bounds.center, box.bounds.center + (Vector3)flow.normalized * 1.5f);
        }
    }
}
