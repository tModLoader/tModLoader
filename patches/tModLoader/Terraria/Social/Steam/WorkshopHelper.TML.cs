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

namespace Terraria.Social.Steam;

public partial class WorkshopHelper
{
	internal static string[] MetadataKeys = new string[8] { "name", "author", "modside", "homepage", "modloaderversion", "version", "modreferences", "versionsummary" };

	// TODO: Per latest testing by Solxan Sept 2025, this doesn't work anymore. replace with SetMetadata via WebAPI
	private static readonly Regex MetadataInDescriptionFallbackRegex = new Regex(@"\[quote=GithubActions\(Don't Modify\)\]Version Summary: (.*) \[/quote\]", RegexOptions.Compiled);

	public class ModPublisherInstance : UGCBased.APublisherInstance
	{
		protected override string GetHeaderText() => ModWorkshopEntry.GetHeaderTextFor(_publishedFileID.m_PublishedFileId, _entryData.Tags, _publicity, _entryData.PreviewImagePath);

		protected override void PrepareContentForUpdate() { }
	}

	internal enum WorkshopSearchReturnState
	{
		NotFound,
		SearchFailed,
		Success,
		RetrievalFailed
	}

	/// <summary>
	/// <code>
	/// Priority is given to "-steamworkshopfolder" argument to ensure if someone has a custom steamapps/workshop folder away from tml, it can be found
	/// If SteamClient is true (ie it is a steam user running a client or host and play),
	///		InstallDir: SteamFiles/Steamapps/common/tModLoader is GetAppInstallDir
	///		WorkshopFolder: SteamFiles/Steamapps/workshop is Path.Combine(GetAppInstallDir, .., .., Workshop)
	///	If SteamClient is false, SteamAvailable = True -> Is FamilyShare or GoG Client. SteamedWraps.FamilyShare differentiates if needed
	///		InstallDir: anywhere, manual.
	///		WorkshopFolder: InstallDir/Steamapps/workshop
	///	If Main.DedServ is true
	///		Use SteamClient reference path if it exists &amp;&amp; Not "-nosteam" supplied
	///		Use NotSteamClient working folder path if "-nosteam" supplied or SteamClient ref path not exists
	/// </code>
	/// </summary>
	public static string GetWorkshopFolder(AppId_t app)
	{
		if (Program.LaunchParameters.TryGetValue("-steamworkshopfolder", out string workshopLocCustom)) {
			if (Directory.Exists(workshopLocCustom))
				return workshopLocCustom;

			Logging.tML.Warn("-steamworkshopfolder path not found: " + workshopLocCustom);
		}

		string steamClientPath = null; // GetAppInstallDir may return null (#2491). Also the default location for dedicated servers and such
		if (SteamedWraps.SteamClient)
			SteamApps.GetAppInstallDir(app, out steamClientPath, 1000);
		steamClientPath ??= ".";
		steamClientPath = Path.Combine(steamClientPath, "..", "..", "workshop");

		if (SteamedWraps.SteamClient || !SteamedWraps.SteamAvailable && !Program.LaunchParameters.ContainsKey("-nosteam") && Directory.Exists(steamClientPath))
			return steamClientPath;

		return Path.Combine("steamapps", "workshop");
	}

	/////// Others ////////////////////
	// Should this be in SteamedWraps or here?
	internal static bool GetPublishIdLocal(TmodFile modFile, out ulong publishId)
	{
		publishId = 0;
		if (modFile == null || !ModOrganizer.TryReadManifest(ModOrganizer.GetParentDir(modFile.path), out var info))
			return false;

		publishId = info.workshopEntryId;
		return true;
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
	internal static void PublishMod(LocalMod mod, string iconPath, string[] publishTags = null, WorkshopItemPublicSettingId? publicity = null)
	{
		var modFile = mod.modFile;
		var bp = mod.properties;

		if (bp.side != ModSide.Server && Main.dedServ)
			throw new Exception(Language.GetTextValue("tModLoader.CommandLinePublishOnlyUsableWithServerSideMods"));

		if (bp.buildVersion != modFile.TModLoaderVersion)
			throw new WebException(Language.GetTextValue("tModLoader.OutdatedModCantPublishError"));

		var changeLogFile = Path.Combine(bp.modSource, "changelog.txt");
		string changeLog;
		if (File.Exists(changeLogFile))
			changeLog = File.ReadAllText(changeLogFile);
		else
			changeLog = "";

		var values = new NameValueCollection
		{
			{ "displayname", bp.displayName },
			{ "displaynameclean", Utils.CleanChatTags(bp.displayName) },
			{ "name", modFile.Name },
			{ "version", $"{bp.version}" },
			{ "author", bp.author },
			{ "homepage", bp.homepage },
			{ "description", bp.description },
			{ "iconpath", iconPath },
			{ "sourcesfolder", bp.modSource },
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
			try {
				// Command Line / Server Publishing.
				SocialAPI.Workshop = new WorkshopSocialModule();
				SocialAPI.Workshop.Initialize();

				if (!SteamedWraps.SteamClient)
					throw new Exception(Language.GetTextValue("tModLoader.CommandLinePublishNeedsSteam"));

				Thread.Sleep(1500); // Solxan: SteamAPI requires 1 or so seconds to initialize

				using (modFile.Open()) {
					HashSet<string> existingTags = [];
					if (SocialAPI.Workshop.TryGetInfoForMod(modFile, out var info)) {
						publicity = publicity ?? info.publicity;
						existingTags = info.tags.ToHashSet();

						// Remove all non-user selected tags (1.4.4, Server), they'll be added later.
						existingTags.IntersectWith(SteamedWraps.ModTags.Select(x => x.InternalNameForAPIs));
					}

					// Use existing Tags if none supplied
					HashSet<string> setTags = [];
					if (publishTags != null) {
						foreach (var publishTag in publishTags) {
							// Supports key without prefix or api internal name without spaces
							var foundTag = SteamedWraps.ModTags.FirstOrDefault(x =>
							string.Equals(x.NameKey.Replace("tModLoader.Tags", "").Replace("Language_", ""), publishTag, StringComparison.InvariantCultureIgnoreCase) ||
							string.Equals(x.InternalNameForAPIs.Replace(" ", ""), publishTag, StringComparison.InvariantCultureIgnoreCase));
							if (foundTag != null)
								setTags.Add(foundTag.InternalNameForAPIs);
							else
								Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.WorkshopTagNotFound", publishTag));
						}
					}
					else {
						setTags = existingTags;
					}

					// Localization Tags
					var autoLang = SocialBrowserModule.GetModLocalizationProgress(modFile, existingTags.Select(x => new WorkshopTagOption(x, x)).ToList());
					setTags.ExceptWith(autoLang.Where(a => !a.setState).Select(b => b.tag.InternalNameForAPIs));
					setTags.UnionWith(autoLang.Where(a => a.setState).Select(b => b.tag.InternalNameForAPIs));

					var publishSettings = new WorkshopItemPublishSettings {
						Publicity = publicity ?? WorkshopItemPublicSettingId.Public,
						UsedTags = setTags.Select(x => new WorkshopTagOption(x, x)).ToArray(),
						PreviewImagePath = iconPath
					};

					SocialAPI.Workshop.PublishMod(modFile, values, publishSettings);
				}
			}
			finally {
				SteamedWraps.OnGameExitCleanup();
			}
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
					numberPages = (int)Math.Ceiling((double)totalItemsQueried / Constants.kNumUGCResultsPerPage);
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
			/// This method does not guarantee that the returned items are 1 to 1 with the provided searchModIds list
			/// If the Mod is not found, it is added to the missingMods out list
			/// </summary>
			internal List<ModDownloadItem> QueryItemsSynchronously(out List<string> missingMods)
			{
				var numPages = Math.Ceiling(queryParameters.searchModIds.Length / (float)Constants.kNumUGCResultsPerPage);
				var items = new List<ModDownloadItem>();
				missingMods = new List<string>();

				for (int i = 0; i < numPages; i++) {
					var pageIds = queryParameters.searchModIds.Take(new Range(i * Constants.kNumUGCResultsPerPage, Constants.kNumUGCResultsPerPage * (i + 1)));
					var idArray = pageIds.Select(x => x.m_ModPubId).ToArray();

					try {
						WaitForQueryResult(SteamedWraps.GenerateDirectItemsQuery(idArray, queryParameters));

						for (int j = 0; j < _queryReturnCount; j++) {
							var itemsIndex = j + i * Constants.kNumUGCResultsPerPage;
							var match = TryGenerateModDownloadItem((uint)j, out var item);
							if (match != WorkshopSearchReturnState.Success) {

								// Currently, only known case is if a mod the user is subbed to is set to hidden & not deleted by the user
								Logging.tML.Warn($"Unable to find Mod with ID {idArray[j]} on the Steam Workshop");
								missingMods.Add(idArray[j]);
								continue;
							}

							item.UpdateInstallState();
							items.Add(item);
						}
					}
					finally {
						ReleaseWorkshopQuery();
					}
				}

				return items;
			}

			// This is used exclusively by Mod Browser and its SpecialMods filter as of Sept 21, 2025
			internal async IAsyncEnumerable<ModDownloadItem> QueryAllWorkshopItems([EnumeratorCancellation] CancellationToken token = default)
			{
				uint currentPage = 1;
				int currentPageAttempts = 0;
				do {
					token.ThrowIfCancellationRequested();
					try {
						try {
							await WaitForQueryResultAsync(SteamedWraps.GenerateAndSubmitModBrowserQuery(currentPage, queryParameters), token);
						}
						catch {
							if (currentPageAttempts == 1)
								throw;

							await Task.Delay(100, token);
							currentPage--;
							currentPageAttempts++;
							continue;
						}

						foreach (var item in await Task.Run(ProcessPageResult))
							yield return item;
					}
					finally {
						ReleaseWorkshopQuery();
					}

					currentPageAttempts = 0;
				} while (++currentPage <= numberPages);
			}

			private IEnumerable<ModDownloadItem> ProcessPageResult()
			{
				// Appx. 10 ms per page of 50 items
				for (uint i = 0; i < _queryReturnCount; i++) {
					var state = TryGenerateModDownloadItem(i, out var mod);
					if (state != WorkshopSearchReturnState.Success || mod.Banned)
						continue;

					yield return mod;
				}
			}

			//TODO: This Method and it's upstream callers needs work to remove default passed values. Deferred during PR #3346
			internal static bool TryGetGroupPublishIdsByInternalName(QueryParameters query, out List<string> modIds)
			{
				modIds = new List<string>();

				var queryHandle = new QueryHelper.AQueryInstance(query);
				// default search of all public mods. If fails, returns false
				if (!queryHandle.TryGroupSearchByInternalName(out var items))
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

			//TODO: This Method and it's upstream callers needs work to remove default passed values. Deferred during PR #3346
			/// <summary>
			/// Only Use if we don't have a PublishID source.
			/// Outputs a List of ModDownloadItems of equal length to QueryParameters.SearchModSlugs
			/// Uses null entries to fill gaps to ensure length consistency
			/// </summary>
			private bool TryGroupSearchByInternalName(out List<ModDownloadItem> items)
			{
				items = new List<ModDownloadItem>();

				foreach (var slug in queryParameters.searchModSlugs) {
					var state = TrySearchByInternalName(slug, out var item);
					if (state == WorkshopSearchReturnState.SearchFailed)
						return false;

					if (state == WorkshopSearchReturnState.Success && !item.Banned)
						items.Add(item);
					else
						items.Add(null);

				}

				return true;
			}

			internal static WorkshopSearchReturnState TryGetModDownloadItem(string modSlug, out ModDownloadItem item)
			{
				var query = new AQueryInstance(new QueryParameters() { queryType = QueryType.SearchDirect, returnDevMetadata = true });

				var state = query.TrySearchByInternalName(modSlug, out item);
				if (state == WorkshopSearchReturnState.SearchFailed || state == WorkshopSearchReturnState.Success)
					return state;

				// search of all mods that were published by this user - Does not work with co-published/non-owner
				// this should catch private / friends-only visibility
				query.queryParameters.queryType = QueryType.SearchUserPublishedOnly;
				state = query.TrySearchByInternalName(modSlug, out item);

				return state;
			}

			/// <summary>
			/// Only Use if we don't have a PublishID source.
			/// This method does NOT guard against banned items. Plan usage accordingly
			/// </summary>
			private WorkshopSearchReturnState TrySearchByInternalName(string slug, out ModDownloadItem item)
			{
				item = null;

				try {
					WaitForQueryResult(SteamedWraps.GenerateAndSubmitModBrowserQuery(page: 1, queryParameters, internalName: slug));

					if (_queryReturnCount == 0) {
						Logging.tML.Info($"No Mod on Workshop with internal name: {slug}");
						return WorkshopSearchReturnState.NotFound;
					}

					var match = TryGenerateModDownloadItem(0, out item);

					return match;
				}
				catch {
					// If Query Fails, we can't publish
					return WorkshopSearchReturnState.SearchFailed;
				}
				finally {
					ReleaseWorkshopQuery();
				}
			}

			/////// Run Queries ////////////////////

			internal async Task WaitForQueryResultAsync(SteamAPICall_t query, CancellationToken token = default)
			{
				_primaryQueryResult = EResult.k_EResultNone;
				_queryHook.Set(query);

				var stopwatch = Stopwatch.StartNew();
				while (true) {
					SteamedWraps.RunCallbacks();
					if (_primaryQueryResult != EResult.k_EResultNone)
						break;

					if (stopwatch.Elapsed.TotalSeconds >= 10)
						throw new TimeoutException("No response from steam workshop query");

					await Task.Delay(1, token);
				}

				if (_primaryQueryResult != EResult.k_EResultOK) {
					SteamedWraps.ReportCheckSteamLogs();
					throw new Exception($"Error: Unable to access Steam Workshop. ERROR CODE: {_primaryQueryResult}");
				}
			}

			[Obsolete("Should not be used because it hides syncronous waiting")]
			internal void WaitForQueryResult(SteamAPICall_t query)
			{
				WaitForQueryResultAsync(query).GetAwaiter().GetResult();
			}

			/////// Process Query Result per Item ////////////////////

			internal WorkshopSearchReturnState TryGenerateModDownloadItem(uint index, out ModDownloadItem item)
			{
				item = null;
				try {
					item = GenerateModDownloadItemFromQuery(index);
					return WorkshopSearchReturnState.Success;
				}
				catch (Exception) {
					return WorkshopSearchReturnState.RetrievalFailed;
				}
			}

			private ModDownloadItem GenerateModDownloadItemFromQuery(uint i)
			{
				// Item Result call data
				SteamUGCDetails_t pDetails = SteamedWraps.FetchItemDetails(_primaryUGCHandle, i);

				PublishedFileId_t id = pDetails.m_nPublishedFileId;

				if (pDetails.m_eResult != EResult.k_EResultOK) {
					throw new SocialBrowserException("Unable to fetch mod PublishId#" + id + " information. " + pDetails.m_eResult);
				}

				string ownerId = pDetails.m_ulSteamIDOwner.ToString();

				DateTime lastUpdate = Utils.UnixTimeStampToDateTime((long)pDetails.m_rtimeUpdated);
				string displayname = pDetails.m_rgchTitle;
				bool banned = pDetails.m_bBanned;

				// Item Tagged data / Player metadata
				SteamedWraps.FetchMetadata(_primaryUGCHandle, i, out var metadata);

				// Developer Metadata
				SteamedWraps.FetchDeveloperMetadata(_primaryUGCHandle, i, out string devMetadataSerialized);
				var devMetadata = DeveloperMetadata.Deserialize(devMetadataSerialized);

				// Backwards compat code for the metadata version change
				if (metadata["versionsummary"] == null)
					metadata["versionsummary"] = metadata["version"];

				string[] missingKeys = MetadataKeys.Where(k => metadata.Get(k) == null).ToArray();

				if (missingKeys.Length != 0) {
					throw new SocialBrowserException($"Mod {id}: '{displayname}' is missing required metadata: {string.Join(',', missingKeys.Select(k => $"'{k}'"))}.");
				}

				if (string.IsNullOrWhiteSpace(metadata["name"])) {
					throw new SocialBrowserException($"Mod has no internal name / slug: {id}: {displayname}"); // Somehow this happened before and broke mod downloads
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

				return new ModDownloadItem(displayname, metadata["name"], cVersion.modV, metadata["author"], metadata["modreferences"], modside, modIconURL, id.m_PublishedFileId.ToString(), (int)downloads, (int)hot, lastUpdate, cVersion.tmlV, metadata["homepage"], ownerId, refsById, banned, devMetadata);
			}
		}
	}
}
