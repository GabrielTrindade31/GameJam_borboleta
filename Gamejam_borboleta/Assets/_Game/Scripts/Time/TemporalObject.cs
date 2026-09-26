using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    [Serializable]
    public class TemporalStateData
    {
        [Tooltip("Nome do estado (aparece no debug). Ex: Semente, Broto, Árvore.")]
        public string name = "Estado";
        [Tooltip("Quando TODAS as condições são verdadeiras, este estado pode ser usado. O último estado válido da lista vence.")]
        public List<TemporalCondition> conditions = new List<TemporalCondition>();
        [Tooltip("O sprite principal aparece?")]
        public bool visible = true;
        [Tooltip("Os colliders do objeto ficam ativos (o jogador colide / pisa)?")]
        public bool solid = true;
        [Tooltip("Opcional: troca o sprite do objeto neste estado.")]
        public Sprite sprite;
        public bool overrideColor;
        public Color color = Color.white;
        public bool overrideScale;
        public Vector3 scale = Vector3.one;
        [Tooltip("Objetos filhos que ficam ativos apenas neste estado (ex: copa da árvore, água, ponte).")]
        public List<GameObject> activeObjects = new List<GameObject>();
        [Tooltip("Marque para destacar este estado como uma consequência (brilho verde ao aparecer).")]
        public bool isConsequence;
    }

    public class TemporalObject : TemporalBehaviour
    {
        [Tooltip("Nome amigável usado no debug.")]
        [SerializeField] private string displayName = "Objeto Temporal";
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private List<Collider2D> colliders = new List<Collider2D>();
        [Tooltip("Lista de estados. O primeiro é o padrão. O último estado cujas condições forem verdadeiras é o ativo.")]
        [SerializeField] private List<TemporalStateData> states = new List<TemporalStateData>();
        [SerializeField] private Color consequenceFlash = new Color(0.35f, 1f, 0.45f);
        [SerializeField] private Color changeFlash = new Color(0.8f, 0.6f, 1f);

        private readonly List<SpriteRenderer> flashTargets = new List<SpriteRenderer>();
        private readonly Dictionary<SpriteRenderer, Color> baseColors = new Dictionary<SpriteRenderer, Color>();
        private int currentIndex = -1;
        private Coroutine flashRoutine;

        public string DisplayName => displayName;
        public string CurrentStateName => currentIndex >= 0 && currentIndex < states.Count ? states[currentIndex].name : "-";
        public event Action<TemporalObject, string> StateChanged;

        public void Setup(string objectName, SpriteRenderer spriteRenderer, params Collider2D[] physics)
        {
            displayName = objectName;
            visual = spriteRenderer;
            colliders = new List<Collider2D>(physics);
            states = new List<TemporalStateData>();
        }

        public TemporalStateData AddState(string stateName, params TemporalCondition[] conditions)
        {
            var state = new TemporalStateData { name = stateName, conditions = new List<TemporalCondition>(conditions) };
            if (visual != null) state.color = visual.color;
            states.Add(state);
            return state;
        }

        private void Awake()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            if (colliders.Count == 0)
            {
                foreach (var c in GetComponents<Collider2D>())
                {
                    if (!c.isTrigger) colliders.Add(c);
                }
            }
        }

        protected override void Refresh(bool instant)
        {
            if (states.Count == 0) return;
            int index = Evaluate();
            if (index == currentIndex) return;

            StopFlash();
            bool first = currentIndex < 0;
            currentIndex = index;
            Apply(states[index]);

            if (first || instant) return;
            var state = states[index];
            Color flash = state.isConsequence ? consequenceFlash : changeFlash;
            flashRoutine = StartCoroutine(Flash(flash));
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Burst(GetFocusPoint(), flash, state.isConsequence ? 24 : 10);
            var player = Context.Player;
            if (fx != null && player != null && Vector2.Distance(player.transform.position, GetFocusPoint()) < 18f)
                fx.FloatingLabel(GetFocusPoint() + Vector3.up * 0.6f, state.name, state.isConsequence ? new Color(1f, 0.85f, 0.4f) : new Color(0.8f, 0.9f, 1f));
            StateChanged?.Invoke(this, state.name);
        }

        public void PreviewFirstState()
        {
            if (states.Count == 0) return;
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            Apply(states[0]);
        }

        private int Evaluate()
        {
            int result = 0;
            for (int i = 0; i < states.Count; i++)
            {
                if (Check(states[i].conditions)) result = i;
            }
            return result;
        }

        private void Apply(TemporalStateData state)
        {
            foreach (var s in states)
            {
                foreach (var go in s.activeObjects)
                {
                    if (go != null && !state.activeObjects.Contains(go)) go.SetActive(false);
                }
            }
            foreach (var go in state.activeObjects)
            {
                if (go != null) go.SetActive(true);
            }

            if (visual != null)
            {
                visual.enabled = state.visible;
                if (state.sprite != null) visual.sprite = state.sprite;
                if (state.overrideColor) visual.color = state.color;
            }
            if (state.overrideScale) transform.localScale = state.scale;
            foreach (var c in colliders)
            {
                if (c != null) c.enabled = state.visible && state.solid;
            }
        }

        private Vector3 GetFocusPoint()
        {
            if (visual != null && visual.enabled) return visual.bounds.center;
            foreach (var go in states[currentIndex].activeObjects)
            {
                if (go != null) return go.transform.position;
            }
            return transform.position;
        }

        private void StopFlash()
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = null;
            foreach (var kv in baseColors)
            {
                if (kv.Key != null) kv.Key.color = kv.Value;
            }
            baseColors.Clear();
        }

        private IEnumerator Flash(Color flash)
        {
            flashTargets.Clear();

            if (visual != null && visual.enabled) flashTargets.Add(visual);
            foreach (var go in states[currentIndex].activeObjects)
            {
                if (go != null) flashTargets.AddRange(go.GetComponentsInChildren<SpriteRenderer>());
            }
            foreach (var r in flashTargets) baseColors[r] = r.color;

            const float duration = 0.6f;
            for (float t = 0f; t < duration; t += UnityEngine.Time.deltaTime)
            {
                float k = t / duration;
                foreach (var r in flashTargets)
                {
                    if (r != null) r.color = Color.Lerp(flash, baseColors[r], k);
                }
                yield return null;
            }
            foreach (var kv in baseColors)
            {
                if (kv.Key != null) kv.Key.color = kv.Value;
            }
            baseColors.Clear();
            flashRoutine = null;
        }
    }
}
