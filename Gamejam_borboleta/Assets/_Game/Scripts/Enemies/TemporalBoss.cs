using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    public enum BossAttack
    {
        Investida,
        Rajada,
        Pancada,
        Invocar
    }

    [Serializable]
    public class BossPhase
    {
        [Tooltip("Nome da fase. Ex: Cria veloz, Rainha furiosa.")]
        public string name = "Fase";
        [Tooltip("Condições (dias, estação, flags). A última fase válida vence.")]
        public List<TemporalCondition> conditions = new List<TemporalCondition>();
        [Tooltip("Mensagem mostrada ao chegar num dia com esta fase.")]
        public string message = "";

        [Header("Ataques")]
        public List<BossAttack> attacks = new List<BossAttack>();
        [Tooltip("Segundos entre ataques.")]
        public float attackInterval = 2f;
        public float moveSpeed = 2f;
        public float dashSpeed = 12f;
        public int projectileCount = 3;
        public float projectileSpeed = 6f;
        [Tooltip("Abertura do leque de projéteis (graus).")]
        public float spread = 30f;
        public int damage = 1;

        [Header("Defesa")]
        [Tooltip("Pode levar dano a qualquer momento.")]
        public bool vulnerable = true;
        [Tooltip("Fica vulnerável quando atordoado depois de uma Pancada ou Investida.")]
        public bool vulnerableWhenStunned = true;
        public float stunTime = 2f;

        [Header("Visual")]
        [Min(0.1f)] public float size = 3f;
        public Color color = Color.white;
        public bool flying;
        public string animation = "";
        public string attackAnimation = "";
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class TemporalBoss : TemporalBehaviour
    {
        [Header("Chefe")]
        [SerializeField] private string displayName = "Chefe";
        [Tooltip("Vida total. O dano causado num dia vale para esse dia e todos os seguintes.")]
        [SerializeField] private int maxHealth = 20;
        [Tooltip("Flag registrada no dia da derrota. Use em portões para liberar a saída.")]
        [SerializeField] private string deathFlag = "ChefeDerrotado";
        [SerializeField] private List<BossPhase> phases = new List<BossPhase>();

        [Header("Arena")]
        [SerializeField] private float arenaLeft = -8f;
        [SerializeField] private float arenaRight = 8f;
        [SerializeField] private float flyHeight = 3.5f;

        [Header("Referências")]
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private SpriteAnimator animator;
        [SerializeField] private bool spriteFacesLeft;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private Collider2D hurtbox;
        [SerializeField] private GameObject remains;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private EnemyProjectile shockwavePrefab;
        [SerializeField] private TemporalEnemy minionPrefab;
        [SerializeField] private int maxMinions = 2;

        private readonly List<Vector2Int> hits = new List<Vector2Int>();
        private readonly List<GameObject> minions = new List<GameObject>();
        private Rigidbody2D body;
        private float baseGravity;
        private Vector2 spawn;
        private BossPhase phase;
        private int phaseIndex = -1;
        private int lastDay = int.MinValue;
        private bool alive = true;
        private bool awake;
        private bool frozen;
        private float attackTimer;
        private float stunTimer;
        private float invulnerableHint;
        private int direction = -1;
        private int attackCursor;
        private Coroutine action;
        private readonly RaycastHit2D[] rayHits = new RaycastHit2D[4];
        private float hover;
        private bool enrageAnnounced;

        private bool Enraged => Health <= maxHealth / 2;
        private Vector3 Center => transform.position + Vector3.up * (phase != null ? phase.size : 3f) * 0.5f;

        public string DisplayName => displayName;
        public bool IsAlive => alive;
        public bool Stunned => stunTimer > 0f;
        public bool CanBeHurt => alive && phase != null && (phase.vulnerable || (Stunned && phase.vulnerableWhenStunned));
        public int Health => Mathf.Max(0, maxHealth - DamageUntil(Now.Day));
        public int MaxHealth => maxHealth;
        public string PhaseName => alive ? (phase != null ? phase.name : "-") : "Derrotado";

        public void Setup(string bossName, int health, string flag, float left, float right, SpriteRenderer sprite, SpriteAnimator anim, bool facesLeft, Collider2D solid, Collider2D hurt, GameObject remainsObject, EnemyProjectile projectile, EnemyProjectile shockwave, TemporalEnemy minion)
        {
            displayName = bossName;
            maxHealth = health;
            deathFlag = flag;
            arenaLeft = left;
            arenaRight = right;
            visual = sprite;
            animator = anim;
            spriteFacesLeft = facesLeft;
            bodyCollider = solid;
            hurtbox = hurt;
            remains = remainsObject;
            projectilePrefab = projectile;
            shockwavePrefab = shockwave;
            minionPrefab = minion;
            phases = new List<BossPhase>();
        }

        public BossPhase AddPhase(BossPhase p)
        {
            phases.Add(p);
            return p;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.freezeRotation = true;
            baseGravity = body.gravityScale;
            spawn = transform.position;
        }

        private int DamageUntil(int day)
        {
            int total = 0;
            foreach (var h in hits)
            {
                if (h.x <= day) total += h.y;
            }
            return total;
        }

        protected override void Refresh(bool instant)
        {
            if (phases.Count == 0) return;
            bool timeChanged = Now.Day != lastDay;
            lastDay = Now.Day;

            bool dead = Context.World.IsActive(deathFlag, Now.Day) || Health <= 0;
            if (dead)
            {
                StopAction();
                ClearMinions();
                SetAlive(false);
                return;
            }

            int index = 0;
            for (int i = 0; i < phases.Count; i++)
            {
                if (Check(phases[i].conditions)) index = i;
            }
            bool phaseChanged = index != phaseIndex;
            if (!timeChanged && !phaseChanged && alive) return;

            phaseIndex = index;
            phase = phases[index];
            StopAction();
            ClearMinions();
            stunTimer = 0f;
            attackTimer = phase.attackInterval;
            attackCursor = 0;
            transform.localScale = new Vector3(phase.size, phase.size, 1f);
            body.gravityScale = phase.flying ? 0f : baseGravity;
            body.position = phase.flying ? spawn + Vector2.up * flyHeight : spawn;
            transform.position = body.position;
            body.linearVelocity = Vector2.zero;
            if (visual != null) visual.color = phase.color;
            if (animator != null && !string.IsNullOrEmpty(phase.animation)) animator.Play(phase.animation, true);
            SetAlive(true);

            if (!instant && awake && !string.IsNullOrEmpty(phase.message) && Context.Hud != null) Context.Hud.ShowMessage(phase.message, 3f);
            if (!instant && awake && phaseChanged)
            {
                var fxPhase = FeedbackFX.Instance;
                if (fxPhase != null) fxPhase.Evolve(Center, phase.color, phase.size * 0.6f);
                if (Context.Hud != null) Context.Hud.Shake(0.3f, 0.3f);
            }
        }

        private void SetAlive(bool value)
        {
            alive = value;
            if (visual != null)
            {
                foreach (var r in visual.GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = value;
            }
            if (hurtbox != null) hurtbox.enabled = value;
            if (bodyCollider != null) bodyCollider.enabled = value;
            if (remains != null) remains.SetActive(!value);
            body.simulated = value;
        }

        private void Update()
        {
            if (Context == null) return;
            var player = Context.Player;
            bool inArena = player != null && player.transform.position.x > arenaLeft - 2f && player.transform.position.x < arenaRight + 4f;
            if (inArena && !awake)
            {
                awake = true;
                if (alive && Context.Hud != null) Context.Hud.ShowMessage($"{displayName}: {PhaseName}", 2.5f);
            }
            var hud = Context.Hud;
            if (hud != null) hud.SetBoss(awake && inArena ? displayName : null, (float)Health / maxHealth, alive, CanBeHurt, Stunned);
            invulnerableHint -= UnityEngine.Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (!alive || phase == null || Context == null) return;

            bool shouldFreeze = TimeStasis.Active;
            if (shouldFreeze != frozen)
            {
                frozen = shouldFreeze;
                body.constraints = frozen ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;
                if (visual != null) visual.color = frozen ? new Color(0.55f, 0.8f, 1f) : phase.color;
            }
            if (frozen || !awake) return;

            float dt = UnityEngine.Time.fixedDeltaTime;
            if (stunTimer > 0f)
            {
                stunTimer -= dt;
                if (visual != null) visual.color = Color.Lerp(phase.color, new Color(1f, 1f, 0.5f), Mathf.PingPong(UnityEngine.Time.time * 6f, 1f));
                if (stunTimer <= 0f && visual != null) visual.color = phase.color;
                if (!phase.flying) body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }
            if (action != null) return;

            Move(dt);
            attackTimer -= dt;
            if (attackTimer <= 0f && phase.attacks.Count > 0)
            {
                var attack = phase.attacks[attackCursor % phase.attacks.Count];
                attackCursor++;
                attackTimer = phase.attackInterval * (Enraged ? 0.7f : 1f);
                action = StartCoroutine(Perform(attack));
            }
        }

        private void Move(float dt)
        {
            var player = Context.Player;
            if (player == null) return;
            float dx = player.transform.position.x - body.position.x;
            direction = dx >= 0f ? 1 : -1;
            Face(direction);
            if (phase.flying)
            {
                hover += dt;
                float targetX = Mathf.Clamp(player.transform.position.x, arenaLeft + 1f, arenaRight - 1f);
                float targetY = spawn.y + flyHeight + Mathf.Sin(hover * 2f) * 0.6f;
                Vector2 target = new Vector2(targetX, targetY);
                body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, (target - body.position).normalized * phase.moveSpeed, phase.moveSpeed * 4f * dt);
            }
            else
            {
                float vx = Mathf.Abs(dx) > 1.5f ? direction * phase.moveSpeed : 0f;
                if ((body.position.x < arenaLeft + 1f && vx < 0f) || (body.position.x > arenaRight - 1f && vx > 0f)) vx = 0f;
                body.linearVelocity = new Vector2(vx, body.linearVelocity.y);
            }
        }

        private void Face(int dir)
        {
            if (visual == null) return;
            var s = visual.transform.localScale;
            s.x = Mathf.Abs(s.x) * dir * (spriteFacesLeft ? -1 : 1);
            visual.transform.localScale = s;
        }

        private IEnumerator Perform(BossAttack attack)
        {
            if (animator != null && !string.IsNullOrEmpty(phase.attackAnimation)) animator.PlayOnce(phase.attackAnimation, phase.animation);
            switch (attack)
            {
                case BossAttack.Investida: yield return Charge(); break;
                case BossAttack.Rajada: yield return Telegraph(0.35f); Volley(); yield return Wait(0.4f); break;
                case BossAttack.Pancada: yield return Slam(); break;
                case BossAttack.Invocar: Summon(); yield return Wait(0.5f); break;
            }
            action = null;
        }

        private IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += UnityEngine.Time.deltaTime)
            {
                while (TimeStasis.Active) yield return null;
                yield return null;
            }
        }

        private IEnumerator Telegraph(float seconds)
        {
            body.linearVelocity = phase.flying ? Vector2.zero : new Vector2(0f, body.linearVelocity.y);
            var fxWarn = FeedbackFX.Instance;
            if (fxWarn != null) fxWarn.Warning(Center, seconds, phase.size * 0.9f, new Color(1f, 0.35f, 0.3f, 0.9f));
            for (float t = 0f; t < seconds; t += UnityEngine.Time.deltaTime)
            {
                while (TimeStasis.Active) yield return null;
                if (visual != null) visual.color = Color.Lerp(phase.color, new Color(1f, 0.3f, 0.3f), Mathf.PingPong(t * 8f, 1f));
                yield return null;
            }
            if (visual != null) visual.color = phase.color;
        }

        private IEnumerator Charge()
        {
            yield return Telegraph(0.6f);
            var fxDust = FeedbackFX.Instance;
            if (fxDust != null) fxDust.Burst(transform.position, new Color(0.85f, 0.75f, 0.6f), 16);
            if (Context.Hud != null) Context.Hud.Shake(0.15f, 0.15f);
            int dir = direction;
            float targetY = body.position.y;
            while (true)
            {
                while (TimeStasis.Active) yield return new WaitForFixedUpdate();
                body.linearVelocity = new Vector2(dir * phase.dashSpeed, phase.flying ? (targetY - body.position.y) * 4f : body.linearVelocity.y);
                if ((dir > 0 && body.position.x >= arenaRight - 1f) || (dir < 0 && body.position.x <= arenaLeft + 1f)) break;
                yield return new WaitForFixedUpdate();
            }
            body.linearVelocity = Vector2.zero;
            Stun(phase.stunTime * 0.5f);
        }

        private void Volley()
        {
            if (projectilePrefab == null || Context.Player == null) return;
            Vector2 origin = (Vector2)transform.position + Vector2.up * phase.size * 0.6f;
            Vector2 aim = ((Vector2)Context.Player.transform.position + Vector2.up * 0.4f - origin).normalized;
            float baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            int count = Mathf.Max(1, phase.projectileCount);
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : (float)i / (count - 1);
                float angle = (baseAngle + Mathf.Lerp(-phase.spread * 0.5f, phase.spread * 0.5f, t)) * Mathf.Deg2Rad;
                var shot = Instantiate(projectilePrefab, origin, Quaternion.identity);
                shot.Launch(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * phase.projectileSpeed, phase.damage, Now.Day);
            }
        }

        private IEnumerator Slam()
        {
            var player = Context.Player;
            float targetX = player != null ? Mathf.Clamp(player.transform.position.x, arenaLeft + 1f, arenaRight - 1f) : body.position.x;
            var fxTarget = FeedbackFX.Instance;
            if (fxTarget != null) fxTarget.Warning(new Vector3(targetX, spawn.y + 0.3f, 0f), 0.75f, phase.size * 0.8f, new Color(1f, 0.25f, 0.2f, 0.95f));
            yield return Telegraph(0.6f);
            if (phase.flying)
            {
                body.gravityScale = 0f;
                Vector2 start = body.position;
                Vector2 end = new Vector2(targetX, spawn.y);
                for (float t = 0f; t < 0.45f; t += UnityEngine.Time.fixedDeltaTime)
                {
                    while (TimeStasis.Active) yield return new WaitForFixedUpdate();
                    body.MovePosition(Vector2.Lerp(start, end, t / 0.45f));
                    yield return new WaitForFixedUpdate();
                }
                body.position = end;
            }
            else
            {
                float flight = 0.8f;
                float gravity = Mathf.Abs(Physics2D.gravity.y) * baseGravity;
                float vy = gravity * flight * 0.5f;
                float vx = (targetX - body.position.x) / flight;
                body.linearVelocity = new Vector2(vx, vy);
                yield return new WaitForFixedUpdate();
                float timeout = 2f;
                while (timeout > 0f && !(body.linearVelocity.y <= 0.01f && Grounded()))
                {
                    while (TimeStasis.Active) yield return new WaitForFixedUpdate();
                    timeout -= UnityEngine.Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }
                body.linearVelocity = Vector2.zero;
            }

            if (Context.Hud != null) Context.Hud.Shake(0.4f, 0.35f);
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Burst(transform.position, new Color(0.8f, 0.7f, 0.5f), 30);
            if (shockwavePrefab != null)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    var wave = Instantiate(shockwavePrefab, (Vector2)transform.position + new Vector2(side * phase.size * 0.4f, 0.4f), Quaternion.identity);
                    wave.Launch(new Vector2(side * phase.projectileSpeed, 0f), phase.damage, Now.Day);
                }
            }
            Stun(phase.stunTime);
            if (phase.flying)
            {
                while (stunTimer > 0f) yield return null;
            }
        }

        private void Summon()
        {
            if (minionPrefab == null) return;
            minions.RemoveAll(m => m == null);
            if (minions.Count >= maxMinions) return;
            float x = Mathf.Clamp(transform.position.x - direction * 2f, arenaLeft + 1f, arenaRight - 1f);
            var minion = Instantiate(minionPrefab, new Vector3(x, spawn.y, 0f), Quaternion.identity);
            minion.MarkSummoned();
            minions.Add(minion.gameObject);
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Burst(minion.transform.position, new Color(0.7f, 0.5f, 1f), 20);
        }

        private bool Grounded()
        {
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = 1 };
            int count = Physics2D.Raycast(body.position + Vector2.up * 0.1f, Vector2.down, filter, rayHits, 0.3f);
            for (int i = 0; i < count; i++)
            {
                if (rayHits[i].rigidbody != body) return true;
            }
            return false;
        }

        private void Stun(float seconds)
        {
            stunTimer = seconds;
            var fxStun = FeedbackFX.Instance;
            if (fxStun != null) fxStun.Attach(transform, Vector3.up * (phase.size * 1.05f + 0.3f), seconds, 1.3f, new Color(1f, 0.95f, 0.4f));
            if (phase.vulnerableWhenStunned && !phase.vulnerable && Context.Hud != null) Context.Hud.ShowMessage("Atordoado! Ataque agora!", 1.2f);
        }

        private void StopAction()
        {
            if (action != null) StopCoroutine(action);
            action = null;
            frozen = false;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void ClearMinions()
        {
            foreach (var m in minions)
            {
                if (m != null) Destroy(m);
            }
            minions.Clear();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!alive || phase == null || frozen) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            float playerBottom = player.BodyCollider != null ? player.BodyCollider.bounds.min.y : player.transform.position.y;
            float top = bodyCollider != null ? bodyCollider.bounds.max.y - 0.3f : transform.position.y + phase.size;
            if (player.Velocity.y < -0.1f && playerBottom > top)
            {
                var combat = player.GetComponent<PlayerCombat>();
                player.Bounce(combat != null ? combat.StompBounce : 12f);
                GameAudio.Play(Sfx.Stomp);
                TakeDamage(1, player.transform.position);
                return;
            }
            if (Stunned) return;
            var health = player.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(phase.damage, transform.position);
        }

        public void TakeDamage(int amount, Vector2 source)
        {
            if (!alive || phase == null) return;
            if (!CanBeHurt)
            {
                if (invulnerableHint <= 0f && Context.Hud != null)
                {
                    Context.Hud.ShowMessage(phase.vulnerableWhenStunned ? "Invulnerável! Espere ele ficar atordoado... ou tente outra época." : "Invulnerável nesta época! Tente outro dia.", 2f);
                    invulnerableHint = 2.5f;
                }
                GameAudio.Play(Sfx.Blocked);
                var fxBlock = FeedbackFX.Instance;
                if (fxBlock != null) fxBlock.Burst(transform.position + Vector3.up * phase.size * 0.5f, new Color(0.7f, 0.8f, 1f), 8);
                return;
            }

            GameAudio.Play(Sfx.BossHit);
            hits.Add(new Vector2Int(Now.Day, amount));
            StartCoroutine(HitFlash());
            var fx = FeedbackFX.Instance;
            if (fx != null)
            {
                fx.Impact(Center, new Color(1f, 0.9f, 0.6f), phase.size * 0.6f);
                fx.ButterflyBurst(Center, 1, phase.size * 0.3f);
                fx.HitStop(0.08f);
            }
            if (Context.Hud != null) Context.Hud.Shake(0.15f, 0.2f);
            if (Health > 0)
            {
                if (Enraged && !enrageAnnounced)
                {
                    enrageAnnounced = true;
                    if (Context.Hud != null) Context.Hud.ShowMessage($"{displayName} está furioso: ataca mais rápido!", 2.2f);
                    if (fx != null) fx.Evolve(Center, new Color(1f, 0.3f, 0.2f), phase.size * 0.7f);
                }
                return;
            }

            Context.World.SetFlag(deathFlag, Now.Day);
            if (Context.Hud != null)
            {
                Context.Hud.ShowMessage($"{displayName} foi derrotado no dia {Now.DisplayDay}!", 3.5f);
                Context.Hud.Shake(0.6f, 0.5f);
            }
            if (fx != null)
            {
                fx.EnemyDeath(Center, phase.color, phase.size);
                fx.ButterflyBurst(Center, 14, phase.size * 0.5f);
                fx.SlowMotion(0.3f, 1.1f);
            }
        }

        private IEnumerator HitFlash()
        {
            if (visual == null) yield break;
            visual.color = Color.white;
            var baseScale = visual.transform.localScale;
            visual.transform.localScale = new Vector3(baseScale.x * 1.12f, baseScale.y * 0.9f, 1f);
            yield return new WaitForSeconds(0.08f);
            visual.color = new Color(1f, 0.45f, 0.45f);
            visual.transform.localScale = baseScale;
            yield return new WaitForSeconds(0.08f);
            if (phase != null) visual.color = frozen ? new Color(0.55f, 0.8f, 1f) : phase.color;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            float y = transform.position.y;
            Gizmos.DrawLine(new Vector3(arenaLeft, y, 0f), new Vector3(arenaRight, y, 0f));
        }
    }
}
