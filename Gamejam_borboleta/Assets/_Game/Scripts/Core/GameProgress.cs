using UnityEngine;

namespace ButterflyStep
{
    public static class GameProgress
    {
        public const int PollenPerLevel = 5;
        private const string UnlockedKey = "bs_unlocked";
        private const string LastKey = "bs_last";

        public static int UnlockedLevel => Mathf.Max(1, PlayerPrefs.GetInt(UnlockedKey, 1));
        public static int LastLevel => Mathf.Clamp(PlayerPrefs.GetInt(LastKey, 1), 1, UnlockedLevel);
        public static bool HasProgress => PlayerPrefs.HasKey(LastKey);

        public static void Unlock(int level)
        {
            if (level > UnlockedLevel) PlayerPrefs.SetInt(UnlockedKey, level);
            PlayerPrefs.Save();
        }

        public static void SetLast(int level)
        {
            PlayerPrefs.SetInt(LastKey, level);
            PlayerPrefs.Save();
        }

        private static string PollenKey(string scene) => $"bs_pollen_{scene}";

        public static bool HasPollen(string scene, int id) => (PlayerPrefs.GetInt(PollenKey(scene), 0) & (1 << id)) != 0;

        public static void CollectPollen(string scene, int id)
        {
            PlayerPrefs.SetInt(PollenKey(scene), PlayerPrefs.GetInt(PollenKey(scene), 0) | (1 << id));
            PlayerPrefs.Save();
        }

        public static int PollenCount(string scene)
        {
            int mask = PlayerPrefs.GetInt(PollenKey(scene), 0);
            int count = 0;
            for (int i = 0; i < PollenPerLevel; i++)
            {
                if ((mask & (1 << i)) != 0) count++;
            }
            return count;
        }

        public static void ResetAll()
        {
            PlayerPrefs.DeleteKey(UnlockedKey);
            PlayerPrefs.DeleteKey(LastKey);
            for (int i = 1; i <= 10; i++) PlayerPrefs.DeleteKey(PollenKey($"Level{i:00}"));
            PlayerPrefs.Save();
        }
    }
}
