using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    public enum EnemyMovement
    {
        Parado,
        Patrulha,
        Persegue,
        Pula,
        Voa
    }

    [Serializable]
    public class EnemyStage
    {
        [Tooltip("Nome do estágio. Ex: Filhote, Adulto, Ancião, Morto de velhice, Congelado.")]
        public string name = "Estágio";
        [Tooltip("Condições (dias, estação, flags). O último estágio válido da lista vence.")]
        public List<TemporalCondition> conditions = new List<TemporalCondition>();
        [Tooltip("Mensagem mostrada quando o jogador chega a um dia em que a criatura está neste estágio (opcional).")]
        public string message = "";

        [Header("Existência")]
        [Tooltip("Desmarque para a criatura não existir neste estágio (morreu de velhice, migrou, hibernou...).")]
        public bool present = true;
        [Tooltip("Quando não existe: mostra os restos (ossos, casca, galho seco).")]
        public bool showRemains;

        [Header("Corpo")]
        [Min(0.1f)] public float size = 1f;
        public Color color = new Color(0.9f, 0.25f, 0.25f);
        [Tooltip("Opcional: troca o sprite neste estágio.")]
        public Sprite sprite;
        [Tooltip("Animação (nome do clip no SpriteAnimator) usada neste estágio. Ex: Walk, Idle, Fly, Hide.")]
        public string animation = "";
        [Tooltip("Animação tocada uma vez ao atirar (opcional).")]
        public string attackAnimation = "";

        [Header("Comportamento")]
        public EnemyMovement movement = EnemyMovement.Patrulha;
        public float speed = 2f;
        public int damage = 1;
        [Min(1)] public int health = 1;
        [Tooltip("Não causa dano ao encostar (ex: casulo, broto).")]
        public bool harmless;
        [Tooltip("Vira uma plataforma sólida e inofensiva (ex: slime congelado no inverno).")]
        public bool solidPlatform;
        [Tooltip("Pesado: quebra plataformas frágeis.")]
        public bool heavy;
        [Tooltip("Pode ser derrotado pulando em cima.")]
        public bool stompable = true;
        [Tooltip("Segundos entre disparos. 0 = não atira.")]
        public float shootInterval;
        public float projectileSpeed = 6f;
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class TemporalEnemy : TemporalBehaviour
    {
        [Header("Estágios no tempo")]
        [SerializeField] private string displayName = "Inimigo";
        [SerializeField] private List<EnemyStage> stages = new List<EnemyStage>();

        [Header("Vida no tempo")]
        [Tooltip("Derrotado num dia = continua morto nesse dia e em todos os seguintes (mas vivo nos anteriores).")]
        [SerializeField] private bool persistentDeath = true;
        [Tooltip("Opcional. Nome da flag de morte (útil para consequências). Vazio = gerado automaticamente.")]
        [SerializeField] private string deathFlag = "";
        [Tooltip("Dia em que a criatura nasce. Antes dele ela não existe; os estágios contam a idade a partir daqui.")]
        [Min(0)] [SerializeField] private int birthDay = 0;
        [Tooltip("Opcional. Flag de morte da mãe/pai: se estava morto no dia do nascimento, esta criatura nunca nasce.")]
        [SerializeField] private string parentDeathFlag = "";

        [Header("Domesticar")]
        [Tooltip("Opcional. Se esta flag estava ativa há 'Tame After Days' dias, a criatura fica mansa (não ataca).")]
        [SerializeField] private string tameFlag = "";
        [SerializeField] private int tameAfterDays = 20;
        [Tooltip("Mansa, ela para e vira uma plataforma onde o Eco pode subir.")]
        [SerializeField] private bool tamePlatform;

        [Header("Movimento")]
        [SerializeField] private float patrolLeft = 3f;
        [SerializeField] private float patrolRight = 3f;
        [SerializeField] private float chaseRange = 6f;
        [SerializeField] private float hopForce = 9f;
        [SerializeField] private float hopInterval = 1.1f;
        [Tooltip("Altura em que voa acima do ponto inicial.")]
        [SerializeField] private float flyHeight = 2f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Referências")]
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private Collider2D hurtbox;
        [Tooltip("Collider no layer Default usado quando o estágio vira plataforma.")]
        [SerializeField] private Collider2D platformCollider;
        [Tooltip("Objeto mostrado quando a criatura não existe mais (restos).")]
        [SerializeField] private GameObject remains;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private SpriteAnimator animator;
        [Tooltip("Marque se o desenho original olha para a esquerda.")]
        [SerializeField] private bool spriteFacesLeft;

        private readonly RaycastHit2D[] rayHits = new RaycastHit2D[4];
        private Rigidbody2D body;
        private float baseGravity;
        private Vector2 spawn;
        private Sprite baseSprite;
        private EnemyStage stage;
        private int stageIndex = -1;
        private int direction = 1;
        private int lastDay = int.MinValue;
        private bool alive = true;
        private bool defeated;
        private float hitStun;
        private float turnCooldown;
        private float actionTimer;
        private float shootTimer;
        private float flyPhase;
        private bool frozen;
        private bool tame;
        private string deathKey;
        private readonly List<Vector2Int> hits = new List<Vector2Int>();
        private bool shownOnce;
        private bool shownDead;
        private bool restoreExplained;
        private SpriteRenderer futureMark;
        private SpriteRenderer[] remainsRenderers;
        private Color[] remainsColors;
        private GameObject remainsGrowth;

        public bool IsHeavy => alive && stage != null && stage.present && stage.heavy && !stage.solidPlatform;
        public bool IsAlive => alive;
        public bool IsTame => tame;
        public string DeathKey => deathKey;
        public string DisplayName => displayName;
        public string StageName => defeated ? "Derrotado" : stage == null ? "Ainda não nasceu" : stage.name + (tame ? " (mansa)" : "");
        private Color StageColor => stage == null ? Color.white : tame ? Color.Lerp(stage.color, new Color(0.55f, 1f, 0.6f), 0.55f) : stage.color;
        private bool ActsAsPlatform => stage != null && (stage.solidPlatform || (tame && tamePlatform));
        private int Health => stage != null ? stage.health - DamageUntil(Now.Day) : 0;

        public void MarkSummoned() => persistentDeath = false;

        private int DamageUntil(int day)
        {
            int total = 0;
            foreach (var h in hits)
            {
                if (h.x <= day) total += h.y;
            }
            return total;
        }

        private bool Unborn()
        {
            if (Now.Day < birthDay) return true;
            return !string.IsNullOrEmpty(parentDeathFlag) && Context.World.IsActive(parentDeathFlag, birthDay);
        }

        public void Setup(string enemyName, SpriteRenderer sprite, Collider2D solid, Collider2D hurt, Collider2D platform, GameObject remainsObject, EnemyProjectile projectile)
        {
            displayName = enemyName;
            visual = sprite;
            bodyCollider = solid;
            hurtbox = hurt;
            platformCollider = platform;
            remains = remainsObject;
            projectilePrefab = projectile;
            stages = new List<EnemyStage>();
        }

        public EnemyStage AddStage(EnemyStage s)
        {
            stages.Add(s);
            return s;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.freezeRotation = true;
            baseGravity = body.gravityScale;
            spawn = transform.position;
            if (visual != null) baseSprite = visual.sprite;
            deathKey = !string.IsNullOrEmpty(deathFlag) ? deathFlag : $"Morte_{gameObject.name}_{Mathf.RoundToInt(spawn.x * 10f)}_{Mathf.RoundToInt(spawn.y * 10f)}";
        }

        protected override void Refresh(bool instant)
        {
            if (stages.Count == 0) return;
            bool timeChanged = Now.Day != lastDay;
            lastDay = Now.Day;
            if (timeChanged) defeated = false;

            if (Unborn())
            {
                stage = null;
                stageIndex = -1;
                SetPresence(false, false, false);
                if (futureMark != null) futureMark.gameObject.SetActive(false);
                return;
            }
            bool deadNow = persistentDeath && Context.World.IsActive(deathKey, Now.Day);
            ShowTimelineChange(deadNow, instant, timeChanged);
            UpdateFutureMark();
            if (deadNow)
            {
                defeated = true;
                SetPresence(false, remains != null, true);
                SettleRemains();
                AgeRemains();
                return;
            }
            if (defeated) return;

            int index = 0;
            for (int i = 0; i < stages.Count; i++)
            {
                if (CheckFrom(stages[i].conditions, birthDay)) index = i;
            }

            bool wasTame = tame;
            tame = !string.IsNullOrEmpty(tameFlag) && Context.World.IsActive(tameFlag, Now.Day - tameAfterDays);
            bool stageChanged = index != stageIndex || tame != wasTame;
            if (!timeChanged && !stageChanged && alive) return;

            stageIndex = index;
            stage = stages[index];
            ApplyStage(timeChanged);

            if (instant || !stageChanged) return;
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Evolve(transform.position + Vector3.up * stage.size * 0.5f, StageColor, stage.size);
            ShowStageMessage();
        }

        private Vector3 Center => transform.position + Vector3.up * (stage != null ? stage.size : 1f) * 0.5f;

        private bool NearPlayer()
        {
            var player = Context.Player;
            return player != null && Vector2.Distance(player.transform.position, transform.position) < 20f;
        }

        private void ShowTimelineChange(bool deadNow, bool instant, bool timeChanged)
        {
            bool wasDead = shownDead;
            bool first = !shownOnce;
            shownOnce = true;
            shownDead = deadNow;
            if (first || instant || !timeChanged || wasDead == deadNow || !NearPlayer()) return;
            var fx = FeedbackFX.Instance;
            if (deadNow)
            {
                if (fx != null)
                {
                    fx.TimeErase(transform.position + Vector3.up * 0.5f, 1.2f);
                    fx.ButterflyBurst(transform.position + Vector3.up * 0.4f, 2, 0.3f);
                }
                GameAudio.Play(Sfx.Erase);
                return;
            }
            if (fx != null) fx.TimeRestore(spawn + Vector2.up * 0.5f, 1.2f);
            GameAudio.Play(Sfx.Restore);
            StartCoroutine(FadeIn());
            if (restoreExplained) return;
            restoreExplained = true;
            Context.World.TryGetFlagDay(deathKey, out int d);
            var hud = Context.Hud;
            if (hud != null) hud.ShowMessage($"{displayName} voltou a existir: antes do dia {d + 1} ele ainda está vivo. O relógio sobre ele marca o dia em que você o derrota.", 3.2f);
        }

        private IEnumerator FadeIn()
        {
            if (visual == null) yield break;
            for (float t = 0f; t < 0.6f; t += UnityEngine.Time.deltaTime)
            {
                if (stage == null) yield break;
                var c = StageColor;
                visual.color = new Color(Mathf.Lerp(0.6f, c.r, t / 0.6f), Mathf.Lerp(0.55f, c.g, t / 0.6f), Mathf.Lerp(1f, c.b, t / 0.6f), t / 0.6f);
                yield return null;
            }
            if (stage != null) visual.color = frozen ? new Color(0.55f, 0.8f, 1f) : StageColor;
        }

        private void UpdateFutureMark()
        {
            bool show = persistentDeath && Context.World.TryGetFlagDay(deathKey, out int d) && Now.Day < d;
            if (!show && futureMark == null) return;
            if (futureMark == null)
            {
                var fx = FeedbackFX.Instance;
                if (fx == null || fx.Library.timeClock == null) return;
                var go = new GameObject("Marca do Tempo");
                go.transform.SetParent(transform, false);
                futureMark = go.AddComponent<SpriteRenderer>();
                futureMark.sprite = fx.Library.timeClock;
                futureMark.sortingOrder = 30;
                if (visual != null) futureMark.sharedMaterial = visual.sharedMaterial;
            }
            futureMark.gameObject.SetActive(show);
        }

        private void AgeRemains()
        {
            if (remains == null || !Context.World.TryGetFlagDay(deathKey, out int d)) return;
            if (remainsRenderers == null)
            {
                var growth = remains.transform.Find("Vida Nova");
                remainsGrowth = growth != null ? growth.gameObject : null;
                var list = new List<SpriteRenderer>();
                foreach (var r in remains.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (remainsGrowth == null || !r.transform.IsChildOf(remainsGrowth.transform)) list.Add(r);
                }
                remainsRenderers = list.ToArray();
                remainsColors = new Color[remainsRenderers.Length];
                for (int i = 0; i < remainsRenderers.Length; i++) remainsColors[i] = remainsRenderers[i].color;
            }
            int age = Now.Day - d;
            float fade = Mathf.Clamp01(age / 40f);
            for (int i = 0; i < remainsRenderers.Length; i++)
            {
                var c = Color.Lerp(remainsColors[i], new Color(0.45f, 0.42f, 0.4f, remainsColors[i].a), fade * 0.7f);
                c.a = remainsColors[i].a * Mathf.Lerp(1f, 0.35f, fade);
                remainsRenderers[i].color = c;
            }
            if (remainsGrowth != null) remainsGrowth.SetActive(age >= 15);
        }

        private void LateUpdate()
        {
            if (futureMark == null || !futureMark.gameObject.activeSelf) return;
            float inv = 1f / Mathf.Max(0.01f, transform.localScale.x);
            float pulse = 1f + Mathf.Sin(UnityEngine.Time.time * 4f) * 0.08f;
            futureMark.transform.localScale = new Vector3(0.75f * inv * pulse, 0.75f * inv * pulse, 1f);
            futureMark.transform.localPosition = new Vector3(0f, 1.15f + 0.4f / Mathf.Max(0.01f, transform.localScale.y), 0f);
            futureMark.transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Time.time * -40f);
            futureMark.color = new Color(1f, 1f, 1f, 0.65f + Mathf.Sin(UnityEngine.Time.time * 4f) * 0.25f);
        }

        private void ApplyStage(bool resetPosition)
        {
            transform.localScale = new Vector3(stage.size, stage.size, 1f);
            if (visual != null)
            {
                visual.color = StageColor;
                if (stage.sprite != null || animator == null) visual.sprite = stage.sprite != null ? stage.sprite : baseSprite;
            }
            if (animator != null && !string.IsNullOrEmpty(stage.animation)) animator.Play(stage.animation, true);

            if (resetPosition)
            {
                body.bodyType = RigidbodyType2D.Dynamic;
                body.position = spawn;
                transform.position = spawn;
                body.linearVelocity = Vector2.zero;
                direction = 1;
                hitStun = 0f;
                actionTimer = hopInterval * 0.5f;
                shootTimer = stage.shootInterval;
                flyPhase = 0f;
            }

            bool flying = stage.movement == EnemyMovement.Voa && stage.present;
            body.gravityScale = flying ? 0f : baseGravity;
            body.bodyType = ActsAsPlatform && stage.present ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            if (ActsAsPlatform) body.linearVelocity = Vector2.zero;

            SetPresence(stage.present, !stage.present && stage.showRemains, true);
        }

        private void SetPresence(bool present, bool showRemains, bool keepBody)
        {
            alive = present;
            if (visual != null)
            {
                foreach (var r in visual.GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = present;
            }
            bool platform = present && ActsAsPlatform;
            bool dangerous = present && stage != null && !stage.harmless && !ActsAsPlatform && !tame;
            if (bodyCollider != null) bodyCollider.enabled = present || showRemains;
            if (hurtbox != null) hurtbox.enabled = dangerous;
            if (platformCollider != null) platformCollider.enabled = platform;
            if (remains != null) remains.SetActive(showRemains);
            body.simulated = keepBody && (present || showRemains);
        }

        private void ShowStageMessage()
        {
            if (string.IsNullOrEmpty(stage.message)) return;
            var hud = Context.Hud;
            var player = Context.Player;
            if (hud == null || player == null) return;
            if (Vector2.Distance(player.transform.position, spawn) < 18f) hud.ShowMessage(stage.message);
        }

        private void UpdateStasis()
        {
            bool shouldFreeze = TimeStasis.Active && alive && stage != null && !ActsAsPlatform;
            if (shouldFreeze == frozen) return;
            frozen = shouldFreeze;
            body.constraints = frozen ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;
            if (visual != null && stage != null) visual.color = frozen ? new Color(0.55f, 0.8f, 1f) : StageColor;
        }

        private void FixedUpdate()
        {
            UpdateStasis();
            if (frozen) return;
            if (!alive || stage == null || Context == null || ActsAsPlatform) return;

            float dt = UnityEngine.Time.fixedDeltaTime;
            hitStun -= dt;
            turnCooldown -= dt;
            actionTimer -= dt;
            if (!tame) UpdateShooting(dt);
            if (hitStun > 0f) return;
            if (tame)
            {
                Patrol();
                body.linearVelocity = new Vector2(body.linearVelocity.x * 0.5f, body.linearVelocity.y);
                FaceDirection();
                return;
            }

            switch (stage.movement)
            {
                case EnemyMovement.Parado:
                    body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                    break;
                case EnemyMovement.Patrulha:
                    Patrol();
                    break;
                case EnemyMovement.Persegue:
                    if (!Chase()) Patrol();
                    break;
                case EnemyMovement.Pula:
                    Hop();
                    break;
                case EnemyMovement.Voa:
                    Fly(dt);
                    break;
            }
            FaceDirection();
        }

        private bool PlayerInRange(float range, out Vector2 delta)
        {
            delta = Vector2.zero;
            var player = Context.Player;
            if (player == null) return false;
            delta = player.transform.position - transform.position;
            return Mathf.Abs(delta.x) < range && Mathf.Abs(delta.y) < range * 0.6f;
        }

        private void Patrol()
        {
            float x = body.position.x;
            if (x > spawn.x + patrolRight) direction = -1;
            else if (x < spawn.x - patrolLeft) direction = 1;
            else if (turnCooldown <= 0f && (WallAhead() || LedgeAhead()))
            {
                direction = -direction;
                turnCooldown = 0.3f;
            }
            body.linearVelocity = new Vector2(direction * stage.speed, body.linearVelocity.y);
        }

        private bool Chase()
        {
            if (!PlayerInRange(chaseRange, out var delta)) return false;
            direction = delta.x >= 0f ? 1 : -1;
            float vx = LedgeAhead() ? 0f : direction * stage.speed;
            body.linearVelocity = new Vector2(vx, body.linearVelocity.y);
            return true;
        }

        private void Hop()
        {
            if (!Grounded()) return;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            if (actionTimer > 0f) return;

            if (PlayerInRange(chaseRange, out var delta)) direction = delta.x >= 0f ? 1 : -1;
            else if (body.position.x > spawn.x + patrolRight) direction = -1;
            else if (body.position.x < spawn.x - patrolLeft) direction = 1;
            if (LedgeAhead()) direction = -direction;

            body.linearVelocity = new Vector2(direction * stage.speed, hopForce);
            actionTimer = hopInterval;
            PlayAttack();
        }

        private void Fly(float dt)
        {
            flyPhase += dt;
            Vector2 target;
            if (PlayerInRange(chaseRange, out _)) target = (Vector2)Context.Player.transform.position + Vector2.up * 0.4f;
            else target = spawn + new Vector2(Mathf.Sin(flyPhase * 0.8f) * Mathf.Max(patrolLeft, patrolRight), flyHeight + Mathf.Sin(flyPhase * 2.1f) * 0.6f);

            Vector2 toTarget = target - body.position;
            direction = toTarget.x >= 0f ? 1 : -1;
            Vector2 desired = toTarget.sqrMagnitude > 0.01f ? toTarget.normalized * stage.speed : Vector2.zero;
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, desired, stage.speed * 4f * dt);
        }

        private void UpdateShooting(float dt)
        {
            if (stage.shootInterval <= 0f || projectilePrefab == null) return;
            shootTimer -= dt;
            if (shootTimer > 0f) return;
            if (!PlayerInRange(chaseRange * 1.6f, out _)) return;

            shootTimer = stage.shootInterval;
            Vector2 origin = (Vector2)transform.position + Vector2.up * stage.size * 0.7f;
            Vector2 aim = ((Vector2)Context.Player.transform.position + Vector2.up * 0.3f - origin).normalized;
            var shot = Instantiate(projectilePrefab, origin, Quaternion.identity);
            shot.Launch(aim * stage.projectileSpeed, stage.damage, Now.Day);
            PlayAttack();
            direction = aim.x >= 0f ? 1 : -1;
        }

        private void FaceDirection()
        {
            if (visual == null) return;
            var s = visual.transform.localScale;
            s.x = Mathf.Abs(s.x) * direction * (spriteFacesLeft ? -1 : 1);
            visual.transform.localScale = s;
        }

        private bool Grounded()
        {
            Vector2 origin = (Vector2)transform.position + Vector2.up * 0.05f;
            return CastSolid(origin, Vector2.down, 0.15f);
        }

        private bool WallAhead()
        {
            float half = stage.size * 0.5f;
            Vector2 origin = (Vector2)transform.position + Vector2.up * half;
            return CastSolid(origin, Vector2.right * direction, half + 0.1f);
        }

        private bool LedgeAhead()
        {
            float half = stage.size * 0.5f;
            Vector2 origin = (Vector2)transform.position + new Vector2(direction * (half + 0.1f), 0.2f);
            return !CastSolid(origin, Vector2.down, 0.6f);
        }

        private void SettleRemains()
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            if (bodyCollider != null) bodyCollider.enabled = false;
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = groundMask };
            int count = Physics2D.Raycast(body.position + Vector2.up * 0.3f, Vector2.down, filter, rayHits, 30f);
            float best = float.MaxValue;
            Vector2 point = body.position;
            for (int i = 0; i < count; i++)
            {
                if (rayHits[i].rigidbody == body || rayHits[i].distance >= best) continue;
                best = rayHits[i].distance;
                point = rayHits[i].point;
            }
            body.bodyType = RigidbodyType2D.Kinematic;
            body.position = point;
            transform.position = point;
        }

        private bool CastSolid(Vector2 origin, Vector2 dir, float distance)
        {
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = groundMask };
            int count = Physics2D.Raycast(origin, dir, filter, rayHits, distance);
            for (int i = 0; i < count; i++)
            {
                if (rayHits[i].rigidbody != body) return true;
            }
            return false;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!alive || stage == null || stage.harmless || ActsAsPlatform || frozen || tame) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            float playerBottom = player.BodyCollider != null ? player.BodyCollider.bounds.min.y : player.transform.position.y;
            float top = transform.position.y + stage.size * 0.6f;
            if (stage.stompable && player.Velocity.y < -0.1f && playerBottom > top)
            {
                var combat = player.GetComponent<PlayerCombat>();
                player.Bounce(combat != null ? combat.StompBounce : 12f);
                GameAudio.Play(Sfx.Stomp);
                TakeDamage(1, player.transform.position);
                return;
            }

            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(stage.damage, transform.position);
        }

        public void TakeDamage(int amount, Vector2 source)
        {
            if (!alive || stage == null || stage.solidPlatform || tame) return;
            hits.Add(new Vector2Int(Now.Day, amount));
            float dir = Mathf.Sign(transform.position.x - source.x);
            if (!frozen) body.linearVelocity = new Vector2(dir * 4f / stage.size, stage.movement == EnemyMovement.Voa ? 0f : 3f);
            hitStun = 0.25f;
            StartCoroutine(HitFlash());
            if (Health <= 0) Die();
            else PlayClipOnce(stage.animation == "Fly" ? "FlyHurt" : "Hurt");
        }

        private void PlayAttack()
        {
            if (stage == null) return;
            if (!string.IsNullOrEmpty(stage.attackAnimation)) PlayClipOnce(stage.attackAnimation);
            else PlayClipOnce(stage.animation == "Fly" ? "FlyAttack" : "Attack");
        }

        private void PlayClipOnce(string clip)
        {
            if (animator == null || stage == null || !animator.Has(clip)) return;
            animator.PlayOnce(clip, stage.animation);
        }

        private void Die()
        {
            body.linearVelocity = Vector2.zero;
            hitStun = 0f;
            GameAudio.Play(Sfx.EnemyDie);
            var fx = FeedbackFX.Instance;
            if (fx != null)
            {
                fx.Dissolve(visual);
                fx.EnemyDeath(Center, StageColor, stage.size);
                fx.HitStop(0.09f);
            }
            var hudFx = Context.Hud;
            if (hudFx != null) hudFx.Shake(0.18f, 0.2f);
            shownDead = persistentDeath;

            defeated = true;
            if (persistentDeath)
            {
                var hud = Context.Hud;
                if (hud != null) hud.ShowMessage($"{displayName} derrotado no dia {Now.DisplayDay}: não existirá mais nos dias seguintes.", 2.2f);
                Context.World.SetFlag(deathKey, Now.Day);
                return;
            }
            SetPresence(false, false, false);
        }

        private IEnumerator HitFlash()
        {
            if (visual == null) yield break;
            visual.color = new Color(1f, 0.35f, 0.35f);
            yield return new WaitForSeconds(0.1f);
            if (stage != null) visual.color = frozen ? new Color(0.55f, 0.8f, 1f) : StageColor;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = Application.isPlaying ? (Vector3)spawn : transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin + Vector3.left * patrolLeft, origin + Vector3.right * patrolRight);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(origin, chaseRange);
        }
    }
}
