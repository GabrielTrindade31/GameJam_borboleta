namespace ButterflyStep
{
    public readonly struct TimeState
    {
        public readonly int Day;
        public readonly int PreviousDay;
        public readonly int Index;
        public readonly int PeriodCount;
        public readonly int MaxDay;
        public readonly Season Season;
        public readonly Season PreviousSeason;
        public readonly int DayOfSeason;
        public readonly int YearNumber;

        public TimeState(int day, int previousDay, int index, int periodCount, int maxDay, SeasonCalendar calendar)
        {
            Day = day;
            PreviousDay = previousDay;
            Index = index;
            PeriodCount = periodCount;
            MaxDay = maxDay;
            Season = calendar.SeasonAt(day);
            PreviousSeason = calendar.SeasonAt(previousDay);
            DayOfSeason = calendar.DayOfSeason(day);
            YearNumber = calendar.YearNumber(day);
        }

        public bool CanGoBack => Index > 0;
        public bool CanGoForward => Index < PeriodCount - 1;
        public int DisplayDay => Day + 1;
        public int Direction => Day > PreviousDay ? 1 : Day < PreviousDay ? -1 : 0;
        public bool SeasonChanged => Season != PreviousSeason;
        public float Normalized => MaxDay <= 0 ? 0f : (float)Day / MaxDay;
    }
}
