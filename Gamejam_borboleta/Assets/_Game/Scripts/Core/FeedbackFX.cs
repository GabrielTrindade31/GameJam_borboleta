using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    [System.Serializable]
    public class FxLibrary
    {
        public Sprite[] slash = new Sprite[0];
        public Sprite[] hitRing = new Sprite[0];
        public Sprite[] sparkle = new Sprite[0];
        public Sprite[] bubbles = new Sprite[0];
        public Sprite[] burst = new Sprite[0];
        public Sprite[] soul = new Sprite[0];
        public Sprite[] butterflies = new Sprite[0];
        public Sprite timeClock;
        public Sprite timeSwirl;
    }

    public class FeedbackFX : MonoBehaviour
    {
        [Tooltip("Sistema de partículas compartilhado usado para explosões de feedback.")]
        [SerializeField] private ParticleSystem burstParticles;
        [SerializeField] private FxLibrary library = new FxLibrary();
        [SerializeField] private Material spriteMaterial;
        [SerializeField] private int sortingOrder = 45;

        private readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();

        public static FeedbackFX Instance { get; private set; }
        public FxLibrary Library => library;

        public void SetParticles(ParticleSystem particles) => burstParticles = particles;
        public void SetLibrary(FxLibrary lib, Material material)
        {
            library = lib;
            spriteMaterial = material;
        }

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void HitStop(float duration)
        {
            StopCoroutine(nameof(HitStopRoutine));
            StartCoroutine(nameof(HitStopRoutine), duration);
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            if (PauseMenu.IsPaused) yield break;
            UnityEngine.Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            if (!PauseMenu.IsPaused) UnityEngine.Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            if (!PauseMenu.IsPaused) UnityEngine.Time.timeScale = 1f;
        }

        public void Burst(Vector3 position, Color color, int count)
        {
            if (burstParticles == null) return;
            var emit = new ParticleSystem.EmitParams
            {
                position = position,
                startColor = color,
                applyShapeToPosition = true
            };
            burstParticles.Emit(emit, count);
        }

        public void Slash(Transform follow, Vector3 localOffset, int facing, Color color, float scale)
        {
            if (library.slash.Length == 0) return;
            var r = Take();
            r.transform.SetParent(follow, false);
            r.transform.localPosition = localOffset;
            r.transform.localRotation = Quaternion.identity;
            StartCoroutine(Animate(r, library.slash, 36f, color, new Vector3(scale * facing, scale, 1f), 0f, 0f));
        }

        public void Impact(Vector3 position, Color color, float scale = 1f)
        {
            PlayAt(library.hitRing, position, 60f, color, scale, Random.Range(0f, 360f), 0f);
            Burst(position, color, 8);
        }

        public void PollenCollect(Vector3 position)
        {
            var gold = new Color(1f, 0.88f, 0.35f);
            PlayAt(library.bubbles, position, 40f, gold, 1.8f, 0f, 0f);
            PlayAt(library.sparkle, position, 50f, Color.white, 1.4f, 0f, 0f);
            ButterflyBurst(position, 3, 0.2f, 6);
            Burst(position, gold, 24);
        }

        public void EnemyDeath(Vector3 position, Color tint, float size)
        {
            float s = Mathf.Max(1f, size) * 1.3f;
            var burstColor = Color.Lerp(Color.white, tint, 0.35f);
            burstColor.a = 0.7f;
            PlayAt(library.burst, position, 60f, burstColor, s * 0.65f, 0f, 0f);
            ButterflyBurst(position, 3 + Mathf.RoundToInt(size * 2f), 0.4f * s);
            TimeErase(position, size);
            Burst(position, tint, 26);
        }

        public void TimeErase(Vector3 position, float size = 1f)
        {
            if (library.timeClock == null) return;
            var r = Take();
            r.transform.position = position;
            StartCoroutine(Clock(r, library.timeClock, Mathf.Max(1f, size), true));
        }

        public void TimeRestore(Vector3 position, float size = 1f)
        {
            if (library.timeSwirl != null)
            {
                var r = Take();
                r.transform.position = position;
                StartCoroutine(Clock(r, library.timeSwirl, Mathf.Max(1f, size), false));
            }
            Burst(position, new Color(0.7f, 0.6f, 1f), 18);
        }

        public void Evolve(Vector3 position, Color color, float size)
        {
            if (library.timeSwirl != null)
            {
                var r = Take();
                r.transform.position = position;
                StartCoroutine(Clock(r, library.timeSwirl, Mathf.Max(1f, size) * 0.8f, true));
            }
            Burst(position, color, 14);
        }

        public void ButterflyBurst(Vector3 position, int count, float spread, int onlyIndex = -1)
        {
            if (library.butterflies.Length == 0) return;
            for (int i = 0; i < count; i++)
            {
                var r = Take();
                int index = onlyIndex >= 0 && onlyIndex < library.butterflies.Length ? onlyIndex : Random.Range(0, library.butterflies.Length);
                r.sprite = library.butterflies[index];
                r.transform.position = position + (Vector3)(Random.insideUnitCircle * spread);
                StartCoroutine(Flutter(r, Random.Range(2.2f, 3.4f), i * 0.06f));
            }
        }

        private IEnumerator Flutter(SpriteRenderer r, float life, float delay)
        {
            r.color = new Color(1f, 1f, 1f, 0f);
            r.transform.localScale = Vector3.zero;
            for (float d = 0f; d < delay; d += UnityEngine.Time.unscaledDeltaTime) yield return null;
            float baseScale = Random.Range(0.55f, 0.8f) / Mathf.Max(0.01f, r.sprite.bounds.size.x);
            Vector3 start = r.transform.position;
            float drift = Random.Range(-1.4f, 1.4f);
            float rise = Random.Range(1.1f, 2f);
            float wobble = Random.Range(2f, 3.5f);
            float seed = Random.value * 10f;
            for (float t = 0f; t < life && r != null; t += UnityEngine.Time.unscaledDeltaTime)
            {
                float k = t / life;
                float flap = 0.2f + 0.8f * Mathf.Abs(Mathf.Cos((t + seed) * 16f));
                r.transform.localScale = new Vector3(baseScale * flap, baseScale, 1f);
                r.transform.position = start + new Vector3(drift * t + Mathf.Sin((t + seed) * wobble) * 0.35f, rise * t + Mathf.Sin((t + seed) * wobble * 2f) * 0.12f, 0f);
                r.transform.rotation = Quaternion.Euler(0f, 0f, -drift * 8f + Mathf.Sin((t + seed) * wobble) * 15f);
                float alpha = Mathf.Min(1f, t * 6f) * (k > 0.6f ? 1f - (k - 0.6f) / 0.4f : 1f);
                r.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            Release(r);
        }

        public void Warning(Vector3 position, float duration, float scale, Color color)
        {
            if (library.hitRing.Length == 0) return;
            var r = Take();
            r.transform.position = position;
            StartCoroutine(LoopFrames(r, library.hitRing, 70f, color, scale, duration, null, Vector3.zero));
        }

        public void Attach(Transform follow, Vector3 offset, float duration, float scale, Color color)
        {
            if (library.sparkle.Length == 0 || follow == null) return;
            var r = Take();
            r.transform.position = follow.position + offset;
            StartCoroutine(LoopFrames(r, library.sparkle, 24f, color, scale, duration, follow, offset));
        }

        public void SlowMotion(float scale, float duration)
        {
            StopCoroutine(nameof(HitStopRoutine));
            StartCoroutine(SlowRoutine(scale, duration));
        }

        private IEnumerator SlowRoutine(float scale, float duration)
        {
            if (PauseMenu.IsPaused) yield break;
            UnityEngine.Time.timeScale = scale;
            yield return new WaitForSecondsRealtime(duration);
            if (!PauseMenu.IsPaused) UnityEngine.Time.timeScale = 1f;
        }

        private IEnumerator LoopFrames(SpriteRenderer r, Sprite[] frames, float fps, Color color, float scale, float duration, Transform follow, Vector3 offset)
        {
            r.color = color;
            r.transform.localScale = new Vector3(scale, scale, 1f);
            for (float t = 0f; t < duration && r != null; t += UnityEngine.Time.deltaTime)
            {
                r.sprite = frames[(int)(t * fps) % frames.Length];
                if (follow != null) r.transform.position = follow.position + offset;
                float fade = Mathf.Clamp01((duration - t) * 4f);
                r.color = new Color(color.r, color.g, color.b, color.a * fade);
                yield return null;
            }
            Release(r);
        }

        public void Dissolve(SpriteRenderer source)
        {
            if (source == null || source.sprite == null) return;
            var r = Take();
            r.transform.position = source.transform.position;
            r.transform.rotation = source.transform.rotation;
            r.transform.localScale = source.transform.lossyScale;
            r.sprite = source.sprite;
            r.flipX = source.flipX;
            r.flipY = source.flipY;
            StartCoroutine(DissolveRoutine(r, source.transform.lossyScale));
        }

        private IEnumerator DissolveRoutine(SpriteRenderer r, Vector3 baseScale)
        {
            Vector3 start = r.transform.position;
            const float duration = 0.55f;
            for (float t = 0f; t < duration && r != null; t += UnityEngine.Time.unscaledDeltaTime)
            {
                float k = t / duration;
                float flash = k < 0.15f ? 1f : 0f;
                r.color = Color.Lerp(new Color(0.75f, 0.65f, 1f, 1f - k), new Color(1f, 1f, 1f, 1f), flash);
                r.transform.localScale = new Vector3(baseScale.x * (1f - k * 0.3f), baseScale.y * (1f - k * 0.3f), 1f);
                r.transform.position = start + (Vector3)Random.insideUnitCircle * 0.03f;
                yield return null;
            }
            Release(r);
        }

        private void PlayAt(Sprite[] frames, Vector3 position, float fps, Color color, float scale, float angle, float rise)
        {
            if (frames == null || frames.Length == 0) return;
            var r = Take();
            r.transform.position = position;
            r.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            StartCoroutine(Animate(r, frames, fps, color, new Vector3(scale, scale, 1f), rise, 0f));
        }

        private SpriteRenderer Take()
        {
            foreach (var p in pool)
            {
                if (p != null && !p.gameObject.activeSelf)
                {
                    p.transform.SetParent(transform, false);
                    p.gameObject.SetActive(true);
                    return p;
                }
            }
            var go = new GameObject("FX");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            if (spriteMaterial != null) sr.sharedMaterial = spriteMaterial;
            pool.Add(sr);
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        private void Release(SpriteRenderer r)
        {
            if (r == null) return;
            r.transform.SetParent(transform, false);
            r.transform.localScale = Vector3.one;
            r.transform.rotation = Quaternion.identity;
            r.flipX = false;
            r.flipY = false;
            r.gameObject.SetActive(false);
        }

        private IEnumerator Animate(SpriteRenderer r, Sprite[] frames, float fps, Color color, Vector3 scale, float rise, float delay)
        {
            r.color = color;
            r.transform.localScale = scale;
            float t = 0f;
            float length = frames.Length / fps;
            Vector3 start = r.transform.localPosition;
            while (t < length && r != null)
            {
                int i = Mathf.Min(frames.Length - 1, (int)(t * fps));
                r.sprite = frames[i];
                if (rise != 0f) r.transform.localPosition = start + Vector3.up * rise * (t / length);
                t += UnityEngine.Time.unscaledDeltaTime;
                yield return null;
            }
            Release(r);
        }

        private IEnumerator Clock(SpriteRenderer r, Sprite sprite, float size, bool outward)
        {
            r.sprite = sprite;
            float duration = 0.9f;
            float spin = outward ? -220f : 260f;
            for (float t = 0f; t < duration && r != null; t += UnityEngine.Time.unscaledDeltaTime)
            {
                float k = t / duration;
                float p = outward ? k : 1f - k;
                float scale = size * 1.7f * Mathf.Lerp(0.35f, 1.35f, 1f - (1f - p) * (1f - p));
                float alpha = outward ? Mathf.Sin(Mathf.Min(1f, k * 1.6f) * Mathf.PI * 0.5f) * (1f - k) : Mathf.Sin(k * Mathf.PI);
                r.transform.localScale = new Vector3(scale, scale, 1f);
                r.transform.rotation = Quaternion.Euler(0f, 0f, spin * k);
                r.color = new Color(1f, 1f, 1f, alpha * 1.4f);
                yield return null;
            }
            Release(r);
        }
    }
}
