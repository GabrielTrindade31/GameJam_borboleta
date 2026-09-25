using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    public interface IFlagProvider
    {
        string FlagKey { get; }
        string Description { get; }
        bool IsActiveAt(WorldState world, int day);
    }

    [DefaultExecutionOrder(-90)]
    public class WorldState : MonoBehaviour
    {
        private const int MaxDepth = 32;

        private readonly Dictionary<string, int> flagDays = new Dictionary<string, int>();
        private readonly Dictionary<string, IFlagProvider> providers = new Dictionary<string, IFlagProvider>();
        private readonly Dictionary<string, SortedList<int, Vector2>> timelines = new Dictionary<string, SortedList<int, Vector2>>();
        private readonly HashSet<string> evaluating = new HashSet<string>();
        private int depth;

        public event Action Changed;

        public IEnumerable<string> DirectFlags => flagDays.Keys;
        public IEnumerable<IFlagProvider> Providers => providers.Values;

        public void RegisterProvider(IFlagProvider provider)
        {
            if (provider == null || string.IsNullOrEmpty(provider.FlagKey)) return;
            providers[provider.FlagKey] = provider;
        }

        public void UnregisterProvider(IFlagProvider provider)
        {
            if (provider == null || string.IsNullOrEmpty(provider.FlagKey)) return;
            if (providers.TryGetValue(provider.FlagKey, out var current) && current == provider) providers.Remove(provider.FlagKey);
        }

        public void SetFlag(string key, int day)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (flagDays.TryGetValue(key, out int existing) && existing <= day) return;
            flagDays[key] = day;
            Changed?.Invoke();
        }

        public void ClearFlag(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (flagDays.Remove(key)) Changed?.Invoke();
        }

        public bool TryGetFlagDay(string key, out int day) => flagDays.TryGetValue(key, out day);

        public bool IsActive(string key, int day)
        {
            if (string.IsNullOrEmpty(key)) return false;
            if (flagDays.TryGetValue(key, out int setDay) && day >= setDay) return true;
            if (!providers.TryGetValue(key, out var provider)) return false;

            if (depth >= MaxDepth || evaluating.Contains(key))
            {
                Debug.LogWarning($"[WorldState] Ciclo de consequências detectado em '{key}'.");
                return false;
            }

            depth++;
            evaluating.Add(key);
            bool result = provider.IsActiveAt(this, day);
            evaluating.Remove(key);
            depth--;
            return result;
        }

        public void SetTimelineValue(string id, int day, Vector2 value, bool overwriteFuture)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!timelines.TryGetValue(id, out var line))
            {
                line = new SortedList<int, Vector2>();
                timelines[id] = line;
            }

            if (overwriteFuture)
            {
                for (int i = line.Count - 1; i >= 0; i--)
                {
                    if (line.Keys[i] > day) line.RemoveAt(i);
                }
            }

            line[day] = value;
            Changed?.Invoke();
        }

        public bool TryGetTimelineValue(string id, int day, out Vector2 value)
        {
            value = default;
            if (string.IsNullOrEmpty(id) || !timelines.TryGetValue(id, out var line)) return false;

            bool found = false;
            for (int i = 0; i < line.Count; i++)
            {
                if (line.Keys[i] > day) break;
                value = line.Values[i];
                found = true;
            }
            return found;
        }

        public int TimelineKeyCount(string id) => timelines.TryGetValue(id, out var line) ? line.Count : 0;

        public void NotifyChanged() => Changed?.Invoke();
    }
}
