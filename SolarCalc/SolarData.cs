namespace SolarCalc
{
    public class SolarData
    {
        //  The formulae used here are taken from https://gml.noaa.gov/grad/solcalc/solareqns.PDF#:~:text=The%20solar%20hour%20angle%2C%20in%20degrees%2C%20is%3A%20ha,the%20following%20equation%3A%20cos%28%EF%81%A6%29%20%3D%20sin%28lat%29sin%28decl%29%20%2B%20cost%28lat%29cos%28decl%29cos%28ha%29
        //  The hard-coded constants don't have names, and many of them are just based on observed behavior, or else are complicated combinations of various other constants, so I haven't bothered pulling them out
        //  and creating const or #define names for them. 

        //  The NOAA formulae are a bit odd in that some of the constants are radian-based, whereas others are degree-based, and you have to read the docs carefully to ensure you know
        //  which are which.  I created a small math class to cover conversion for the trig functions (since the .NET Math library is radians-based).

        //  The formulae for calculating equinoxes and solstices are even more complicated --and, worse, are iterative (and generally accurate only up to a minute, depending on what sort of
        //  epsilon value you're willing to calcuate to.  I've simply hard-coded the next few years of those in a table, since they are invariant with respect to city location.
        public SolarData(DateTime date, double latitude, double longitude, double utcOffsetInHours)
        {
            double daysInYear = DateTime.IsLeapYear(date.Year) ? 366 : 365;
            double fractionalYear = (2 * Math.PI / daysInYear) * (date.DayOfYear - 1 + (date.Hour - 12) / 24);
            double equationOfTimeInMinutes = 229.18 * (0.000075 + 0.001868 * Math.Cos(fractionalYear) - 0.032077 * Math.Sin(fractionalYear) - 0.014615 * Math.Cos(2 * fractionalYear) - 0.040849 * Math.Sin(2 * fractionalYear));
            double solarDeclinationAngleInRadians = 0.006918 - 0.399912 * Math.Cos(fractionalYear) + 0.070257 * Math.Sin(fractionalYear) - 0.006758 * Math.Cos(2 * fractionalYear) + 0.000907 * Math.Sin(2 * fractionalYear)
                - 0.002697 * Math.Cos(3 * fractionalYear) + 0.00148 * Math.Sin(3 * fractionalYear);
            double timeOffset = equationOfTimeInMinutes + 4 * longitude - 60 * utcOffsetInHours;
            double trueSolarTime = date.Hour * 60 + date.Minute + date.Second / 60 + timeOffset;
            double solarHourAngleInDegrees = (trueSolarTime / 4) - 180;

            double cosphi = MathDegree.SinDegree(latitude) * Math.Sin(solarDeclinationAngleInRadians) + MathDegree.CosDegree(latitude) * Math.Cos(solarDeclinationAngleInRadians) * MathDegree.CosDegree(solarHourAngleInDegrees);
            double solarZenithAngle = MathDegree.AcosDegree(cosphi);
            double altitude = 90 - solarZenithAngle;

            double compcostheta = -1 * ((MathDegree.SinDegree(latitude) * MathDegree.CosDegree(solarZenithAngle)) - Math.Sin(solarDeclinationAngleInRadians)) / (MathDegree.CosDegree(latitude) * MathDegree.SinDegree(solarZenithAngle));
            double solarAzimuthAngle = 180 - MathDegree.AcosDegree(compcostheta);

            double horizonHourAngle = MathDegree.AcosDegree((MathDegree.CosDegree(90.833) / (MathDegree.CosDegree(latitude) * Math.Cos(solarDeclinationAngleInRadians))) - MathDegree.TanDegree(latitude) * Math.Tan(solarDeclinationAngleInRadians));
            double sunriseInMinutesUTC = 720 - 4 * (longitude + horizonHourAngle) - equationOfTimeInMinutes;
            double sunsetInMinutesUTC = 720 - 4 * (longitude - horizonHourAngle) - equationOfTimeInMinutes;
            double solarNoonInMinutesUTC = 720 - 4 * (longitude) - equationOfTimeInMinutes;

            double sunriseInMinutes = ((sunriseInMinutesUTC + 60 * utcOffsetInHours) + 1440) % 1440;
            double sunsetInMinutes = ((sunsetInMinutesUTC + 60 * utcOffsetInHours) + 1440) % 1440;
            double solarNoonInMinutes = ((solarNoonInMinutesUTC + 60 * utcOffsetInHours) + 1440) % 1440;


            double minutesOfDaylight = sunsetInMinutes - sunriseInMinutes;

            DaysInYear = daysInYear;
            Altitude = altitude;
            Azimuth = solarAzimuthAngle;

            Sunrise = new DateTime(date.Year,
                                            date.Month,
                                            date.Day,
                                            (int)(sunriseInMinutes) / 60,
                                            (int)(sunriseInMinutes) % 60,
                                            (int)((sunriseInMinutes - (int)(sunriseInMinutes)) * 60)
                                            );
            Sunset = new DateTime(date.Year,
                                           date.Month,
                                           date.Day,
                                           (int)(sunsetInMinutes) / 60,
                                           (int)(sunsetInMinutes) % 60,
                                           (int)((sunsetInMinutes - (int)(sunsetInMinutes)) * 60)
                                           );
            SolarNoon = new DateTime(date.Year,
                                           date.Month,
                                           date.Day,
                                           (int)(solarNoonInMinutes) / 60,
                                           (int)(solarNoonInMinutes) % 60,
                                           (int)((solarNoonInMinutes - (int)(solarNoonInMinutes)) * 60)
                                           );
            Daylight = new TimeSpan((int)(minutesOfDaylight) / 60,
                                            (int)(minutesOfDaylight) % 60,
                                            (int)((minutesOfDaylight - (int)(minutesOfDaylight)) * 60)
                                           );

        }

        public double DaysInYear { get; private set; }
        public double Altitude { get; private set; }
        public double Azimuth { get; private set; }
        public DateTime Sunrise { get; private set; }
        public DateTime Sunset { get; private set; }
        public DateTime SolarNoon { get; private set; }
        public TimeSpan Daylight { get; private set; }
    }
}
