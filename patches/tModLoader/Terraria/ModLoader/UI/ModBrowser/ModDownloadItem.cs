using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Core;
using Terraria.Social.Base;
using Terraria.Social.Steam;

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
	public readonly DateTime LastUpdatedTimeStamp;
	public readonly bool Banned;
	internal readonly DeveloperMetadata LongFormDevMetadata;
	internal readonly DeveloperMetadata BrowserVersionDevMetadata;

	public readonly string ModReferencesBySlug;
	public readonly ModPubId_t[] ModReferenceByModId;

	public readonly ModSide ModSide;
	public readonly int Downloads;
	public readonly int Hot;
	public readonly uint Upvotes;
	public readonly uint Downvotes;
	public readonly float VoteScore;
	public readonly string Homepage;
	public readonly Version ModloaderVersion;
	public readonly List<string> SupportedVersions;

	internal LocalMod Installed { get; set; }
	public bool NeedUpdate { get; private set; }
	public bool AppNeedRestartToReinstall { get; private set; }
	public bool IsInstalled => Installed != null;

	public ModDownloadItem(
		// Properties that are invariant with tml Browser Version
		string displayName, string name,  string author, string homepage,
		int downloads, int hot, string modIconUrl, string publishId, string ownerId,
		uint upvotes, uint downvotes, float voteScore, List<string> supportedVersions,
		bool banned, DeveloperMetadata longFormDevMetadata,	DeveloperMetadata browserVersionDevMetadata,
		DateTime lastUpdatedTimeStamp,

		// Properties that could be variant with tML Browser Version
		string modReferences, string[] referencesById, ModSide modSide,

		// Properties that are variant with tML Browser Version
		Version version, Version modloaderversion
		)
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
		LastUpdatedTimeStamp = lastUpdatedTimeStamp;
		Version = version;
		ModloaderVersion = modloaderversion;
		Banned = banned;
		LongFormDevMetadata = longFormDevMetadata;
		BrowserVersionDevMetadata = browserVersionDevMetadata;
		Upvotes = upvotes;
		Downvotes = downvotes;
		VoteScore = voteScore;

		UpdateInstallState();
	}

	internal void UpdateInstallState()
	{
		// Remember this method is blocking, it does network stuff... - DarioDaf

		// Check against installed mods for updates.
		//TODO: This should assess the source of the ModDownloadItem and ensure matches with the active SocialBrowserModule instance for safety, but eh.
		Installed = Interface.modBrowser.SocialBackend.IsItemInstalled(ModName);

		NeedUpdate = IsInstalled && Interface.modBrowser.SocialBackend.DoesItemNeedUpdate(PublishId, Installed, Version);
		
		// The below line is to identify the transient state where it isn't installed, but Steam considers it as such - Solxan
		// Steam keeps a cache once a download starts, and doesn't clean up cache until game close, which gets very confusing.
		AppNeedRestartToReinstall = Installed == null && Interface.modBrowser.SocialBackend.DoesAppNeedRestartToReinstallItem(PublishId);
	}

	internal bool IsReupload()
	{
		if (Installed is null)
			return false;

		if (!WorkshopHelper.GetPublishIdLocal(Installed.modFile, out var localPublishId))
			return false;

		return localPublishId.ToString() != PublishId.m_ModPubId;
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

	public List<ModVersionHash> GetKnownWorkshopVersionHashes()
	{
		return LongFormDevMetadata.modVersionHashes.Concat(BrowserVersionDevMetadata.modVersionHashes).ToList();
	}

	public static IEnumerable<ModDownloadItem> NeedsInstallOrUpdate(IEnumerable<ModDownloadItem> downloads)
	{
		return downloads.Where(item => {
			if (item == null)
				return false;

			return !item.IsInstalled || item.NeedUpdate;
		});
	}
}