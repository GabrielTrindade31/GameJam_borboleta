using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    [DefaultExecutionOrder(-80)]
    public class TimeManager : MonoBehaviour
    {
        [Header("Linha do tempo da fase (em dias)")]
        [Tooltip("A fase sempre começa no dia 1 (dia 0 interno). Nunca é possível voltar para antes dele.")]
        [Min(1)] [SerializeField] private int maxDay = 60;
        [Tooltip("Quantos dias cada salto no tempo avança ou retrocede.")]
        [Min(1)] [SerializeField] private int dayStep = 10;
        [Tooltip("Opcional. Se preenchido, usa exatamente estes dias (contados a partir de 0). O menor vira o início.")]
        [SerializeField] private List<int> customDays = new List<int>();
        [Tooltip("Em qual período a fase começa (0 = o primeiro). Use o último para fases que começam no futuro e exigem voltar no tempo.")]
        [Min(0)] [SerializeField] private int startIndex = 0;

        [Header("Calendário")]
        [SerializeField] private SeasonCalendar calendar = new SeasonCalendar();

        [Header("Regras")]
        [Tooltip("Tempo mínimo em segundos entre dois saltos temporais.")]
        [SerializeField] private float changeCooldown = 0.35f;
        [Tooltip("Collider usado para impedir que o jogador apareça dentro de algo sólido após mudar de dia.")]
        [SerializeField] private Collider2D occupantCheck;
        [SerializeField] private LayerMask solidMask = ~0;

        private readonly List<int> days = new List<int>();
        private readonly Collider2D[] overlapBuffer = new Collider2D[16];
        private int index;
        private float nextChangeTime;
        private int peekOrigin;

        public event Action<TimeState> BeforeTimeChange;
        public event Action<TimeState> TimeChanged;
        public event Action<int> TimeChangeBlocked;
        public event Action<bool> PeekChanged;

        public TimeState State { get; private set; }
        public IReadOnlyList<int> Days => days;
        public SeasonCalendar Calendar => calendar;
        public int StartDay => days.Count > 0 ? days[0] : 0;
        public int CurrentDay => State.Day;
        public bool IsPeeking { get; private set; }
        public int PeekOriginDay => days.Count > 0 ? days[peekOrigin] : 0;

        public void SetOccupant(Collider2D occupant) => occupantCheck = occupant;

        private void Awake()
        {
            BuildDays();
            index = Mathf.Clamp(startIndex, 0, days.Count - 1);
            State = MakeState(index, days[index]);
        }

        private void Start()
        {
            TimeChanged?.Invoke(State);
        }

        private void BuildDays()
        {
            days.Clear();
            if (customDays != null && customDays.Count > 0)
            {
                foreach (int d in customDays)
                {
                    if (d >= 0 && !days.Contains(d)) days.Add(d);
                }
                days.Sort();
            }
            if (days.Count > 0) return;

            for (int d = 0; d <= maxDay; d += dayStep) days.Add(d);
            if (days[days.Count - 1] != maxDay) days.Add(maxDay);
        }

        private TimeState MakeState(int newIndex, int previousDay)
        {
            return new TimeState(days[newIndex], previousDay, newIndex, days.Count, days[days.Count - 1], calendar);
        }

        public bool BeginPeek(int direction)
        {
            int target = index + direction;
            if (IsPeeking || target < 0 || target >= days.Count)
            {
                TimeChangeBlocked?.Invoke(direction);
                return false;
            }
            BeforeTimeChange?.Invoke(State);
            peekOrigin = index;
            IsPeeking = true;
            PeekChanged?.Invoke(true);
            ApplyIndex(target, State.Day);
            return true;
        }

        public bool PeekStep(int direction)
        {
            int target = index + direction;
            if (!IsPeeking || target < 0 || target >= days.Count)
            {
                TimeChangeBlocked?.Invoke(direction);
                return false;
            }
            ApplyIndex(target, State.Day);
            return true;
        }

        public void EndPeek()
        {
            if (!IsPeeking) return;
            IsPeeking = false;
            if (index != peekOrigin) ApplyIndex(peekOrigin, State.Day);
            PeekChanged?.Invoke(false);
            nextChangeTime = UnityEngine.Time.time + changeCooldown;
        }

        public bool StepForward() => TryChange(index + 1, 1);
        public bool StepBack() => TryChange(index - 1, -1);
        public bool JumpToIndex(int target) => TryChange(target, target >= index ? 1 : -1, true);

        public void Broadcast() => TimeChanged?.Invoke(State);

        private bool TryChange(int target, int direction, bool ignoreCooldown = false)
        {
            if (target < 0 || target >= days.Count || target == index)
            {
                TimeChangeBlocked?.Invoke(direction);
                return false;
            }
            if (IsPeeking) return false;
            if (!ignoreCooldown && UnityEngine.Time.time < nextChangeTime) return false;

            BeforeTimeChange?.Invoke(State);

            int oldIndex = index;
            ApplyIndex(target, State.Day);

            if (IsOccupantBlocked())
            {
                ApplyIndex(oldIndex, State.Day);
                TimeChangeBlocked?.Invoke(direction);
                return false;
            }

            nextChangeTime = UnityEngine.Time.time + changeCooldown;
            return true;
        }

        private void ApplyIndex(int newIndex, int previousDay)
        {
            index = newIndex;
            State = MakeState(newIndex, previousDay);
            TimeChanged?.Invoke(State);
            Physics2D.SyncTransforms();
        }

        private bool IsOccupantBlocked()
        {
            if (occupantCheck == null || !occupantCheck.enabled) return false;

            Bounds b = occupantCheck.bounds;
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = solidMask };
            int count = Physics2D.OverlapBox(b.center, b.size * 0.8f, 0f, filter, overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = overlapBuffer[i];
                if (hit == occupantCheck || hit.attachedRigidbody == occupantCheck.attachedRigidbody) continue;
                if (hit.usedByEffector) continue;
                return true;
            }
            return false;
        }
    }
}
