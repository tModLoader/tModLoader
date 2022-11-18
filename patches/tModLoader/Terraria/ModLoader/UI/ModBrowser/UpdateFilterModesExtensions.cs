using Terraria.Localization;

namespace Terraria.ModLoader.UI.ModBrowser;

public static class UpdateFilterModesExtensions
{
	public static string ToFriendlyString(this UpdateFilter updateFilterMode)
	{
		switch (updateFilterMode) {
			case UpdateFilter.All:
				return Language.GetTextValue("tModLoader.MBShowAllMods");
			case UpdateFilter.Available:
				return Language.GetTextValue("tModLoader.MBShowNotInstalledUpdates");
			case UpdateFilter.UpdateOnly:
				return Language.GetTextValue("tModLoader.MBShowUpdates");
			case UpdateFilter.InstalledOnly:
				return Language.GetTextValue("tModLoader.MBShowInstalled");
		}
		return "Unknown Sort";
	}
}
