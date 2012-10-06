using System;
using Quartz;

namespace LightController
{
    public static class QuartzExtensions{
        public  static DailyTimeIntervalScheduleBuilder Around(this DailyTimeIntervalScheduleBuilder x, TimeOfDay time)
        {
            return x.OnEveryDay()
                .StartingDailyAt(time).WithRepeatCount(0)
                .EndingDailyAt(new TimeOfDay(time.Hour, time.Minute + 5));
        }

        public static DailyTimeIntervalScheduleBuilder SchoolNights(this DailyTimeIntervalScheduleBuilder x)
        {
            return x.OnDaysOfTheWeek(DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday);
        }

        public static DailyTimeIntervalScheduleBuilder WeekendNights(this DailyTimeIntervalScheduleBuilder x)
        {
            return x.OnDaysOfTheWeek(DayOfWeek.Friday, DayOfWeek.Saturday);
        }
    }
}