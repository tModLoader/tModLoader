namespace Terraria.ModLoader.UI.ModBrowser;

// Order is important for UI and texture placement
public enum ModBrowserSortMode
{
	DownloadsDescending,
	RecentlyPublished,
	RecentlyUpdated,
	Hot,
	//DisplayNameAtoZ, // Not currently doable in Steam Workshop - Solxan 2023-05
	//DisplayNameZtoA, // Not currently doable in Steam Workshop - Solxan 2023-05
	//DownloadsAscending, // Not currently doable in Steam Workshop - Solxan 2023-05
	//Relevance, // Currently handled at the Steam Workshop SteamedWraps.CalculateQuerySort. If SearchBox contains text, Relevance is auto-enforced
}
