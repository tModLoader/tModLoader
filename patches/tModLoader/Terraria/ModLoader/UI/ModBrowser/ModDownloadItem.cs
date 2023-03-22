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
	public readonly string PublishId;
	public readonly bool HasUpdate;
	public readonly bool UpdateIsDowngrade;

	internal bool NeedsGameRestart;
	internal LocalMod Installed { get; set; }
	public readonly string Version;

	internal readonly string Author;
	internal readonly string ModIconUrl;
	internal ModIconStatus ModIconStatus = ModIconStatus.UNKNOWN;
	internal readonly DateTime TimeStamp;
	internal readonly string ModReferences;
	internal readonly ModSide ModSide;
	internal readonly int Downloads;
	internal readonly int Hot;
	internal readonly string Homepage;
	internal readonly string ModloaderVersion;

	private bool IsInstalled => Installed != null;

	public ModDownloadItem(string displayName, string name, string version, string author, string modReferences, ModSide modSide, string modIconUrl, string publishId, int downloads, int hot, DateTime timeStamp, string modloaderversion, string homepage)
	{
		// Check against installed mods for updates
		Installed = Interface.modBrowser.SocialBackend.IsItemInstalled(name);
		bool update = Installed != null && Interface.modBrowser.SocialBackend.DoesItemNeedUpdate(publishId, Installed, new System.Version(version));

		// The below line is to identify the transient state where it isn't installed, but Steam considers it as such
		bool needsRestart = Installed == null && Interface.modBrowser.SocialBackend.DoesAppNeedRestartToReinstallItem(publishId);

		ModName = name;
		DisplayName = displayName;
		DisplayNameClean = string.Join("", ChatManager.ParseMessage(displayName, Color.White).Where(x => x.GetType() == typeof(TextSnippet)).Select(x => x.Text));
		PublishId = publishId;

		Author = author;
		ModReferences = modReferences;
		ModSide = modSide;
		ModIconUrl = modIconUrl;
		Downloads = downloads;
		Hot = hot;
		Homepage = homepage;
		TimeStamp = timeStamp;
		HasUpdate = update;
		UpdateIsDowngrade = false;
		NeedsGameRestart = needsRestart;
		Version = version;
		ModloaderVersion = modloaderversion;
	}

	internal ModDownloadItem(string displayName, string publishId, LocalMod installed)
	{
		DisplayName = displayName;
		DisplayNameClean = string.Join("", ChatManager.ParseMessage(displayName, Color.White).Where(x => x.GetType() == typeof(TextSnippet)).Select(x => x.Text));
		PublishId = publishId;
		Installed = installed;
	}

	// Below needs to be re looked at if browser doesm't have all items

	internal Task InnerDownloadWithDeps()
	{
		var downloads = new HashSet<ModDownloadItem>() { this };
		downloads.Add(this);
		GetDependenciesRecursive(this, ref downloads);
		return Interface.modBrowser.SocialBackend.SetupDownload(downloads.ToList(), Interface.modBrowserID);
	}

	private IEnumerable<ModDownloadItem> GetDependencies()
	{
		return ModReferences.Split(',')
			.Select(Interface.modBrowser.SocialBackend.FindDownloadItem)
			.Where(item => item != null && (!item.IsInstalled || (item.HasUpdate && !item.UpdateIsDowngrade)));
	}

	private void GetDependenciesRecursive(ModDownloadItem item, ref HashSet<ModDownloadItem> set)
	{
		var deps = item.GetDependencies();
		set.UnionWith(deps);

		// Cyclic dependency should never happen, as it's not allowed
		//TODO: What if the same mod is a dependency twice, but different versions?

		foreach (var dep in deps) {
			GetDependenciesRecursive(dep, ref set);
		}
	}
}