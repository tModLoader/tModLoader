using Terraria.Localization;

namespace Terraria.ModLoader.UI.ModBrowser;

public static class ModBrowserSortModesExtensions
{
	public static string ToFriendlyString(this ModBrowserSortMode sortmode)
	{
		switch (sortmode) {
			case ModBrowserSortMode.DisplayNameAtoZ:
				return Language.GetTextValue("tModLoader.ModsSortNamesAlph");
			case ModBrowserSortMode.DisplayNameZtoA:
				return Language.GetTextValue("tModLoader.ModsSortNamesReverseAlph");
			case ModBrowserSortMode.DownloadsDescending:
				return Language.GetTextValue("tModLoader.MBSortDownloadDesc");
			case ModBrowserSortMode.DownloadsAscending:
				return Language.GetTextValue("tModLoader.MBSortDownloadAsc");
			case ModBrowserSortMode.RecentlyUpdated:
				return Language.GetTextValue("tModLoader.MBSortByRecentlyUpdated");
			case ModBrowserSortMode.Hot:
				return Language.GetTextValue("tModLoader.MBSortByPopularity");
		}
		return "Unknown Sort";
	}
}
