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
		if (Directory.Exists(workshopLoc) && !Program.LaunchParameters.ContainsKey("-nosteam"))
			return workshopLoc;

		// Load mods installed by GoG / Manually copied steamapps\workshop directories.
		return Path.Combine("steamapps", "workshop");
	}

	/////// Workshop Dependencies ////////////////////

	/*
	private static List<ulong> GetDependencies(ulong publishedId)
	{
		var query = new QueryHelper.AQueryInstance(new QueryParameters());
		query.FastQueryItem(publishedId);
		return query.ugcChildren;
	}

	internal static void GetDependenciesRecursive(ulong publishedId, ref HashSet<ulong> set)
	{
		var deps = GetDependencies(publishedId);
		set.UnionWith(deps);

		foreach (ulong dep in deps)
			GetDependenciesRecursive(dep, ref set);
	}
	*/

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
	private static (System.Version modV, string tmlV) CalculateRelevantVersion(string mbDescription, NameValueCollection metadata)
	{
		(System.Version modV, string tmlV) selectVersion = new(new System.Version(metadata["version"].Replace("v", "")), metadata["modloaderversion"]);
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
	private static void InnerCalculateRelevantVersion(ref (System.Version modV, string tmlV) selectVersion, string versionSummary)
	{
		foreach (var item in VersionSummaryToArray(versionSummary)) {
			if (selectVersion.modV < item.modVersion && BuildInfo.tMLVersion.MajorMinor() >= item.tmlVersion.MajorMinor()) {
				selectVersion.modV = item.modVersion;
				selectVersion.tmlV = item.tmlVersion.MajorMinor().ToString();
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
		/////// Workshop Special Statics ////////////////////

		internal const int QueryPagingConst = Steamworks.Constants.kNumUGCResultsPerPage;
		internal static int IncompleteModCount;
		internal static int HiddenModCount;
		internal static uint TotalItemsQueried;

		/////// Used for making code hear easier on common calls ////////////////////
		internal static List<ModDownloadItem> Items => WorkshopBrowserModule.Instance.Items;
		internal static IReadOnlyList<LocalMod> InstalledMods => WorkshopBrowserModule.Instance.InstalledItems;


		/////// Used for Publishing ////////////////////
		internal static bool TryGetPublishIdByInternalName(QueryParameters query, out List<string> modIds)
		{
			modIds = new List<string>();

			var queryHandle = new AQueryInstance(query);
			if (!queryHandle.TrySearchByInternalName(out List<ModDownloadItem> items))
				return false;

			for (int i = 0; i < query.searchModSlugs.Count; i++) {
				modIds.Add(items[i] == null ? "0" : items[i].PublishId);
			}

			return true;
		}

		/*
		internal static ulong GetSteamOwner(string modId)
		{
			var mod = new AQueryInstance(new ).FastQueryItem(modId);
			return pDetails.m_ulSteamIDOwner;
		}
		*/

		// Yield returns null if an error happens and the result is cut short
		internal static async IAsyncEnumerable<ModDownloadItem> QueryWorkshop(QueryParameters queryParams, CancellationToken token)
		{
			HiddenModCount = IncompleteModCount = 0;
			TotalItemsQueried = 0;
			Items.Clear();

			if (!SteamedWraps.SteamAvailable) {
				if (!SteamedWraps.TryInitViaGameServer()) {
					Utils.ShowFancyErrorMessage(Language.GetTextValue("tModLoader.NoWorkshopAccess"), 0);
					yield return null;
				}

				var start = DateTime.Now; // lets wait a few seconds for steam to actually init. It if times out, then another query later will fail, oh well :|
				while (!SteamGameServer.BLoggedOn() && (DateTime.Now - start) < TimeSpan.FromSeconds(5)) {
					SteamedWraps.ForceCallbacks();
				}
			}

			var queryHandle = new AQueryInstance(queryParams);

			await foreach (var item in queryHandle.QueryAllWorkshopItems(token)) {
				if (item is not null)
					Items.Add(item);
				yield return item;
			}
		}

		internal class AQueryInstance
		{
			private CallResult<SteamUGCQueryCompleted_t> _queryHook;
			protected UGCQueryHandle_t _primaryUGCHandle;
			protected EResult _primaryQueryResult;
			protected uint _queryReturnCount;
			protected string _nextCursor;
			internal List<ulong> ugcChildren = new List<ulong>();
			internal bool stopCurrentQuery;
			internal QueryParameters queryParameters;

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
				_nextCursor = pCallback.m_rgchNextCursor;

				if (TotalItemsQueried == 0 && pCallback.m_unTotalMatchingResults > 0)
					TotalItemsQueried = pCallback.m_unTotalMatchingResults;
			}

			/// <summary>
			/// Ought be called to release the existing query when we are done with it. Frees memory associated with the handle.
			/// </summary>
			private void ReleaseWorkshopQuery()
			{
				SteamedWraps.ReleaseWorkshopHandle(_primaryUGCHandle);
			}

			/// <summary>
			/// For direct information gathering of a particular mod/workshop item
			/// </summary>
			internal ModDownloadItem FastQueryItem(string modId, out string modOwner)
			{
				TryRunQuery(SteamedWraps.GenerateDirectItemsQuery(new string[] { modId }));

				var result = GenerateModDownloadItemFromQuery(0);
				modOwner = "";
				
				ReleaseWorkshopQuery();
				return result;
			}

			internal async IAsyncEnumerable<ModDownloadItem> QueryAllWorkshopItems([EnumeratorCancellation] CancellationToken token = default)
			{
				do {
					token.ThrowIfCancellationRequested();

					// Appx. 0.5 seconds per page of 50 items during testing. No way to parallelize.
					//TODO: Review an upgrade of ModBrowser to load items over time (ie paging Mod Browser).

					string currentPage = _nextCursor;
					if (!TryRunQuery(SteamedWraps.GenerateModBrowserQuery(currentPage, queryParameters))) {
						ReleaseWorkshopQuery();

						// If it failed, make a second attempt after 100 ms
						await Task.Delay(100, token);
						if (!TryRunQuery(SteamedWraps.GenerateModBrowserQuery(currentPage, queryParameters))) {
							ReleaseWorkshopQuery();
							// Exit for error fetching stuff (will leave the status as not complete (could in alternative throw an error for clearer info)
							yield return null;
							yield break;
						}
					}

					// Appx. 10 ms per page of 50 items
					foreach (var item in ProcessPageResult())
						yield return item;

					ReleaseWorkshopQuery();
				} while (TotalItemsQueried != Items.Count + IncompleteModCount + HiddenModCount && !stopCurrentQuery);
			}

			// Only use if we don't have a guaranteed PublishID source
			internal bool TrySearchByInternalName(out List<ModDownloadItem> items)
			{
				string currentPage = _nextCursor;
				items = new List<ModDownloadItem>();

				foreach (var slug in queryParameters.searchModSlugs) {
					// If Query Fails, we can't publish.
					if (!TryRunQuery(SteamedWraps.GenerateModBrowserQuery(currentPage, queryParameters, internalName: slug))) {
						ReleaseWorkshopQuery();
						return false;
					}

					if (_queryReturnCount == 0) {
						items.Add(null);
						ReleaseWorkshopQuery();
						continue;
					}

					items.Add(GenerateModDownloadItemFromQuery(0));
					ReleaseWorkshopQuery();
				}

				return true;
			}

			internal bool TryRunQuery(SteamAPICall_t query)
			{
				_primaryQueryResult = EResult.k_EResultNone;
				_queryHook.Set(query);

				var stopwatch = Stopwatch.StartNew();
				do {
					if (stopwatch.Elapsed.TotalSeconds >= 10) // 10 seconds maximum allotted time before no connection is assumed
						_primaryQueryResult = EResult.k_EResultTimeout;

					SteamedWraps.ForceCallbacks();
				}
				while (_primaryQueryResult == EResult.k_EResultNone);

				return HandleError(_primaryQueryResult);
			}

			internal static bool HandleError(EResult eResult)
			{
				if (eResult == EResult.k_EResultOK || eResult == EResult.k_EResultNone)
					return true;

				if (eResult == EResult.k_EResultAccessDenied) {
					Utils.ShowFancyErrorMessage("Error: Access to Steam Workshop was denied.", 0);
				}
				else if (eResult == EResult.k_EResultTimeout) {
					Utils.ShowFancyErrorMessage("Error: Operation Timed Out. No callback received from Steam Servers.", 0);
				}
				else {
					Utils.ShowFancyErrorMessage("Error: Unable to access Steam Workshop. " + eResult, 0);
					SteamedWraps.ReportCheckSteamLogs();
				}
				return false;
			}

			internal ModDownloadItem GenerateModDownloadItemFromQuery(uint i)
			{
				// Item Result call data
				SteamUGCDetails_t pDetails = SteamedWraps.FetchItemDetails(_primaryUGCHandle, i);

				PublishedFileId_t id = pDetails.m_nPublishedFileId;

				if (pDetails.m_eVisibility != ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic) {
					HiddenModCount++;
					return null;
				}

				if (pDetails.m_eResult != EResult.k_EResultOK) {
					Logging.tML.Warn("Unable to fetch mod PublishId#" + id + " information. " + pDetails.m_eResult);
					HiddenModCount++;
					return null;
				}

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
					IncompleteModCount++;
					return null;
				}

				if (string.IsNullOrWhiteSpace(metadata["name"])) {
					Logging.tML.Warn($"Mod has no name: {id}"); // Somehow this happened before and broke mod downloads
					IncompleteModCount++;
					return null;
				}

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

				return new ModDownloadItem(displayname, metadata["name"], cVersion.modV.ToString(), metadata["author"], metadata["modreferences"], modside, modIconURL, id.m_PublishedFileId.ToString(), (int)downloads, (int)hot, lastUpdate, cVersion.tmlV, metadata["homepage"]);
			}

			private IEnumerable<ModDownloadItem> ProcessPageResult()
			{
				for (uint i = 0; i < _queryReturnCount; i++) {
					var mod = GenerateModDownloadItemFromQuery(i);
					if (mod == null)
						continue;

					yield return mod;
				}
			}
		}
	}
}
