using Terraria.Localization;

namespace Terraria.ModLoader.UI.ModBrowser;

public static class ModSideFilterModesExtensions
{
	public static string ToFriendlyString(this ModSideFilter modSideFilterMode)
	{
		switch (modSideFilterMode) {
			case ModSideFilter.All:
				return Language.GetTextValue("tModLoader.MBShowMSAll");
			case ModSideFilter.Both:
				return Language.GetTextValue("tModLoader.MBShowMSBoth");
			case ModSideFilter.Client:
				return Language.GetTextValue("tModLoader.MBShowMSClient");
			case ModSideFilter.Server:
				return Language.GetTextValue("tModLoader.MBShowMSServer");
			case ModSideFilter.NoSync:
				return Language.GetTextValue("tModLoader.MBShowMSNoSync");
		}
		return "Unknown Sort";
	}
}
