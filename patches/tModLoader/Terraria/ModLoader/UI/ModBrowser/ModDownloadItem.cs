using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Core;
using Terraria.Social.Base;

namespace Terraria.ModLoader.UI.ModBrowser;

public class ModDownloadItem
{
	public readonly string ModName;
	public readonly string DisplayName;
	public readonly string DisplayNameClean; // No chat tags: for search and sort functionality.
	public readonly ModPubId_t PublishId;
	public readonly string OwnerId;
	public readonly Version Version;

	public readonly string Author;
	public readonly string ModIconUrl;
	public readonly DateTime TimeStamp;
	public readonly bool Banned;
	public readonly DeveloperMetadata DevMetadata;

	public readonly string ModReferencesBySlug;
	public readonly ModPubId_t[] ModReferenceByModId;

	public readonly ModSide ModSide;
	public readonly int Downloads;
	public readonly int Hot;
	public readonly string Homepage;
	public readonly Version ModloaderVersion;

	internal LocalMod Installed;
	public bool NeedUpdate { get; private set; }
	public bool AppNeedRestartToReinstall { get; private set; }

	public bool IsInstalled => Installed != null;

	public ModDownloadItem(string displayName, string name, Version version, string author, string modReferences, ModSide modSide, string modIconUrl, string publishId, int downloads, int hot, DateTime timeStamp, Version modloaderversion, string homepage, string ownerId, string[] referencesById, bool banned, DeveloperMetadata devMetadata)
	{
		ModName = name;
		DisplayName = displayName;
		DisplayNameClean = Utils.CleanChatTags(displayName);
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
		Banned = banned;
		DevMetadata = devMetadata;
	}

	internal void UpdateInstallState()
	{
		// Remember this method is blocking, it does network stuff... - DarioDaf

		// Check against installed mods for updates.
		//TODO: This should assess the source of the ModDownloadItem and ensure matches with the active SocialBrowserModule instance for safety, but eh.
		Installed = Interface.modBrowser.SocialBackend.IsItemInstalled(ModName);

		NeedUpdate = Installed != null && Interface.modBrowser.SocialBackend.DoesItemNeedUpdate(PublishId, Installed, Version);
		// The below line is to identify the transient state where it isn't installed, but Steam considers it as such - Solxan
		// Steam keeps a cache once a download starts, and doesn't clean up cache until game close, which gets very confusing.
		AppNeedRestartToReinstall = Installed == null && Interface.modBrowser.SocialBackend.DoesAppNeedRestartToReinstallItem(PublishId);
	}

	public override bool Equals(object obj) => Equals(obj as ModDownloadItem);

	// Custom Equality for Mod Browser efficiency
	private (string, string, Version) GetComparable()
	{
		return (ModName, PublishId.m_ModPubId, Version);
	}

	// Explicit Equals was required due to a bizarre issue where two ModDownloadItems with equal properties
	//	were not found equal in CachedInstalledModDownloadItems.Contains(item). - Solxan 2023-07-29
	public bool Equals(ModDownloadItem item)
	{
		if (item is null)
			return false;
		return GetComparable() == item.GetComparable();
	}

	public override int GetHashCode()
	{
		return GetComparable().GetHashCode();
	}

	public static IEnumerable<ModDownloadItem> NeedsInstallOrUpdate(IEnumerable<ModDownloadItem> downloads)
	{
		return downloads.Where(item => {
			if (item == null)
				return false;

			item.UpdateInstallState();
			return !item.IsInstalled || item.NeedUpdate;
		});
	}
}