using UnityEngine;

namespace ButterflyStep
{
    public static class GameSettings
    {
        private const string MusicKey = "bs_opt_music";
        private const string SfxKey = "bs_opt_sfx";
        private const string ShakeKey = "bs_opt_shake";
        private const string HintsKey = "bs_opt_hints";

        public static float Music
        {
            get => PlayerPrefs.GetFloat(MusicKey, 0.8f);
            set { PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        }

        public static float Sfx
        {
            get => PlayerPrefs.GetFloat(SfxKey, 0.8f);
            set { PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        }

        public static bool ScreenShake
        {
            get => PlayerPrefs.GetInt(ShakeKey, 1) == 1;
            set { PlayerPrefs.SetInt(ShakeKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool Hints
        {
            get => PlayerPrefs.GetInt(HintsKey, 1) == 1;
            set { PlayerPrefs.SetInt(HintsKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool Fullscreen
        {
            get => Screen.fullScreen;
            set => Screen.fullScreen = value;
        }
    }
}
