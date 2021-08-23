using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarCalc
{
    /// <summary>
    /// Currently there is a bug in the .NET 6.0 Android implementation of exposing timezones due to a 
    /// change in Android -- post-Preview 7 bits should have the fix, and in the meantime, I've created a fake timezone
    /// to use.
    /// </summary>
    static public class FakeTimeZone
    {
        static public TimeZoneInfo GetFakeTimeZoneInfo()
        {
            TimeZoneInfo.TransitionTime startTransition, endTransition;
            startTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 2, DayOfWeek.Sunday);
            endTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 11, 1, DayOfWeek.Sunday);
            TimeSpan delta = new(1, 0, 0);
            TimeZoneInfo.AdjustmentRule adjustment;
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1999, 10, 1), DateTime.MaxValue.Date, delta, startTransition, endTransition);
            TimeZoneInfo.AdjustmentRule[] adjustments = { adjustment };
            TimeZoneInfo cityTimeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("Seattle Standard Time", new TimeSpan(-8, 0, 0), "(GMT-8:00) Seattle Standard Time", "Seattle Standard Time",
                "Seattle Daylight Time", adjustments);

            return cityTimeZoneInfo;
        }
    }
}
