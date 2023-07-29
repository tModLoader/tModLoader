namespace Terraria.ModLoader.UI.ModBrowser;

public enum ModBrowserSortMode
{
	//DisplayNameAtoZ, // Not currently doable in Steam Workshop - Solxan 2023-05
	//Relevance, // Currently handled at the Steam Workshop SteamedWraps.CalculateQuerySort. If SearchBox contains text, Relevance is auto-enforced
	//DisplayNameZtoA, // Not currently doable in Steam Workshop - Solxan 2023-05
	DownloadsDescending,
	//DownloadsAscending, // Not currently doable in Steam Workshop - Solxan 2023-05
	RecentlyUpdated,
	Hot,
}
