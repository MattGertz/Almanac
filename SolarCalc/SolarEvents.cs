using System;
using System.Collections.Generic;

namespace SolarCalc
{
    public static class SolarEvents
    {
        public enum SolarEvent
        {
            VernalEquinox,
            SummerSolstice,
            AutumnalEquinox,
            WinterSolstice
        }

        private static SolarEvent GetEventType(DateTime eventDate)
        {
            SolarEvent solarEvent = SolarEvent.VernalEquinox;
            switch (eventDate.Month)
            {
                case 3: solarEvent = SolarEvent.VernalEquinox; break;
                case 6: solarEvent = SolarEvent.SummerSolstice; break;
                case 9: solarEvent = SolarEvent.AutumnalEquinox; break;
                case 12: solarEvent = SolarEvent.WinterSolstice; break;
            }
            return solarEvent;
        }

        public static string GetSolarEventName(SolarEvent solarEvent)
        {
            return solarEvent switch
            {
                SolarEvent.VernalEquinox => "Vernal Equinox",
                SolarEvent.SummerSolstice => "Summer Solstice",
                SolarEvent.AutumnalEquinox => "Autumnal Equinox",
                SolarEvent.WinterSolstice => "Winter Solstice",
                _ => "Invalid solar event",
            };
        }

        private static readonly List<DateTime> eventDates = new()
        {
            // These are ordered date-wise
            {new DateTime(2021,3,20,9,37,0)},
            {new DateTime(2021,6,20,3,32,0)},
            {new DateTime(2021,9,20,19,21,0) },
            {new DateTime(2021,12,20,15,59,0) },
            {new DateTime(2022,3,20,15,33,0) },
            {new DateTime(2022,6,20,9,14,0) },
            {new DateTime(2022,9,20,1,4,0) },
            {new DateTime(2022,12,20,21,48,0) },
            {new DateTime(2023,3,20,21,24,0) },
            {new DateTime(2023,6,20,14,58,0) },
            {new DateTime(2023,9,20,6,50,0) },
            {new DateTime(2023,12,20,3,27,0) },
            {new DateTime(2024,3,20,3,6,0) },
            {new DateTime(2024,6,20,20,51,0) },
            {new DateTime(2024,9,20,12,44,0) },
            {new DateTime(2024,12,20,9,21,0) },
            {new DateTime(2025,3,20,9,1,0) },
            {new DateTime(2025,6,20,2,42,0) },
            {new DateTime(2025,9,20,18,19,0) },
            {new DateTime(2025,12,20,15,3,0) }
        };
        static public DateTime? GetSolarEventUTC(int year, SolarEvent solarEvent)
        {
            for (int i = 0; i < eventDates.Count; i++)
            {
                if (eventDates[i].Year == year && GetEventType(eventDates[i]) == solarEvent)
                    return eventDates[i];
            }
            return null;
        }

        static public DateTime? GetNextSolarEventUTC(DateTime date, out SolarEvent solarEvent)
        {
            for (int i = 0; i < eventDates.Count; i++)
            {
                DateTime compdate = (DateTime)(eventDates[i]);
                if (compdate.CompareTo(date) > 0)
                {
                    solarEvent = GetEventType(eventDates[i]);
                    return (DateTime)eventDates[i];
                }
            }
            solarEvent = SolarEvent.VernalEquinox;
            return null;
        }

    }
}
