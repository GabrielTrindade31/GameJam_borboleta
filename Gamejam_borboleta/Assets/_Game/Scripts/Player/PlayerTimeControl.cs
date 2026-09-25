using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerTimeControl : MonoBehaviour
    {
        [SerializeField] private ParticleSystem timeParticles;

        private PlayerInputReader input;
        private PlayerController controller;
        private PlayerHealth health;
        private TimeManager time;
        private TimeStasis stasis;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            controller = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();
        }

        private void Start()
        {
            var ctx = LevelContext.Current;
            time = ctx.Time;
            stasis = ctx.Stasis;
            time.TimeChanged += OnTimeChanged;
            time.PeekChanged += OnPeekChanged;
        }

        private void OnDestroy()
        {
            if (time == null) return;
            time.TimeChanged -= OnTimeChanged;
            time.PeekChanged -= OnPeekChanged;
        }

        private void Update()
        {
            if (time == null || PauseMenu.IsPaused) return;
            bool dead = health != null && health.IsDead;

            if (time.IsPeeking)
            {
                if (!input.PeekHeld || dead) time.EndPeek();
                else if (input.TimeBackPressed) time.PeekStep(-1);
                else if (input.TimeForwardPressed) time.PeekStep(1);
                return;
            }

            if (dead || (controller != null && !controller.ControlEnabled)) return;

            if (input.PeekHeld)
            {
                if (input.TimeBackPressed) time.BeginPeek(-1);
                else if (input.TimeForwardPressed) time.BeginPeek(1);
                return;
            }

            if (input.TimeBackPressed) time.StepBack();
            else if (input.TimeForwardPressed) time.StepForward();
            else if (input.StasisPressed && stasis != null && stasis.TryActivate()) GameAudio.Play(Sfx.Stasis);
            else if (input.StasisPressed && stasis != null)
            {
                GameAudio.Play(Sfx.Blocked);
                var hud = LevelContext.Current.Hud;
                if (hud != null) hud.ShowMessage(stasis.Unlocked ? "A pausa do tempo ainda está recarregando." : "Eco ainda não sabe pausar o tempo.", 1.5f);
            }
        }

        private void OnPeekChanged(bool peeking)
        {
            if (controller != null) controller.SetGhost(peeking);
        }

        private void OnTimeChanged(TimeState state)
        {
            if (state.Direction != 0 && timeParticles != null) timeParticles.Play();
        }
    }
}
