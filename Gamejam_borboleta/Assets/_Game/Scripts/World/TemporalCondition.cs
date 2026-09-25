using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    public enum ConditionType
    {
        DaysSinceStartAtLeast,
        DaysSinceStartBelow,
        SeasonIs,
        SeasonIsNot,
        FlagActive,
        FlagInactive,
        FlagActiveAtDay,
        FlagInactiveAtDay
    }

    [Serializable]
    public class TemporalCondition
    {
        [Tooltip("Tipo de teste.\nDaysSinceStart: dias passados desde o início da fase.\nSeason: estação do ano.\nFlagActive/FlagInactive: uma ação ou consequência do mundo.\nAtDay: flag em um dia exato.")]
        public ConditionType type = ConditionType.DaysSinceStartAtLeast;
        [Tooltip("Nome da flag (apenas para os tipos Flag). Ex: L1_PlantaRegada")]
        public string flag;
        [Tooltip("Dias. Para FlagActive/FlagInactive: há quantos dias a flag precisa estar ativa (0 = agora). Para AtDay: o dia exato.")]
        public int value;
        [Tooltip("Estação (apenas para SeasonIs/SeasonIsNot).")]
        public Season season;

        public TemporalCondition() { }

        public TemporalCondition(ConditionType type, int value, string flag = null)
        {
            this.type = type;
            this.value = value;
            this.flag = flag;
        }

        public static TemporalCondition Since(int days) => new TemporalCondition(ConditionType.DaysSinceStartAtLeast, days);
        public static TemporalCondition Before(int days) => new TemporalCondition(ConditionType.DaysSinceStartBelow, days);
        public static TemporalCondition In(Season s) => new TemporalCondition(ConditionType.SeasonIs, 0) { season = s };
        public static TemporalCondition NotIn(Season s) => new TemporalCondition(ConditionType.SeasonIsNot, 0) { season = s };
        public static TemporalCondition Flag(string key, int daysAgo = 0) => new TemporalCondition(ConditionType.FlagActive, daysAgo, key);
        public static TemporalCondition NotFlag(string key, int daysAgo = 0) => new TemporalCondition(ConditionType.FlagInactive, daysAgo, key);
        public static TemporalCondition FlagAt(string key, int day) => new TemporalCondition(ConditionType.FlagActiveAtDay, day, key);
        public static TemporalCondition NotFlagAt(string key, int day) => new TemporalCondition(ConditionType.FlagInactiveAtDay, day, key);

        public bool Evaluate(int day, int startDay, SeasonCalendar calendar, WorldState world)
        {
            switch (type)
            {
                case ConditionType.DaysSinceStartAtLeast: return day - startDay >= value;
                case ConditionType.DaysSinceStartBelow: return day - startDay < value;
                case ConditionType.SeasonIs: return calendar != null && calendar.SeasonAt(day) == season;
                case ConditionType.SeasonIsNot: return calendar == null || calendar.SeasonAt(day) != season;
                case ConditionType.FlagActive: return world != null && world.IsActive(flag, day - value);
                case ConditionType.FlagInactive: return world == null || !world.IsActive(flag, day - value);
                case ConditionType.FlagActiveAtDay: return world != null && world.IsActive(flag, value);
                case ConditionType.FlagInactiveAtDay: return world == null || !world.IsActive(flag, value);
                default: return false;
            }
        }

        public override string ToString()
        {
            switch (type)
            {
                case ConditionType.DaysSinceStartAtLeast: return $"+{value} dias";
                case ConditionType.DaysSinceStartBelow: return $"< +{value} dias";
                case ConditionType.SeasonIs: return SeasonCalendar.Name(season);
                case ConditionType.SeasonIsNot: return $"não {SeasonCalendar.Name(season)}";
                case ConditionType.FlagActive: return value > 0 ? $"{flag} há {value} dias" : flag;
                case ConditionType.FlagInactive: return value > 0 ? $"!{flag} há {value} dias" : $"!{flag}";
                case ConditionType.FlagActiveAtDay: return $"{flag} no dia {value + 1}";
                case ConditionType.FlagInactiveAtDay: return $"!{flag} no dia {value + 1}";
                default: return type.ToString();
            }
        }

        public static bool All(List<TemporalCondition> conditions, int day, int startDay, SeasonCalendar calendar, WorldState world)
        {
            if (conditions == null) return true;
            foreach (var c in conditions)
            {
                if (c != null && !c.Evaluate(day, startDay, calendar, world)) return false;
            }
            return true;
        }
    }
}
