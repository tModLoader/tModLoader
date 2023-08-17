using Terraria.Localization;

namespace Terraria.ModLoader.UI.ModBrowser;

public static class SearchFilterModesExtensions
{
	public static string ToFriendlyString(this SearchFilter searchFilterMode)
	{
		switch (searchFilterMode) {
			case SearchFilter.Name:
				return Language.GetTextValue("tModLoader.ModsSearchByModName");
			case SearchFilter.Author:
				return Language.GetTextValue("tModLoader.ModsSearchByAuthor");
		}
		return "Unknown Sort";
	}
}
