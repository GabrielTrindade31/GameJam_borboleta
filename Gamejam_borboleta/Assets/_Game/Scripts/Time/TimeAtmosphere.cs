using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ButterflyStep
{
    [Serializable]
    public class SeasonLook
    {
        public Season season;
        public Color sky = Color.cyan;
        public Color light = Color.white;
        [Tooltip("Partículas de clima desta estação (pétalas, pólen, folhas, neve). Opcional.")]
        public ParticleSystem weather;
    }

    public class TimeAtmosphere : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private SeasonLook[] looks = DefaultLooks();
        [Tooltip("Quanto o céu escurece ao longo da fase (0 = nada).")]
        [Range(0f, 0.5f)] [SerializeField] private float progressDarkening = 0.1f;
        [SerializeField] private float blendSpeed = 3f;

        private Color targetSky;
        private Color targetLight;
        private TimeManager time;

        public SeasonLook[] Looks => looks;

        public void Setup(Camera cam, Light2D light)
        {
            targetCamera = cam;
            globalLight = light;
        }

        private void Start()
        {
            time = LevelContext.Current.Time;
            time.TimeChanged += OnTimeChanged;
            Apply(time.State, true);
        }

        private void OnDestroy()
        {
            if (time != null) time.TimeChanged -= OnTimeChanged;
        }

        private void OnTimeChanged(TimeState state) => Apply(state, false);

        private SeasonLook Find(Season season)
        {
            foreach (var l in looks)
            {
                if (l != null && l.season == season) return l;
            }
            return null;
        }

        private void Apply(TimeState state, bool instant)
        {
            var look = Find(state.Season);
            if (look == null) return;
            targetSky = Color.Lerp(look.sky, Color.black, state.Normalized * progressDarkening);
            targetLight = look.light;

            foreach (var l in looks)
            {
                if (l == null || l.weather == null) continue;
                bool on = l == look;
                if (on && !l.weather.isPlaying) l.weather.Play();
                else if (!on && l.weather.isPlaying) l.weather.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (!instant) return;
            if (targetCamera != null) targetCamera.backgroundColor = targetSky;
            if (globalLight != null) globalLight.color = targetLight;
        }

        private void Update()
        {
            float k = UnityEngine.Time.deltaTime * blendSpeed;
            if (targetCamera != null) targetCamera.backgroundColor = Color.Lerp(targetCamera.backgroundColor, targetSky, k);
            if (globalLight != null) globalLight.color = Color.Lerp(globalLight.color, targetLight, k);
        }

        private static SeasonLook[] DefaultLooks()
        {
            return new[]
            {
                new SeasonLook { season = Season.Primavera, sky = new Color(0.62f, 0.82f, 0.9f), light = new Color(1f, 0.98f, 0.95f) },
                new SeasonLook { season = Season.Verao, sky = new Color(0.45f, 0.72f, 0.95f), light = new Color(1f, 0.95f, 0.82f) },
                new SeasonLook { season = Season.Outono, sky = new Color(0.85f, 0.66f, 0.5f), light = new Color(1f, 0.85f, 0.7f) },
                new SeasonLook { season = Season.Inverno, sky = new Color(0.72f, 0.78f, 0.86f), light = new Color(0.82f, 0.88f, 1f) }
            };
        }
    }
}
