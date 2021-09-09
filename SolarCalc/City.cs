using System;

namespace SolarCalc
{
    public class City
    {
        public City(string name, double latitude, double longitude, string timeZoneID)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            CityTimeZoneID = timeZoneID;

            // Not working on Android:
            // CityTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(CityTimeZoneID);
            CityTimeZoneInfo = FakeTimeZone.GetFakeTimeZoneInfo();

        }
        public string Name { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public string CityTimeZoneID { get; private set; }
        public TimeZoneInfo CityTimeZoneInfo { get; private set; }

        private bool GetIsDst(DateTime date)
        {
            return CityTimeZoneInfo.IsDaylightSavingTime(date);
        }
        private double GetUTCOffset(DateTime date)
        {
            double baseUTC = CityTimeZoneInfo.BaseUtcOffset.Hours + CityTimeZoneInfo.BaseUtcOffset.Minutes / 60.0;
            if (GetIsDst(date)) { baseUTC += 1.0; }
            return baseUTC;
        }
        public SolarData AnalyzeDate(DateTime date)
        {

            return new SolarData(date, Latitude, Longitude, GetUTCOffset(date));
        }

        public DateTime? GetSolarEvent(int year, SolarEvents.SolarEvent solarEvent)
        {
            DateTime? eventDate = SolarEvents.GetSolarEventUTC(year, solarEvent);
            if (eventDate != null)
            {
                eventDate.Value.AddHours(GetUTCOffset(eventDate.Value));
            }
            return eventDate;
        }

        public string GetTimeZoneString(DateTime date)
        {
            return GetIsDst(date) ? CityTimeZoneInfo.DaylightName : CityTimeZoneInfo.StandardName;
        }
    }
}
