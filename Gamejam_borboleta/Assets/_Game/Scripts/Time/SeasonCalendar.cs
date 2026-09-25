using System;
using UnityEngine;

namespace ButterflyStep
{
    public enum Season
    {
        Primavera,
        Verao,
        Outono,
        Inverno
    }

    [Serializable]
    public class SeasonCalendar
    {
        [Tooltip("Quantos dias dura cada estação. Um ano tem 4 estações.")]
        [Min(1)] public int daysPerSeason = 30;
        [Tooltip("Estação em que a fase começa.")]
        public Season startSeason = Season.Primavera;
        [Tooltip("Dia da estação em que a fase começa (1 = primeiro dia).")]
        [Min(1)] public int startDayOfSeason = 1;

        public int DaysPerYear => daysPerSeason * 4;

        private int Absolute(int day) => (int)startSeason * daysPerSeason + (startDayOfSeason - 1) + Mathf.Max(0, day);

        public Season SeasonAt(int day) => (Season)(Absolute(day) / daysPerSeason % 4);

        public int DayOfSeason(int day) => Absolute(day) % daysPerSeason + 1;

        public int YearNumber(int day) => Absolute(day) / DaysPerYear + 1;

        public static string Name(Season season)
        {
            switch (season)
            {
                case Season.Primavera: return "Primavera";
                case Season.Verao: return "Verão";
                case Season.Outono: return "Outono";
                default: return "Inverno";
            }
        }
    }
}
