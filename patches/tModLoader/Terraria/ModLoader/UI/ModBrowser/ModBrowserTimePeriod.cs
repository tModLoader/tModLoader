using Terraria.Localization;

namespace Terraria.ModLoader.UI.ModBrowser;

// These match the time periods presented in https://steamcommunity.com/workshop/browse/?appid=1281930&browsesort=trend
public enum ModBrowserTimePeriod
{
	Today,
	OneWeek,
	ThreeMonths,
	SixMonths,
	OneYear,
	AllTime,
}

public static class ModBrowserTimePeriodExtensions
{
	public static string ToFriendlyString(this ModBrowserTimePeriod modBrowserTimePeriod)
	{
		switch (modBrowserTimePeriod) {
			case ModBrowserTimePeriod.Today:
				return Language.GetTextValue("tModLoader.Today");
			case ModBrowserTimePeriod.OneWeek:
				return Language.GetTextValue("tModLoader.OneWeek");
			case ModBrowserTimePeriod.ThreeMonths:
				return Language.GetTextValue("tModLoader.ThreeMonths");
			case ModBrowserTimePeriod.SixMonths:
				return Language.GetTextValue("tModLoader.SixMonths");
			case ModBrowserTimePeriod.OneYear:
				return Language.GetTextValue("tModLoader.OneYear");
			case ModBrowserTimePeriod.AllTime:
				return Language.GetTextValue("tModLoader.AllTime");
		}
		return "Unknown Time Period";
	}
}
