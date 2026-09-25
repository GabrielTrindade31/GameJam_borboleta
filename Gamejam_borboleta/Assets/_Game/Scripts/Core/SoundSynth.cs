using System;
using UnityEngine;

namespace ButterflyStep
{
    public static class SoundSynth
    {
        public const int Rate = 22050;

        public enum Wave
        {
            Sine,
            Square,
            Triangle,
            Saw,
            Noise
        }

        private static readonly System.Random Rng = new System.Random(1234);

        private static float Osc(Wave wave, float phase)
        {
            float p = phase - Mathf.Floor(phase);
            switch (wave)
            {
                case Wave.Sine: return Mathf.Sin(p * Mathf.PI * 2f);
                case Wave.Square: return p < 0.5f ? 0.6f : -0.6f;
                case Wave.Triangle: return 1f - 4f * Mathf.Abs(p - 0.5f);
                case Wave.Saw: return (p * 2f - 1f) * 0.6f;
                default: return (float)(Rng.NextDouble() * 2.0 - 1.0);
            }
        }

        public static float[] Tone(float fromHz, float toHz, float duration, Wave wave, float volume, float attack = 0.005f, float release = 0.06f)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(duration * Rate));
            var data = new float[n];
            float phase = 0f;
            float held = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float hz = Mathf.Lerp(fromHz, toHz, t);
                phase += hz / Rate;
                float time = (float)i / Rate;
                float env = Mathf.Min(1f, time / Mathf.Max(0.0001f, attack)) * Mathf.Min(1f, (duration - time) / Mathf.Max(0.0001f, release));
                float sample = Osc(wave, phase);
                if (wave == Wave.Noise)
                {
                    float hold = Mathf.Max(1f, Rate / Mathf.Max(60f, hz));
                    if (i % Mathf.RoundToInt(hold) == 0) held = sample;
                    sample = held;
                }
                data[i] = sample * env * volume;
            }
            return data;
        }

        public static float[] Concat(params float[][] parts)
        {
            int total = 0;
            foreach (var p in parts) total += p.Length;
            var data = new float[total];
            int offset = 0;
            foreach (var p in parts)
            {
                Array.Copy(p, 0, data, offset, p.Length);
                offset += p.Length;
            }
            return data;
        }

        public static float[] Mix(params float[][] parts)
        {
            int length = 0;
            foreach (var p in parts) length = Mathf.Max(length, p.Length);
            var data = new float[length];
            foreach (var p in parts)
            {
                for (int i = 0; i < p.Length; i++) data[i] += p[i];
            }
            return data;
        }

        public static void AddAt(float[] target, float[] source, int offset, float gain = 1f)
        {
            for (int i = 0; i < source.Length; i++)
            {
                int j = offset + i;
                if (j >= target.Length) j -= target.Length;
                target[j] += source[i] * gain;
            }
        }

        public static AudioClip Clip(string name, float[] data)
        {
            float peak = 0f;
            foreach (var s in data) peak = Mathf.Max(peak, Mathf.Abs(s));
            if (peak > 0.95f)
            {
                float k = 0.95f / peak;
                for (int i = 0; i < data.Length; i++) data[i] *= k;
            }
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static float Note(int semitonesFromA4) => 440f * Mathf.Pow(2f, semitonesFromA4 / 12f);
    }
}
