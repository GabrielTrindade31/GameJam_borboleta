using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ButterflyStep
{
    [Serializable]
    public class SeasonalSpriteEntry
    {
        public SpriteRenderer renderer;
        [Tooltip("Primavera, Verão, Outono, Inverno.")]
        public Sprite[] sprites = new Sprite[4];
    }

    public class MenuSeasonCycle : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float secondsPerSeason = 6f;
        [SerializeField] private Color[] skyColors =
        {
            new Color(0.62f, 0.82f, 0.9f), new Color(0.45f, 0.72f, 0.95f), new Color(0.85f, 0.66f, 0.5f), new Color(0.72f, 0.78f, 0.86f)
        };
        [SerializeField] private Color[] tintColors =
        {
            new Color(1f, 1f, 1f), new Color(1f, 0.97f, 0.88f), new Color(1f, 0.88f, 0.75f), new Color(0.85f, 0.9f, 1f)
        };
        [SerializeField] private List<SeasonalSpriteEntry> seasonalSprites = new List<SeasonalSpriteEntry>();
        [SerializeField] private List<SpriteRenderer> tinted = new List<SpriteRenderer>();
        [SerializeField] private ParticleSystem[] weather = new ParticleSystem[4];
        [SerializeField] private Text seasonLabel;

        private int season = -1;
        private float timer;

        public void Setup(Camera cam, Text label)
        {
            targetCamera = cam;
            seasonLabel = label;
            seasonalSprites = new List<SeasonalSpriteEntry>();
            tinted = new List<SpriteRenderer>();
        }

        public void AddSeasonal(SpriteRenderer renderer, Sprite spring, Sprite summer, Sprite autumn, Sprite winter)
        {
            seasonalSprites.Add(new SeasonalSpriteEntry { renderer = renderer, sprites = new[] { spring, summer, autumn, winter } });
        }

        public void AddTinted(SpriteRenderer renderer) => tinted.Add(renderer);

        public void SetWeather(ParticleSystem[] systems) => weather = systems;

        private void Start() => Apply(0);

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= secondsPerSeason)
            {
                timer = 0f;
                Apply((season + 1) % 4);
            }
            if (targetCamera != null) targetCamera.backgroundColor = Color.Lerp(targetCamera.backgroundColor, skyColors[season], Time.unscaledDeltaTime * 1.5f);
            Color tint = tintColors[season];
            foreach (var r in tinted)
            {
                if (r != null) r.color = Color.Lerp(r.color, tint, Time.unscaledDeltaTime * 1.5f);
            }
        }

        private void Apply(int next)
        {
            season = next;
            foreach (var e in seasonalSprites)
            {
                if (e.renderer != null && e.sprites.Length > season && e.sprites[season] != null) e.renderer.sprite = e.sprites[season];
            }
            for (int i = 0; i < weather.Length; i++)
            {
                if (weather[i] == null) continue;
                if (i == season) weather[i].Play();
                else weather[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            if (seasonLabel != null) seasonLabel.text = SeasonCalendar.Name((Season)season).ToUpper();
            GameAudio.PlayMusic((Season)season);
        }
    }
}
