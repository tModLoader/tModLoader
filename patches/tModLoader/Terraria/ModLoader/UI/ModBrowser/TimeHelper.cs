using System;
using Terraria.Localization;

namespace Terraria.ModLoader.UI.ModBrowser
{
	internal class TimeHelper
	{
		private const int SECOND = 1;
		private const int MINUTE = 60 * SECOND;
		private const int HOUR = 60 * MINUTE;
		private const int DAY = 24 * HOUR;
		private const int MONTH = 30 * DAY;

		// Note: Polish has different plural for numbers ending in 2,3,4. Too complicated to do though.
		public static string HumanTimeSpanString(DateTime yourDate) {
			var ts = new TimeSpan(DateTime.UtcNow.Ticks - yourDate.Ticks);
			double delta = Math.Abs(ts.TotalSeconds);

			if (delta < 1 * MINUTE)
				return Language.GetTextValue("tModLoader.XSecondsAgo", ts.Seconds);

			if (delta < 60 * MINUTE)
				return Language.GetTextValue("tModLoader.XMinutesAgo", ts.Minutes);

			if (delta < 24 * HOUR)
				return Language.GetTextValue("tModLoader.XHoursAgo", ts.Hours);

			if (delta < 30 * DAY)
				return Language.GetTextValue("tModLoader.XDaysAgo", ts.Days);

			if (delta < 12 * MONTH) {
				int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
				return Language.GetTextValue("tModLoader.XMonthsAgo", months);
			}
			else {
				int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
				return Language.GetTextValue("tModLoader.XYearsAgo", years);
			}
		}
	}
}
