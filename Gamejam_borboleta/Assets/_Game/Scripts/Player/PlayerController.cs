using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float acceleration = 70f;
        [SerializeField] private float deceleration = 80f;
        [Range(0f, 1f)] [SerializeField] private float airControl = 0.75f;

        [Header("Pulo")]
        [Tooltip("Altura máxima do pulo em unidades (1 unidade = 1 bloco).")]
        [SerializeField] private float jumpHeight = 3.2f;
        [SerializeField] private float gravityScale = 3.5f;
        [SerializeField] private float fallGravityMultiplier = 1.7f;
        [Tooltip("Gravidade extra quando o botão de pulo é solto cedo (pulo variável).")]
        [SerializeField] private float jumpCutGravityMultiplier = 2.4f;
        [SerializeField] private float maxFallSpeed = 18f;
        [Tooltip("Tempo em que ainda é possível pular depois de sair de uma borda.")]
        [SerializeField] private float coyoteTime = 0.1f;
        [Tooltip("Tempo em que um pulo apertado antes de tocar o chão ainda é aceito.")]
        [SerializeField] private float jumpBufferTime = 0.12f;

        [Header("Água")]
        [SerializeField] private float swimSpeed = 4.2f;
        [SerializeField] private float swimStroke = 6f;
        [Tooltip("Velocidade do salto para sair da água perto da superfície.")]
        [SerializeField] private float leapVelocity = 13f;
        [SerializeField] private float maxSinkSpeed = 3f;
        [Tooltip("Quanto do corpo fica submerso quando Eco boia parado (0 a 1).")]
        [SerializeField] private float floatLevel = 0.7f;

        [Header("Chão")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundCheckDepth = 0.08f;

        [Header("Referências")]
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private Transform visual;
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem dust;

        private Rigidbody2D body;
        private PlayerInputReader input;
        private readonly Collider2D[] groundHits = new Collider2D[8];
        private float coyoteTimer;
        private float jumpBufferTimer;
        private float stunTimer;
        private bool isGrounded;
        private bool wasGrounded;
        private bool jumping;
        private bool controlEnabled = true;
        private Vector3 visualBaseScale = Vector3.one;
        private Vector3 squash = Vector3.one;
        private WaterBody water;
        private PlayerHealth health;
        private Vector2 wind;
        private ClimbZone climbZone;
        private bool climbing;

        public bool IsGrounded => isGrounded;
        public int Facing { get; private set; } = 1;
        public Vector2 Velocity => body.linearVelocity;
        public Collider2D BodyCollider => bodyCollider;
        public PlayerInputReader Input => input;
        public bool ControlEnabled => controlEnabled;
        public bool InWater => water != null;
        public bool IsClimbing => climbing;

        public void AddWind(Vector2 force) => wind += force;

        public void SetClimbZone(ClimbZone zone, bool inside)
        {
            if (inside) climbZone = zone;
            else if (climbZone == zone)
            {
                climbZone = null;
                climbing = false;
            }
        }

        public void EnterWater(WaterBody area)
        {
            water = area;
            jumping = false;
        }

        public void ExitWater(WaterBody area)
        {
            if (water == area) water = null;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInputReader>();
            if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
            if (visual != null) visualBaseScale = visual.localScale;
            if (animator == null && visual != null) animator = visual.GetComponent<Animator>();
            body.gravityScale = gravityScale;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void SetControlEnabled(bool enabled)
        {
            controlEnabled = enabled;
            if (!enabled) body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }

        public bool IsGhost { get; private set; }

        public void SetGhost(bool ghost)
        {
            IsGhost = ghost;
            controlEnabled = !ghost;
            body.simulated = !ghost;
            if (ghost) body.linearVelocity = Vector2.zero;
            if (visual == null) return;
            foreach (var r in visual.GetComponentsInChildren<SpriteRenderer>())
            {
                r.color = ghost ? new Color(0.6f, 0.85f, 1f, 0.45f) : Color.white;
            }
        }

        public void Respawn(Vector2 position)
        {
            if (IsGhost) SetGhost(false);
            body.position = position;
            transform.position = position;
            body.linearVelocity = Vector2.zero;
            stunTimer = 0f;
            jumping = false;
            controlEnabled = true;
            water = null;
            Physics2D.SyncTransforms();
        }

        public void ApplyKnockback(Vector2 velocity, float stunDuration)
        {
            body.linearVelocity = velocity;
            stunTimer = stunDuration;
            jumping = false;
        }

        public void Bounce(float velocity, bool fullHeight = false)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, velocity);
            jumping = !fullHeight;
            squash = new Vector3(0.75f, 1.3f, 1f);
        }

        private void Update()
        {
            if (controlEnabled && input.JumpPressed) jumpBufferTimer = jumpBufferTime;
            else jumpBufferTimer -= UnityEngine.Time.deltaTime;

            coyoteTimer = isGrounded ? coyoteTime : coyoteTimer - UnityEngine.Time.deltaTime;
            stunTimer -= UnityEngine.Time.deltaTime;

            if (controlEnabled && stunTimer <= 0f)
            {
                float x = input.MoveX;
                if (x > 0.1f) Facing = 1;
                else if (x < -0.1f) Facing = -1;
            }

            UpdateVisual();
        }

        private void FixedUpdate()
        {
            CheckGround();

            float dt = UnityEngine.Time.fixedDeltaTime;
            Vector2 v = body.linearVelocity;
            bool canControl = controlEnabled && stunTimer <= 0f;

            if (water != null && !water.isActiveAndEnabled) water = null;
            if (water != null && Swim(ref v, canControl, dt))
            {
                body.linearVelocity = v;
                return;
            }

            if (Climb(ref v, canControl))
            {
                wind = Vector2.zero;
                body.linearVelocity = v;
                return;
            }

            float targetX = canControl ? input.MoveX * moveSpeed : 0f;
            float rate = Mathf.Abs(targetX) > 0.01f ? acceleration : deceleration;
            if (!isGrounded) rate *= airControl;
            if (stunTimer > 0f) rate *= 0.15f;
            v.x = Mathf.MoveTowards(v.x, targetX + wind.x * 0.4f, rate * dt);
            v.y += wind.y * dt;
            wind = Vector2.zero;

            if (canControl && jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                v.y = Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(Physics2D.gravity.y) * gravityScale);
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                jumping = true;
                isGrounded = false;
                squash = new Vector3(0.75f, 1.3f, 1f);
                SetAnimTrigger("Jump");
                GameAudio.Play(Sfx.Jump);
                if (dust != null) dust.Play();
            }

            float gravity = gravityScale;
            if (v.y < 0f) gravity *= fallGravityMultiplier;
            else if (v.y > 0f && jumping && !(controlEnabled && input.JumpHeld)) gravity *= jumpCutGravityMultiplier;
            body.gravityScale = gravity;

            v.y = Mathf.Max(v.y, -maxFallSpeed);
            body.linearVelocity = v;
        }

        private bool Climb(ref Vector2 v, bool canControl)
        {
            if (climbZone == null || !climbZone.isActiveAndEnabled)
            {
                climbing = false;
                return false;
            }
            float moveY = canControl ? input.MoveY : 0f;
            if (!climbing && canControl && Mathf.Abs(moveY) > 0.3f && !(isGrounded && moveY < 0f)) climbing = true;
            if (!climbing) return false;
            if (isGrounded && moveY < -0.3f)
            {
                climbing = false;
                return false;
            }
            if (canControl && jumpBufferTimer > 0f)
            {
                climbing = false;
                return false;
            }
            v = new Vector2((canControl ? input.MoveX : 0f) * moveSpeed * 0.45f, moveY * 4.5f);
            body.gravityScale = 0f;
            jumping = false;
            coyoteTimer = coyoteTime;
            return true;
        }

        private bool Swim(ref Vector2 v, bool canControl, float dt)
        {
            if (bodyCollider == null) return false;
            Bounds b = bodyCollider.bounds;
            float submerged = Mathf.Clamp01((water.SurfaceY - b.min.y) / Mathf.Max(0.1f, b.size.y));
            if (submerged < 0.4f) return false;

            if (water.Deadly)
            {
                v.x = Mathf.MoveTowards(v.x, water.Flow.x * 0.3f, 20f * dt);
                v.y = Mathf.MoveTowards(v.y, -1.2f, 30f * dt);
                body.gravityScale = 0f;
                if (health == null) health = GetComponent<PlayerHealth>();
                if (health != null && !health.IsDead) health.Kill();
                return true;
            }

            float moveX = canControl ? input.MoveX : 0f;
            float moveY = canControl ? input.MoveY : 0f;
            Vector2 flow = water.Flow;
            v.x = Mathf.MoveTowards(v.x, moveX * swimSpeed + flow.x, 16f * dt);

            float lift = (submerged - floatLevel) * 14f;
            if (moveY < -0.3f) lift = -9f;
            else if (moveY > 0.3f) lift = Mathf.Max(lift, 6f);
            v.y += (lift + flow.y) * dt;
            v.y *= 1f - Mathf.Clamp01(2.5f * dt);

            if (canControl && jumpBufferTimer > 0f)
            {
                jumpBufferTimer = 0f;
                if (submerged < 0.95f)
                {
                    v.y = leapVelocity;
                    jumping = true;
                    squash = new Vector3(0.75f, 1.3f, 1f);
                    water.Splash(b.center.x, -leapVelocity);
                    GameAudio.Play(Sfx.Jump);
                }
                else
                {
                    v.y = Mathf.Max(v.y, swimStroke);
                    squash = new Vector3(1.15f, 0.9f, 1f);
                    water.Splash(b.center.x, 2f);
                }
            }

            v.y = Mathf.Clamp(v.y, -maxSinkSpeed, leapVelocity);
            body.gravityScale = jumping && v.y > 0f ? gravityScale : 0f;
            return true;
        }

        private void CheckGround()
        {
            wasGrounded = isGrounded;
            isGrounded = false;
            if (bodyCollider == null) return;

            Bounds b = bodyCollider.bounds;
            var center = new Vector2(b.center.x, b.min.y - groundCheckDepth * 0.5f);
            var size = new Vector2(b.size.x * 0.9f, groundCheckDepth);
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = groundMask };
            int count = Physics2D.OverlapBox(center, size, 0f, filter, groundHits);

            for (int i = 0; i < count; i++)
            {
                if (groundHits[i].attachedRigidbody == body) continue;
                if (body.linearVelocity.y > 0.05f && groundHits[i].usedByEffector) continue;
                isGrounded = body.linearVelocity.y <= 0.05f || !jumping;
                if (isGrounded) break;
            }

            if (isGrounded)
            {
                jumping = false;
                if (!wasGrounded)
                {
                    squash = new Vector3(1.25f, 0.8f, 1f);
                    GameAudio.Play(Sfx.Land);
                    if (dust != null) dust.Play();
                }
            }
        }

        private void UpdateVisual()
        {
            if (visual == null) return;
            squash = Vector3.Lerp(squash, Vector3.one, UnityEngine.Time.deltaTime * 12f);
            visual.localScale = new Vector3(visualBaseScale.x * squash.x * Facing, visualBaseScale.y * squash.y, visualBaseScale.z);

            if (animator == null) return;
            SetAnimFloat("Speed", Mathf.Abs(body.linearVelocity.x));
            SetAnimFloat("VelocityY", body.linearVelocity.y);
            SetAnimBool("Grounded", isGrounded);
        }

        public void SetAnimTrigger(string name)
        {
            if (animator != null && HasParam(name)) animator.SetTrigger(name);
        }

        private void SetAnimFloat(string name, float value)
        {
            if (HasParam(name)) animator.SetFloat(name, value);
        }

        private void SetAnimBool(string name, bool value)
        {
            if (HasParam(name)) animator.SetBool(name, value);
        }

        private bool HasParam(string name)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;
            foreach (var p in animator.parameters)
            {
                if (p.name == name) return true;
            }
            return false;
        }
    }
}
