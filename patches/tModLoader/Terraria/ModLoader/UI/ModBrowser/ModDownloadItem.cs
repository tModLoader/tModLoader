using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Core;
using Terraria.Social.Steam;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI.ModBrowser;

public class ModDownloadItem
{
	public readonly string ModName;
	public readonly string DisplayName;
	public readonly string DisplayNameClean; // No chat tags: for search and sort functionality.
	public readonly ModPubId_t PublishId;
	public readonly string OwnerId;
	public readonly string Version;

	public readonly string Author;
	public readonly string ModIconUrl;
	public readonly DateTime TimeStamp;

	// @TODO: Redundant
	public readonly string ModReferencesBySlug;
	public readonly string[] ModReferenceByModId;

	public readonly ModSide ModSide;
	public readonly int Downloads;
	public readonly int Hot;
	public readonly string Homepage;
	public readonly string ModloaderVersion;

	public ModDownloadItem(string displayName, string name, string version, string author, string modReferences, ModSide modSide, string modIconUrl, string publishId, int downloads, int hot, DateTime timeStamp, string modloaderversion, string homepage, string ownerId, string[] referencesById)
	{
		// Check against installed mods for updates
		/*
		Installed = Interface.modBrowser.SocialBackend.IsItemInstalled(name);
		bool update = Installed != null && Interface.modBrowser.SocialBackend.DoesItemNeedUpdate(publishId, Installed, new System.Version(version));

		// The below line is to identify the transient state where it isn't installed, but Steam considers it as such
		bool needsRestart = Installed == null && Interface.modBrowser.SocialBackend.DoesAppNeedRestartToReinstallItem(publishId);
		*/

		ModName = name;
		DisplayName = displayName;
		DisplayNameClean = string.Join("", ChatManager.ParseMessage(displayName, Color.White).Where(x => x.GetType() == typeof(TextSnippet)).Select(x => x.Text));
		PublishId = new ModPubId_t { m_ModPubId = publishId };
		OwnerId = ownerId;

		Author = author;
		ModReferencesBySlug = modReferences;
		ModReferenceByModId = referencesById;
		ModSide = modSide;
		ModIconUrl = modIconUrl;
		Downloads = downloads;
		Hot = hot;
		Homepage = homepage;
		TimeStamp = timeStamp;
		Version = version;
		ModloaderVersion = modloaderversion;
	}

	// @TODO: Below needs to be re looked at if browser doesm't have all items

	internal Task InnerDownloadWithDeps()
	{
		var downloads = new HashSet<ModDownloadItem>() { this };

		Interface.modBrowser.SocialBackend.GetDependenciesRecursive(new HashSet<ModPubId_t>() { this.PublishId }, ref downloads);

		return Interface.modBrowser.SocialBackend.SetupDownload(FilterOutInstalled(downloads).ToList(), Interface.modBrowserID);
	}

	private IEnumerable<ModDownloadItem> FilterOutInstalled(IEnumerable<ModDownloadItem> downloads)
	{
		// Should cache installed???
		return downloads.Where(item => !item.IsInstalled || (item.HasUpdate && !item.UpdateIsDowngrade));
	}
}