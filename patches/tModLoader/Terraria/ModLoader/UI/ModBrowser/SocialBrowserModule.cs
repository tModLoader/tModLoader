using System;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader.UI.ModBrowser;

public interface SocialBrowserModule
{
	
}

public struct QueryParameters
{
	public List<string> searchTags;
	public List<ulong> searchModIds;
	public List<string> searchModSlugs;
	public string searchText;

	public UIBrowserFilterToggle<ModBrowserSortMode> sortAscending;
}
