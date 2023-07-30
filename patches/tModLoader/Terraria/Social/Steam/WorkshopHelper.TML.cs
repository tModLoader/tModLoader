using Microsoft.Xna.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;
using Terraria.UI.Chat;

namespace Terraria.Social.Steam;

public partial class WorkshopHelper
{
	internal static string[] MetadataKeys = new string[8] { "name", "author", "modside", "homepage", "modloaderversion", "version", "modreferences", "versionsummary" };
	private static readonly Regex MetadataInDescriptionFallbackRegex = new Regex(@"\[quote=GithubActions\(Don't Modify\)\]Version Summary: (.*) \[/quote\]", RegexOptions.Compiled);

	public class ModPublisherInstance : UGCBased.APublisherInstance
	{
		protected override string GetHeaderText() => ModWorkshopEntry.GetHeaderTextFor(_publishedFileID.m_PublishedFileId, _entryData.Tags, _publicity, _entryData.PreviewImagePath);

		protected override void PrepareContentForUpdate() { }
	}

	/////// Workshop Mod Download Location ////////////////////
	public static string GetWorkshopFolder(AppId_t app)
	{
		if (Program.LaunchParameters.TryGetValue("-steamworkshopfolder", out string workshopLocCustom)) {
			if (Directory.Exists(workshopLocCustom))
				return workshopLocCustom;

			Logging.tML.Warn("-steamworkshopfolder path not found: " + workshopLocCustom);
		}

		string installLoc = null;
		if (SteamedWraps.SteamClient) // get app install dir if possible
			SteamApps.GetAppInstallDir(app, out installLoc, 1000);

		installLoc ??= "."; // GetAppInstallDir may return null (#2491). Also the default location for dedicated servers and such

		var workshopLoc = Path.Combine(installLoc, "..", "..", "workshop");
		if (Directory.Exists(workshopLoc) && !Program.LaunchParameters.ContainsKey("-nosteam") && !SteamedWraps.FamilyShared)
			return workshopLoc;

		// Load mods installed by GoG / Manually copied steamapps\workshop directories.
		return Path.Combine("steamapps", "workshop");
	}

	/////// Others ////////////////////

	internal static ModDownloadItem GetModDownloadItem(string modSlug)
	{
		var query = new QueryHelper.AQueryInstance(new QueryParameters() { searchModSlugs = new string[] { modSlug } });
		if (!query.TrySearchByInternalName(out var items))
			return null;

		return items[0];
	}

	// Should this be in SteamedWraps or here?
	internal static bool GetPublishIdLocal(TmodFile modFile, out ulong publishId)
	{
		publishId = 0;
		if (modFile == null || !ModOrganizer.TryReadManifest(ModOrganizer.GetParentDir(modFile.path), out var info))
			return false;

		publishId = info.workshopEntryId;
		return true;
	}

	/////// Used for Publishing ////////////////////
	internal static bool TryGetPublishIdByInternalName(QueryParameters query, out List<string> modIds)
	{
		modIds = new List<string>();

		var queryHandle = new QueryHelper.AQueryInstance(query);
		if (!queryHandle.TrySearchByInternalName(out List<ModDownloadItem> items))
			return false;

		for (int i = 0; i < query.searchModSlugs.Length; i++) {
			if (items[i] is null) {
				Logging.tML.Info($"Unable to find the PublishID for {query.searchModSlugs[i]}");
				modIds.Add("0");
			}
			else
				modIds.Add(items[i].PublishId.m_ModPubId);
		}

		return true;
	}

	internal static ulong GetSteamOwner(string modId)
	{
		var mods = new QueryHelper.AQueryInstance(new QueryParameters() { searchModIds = new ModPubId_t[] { new ModPubId_t() { m_ModPubId = modId } } }).QueryItemsSynchronously();
		return ulong.Parse(mods[0].OwnerId);
	}

	/////// Workshop Version Calculation Helpers ////////////////////
	private static (System.Version modV, System.Version tmlV) CalculateRelevantVersion(string mbDescription, NameValueCollection metadata)
	{
		(System.Version modV, System.Version tmlV) selectVersion = new(new System.Version(metadata["version"].Replace("v", "")), new System.Version(metadata["modloaderversion"].Replace("tModLoader v", "")));
		// Backwards compat after metadata version change
		if (!metadata["versionsummary"].Contains(':'))
			return selectVersion;

		InnerCalculateRelevantVersion(ref selectVersion, metadata["versionsummary"]);

		// Handle Github Actions metadata from description
		// Nominal string: [quote=GithubActions(Don't Modify)]Version Summary: YYYY.MM:#.#.#.#;YYYY.MM:#.#.#.#;... [/quote]
		Match match = MetadataInDescriptionFallbackRegex.Match(mbDescription);
		if (match.Success) {
			InnerCalculateRelevantVersion(ref selectVersion, (match.Groups[1].Value));
		}

		return selectVersion;
	}

	// This and VersionSummaryToArray need a refactor for cleaner code. Not bad for now
	private static void InnerCalculateRelevantVersion(ref (System.Version modV, System.Version tmlV) selectVersion, string versionSummary)
	{
		foreach (var item in VersionSummaryToArray(versionSummary)) {
			if (item.tmlVersion.MajorMinor() > BuildInfo.tMLVersion.MajorMinor())
				continue;

			if (selectVersion.modV < item.modVersion || selectVersion.tmlV.MajorMinor() < item.tmlVersion.MajorMinor()) {
				selectVersion.modV = item.modVersion;
				selectVersion.tmlV = item.tmlVersion; //item.tmlVersion.MajorMinor().ToString();
			}
		}
	}

	private static (System.Version tmlVersion, System.Version modVersion)[] VersionSummaryToArray(string versionSummary)
	{
		return versionSummary.Split(";").Select(s => (new System.Version(s.Split(":")[0]), new System.Version(s.Split(":")[1]))).ToArray();
	}

	//////// Workshop Publishing ////////////////////
	internal static void PublishMod(LocalMod mod, string iconPath)
	{
		var modFile = mod.modFile;
		var bp = mod.properties;

		if (bp.buildVersion != modFile.TModLoaderVersion)
			throw new WebException(Language.GetTextValue("OutdatedModCantPublishError.BetaModCantPublishError"));

		var changeLogFile = Path.Combine(ModCompile.ModSourcePath, modFile.Name, "changelog.txt");
		string changeLog;
		if (File.Exists(changeLogFile))
			changeLog = File.ReadAllText(changeLogFile);
		else
			changeLog = "";

		var workshopDescFile = Path.Combine(ModCompile.ModSourcePath, modFile.Name, "description_workshop.txt");
		string workshopDesc;
		if (File.Exists(workshopDescFile))
			workshopDesc = File.ReadAllText(workshopDescFile);
		else
			workshopDesc = bp.description;

		var values = new NameValueCollection
		{
			{ "displayname", bp.displayName },
			{ "displaynameclean", string.Join("", ChatManager.ParseMessage(bp.displayName, Color.White).Where(x => x.GetType() == typeof(TextSnippet)).Select(x => x.Text)) },
			{ "name", modFile.Name },
			{ "version", $"{bp.version}" },
			{ "author", bp.author },
			{ "homepage", bp.homepage },
			{ "description", workshopDesc },
			{ "iconpath", iconPath },
			{ "sourcesfolder", Path.Combine(ModCompile.ModSourcePath, modFile.Name) },
			{ "modloaderversion", $"{modFile.TModLoaderVersion}" },
			{ "modreferences", string.Join(", ", bp.modReferences.Select(x => x.mod)) },
			{ "modside", bp.side.ToFriendlyString() },
			{ "changelog" , changeLog }
		};

		if (string.IsNullOrWhiteSpace(values["author"]))
			throw new WebException($"You need to specify an author in build.txt");

		if (string.IsNullOrWhiteSpace(values["version"]))
			throw new WebException($"You need to specify a version in build.txt");

		if (!Main.dedServ) {
			Main.MenuUI.SetState(new WorkshopPublishInfoStateForMods(Interface.modSources, modFile, values));
		}
		else {
			SocialAPI.LoadSteam();

			var publishSetttings = new WorkshopItemPublishSettings {
				Publicity = WorkshopItemPublicSettingId.Public,
				UsedTags = Array.Empty<WorkshopTagOption>(),
				PreviewImagePath = iconPath
			};
			SteamedWraps.SteamClient = true;
			SocialAPI.Workshop.PublishMod(modFile, values, publishSetttings);
		}
	}

	internal static class QueryHelper
	{
		/////// Used for Mod Browser ////////////////////

		internal static async IAsyncEnumerable<ModDownloadItem> QueryWorkshop(QueryParameters queryParams, [EnumeratorCancellation] CancellationToken token)
		{
			var queryHandle = new AQueryInstance(queryParams);

			await foreach (var item in queryHandle.QueryAllWorkshopItems(token)) {
				if (item is not null)
					yield return item;
			}
		}

		/////// Used for Query Caching per Steam Requirements ////////////////////

		internal class AQueryInstance
		{
			private CallResult<SteamUGCQueryCompleted_t> _queryHook;
			protected UGCQueryHandle_t _primaryUGCHandle;
			protected EResult _primaryQueryResult;
			protected uint _queryReturnCount;
			internal List<ulong> ugcChildren = new List<ulong>();
			internal QueryParameters queryParameters;

			internal int numberPages = 0;
			internal uint totalItemsQueried = 0;

			/////// Query basic implemenatation ////////////////////

			internal AQueryInstance(QueryParameters queryParameters)
			{
				_queryHook = CallResult<SteamUGCQueryCompleted_t>.Create(OnWorkshopQueryInitialized);
				this.queryParameters = queryParameters;
			}

			private void OnWorkshopQueryInitialized(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
			{
				_primaryUGCHandle = pCallback.m_handle;
				_primaryQueryResult = pCallback.m_eResult;
				_queryReturnCount = pCallback.m_unNumResultsReturned;

				if (totalItemsQueried == 0 && pCallback.m_unTotalMatchingResults > 0) {
					totalItemsQueried = pCallback.m_unTotalMatchingResults;
					numberPages = ((int)totalItemsQueried / Constants.kNumUGCResultsPerPage) + 1; 
				}
					
			}

			/// <summary>
			/// Ought be called to release the existing query when we are done with it. Frees memory associated with the handle.
			/// </summary>
			private void ReleaseWorkshopQuery()
			{
				SteamedWraps.ReleaseWorkshopHandle(_primaryUGCHandle);
			}

			/////// Queries ////////////////////

			/// <summary>
			/// For direct information gathering of particular mod/workshop items. Synchronous.
			/// Array Sizes are left One To One. If the Mod is not found, the array space is filled with a null.
			/// </summary>
			internal ModDownloadItem[] QueryItemsSynchronously()
			{
				var numPages = Math.Ceiling(queryParameters.searchModIds.Length / (float)Constants.kNumUGCResultsPerPage);
				var items = new ModDownloadItem[queryParameters.searchModIds.Length];

				for (int i = 0; i < numPages; i++) {
					var pageIds = queryParameters.searchModIds.Take(new Range(i * Constants.kNumUGCResultsPerPage, Constants.kNumUGCResultsPerPage * (i + 1) - 1));
					var idArray = pageIds.Select(x => x.m_ModPubId).ToArray();

					if (!TryRunQuery(SteamedWraps.GenerateDirectItemsQuery(idArray))) {
						throw new Exception($"Unexpectedly failed to query information reqarding items {pageIds}.");
					}

					for (int j = 0; j < i * Constants.kNumUGCResultsPerPage + _queryReturnCount; j++) {
						items[j] = GenerateModDownloadItemFromQuery((uint)j);
						if (items[j] is null) {
							Logging.tML.Info($"Unable to find Mod with ID {idArray[j]}");
							continue;
						}

						items[j].UpdateInstallState();
					}

					ReleaseWorkshopQuery();
				}
				
				return items;
			}

			internal async IAsyncEnumerable<ModDownloadItem> QueryAllWorkshopItems([EnumeratorCancellation] CancellationToken token = default)
			{
				uint currentPage = 1;
				do {
					token.ThrowIfCancellationRequested();

					if (!await TryRunQueryAsync(SteamedWraps.GenerateAndSubmitModBrowserQuery(currentPage, queryParameters), token)) {
						ReleaseWorkshopQuery();

						// If it failed, make a second attempt after 100 ms
						await Task.Delay(100, token);
						// @TODO: "Solxan" This code blocks because is not an `await` and doesn't use async sleeps
						if (!await TryRunQueryAsync(SteamedWraps.GenerateAndSubmitModBrowserQuery(currentPage, queryParameters), token)) {
							ReleaseWorkshopQuery();
							// Exit for error fetching stuff
							throw new SocialBrowserException("Workshop Query Failed");
						}
					}

					foreach (var item in await Task.Run(ProcessPageResult))
						yield return item;

					ReleaseWorkshopQuery();
				} while (++currentPage <= numberPages);
			}

			private IEnumerable<ModDownloadItem> ProcessPageResult()
			{
				// Appx. 10 ms per page of 50 items
				for (uint i = 0; i < _queryReturnCount; i++) {
					var mod = GenerateModDownloadItemFromQuery(i);
					if (mod == null)
						continue;

					yield return mod;
				}
			}

			/// <summary>
			/// Only Use if we don't have a PublishID source.
			/// Outputs a List of ModDownloadItems of equal length to QueryParameters.SearchModSlugs
			/// Uses null entries to fill gaps to ensure length consistency
			/// </summary>
			internal bool TrySearchByInternalName(out List<ModDownloadItem> items)
			{
				items = new List<ModDownloadItem>();

				foreach (var slug in queryParameters.searchModSlugs) {
					// If Query Fails, we can't publish.
					if (!TryRunQuery(SteamedWraps.GenerateAndSubmitModBrowserQuery(page: 1, queryParameters, internalName: slug))) {
						ReleaseWorkshopQuery();
						
						return false;
					}

					if (_queryReturnCount == 0) {
						Logging.tML.Info($"No Mod on Workshop with internal name: {slug}");
						items.Add(null);
						ReleaseWorkshopQuery();
						continue;
					}

					items.Add(GenerateModDownloadItemFromQuery(0));
					ReleaseWorkshopQuery();
				}

				return true;
			}

			/////// Run Queries ////////////////////

			internal async Task<bool> TryRunQueryAsync(SteamAPICall_t query, CancellationToken token = default)
			{
				_primaryQueryResult = EResult.k_EResultNone;
				_queryHook.Set(query);

				Stopwatch stopwatch = Stopwatch.StartNew();
				do {
					await SteamedWraps.ForceCallbacks(token);
				} while (
					(_primaryQueryResult == EResult.k_EResultNone) &&
					(stopwatch.Elapsed.TotalSeconds < 10)
				);
				if (_primaryQueryResult == EResult.k_EResultNone) {
					_primaryQueryResult = EResult.k_EResultTimeout;
				}

				// @TODO: HandleError calls show fancy page and UI stuff, is this ok since it is scheduled?
				// Also shouldn't all the UI and handling happen in the browser and not in Workshop methods?
				return HandleError(_primaryQueryResult);
			}

			[Obsolete("Should not be used because it hides syncronous waiting")]
			internal bool TryRunQuery(SteamAPICall_t query)
			{
				return TryRunQueryAsync(query).GetAwaiter().GetResult();
			}

			internal static bool HandleError(EResult eResult)
			{
				if (eResult == EResult.k_EResultOK || eResult == EResult.k_EResultNone)
					return true;

				SteamedWraps.ReportCheckSteamLogs();
				throw new Exception($"Error: Unable to access Steam Workshop. ERROR CODE: {eResult}");
			}

			/////// Process Query Result per Item ////////////////////

			internal ModDownloadItem GenerateModDownloadItemFromQuery(uint i)
			{
				// Item Result call data
				SteamUGCDetails_t pDetails = SteamedWraps.FetchItemDetails(_primaryUGCHandle, i);

				PublishedFileId_t id = pDetails.m_nPublishedFileId;

				if (pDetails.m_eVisibility != ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic) {
					return null;
				}

				if (pDetails.m_eResult != EResult.k_EResultOK) {
					Logging.tML.Warn("Unable to fetch mod PublishId#" + id + " information. " + pDetails.m_eResult);
					return null;
				}

				string ownerId = pDetails.m_ulSteamIDOwner.ToString();

				DateTime lastUpdate = Utils.UnixTimeStampToDateTime((long)pDetails.m_rtimeUpdated);
				string displayname = pDetails.m_rgchTitle;

				// Item Tagged data
				SteamedWraps.FetchMetadata(_primaryUGCHandle, i, out var metadata);

				// Backwards compat code for the metadata version change
				if (metadata["versionsummary"] == null)
					metadata["versionsummary"] = metadata["version"];

				string[] missingKeys = MetadataKeys.Where(k => metadata.Get(k) == null).ToArray();

				if (missingKeys.Length != 0) {
					Logging.tML.Warn($"Mod '{displayname}' is missing required metadata: {string.Join(',', missingKeys.Select(k => $"'{k}'"))}.");
					return null;
				}

				if (string.IsNullOrWhiteSpace(metadata["name"])) {
					Logging.tML.Warn($"Mod has no name: {id}"); // Somehow this happened before and broke mod downloads
					return null;
				}

				string[] refsById = SteamedWraps.FetchItemDependencies(_primaryUGCHandle, i, pDetails.m_unNumChildren).Select(x => x.m_PublishedFileId.ToString()).ToArray();

				// Partial Description - we don't include Long Description so this is only first handful of characters
				string description = pDetails.m_rgchDescription;

				var cVersion = CalculateRelevantVersion(description, metadata);

				// Assign ModSide Enum
				ModSide modside = ModSide.Both;

				if (metadata["modside"] == "Client")
					modside = ModSide.Client;

				if (metadata["modside"] == "Server")
					modside = ModSide.Server;

				if (metadata["modside"] == "NoSync")
					modside = ModSide.NoSync;

				// Preview Image url
				SteamedWraps.FetchPreviewImageUrl(_primaryUGCHandle, i, out string modIconURL);

				// Item Statistics
				SteamedWraps.FetchPlayTimeStats(_primaryUGCHandle, i, out var hot, out var downloads);

				return new ModDownloadItem(displayname, metadata["name"], cVersion.modV, metadata["author"], metadata["modreferences"], modside, modIconURL, id.m_PublishedFileId.ToString(), (int)downloads, (int)hot, lastUpdate, cVersion.tmlV, metadata["homepage"], ownerId, refsById);
			}
		}
	}
}
