using System;

namespace SolarCalc
{

    public class SolarData
    {
        const int DaysInSolarYear = 365;
        const int DaysInSolarLeapYear = 366;
        const int HoursInSolarDay = 24;
        const int MinutesPerSolarHour = 60;
        const int SecondsPerSolarMinute = 60;
        const int MinutesPerSolarDay = 24 * MinutesPerSolarHour;
        const int DegreesPerCircle = 360;

        //  The formulae used here are taken from https://gml.noaa.gov/grad/solcalc/solareqns.PDF#:~:text=The%20solar%20hour%20angle%2C%20in%20degrees%2C%20is%3A%20ha,the%20following%20equation%3A%20cos%28%EF%81%A6%29%20%3D%20sin%28lat%29sin%28decl%29%20%2B%20cost%28lat%29cos%28decl%29cos%28ha%29
        //  The hard-coded constants don't have names, and many of them are just based on observed behavior, or else are complicated combinations of various other constants, so I haven't bothered pulling them out
        //  and creating const or #define names for them. 

        //  The NOAA formulae are a bit odd in that some of the constants are radian-based, whereas others are degree-based, and you have to read the docs carefully to ensure you know
        //  which are which.  I created a small math class to cover conversion for the trig functions (since the .NET Math library is radians-based).

        //  The formulae for calculating equinoxes and solstices are even more complicated --and, worse, are iterative (and generally accurate only up to a minute, depending on what sort of
        //  epsilon value you're willing to calcuate to.  I've simply hard-coded the next few years of those in a table, since they are invariant with respect to city location.
        public SolarData(DateTime date, double latitude, double longitude, double utcOffsetInHours)
        {
            double daysInYear = DateTime.IsLeapYear(date.Year) ? DaysInSolarLeapYear : DaysInSolarYear;

            // The constants in this section have radian-based units, so we need to use radian-based trigonometry
            // First we need to figure out how far into the year we are, which is all the days prior to this date plus some fractional amount of one day
            double fractionalYear = (2 * Math.PI / daysInYear) * (date.DayOfYear - 1 + (date.Hour - (HoursInSolarDay/2)) / HoursInSolarDay);

            // Next, we get the "equation of time," which is the current discrepency between solar time and clock time, which varies because the solar year is not precisely 24 hours, because the
            // Earth precesses, because it's speed around the sun varies throughout the year, and so on. We can only approximate this value (and there are many ways of doing so) since Earth's
            // orbit is eccentric due to all sorts of weird phenomena having to due with gravity, moments of inertia (global), and so on, but the equation I'm using gets us close enough.
            // (If you were to track this deviation by mapping the sun's position at noon on any given longitude through the year, it would look like a figure eight centered on the equator.
            // That's called the analemma, and most globes that you buy will show it.) We'll use this value to help us correct the position of the sun at any given time.
            double equationOfTimeInMinutes = 229.18 * (0.000075 + 0.001868 * Math.Cos(fractionalYear) - 0.032077 * Math.Sin(fractionalYear) - 0.014615 * Math.Cos(2 * fractionalYear) - 0.040849 * Math.Sin(2 * fractionalYear));
 
            // Finally, we'll get the solar declination, which is a statement about the "altitude" of the sun at noon at the current fractional year.  This varies due to Earth having a 
            // tilt of 23.44 degrees (about 0.4 radians) from the solar ecliptic.  Because the axis is fixed in direction (pointing towards Polaris at its northern end), its orientation with
            // respect to the sun varies from +23.44 to -23.44 over the course of a year.  Earth's orbit is also slightly eliptical (affecting orbital speed), and it's also not a perfect sphere.
            // These equations account for the former, but not the latter, so our calculation is for some ideal "sea level" position on a hypothetical spherical Earth, which is close enough.
            double solarDeclinationAngleInRadians = 0.006918 - 0.399912 * Math.Cos(fractionalYear) + 0.070257 * Math.Sin(fractionalYear) - 0.006758 * Math.Cos(2 * fractionalYear) + 0.000907 * Math.Sin(2 * fractionalYear)
                - 0.002697 * Math.Cos(3 * fractionalYear) + 0.00148 * Math.Sin(3 * fractionalYear);

            // We now know where the sun is at noon for (0.0) position, and now we need to state that in terms of the current hour for the user-supplied longitude
            // At this point, we're done with radian values for the moment, so we are free to calculate the solar hour angle in terms of degrees for the user's ease
            // (Generally I'm a fan of the metric system, but nobody really thinks in terms of radians when doing astronomy)
            double timeOffset = equationOfTimeInMinutes + 4 * longitude - MinutesPerSolarHour * utcOffsetInHours;
            double trueSolarTime = date.Hour * MinutesPerSolarHour + date.Minute + date.Second / SecondsPerSolarMinute + timeOffset;
            double solarHourAngleInDegrees = (trueSolarTime / 4) - DegreesPerCircle/2;

            // And now we are dealing with a degree value (latitude) and a radian value (solar declination) -- sigh -- but once we have cos(phi), we can derive phi as a value of degrees and
            // get the zenith altitude in degrees.  We've already calculated the solar declination from zenith and the difference in that for the given hour, but we need to state that in terms of the user-supplied latitude.
            double cosphi = MathDegree.SinDegree(latitude) * Math.Sin(solarDeclinationAngleInRadians) + MathDegree.CosDegree(latitude) * Math.Cos(solarDeclinationAngleInRadians) * MathDegree.CosDegree(solarHourAngleInDegrees);
            double solarZenithAngle = MathDegree.AcosDegree(cosphi);

            // The solar zenith angle is the degrees away from zenith, but astronomers think of zenith in terms of altitude from the horizon, so let's present the supplment of the zenith value
            // to satisfy that expectation
            double altitude = DegreesPerCircle / 4 - solarZenithAngle; // Distance from horizon to zenith is a quarter of a circle

            // Similar degree/radian issue when dealing with the azimuth with respect to caluculating cos(theta), but here everyone agrees on what azimuth means.
            // Basically, we are convolving the zenith and declination angles with respect  the user-supplied latitude.
            double compcostheta = -1 * ((MathDegree.SinDegree(latitude) * MathDegree.CosDegree(solarZenithAngle)) - Math.Sin(solarDeclinationAngleInRadians)) / (MathDegree.CosDegree(latitude) * MathDegree.SinDegree(solarZenithAngle));
            double solarAzimuthAngle = DegreesPerCircle / 2 - MathDegree.AcosDegree(compcostheta); // Distance from east to west is half a circle

            // Now, we can consider the amount of time it takes at the user-supplied latitude for the sun to transit across the sky.  We do this by calculating the horizon hour angle,
            // which is an angular measurement of the sun's transit in the sky from highest to lowest based on solar declination (and therefore current date) and the user-supplied latitude.
            // We can then subtract or add this to a noon-relevant value to calculate sunrise and sunset, correcting for the supplied location's position in its time zone.  And by
            // this, I mean its ideal time zone, which is 15 degrees wide on the sphere of the Earth -- since timezones are not perfect 15 degree sections by instead are politically-drawn,
            // you can actually be positioned well outside of an ideal time zone.  All of the time calculations are in minutes from midnight UTC.

            double horizonHourAngle = MathDegree.AcosDegree((MathDegree.CosDegree(90.833) / (MathDegree.CosDegree(latitude) * Math.Cos(solarDeclinationAngleInRadians))) - MathDegree.TanDegree(latitude) * Math.Tan(solarDeclinationAngleInRadians));
            double sunriseInMinutesUTC = (MinutesPerSolarDay/2) - 4 * (longitude + horizonHourAngle) - equationOfTimeInMinutes;
            double sunsetInMinutesUTC = (MinutesPerSolarDay / 2) - 4 * (longitude - horizonHourAngle) - equationOfTimeInMinutes;
            double solarNoonInMinutesUTC = (MinutesPerSolarDay / 2) - 4 * (longitude) - equationOfTimeInMinutes;

            // Now we need to convert this to the user-supplied time zone.  Technically, the user could supply time zone that doesn't represent the global coordinates,
            // so we need to do a modulus calculation to ensure that we stay within a 24-hour clock.

            double unnormalizedSunriseInMinutes = (sunriseInMinutesUTC + MinutesPerSolarHour * utcOffsetInHours);
            double unnormalizedSunsetInMinutes = (sunsetInMinutesUTC + MinutesPerSolarHour * utcOffsetInHours);
            double unnormalizedSolarNoonInMinutes = (solarNoonInMinutesUTC + MinutesPerSolarHour * utcOffsetInHours);

            // However, technically this could mean that sunrise could be the day before "today" (wrt to the time zone supplied), or sunset the day after "today,"
            // so I need to keep track of that as well.  (For example, if supplying coordinates for Seattle but specifying a time zone in Germany, sunset for Seattle would actually
            // be early in the morning of the next day relative to Germany.)
            bool sunrisePrevDay = (unnormalizedSunriseInMinutes < 0);
            bool sunsetNextDay = (unnormalizedSunsetInMinutes >= MinutesPerSolarDay); // Clock is zero-based
            double sunriseInMinutes = sunrisePrevDay?(unnormalizedSunriseInMinutes + MinutesPerSolarDay) % MinutesPerSolarDay: unnormalizedSunriseInMinutes;
            double sunsetInMinutes = sunsetNextDay?(unnormalizedSunsetInMinutes + MinutesPerSolarDay) % MinutesPerSolarDay: unnormalizedSunsetInMinutes;

            // Solar noon could be either before or after today if the timezone doesn't match the location, albeit generally within an hour of either edge
            bool noonPrevDay = unnormalizedSolarNoonInMinutes < 0;
            bool noonNextDay = unnormalizedSolarNoonInMinutes >= MinutesPerSolarDay;
            double solarNoonInMinutes = (noonPrevDay || noonNextDay)?(unnormalizedSolarNoonInMinutes + MinutesPerSolarDay) % MinutesPerSolarDay: unnormalizedSolarNoonInMinutes;

            // The minutes of sunlight is just the difference between sunrise and sunset (avoiding time zone/location weirdness)
            double minutesOfDaylight = unnormalizedSunsetInMinutes - unnormalizedSunriseInMinutes;

            // That's everything!  Let's package it up for the calling function, fixing the day of the year for sunrise, sunset, and noon in cases of coordinate/timezone mismatch...
            DaysInYear = daysInYear;
            Altitude = altitude;
            Azimuth = solarAzimuthAngle;

            Sunrise = new DateTime(date.Year,
                                            date.Month,
                                            date.Day,
                                            (int)(sunriseInMinutes) / MinutesPerSolarHour,
                                            (int)(sunriseInMinutes) % MinutesPerSolarHour,
                                            (int)((sunriseInMinutes - (int)(sunriseInMinutes)) * SecondsPerSolarMinute)
                                            );
            if (sunrisePrevDay) { Sunrise = Sunrise.AddDays(-1); } // Sunrise was actually the day before relative to the user-supplied time zone

            Sunset = new DateTime(date.Year,
                                           date.Month,
                                           date.Day,
                                           (int)(sunsetInMinutes) / MinutesPerSolarHour,
                                           (int)(sunsetInMinutes) % MinutesPerSolarHour,
                                           (int)((sunsetInMinutes - (int)(sunsetInMinutes)) * SecondsPerSolarMinute)
                                           );
            if (sunsetNextDay) { Sunset = Sunset.AddDays(1); } // Sunset is actually the day after relative to the user-supplied time zone
            SolarNoon = new DateTime(date.Year,
                                           date.Month,
                                           date.Day,
                                           (int)(solarNoonInMinutes) / MinutesPerSolarHour,
                                           (int)(solarNoonInMinutes) % MinutesPerSolarHour,
                                           (int)((solarNoonInMinutes - (int)(solarNoonInMinutes)) * SecondsPerSolarMinute)
                                           );
            if (noonPrevDay || noonNextDay) { SolarNoon = SolarNoon.AddDays(noonNextDay ? 1 : -1); }
            Daylight = new TimeSpan((int)(minutesOfDaylight) / MinutesPerSolarHour,
                                            (int)(minutesOfDaylight) % MinutesPerSolarHour,
                                            (int)((minutesOfDaylight - (int)(minutesOfDaylight)) * SecondsPerSolarMinute)
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
