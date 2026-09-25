using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    public enum Sfx
    {
        Jump,
        Land,
        Swing,
        Hit,
        EnemyDie,
        Hurt,
        PlayerDie,
        TimeForward,
        TimeBack,
        Blocked,
        Pickup,
        Pollen,
        Checkpoint,
        Stasis,
        Interact,
        BossHit,
        UiMove,
        UiSelect,
        Stomp,
        Complete,
        Splash,
        Erase,
        Restore
    }

    public class GameAudio : MonoBehaviour
    {
        private const float SfxVolume = 0.55f;
        private const float MusicVolume = 0.32f;

        private static GameAudio instance;
        private static bool quitting;

        private readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
        private readonly Dictionary<Season, AudioClip> music = new Dictionary<Season, AudioClip>();
        private readonly List<AudioSource> voices = new List<AudioSource>();
        private AudioSource musicA;
        private AudioSource musicB;
        private Season? currentSeason;
        private Coroutine fade;

        public static GameAudio Instance
        {
            get
            {
                if (instance == null && !quitting)
                {
                    var go = new GameObject("GameAudio");
                    instance = go.AddComponent<GameAudio>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public static void Play(Sfx sound, float pitchVariation = 0.06f)
        {
            var audio = Instance;
            if (audio != null) audio.PlayInternal(sound, pitchVariation);
        }

        public static void PlayMusic(Season season)
        {
            var audio = Instance;
            if (audio != null) audio.SetSeason(season);
        }

        private void OnApplicationQuit() => quitting = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            quitting = false;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            for (int i = 0; i < 10; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                voices.Add(src);
            }
            musicA = CreateMusicSource();
            musicB = CreateMusicSource();
            BuildSfx();
        }

        private AudioSource CreateMusicSource()
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = false;
            src.volume = 0f;
            return src;
        }

        private void PlayInternal(Sfx sound, float pitchVariation)
        {
            if (!clips.TryGetValue(sound, out var clip)) return;
            AudioSource free = null;
            foreach (var v in voices)
            {
                if (!v.isPlaying)
                {
                    free = v;
                    break;
                }
            }
            if (free == null) free = voices[Random.Range(0, voices.Count)];
            free.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            free.PlayOneShot(clip, SfxVolume);
        }

        private void SetSeason(Season season)
        {
            if (currentSeason == season) return;
            currentSeason = season;
            if (!music.TryGetValue(season, out var clip))
            {
                clip = BuildMusic(season);
                music[season] = clip;
            }
            var next = musicA.isPlaying && musicA.volume > 0.01f ? musicB : musicA;
            var previous = next == musicA ? musicB : musicA;
            next.clip = clip;
            next.time = previous.isPlaying && previous.clip != null ? previous.time % clip.length : 0f;
            next.Play();
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(Crossfade(previous, next));
        }

        private IEnumerator Crossfade(AudioSource from, AudioSource to)
        {
            for (float t = 0f; t < 1.2f; t += Time.unscaledDeltaTime)
            {
                float k = t / 1.2f;
                to.volume = MusicVolume * k;
                from.volume = Mathf.Min(from.volume, MusicVolume * (1f - k));
                yield return null;
            }
            to.volume = MusicVolume;
            from.volume = 0f;
            from.Stop();
        }

        private void BuildSfx()
        {
            var W = SoundSynth.Wave.Square;
            clips[Sfx.Jump] = SoundSynth.Clip("jump", SoundSynth.Tone(300f, 620f, 0.12f, W, 0.35f));
            clips[Sfx.Land] = SoundSynth.Clip("land", SoundSynth.Tone(400f, 120f, 0.07f, SoundSynth.Wave.Noise, 0.3f));
            clips[Sfx.Swing] = SoundSynth.Clip("swing", SoundSynth.Tone(1800f, 500f, 0.1f, SoundSynth.Wave.Noise, 0.25f));
            clips[Sfx.Hit] = SoundSynth.Clip("hit", SoundSynth.Mix(SoundSynth.Tone(220f, 90f, 0.12f, W, 0.4f), SoundSynth.Tone(900f, 200f, 0.08f, SoundSynth.Wave.Noise, 0.3f)));
            clips[Sfx.Stomp] = SoundSynth.Clip("stomp", SoundSynth.Tone(500f, 160f, 0.14f, W, 0.4f));
            clips[Sfx.EnemyDie] = SoundSynth.Clip("enemydie", SoundSynth.Concat(SoundSynth.Tone(400f, 200f, 0.08f, W, 0.35f), SoundSynth.Tone(250f, 60f, 0.2f, SoundSynth.Wave.Noise, 0.35f)));
            clips[Sfx.Hurt] = SoundSynth.Clip("hurt", SoundSynth.Tone(600f, 150f, 0.22f, SoundSynth.Wave.Saw, 0.45f));
            clips[Sfx.PlayerDie] = SoundSynth.Clip("die", SoundSynth.Concat(SoundSynth.Tone(500f, 300f, 0.15f, SoundSynth.Wave.Triangle, 0.5f), SoundSynth.Tone(300f, 80f, 0.5f, SoundSynth.Wave.Triangle, 0.5f)));
            clips[Sfx.TimeForward] = SoundSynth.Clip("timefwd", SoundSynth.Mix(SoundSynth.Tone(200f, 1200f, 0.35f, SoundSynth.Wave.Sine, 0.35f, 0.02f, 0.15f), SoundSynth.Tone(300f, 1800f, 0.35f, SoundSynth.Wave.Triangle, 0.15f, 0.02f, 0.15f)));
            clips[Sfx.TimeBack] = SoundSynth.Clip("timeback", SoundSynth.Mix(SoundSynth.Tone(1200f, 200f, 0.35f, SoundSynth.Wave.Sine, 0.35f, 0.02f, 0.15f), SoundSynth.Tone(1800f, 300f, 0.35f, SoundSynth.Wave.Triangle, 0.15f, 0.02f, 0.15f)));
            clips[Sfx.Blocked] = SoundSynth.Clip("blocked", SoundSynth.Concat(SoundSynth.Tone(160f, 150f, 0.08f, W, 0.35f), SoundSynth.Tone(120f, 110f, 0.12f, W, 0.35f)));
            clips[Sfx.Pickup] = SoundSynth.Clip("pickup", SoundSynth.Concat(SoundSynth.Tone(660f, 660f, 0.07f, SoundSynth.Wave.Triangle, 0.4f), SoundSynth.Tone(990f, 990f, 0.12f, SoundSynth.Wave.Triangle, 0.4f)));
            clips[Sfx.Pollen] = SoundSynth.Clip("pollen", SoundSynth.Concat(SoundSynth.Tone(784f, 784f, 0.06f, SoundSynth.Wave.Sine, 0.4f), SoundSynth.Tone(988f, 988f, 0.06f, SoundSynth.Wave.Sine, 0.4f), SoundSynth.Tone(1319f, 1319f, 0.18f, SoundSynth.Wave.Sine, 0.4f)));
            clips[Sfx.Checkpoint] = SoundSynth.Clip("checkpoint", SoundSynth.Concat(SoundSynth.Tone(523f, 523f, 0.1f, SoundSynth.Wave.Triangle, 0.4f), SoundSynth.Tone(659f, 659f, 0.1f, SoundSynth.Wave.Triangle, 0.4f), SoundSynth.Tone(784f, 784f, 0.25f, SoundSynth.Wave.Triangle, 0.4f)));
            clips[Sfx.Stasis] = SoundSynth.Clip("stasis", SoundSynth.Mix(SoundSynth.Tone(900f, 300f, 0.5f, SoundSynth.Wave.Sine, 0.35f, 0.01f, 0.3f), SoundSynth.Tone(1350f, 450f, 0.5f, SoundSynth.Wave.Sine, 0.2f, 0.01f, 0.3f)));
            clips[Sfx.Interact] = SoundSynth.Clip("interact", SoundSynth.Tone(440f, 880f, 0.1f, SoundSynth.Wave.Triangle, 0.35f));
            clips[Sfx.BossHit] = SoundSynth.Clip("bosshit", SoundSynth.Mix(SoundSynth.Tone(140f, 60f, 0.25f, W, 0.45f), SoundSynth.Tone(600f, 100f, 0.2f, SoundSynth.Wave.Noise, 0.35f)));
            clips[Sfx.UiMove] = SoundSynth.Clip("uimove", SoundSynth.Tone(700f, 700f, 0.04f, SoundSynth.Wave.Square, 0.2f));
            clips[Sfx.UiSelect] = SoundSynth.Clip("uiselect", SoundSynth.Concat(SoundSynth.Tone(700f, 700f, 0.05f, SoundSynth.Wave.Square, 0.25f), SoundSynth.Tone(1050f, 1050f, 0.09f, SoundSynth.Wave.Square, 0.25f)));
            clips[Sfx.Complete] = SoundSynth.Clip("complete", SoundSynth.Concat(SoundSynth.Tone(523f, 523f, 0.12f, SoundSynth.Wave.Triangle, 0.4f), SoundSynth.Tone(659f, 659f, 0.12f, SoundSynth.Wave.Triangle, 0.4f), SoundSynth.Tone(784f, 784f, 0.12f, SoundSynth.Wave.Triangle, 0.4f), SoundSynth.Tone(1047f, 1047f, 0.4f, SoundSynth.Wave.Triangle, 0.4f)));
            clips[Sfx.Splash] = SoundSynth.Clip("splash", SoundSynth.Mix(SoundSynth.Tone(1400f, 300f, 0.28f, SoundSynth.Wave.Noise, 0.32f, 0.005f, 0.2f), SoundSynth.Tone(420f, 180f, 0.12f, SoundSynth.Wave.Sine, 0.25f)));
            clips[Sfx.Erase] = SoundSynth.Clip("erase", SoundSynth.Mix(SoundSynth.Tone(880f, 220f, 0.45f, SoundSynth.Wave.Sine, 0.3f, 0.01f, 0.3f), SoundSynth.Tone(1320f, 330f, 0.45f, SoundSynth.Wave.Triangle, 0.12f, 0.01f, 0.3f)));
            clips[Sfx.Restore] = SoundSynth.Clip("restore", SoundSynth.Mix(SoundSynth.Tone(220f, 880f, 0.45f, SoundSynth.Wave.Sine, 0.3f, 0.1f, 0.1f), SoundSynth.Tone(330f, 1320f, 0.45f, SoundSynth.Wave.Triangle, 0.12f, 0.1f, 0.1f)));
        }

        private static AudioClip BuildMusic(Season season)
        {
            int[] scale;
            int[][] chords;
            float bpm;
            float density;
            switch (season)
            {
                case Season.Primavera:
                    scale = new[] { -9, -7, -5, -2, 0, 3, 5, 7, 10, 12 };
                    chords = new[] { new[] { -9, -5, -2 }, new[] { -14, -9, -5 }, new[] { -12, -9, -5 }, new[] { -16, -12, -9 } };
                    bpm = 104f; density = 0.8f; break;
                case Season.Verao:
                    scale = new[] { -14, -12, -9, -7, -5, -2, 0, 3, 5, 7 };
                    chords = new[] { new[] { -14, -10, -7 }, new[] { -9, -5, -2 }, new[] { -11, -7, -4 }, new[] { -16, -12, -9 } };
                    bpm = 116f; density = 0.95f; break;
                case Season.Outono:
                    scale = new[] { -12, -10, -7, -5, -3, 0, 2, 5, 7, 9 };
                    chords = new[] { new[] { -12, -9, -5 }, new[] { -16, -12, -9 }, new[] { -19, -15, -12 }, new[] { -17, -14, -10 } };
                    bpm = 92f; density = 0.65f; break;
                default:
                    scale = new[] { -7, -5, -2, 0, 3, 5, 7, 10, 12, 15 };
                    chords = new[] { new[] { -7, -4, 0 }, new[] { -11, -7, -4 }, new[] { -14, -10, -7 }, new[] { -12, -9, -5 } };
                    bpm = 72f; density = 0.45f; break;
            }

            float beat = 60f / bpm;
            int bars = 8;
            int total = Mathf.RoundToInt(bars * 4 * beat * SoundSynth.Rate);
            var data = new float[total];
            var rng = new System.Random((int)season * 97 + 11);
            int noteIndex = 4;

            for (int bar = 0; bar < bars; bar++)
            {
                var chord = chords[bar % chords.Length];
                int barStart = Mathf.RoundToInt(bar * 4 * beat * SoundSynth.Rate);
                SoundSynth.AddAt(data, SoundSynth.Tone(SoundSynth.Note(chord[0] - 12), SoundSynth.Note(chord[0] - 12), beat * 3.8f, SoundSynth.Wave.Triangle, 0.35f, 0.02f, 0.4f), barStart);
                foreach (int n in chord)
                {
                    SoundSynth.AddAt(data, SoundSynth.Tone(SoundSynth.Note(n), SoundSynth.Note(n), beat * 4f, SoundSynth.Wave.Sine, 0.07f, 0.4f, 0.8f), barStart);
                }
                for (int step = 0; step < 8; step++)
                {
                    if (rng.NextDouble() > density) continue;
                    noteIndex = Mathf.Clamp(noteIndex + rng.Next(-2, 3), 0, scale.Length - 1);
                    int pos = barStart + Mathf.RoundToInt(step * beat * 0.5f * SoundSynth.Rate);
                    float hz = SoundSynth.Note(scale[noteIndex]);
                    SoundSynth.AddAt(data, SoundSynth.Tone(hz, hz, beat * 0.45f, SoundSynth.Wave.Triangle, 0.16f, 0.01f, 0.2f), pos);
                    if (season == Season.Verao && step % 2 == 1)
                    {
                        SoundSynth.AddAt(data, SoundSynth.Tone(6000f, 3000f, 0.03f, SoundSynth.Wave.Noise, 0.05f), pos);
                    }
                }
            }
            return SoundSynth.Clip($"music_{season}", data);
        }
    }
}
