using Microsoft.Maui.Controls;
using SolarCalc;
using System;
using System.Collections.ObjectModel;

namespace Almanac
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			// Initialize listview with time zones
			PopulateTimeZoneInformation();
		}

        private void PopulateTimeZoneInformation()
        {
			ReadOnlyCollection<TimeZoneInfo> tzCollection;
			tzCollection = TimeZoneInfo.GetSystemTimeZones();
			if (tzCollection.Count == 0)
            {
				// Yuck, no timezones, so just throw in a fake
				Collection<string> fakeData = new();
				fakeData.Add("Seattle Standard Time");
				this.cityTimeZone.ItemsSource = fakeData;
				this.cityTimeZone.SelectedIndex = 0;
			}
			else
            {
				this.cityTimeZone.ItemsSource = tzCollection;
				this.cityTimeZone.SelectedIndex = 0;
            }
		}

        public void OnAnalyze(object sender, EventArgs e)
		{
			Analyze();
		}
		public void OnCityNameCompleted(object sender, EventArgs e)
		{
			// Do some validation, then...
		}
		public void OnLatitudeCompleted(object sender, EventArgs e)
		{
			// Do some validation
			double cityLat = 0;
			try
			{
				cityLat = Convert.ToDouble(this.cityLatitude.Text);
			}
			catch (FormatException)
			{
				this.cityLatitude.Text = "0.0";
			}

			if (cityLat > 90 || cityLat < -90)
			{
				this.cityLatitude.Text = "0.0";
			}

		}
		public void OnLongitudeCompleted(object sender, EventArgs e)
		{
			// Do some validation
			double cityLong = 0;
			try
			{
				cityLong = Convert.ToDouble(this.cityLongitude.Text);
			}
			catch (FormatException)
			{
				this.cityLongitude.Text = "0.0";
			}

			if (cityLong > 180 || cityLong < -180)
			{
				this.cityLongitude.Text = "0.0";
			}
		}

		public void OnTimeZoneSelected(object sender, EventArgs e)
		{
			// Do some validation, then...
		}

		private void Analyze()
		{
			// Ensure all of the labels are visible:
			this.labelSolarInformation.IsVisible = true;
			this.outputGrid.IsVisible = true;

			//string cityName = this.cityName.Text;
			string cityName = "Unused currently";
			double cityLat = Convert.ToDouble(this.cityLatitude.Text);
			double cityLong = Convert.ToDouble(this.cityLongitude.Text);

			TimeZoneInfo cityTimeZoneInfo = null;
			string timeZoneID = "";
			if (this.cityTimeZone.SelectedItem is not null)
			{
				timeZoneID = this.cityTimeZone.SelectedItem.ToString();
                try 
				{
					cityTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneID);
				}
				catch (TimeZoneNotFoundException)
                {
					// Currently happens with Android; we'll fake it.
                }
			}
			if (cityTimeZoneInfo is null)
            {
				cityTimeZoneInfo = FakeTimeZone.GetFakeTimeZoneInfo();
				timeZoneID = cityTimeZoneInfo.DisplayName;
			}

			// These next few lines allow me to create a DateTime that isn't local to where I'm running it
			// or to DTC -- I can't otherwise construct a DateTime for a a city out of the running timezone
			// that uses the appropriate time zone info
			DateTime thisTime = DateTime.UtcNow;
			DateTime cityTime = TimeZoneInfo.ConvertTime(thisTime, cityTimeZoneInfo);

			City city = new(cityName, cityLat, cityLong, timeZoneID);

			SolarData currentData = city.AnalyzeDate(cityTime);
			SolarData zenithData = city.AnalyzeDate(currentData.SolarNoon);

			this.cityTime.Text = $"{cityTime} ({city.GetTimeZoneString(cityTime)})";
			this.solarAltitude.Text = $"{currentData.Altitude:F3}°";
			this.solarAzimuth.Text = $"{currentData.Azimuth:F3}°";

			// When displaying zenith, sunrise, and sunset times, we need to be mindful that the location date
			// might not match the timezone, and so these events could actually fall outside of the current "day"
			// from the perspective of the time zone -- we need to let the user know that.
			if (currentData.SolarNoon.Day == cityTime.Day)
			{
				this.solarZenithTime.Text = $"{currentData.SolarNoon.ToShortTimeString()}";
			}
			else
            {
				this.solarZenithTime.Text = $"{currentData.SolarNoon.ToShortTimeString()} ({currentData.SolarNoon.ToShortDateString()})";
			}
			this.solarZenithAltitude.Text = $"{zenithData.Altitude:F3}°";

			if (currentData.Sunrise.Day == cityTime.Day)
			{
				this.solarSunrise.Text = $"{currentData.Sunrise.ToShortTimeString()}";
			}
			else
            {
				this.solarSunrise.Text = $"{currentData.Sunrise.ToShortTimeString()} ({currentData.Sunrise.ToShortDateString()})";
			}
			if (currentData.Sunset.Day == cityTime.Day)
			{
				this.solarSunset.Text = $"{currentData.Sunset.ToShortTimeString()}";
			}
			else
			{
				this.solarSunset.Text = $"{currentData.Sunset.ToShortTimeString()} ({currentData.Sunset.ToShortDateString()})";
			}
			this.solarDaylight.Text = $"{currentData.Daylight.Hours}h {currentData.Daylight.Minutes}m {currentData.Daylight.Seconds}s";

            DateTime? nextSolarEventTime = SolarEvents.GetNextSolarEventUTC(thisTime, out SolarEvents.SolarEvent nextSolarEvent);
            if (nextSolarEventTime is not null)
			{
				DateTime nextSolarEventCityTime = TimeZoneInfo.ConvertTime(nextSolarEventTime.Value, cityTimeZoneInfo);
				SolarData nextSolarEventData = city.AnalyzeDate(nextSolarEventCityTime);
				SolarData zenithEventData = city.AnalyzeDate(nextSolarEventData.SolarNoon);


				this.solarNextEvent.Text = $"{SolarEvents.GetSolarEventName(nextSolarEvent)} ({nextSolarEventCityTime})";

				// This is the maximum altitude of the sun that day, not the altitude at the minute which the event occurs
				this.solarZenithAltitudeEvent.Text = $"{zenithEventData.Altitude:F3}°";

				// Again, sunrise/senset could fall on a different day than the event time, so we need to make that clear:
				if (nextSolarEventData.Sunrise.Day == nextSolarEventCityTime.Day)
				{
					this.solarSunriseEvent.Text = $"{nextSolarEventData.Sunrise.ToShortTimeString()}";
				}
				else
				{
					this.solarSunriseEvent.Text = $"{nextSolarEventData.Sunrise.ToShortTimeString()} ({nextSolarEventData.Sunrise.ToShortDateString()})";
				}
				if (nextSolarEventData.Sunset.Day == nextSolarEventCityTime.Day)
				{
					this.solarSunsetEvent.Text = $"{nextSolarEventData.Sunset.ToShortTimeString()}";
				}
				else
				{
					this.solarSunsetEvent.Text = $"{nextSolarEventData.Sunset.ToShortTimeString()} ({nextSolarEventData.Sunset.ToShortDateString()})";
				}
				this.solarDaylightEvent.Text = $"{nextSolarEventData.Daylight.Hours}h {nextSolarEventData.Daylight.Minutes}m {nextSolarEventData.Daylight.Seconds}s";
			}
		}
	}
}
