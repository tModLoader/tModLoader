using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Core;
using Terraria.Social.Base;
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

	public readonly string ModReferencesBySlug;
	public readonly ModPubId_t[] ModReferenceByModId;

	public readonly ModSide ModSide;
	public readonly int Downloads;
	public readonly int Hot;
	public readonly string Homepage;
	public readonly string ModloaderVersion;

	private LocalMod Installed; // Shoudn't be internal
	public bool NeedUpdate { get; private set; }
	public bool AppNeedRestartToReinstall { get; private set; }

	public bool IsInstalled => Installed != null;
	public bool IsEnabled => IsInstalled && Installed.Enabled;

	public ModDownloadItem(string displayName, string name, string version, string author, string modReferences, ModSide modSide, string modIconUrl, string publishId, int downloads, int hot, DateTime timeStamp, string modloaderversion, string homepage, string ownerId, string[] referencesById)
	{
		ModName = name;
		DisplayName = displayName;
		DisplayNameClean = string.Join("", ChatManager.ParseMessage(displayName, Color.White).Where(x => x.GetType() == typeof(TextSnippet)).Select(x => x.Text));
		PublishId = new ModPubId_t { m_ModPubId = publishId };
		OwnerId = ownerId;

		Author = author;
		ModReferencesBySlug = modReferences;
		ModReferenceByModId = Array.ConvertAll(referencesById, x => new ModPubId_t() { m_ModPubId = x});
		ModSide = modSide;
		ModIconUrl = modIconUrl;
		Downloads = downloads;
		Hot = hot;
		Homepage = homepage;
		TimeStamp = timeStamp;
		Version = version;
		ModloaderVersion = modloaderversion;
	}

	internal void UpdateInstallState()
	{
		// Remember this method is blocking, it does network stuff... - DarioDaf

		// Check against installed mods for updates.
		//TODO: This should assess the source of the ModDownloadItem and ensure matches with the active SocialBrowserModule instance for safety, but eh.
		Installed = Interface.modBrowser.SocialBackend.IsItemInstalled(this.ModName);

		NeedUpdate = Installed != null && Interface.modBrowser.SocialBackend.DoesItemNeedUpdate(this.PublishId, Installed, new System.Version(this.Version));
		// The below line is to identify the transient state where it isn't installed, but Steam considers it as such
		AppNeedRestartToReinstall = Installed == null && Interface.modBrowser.SocialBackend.DoesAppNeedRestartToReinstallItem(this.PublishId);
	}

	internal Task InnerDownloadWithDeps()
	{
		var downloads = new HashSet<ModDownloadItem>() { this };

		if (ModReferenceByModId.Length > 0)
			Interface.modBrowser.SocialBackend.GetDependenciesRecursive(ref downloads);

		return Interface.modBrowser.SocialBackend.SetupDownload(FilterOutInstalled(downloads).ToList(), Interface.modBrowserID);
	}

	public static IEnumerable<ModDownloadItem> FilterOutInstalled(IEnumerable<ModDownloadItem> downloads)
	{
		return downloads.Where(item => {
			if (item == null)
				return false;

			item.UpdateInstallState();
			return !item.IsInstalled || item.NeedUpdate;
		});
	}
}