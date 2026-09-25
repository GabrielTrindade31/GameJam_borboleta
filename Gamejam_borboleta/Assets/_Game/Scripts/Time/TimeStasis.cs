using System;
using UnityEngine;

namespace ButterflyStep
{
    public class TimeStasis : MonoBehaviour
    {
        [Tooltip("Quanto tempo (segundos reais) inimigos e projéteis ficam congelados.")]
        [SerializeField] private float duration = 3f;
        [Tooltip("Tempo para a energia recarregar depois do uso.")]
        [SerializeField] private float rechargeTime = 7f;
        [Tooltip("Desmarque em fases onde a pausa ainda não foi ensinada.")]
        [SerializeField] private bool unlocked = true;

        private float activeUntil;
        private float energy = 1f;

        public static bool Active { get; private set; }
        public float Energy => energy;
        public bool Unlocked => unlocked;
        public float Remaining => Mathf.Max(0f, activeUntil - UnityEngine.Time.time);

        public event Action<bool> StasisChanged;

        public void SetUnlocked(bool value) => unlocked = value;

        private void Awake() => Active = false;

        private void OnDestroy() => Active = false;

        public bool TryActivate()
        {
            if (!unlocked || Active || energy < 1f) return false;
            Active = true;
            energy = 0f;
            activeUntil = UnityEngine.Time.time + duration;
            StasisChanged?.Invoke(true);
            return true;
        }

        private void Update()
        {
            if (Active)
            {
                if (UnityEngine.Time.time >= activeUntil)
                {
                    Active = false;
                    StasisChanged?.Invoke(false);
                }
                return;
            }
            if (energy < 1f) energy = Mathf.Min(1f, energy + UnityEngine.Time.deltaTime / rechargeTime);
        }
    }
}
