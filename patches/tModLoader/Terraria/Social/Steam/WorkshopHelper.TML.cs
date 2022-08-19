using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;

namespace Terraria.Social.Steam
{
	public partial class WorkshopHelper
	{
		internal static string[] MetadataKeys = new string[7] { "name", "author", "modside", "homepage", "modloaderversion", "version", "modreferences" };
		private static readonly Regex MetadataInDescriptionFallbackRegex = new Regex(@"\[quote=GithubActions\(Don't Modify\)\]Version Summary: (.*) \[/quote\]", RegexOptions.Compiled);

		public class ModPublisherInstance : UGCBased.APublisherInstance
		{
			protected override string GetHeaderText() => ModWorkshopEntry.GetHeaderTextFor(_publishedFileID.m_PublishedFileId, _entryData.Tags, _publicity, _entryData.PreviewImagePath);

			protected override void PrepareContentForUpdate() { }
		}

		public static string GetWorkshopFolder(AppId_t app) {
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

		internal static bool GetPublishIdLocal(TmodFile modFile, out ulong publishId) {
			publishId = 0;
			if (modFile == null || !ModOrganizer.TryReadManifest(ModOrganizer.GetParentDir(modFile.path), out var info))
				return false;

			publishId = info.workshopEntryId;
			return true;
		}

		/// <summary>
		/// Downloads all UIModDownloadItems provided.
		/// </summary>
		internal static Task SetupDownload(List<ModDownloadItem> items) {
			//Set UIWorkshopDownload
			UIWorkshopDownload uiProgress = null;

			// Can't update enabled items due to in-use file access constraints
			var needFreeInUseMods = items.Any(item => item.Installed != null && item.Installed.Enabled);
			if (needFreeInUseMods)
				ModLoader.ModLoader.Unload();

			if (!Main.dedServ) {
				uiProgress = new UIWorkshopDownload(Interface.modBrowser);
				Main.MenuUI.SetState(uiProgress);
			}

			return Task.Run(() => InnerDownload(uiProgress, items, needFreeInUseMods));
		}

		private static void InnerDownload(UIWorkshopDownload uiProgress, List<ModDownloadItem> items, bool reloadWhenDone) {
			foreach (var item in items) {
				var publishId = new PublishedFileId_t(ulong.Parse(item.PublishId));
				bool forceUpdate = SteamedWraps.DoesWorkshopItemNeedUpdate(publishId) || !SteamedWraps.IsWorkshopItemInstalled(publishId);

				uiProgress?.PrepUIForDownload(item.DisplayName);
				Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.BeginDownload", item.DisplayName));
				SteamedWraps.Download(uiProgress, forceUpdate, publishId);
			}

			if (reloadWhenDone)
				ModLoader.ModLoader.Reload();
		}

		internal static class QueryHelper
		{
			internal const int QueryPagingConst = Steamworks.Constants.kNumUGCResultsPerPage;
			internal static int IncompleteModCount;
			internal static int HiddenModCount;
			internal static uint TotalItemsQueried;
			internal static EResult ErrorState = EResult.k_EResultNone;

			internal static List<ModDownloadItem> Items = new List<ModDownloadItem>();

			internal static bool FetchDownloadItems() {
				if (!QueryWorkshop())
				return false;

				return true;
			}

			internal static ModDownloadItem FindModDownloadItem(string modName)
			=> Items.FirstOrDefault(x => x.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase));

			internal static bool QueryWorkshop() {
				HiddenModCount = IncompleteModCount = 0;
				TotalItemsQueried = 0;
				Items.Clear();
				ErrorState = EResult.k_EResultNone;

				AQueryInstance.InstalledMods = ModOrganizer.FindWorkshopMods();

				if (!SteamedWraps.SteamAvailable) {
					if (!SteamedWraps.TryInitViaGameServer()) {
						Utils.ShowFancyErrorMessage(Language.GetTextValue("tModLoader.NoWorkshopAccess"), 0);
						return false;
					}

					var start = DateTime.Now; // lets wait a few seconds for steam to actually init. It if times out, then another query later will fail, oh well :|
					while (!SteamGameServer.BLoggedOn() && (DateTime.Now - start) < TimeSpan.FromSeconds(5)) {
						SteamedWraps.ForceCallbacks();
					}
				}

				if (!new AQueryInstance().QueryAllPagesSerial())
					return false;

				AQueryInstance.InstalledMods = null;
				return true;
			}

			internal static bool HandleError(EResult eResult) {
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

			internal class AQueryInstance
			{
				private CallResult<SteamUGCQueryCompleted_t> _queryHook;
				protected UGCQueryHandle_t _primaryUGCHandle;
				protected EResult _primaryQueryResult;
				protected uint _queryReturnCount;
				protected string _nextCursor;
				internal List<ulong> ugcChildren = new List<ulong>();

				internal static IReadOnlyList<LocalMod> InstalledMods;

				internal AQueryInstance() {
					_queryHook = CallResult<SteamUGCQueryCompleted_t>.Create(OnWorkshopQueryCompleted);
				}

				internal bool QueryAllPagesSerial() {
					do {
						// Appx. 0.4 seconds per page of 50 items during testing. No way to parallelize.
						// Note: Returning the Long Description makes up appx. 2/3 of the time spent fetching mods for above.
						//	Disabling Long Description takes ~0.14 seconds per page of 50 items. Long Description not needed right now.
						//TODO: Review an upgrade of ModBrowser to load only 1000 items at a time (ie paging Mod Browser).
						_primaryQueryResult = EResult.k_EResultNone;
						QueryForPage();

						if (!HandleError(ErrorState))
							return false;
					} while (TotalItemsQueried != Items.Count + IncompleteModCount + HiddenModCount);
					return true;
				}

				internal void QueryForPage() {
					_queryHook.Set(SteamedWraps.GenerateFullQuery(_nextCursor));
					var stopwatch = Stopwatch.StartNew();

					do {
						if (stopwatch.Elapsed.TotalSeconds >= 10) // 10 seconds maximum allotted time before no connection is assumed
							_primaryQueryResult = EResult.k_EResultTimeout;

						SteamedWraps.ForceCallbacks();
					}
					while (_primaryQueryResult == EResult.k_EResultNone);

					if (_primaryQueryResult != EResult.k_EResultOK) {
						if (ErrorState != EResult.k_EResultNone) // If failed in a previous attempt at this page
							return;

						ErrorState = _primaryQueryResult;
						ReleaseWorkshopQuery();

						Thread.Sleep(100); // If it fails, pause for 100 ms and make a second attempt
						QueryForPage();
						return;
					}

					QueryPageResult();
				}

				private void QueryPageResult() {
					for (uint i = 0; i < _queryReturnCount; i++) {
						// Item Result call data
						SteamUGCDetails_t pDetails = SteamedWraps.FetchItemDetails(_primaryUGCHandle, i);

						PublishedFileId_t id = pDetails.m_nPublishedFileId;

						if (pDetails.m_eVisibility != ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic) {
							HiddenModCount++;
							continue;
						}

						if (pDetails.m_eResult != EResult.k_EResultOK) {
							Logging.tML.Warn("Unable to fetch mod PublishId#" + id + " information. " + pDetails.m_eResult);
							HiddenModCount++;
							continue;
						}

						DateTime lastUpdate = Utils.UnixTimeStampToDateTime((long)pDetails.m_rtimeUpdated);
						string displayname = pDetails.m_rgchTitle;

						// Item Tagged data
						SteamedWraps.FetchMetadata(_primaryUGCHandle, i, out var metadata);

						string[] missingKeys = MetadataKeys.Where(k => metadata.Get(k) == null).ToArray();

						if (missingKeys.Length != 0) {
							Logging.tML.Warn($"Mod '{displayname}' is missing required metadata: {string.Join(',', missingKeys.Select(k => $"'{k}'"))}.");
							IncompleteModCount++;
							continue;
						}

						if (string.IsNullOrWhiteSpace(metadata["name"])) {
							Logging.tML.Warn($"Mod has no name: {id}"); // Somehow this happened before and broke mod downloads
							IncompleteModCount++;
							continue;
						}

						// Partial Description - we don't include Long Description so this is only first handful of characters
						string description = pDetails.m_rgchDescription;

						var cVersion = CalculateRelevantVersion(description, metadata["version"]);

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

						// Check against installed mods for updates
						
						bool updateIsDowngrade = false;
						
						var installed = InstalledMods.FirstOrDefault(m => m.Name == metadata["name"]);
						bool update = installed != null && DoesWorkshopItemNeedUpdate(id, installed, cVersion);

						// The below line is to identify the transient state where it isn't installed, but Steam considers it as such
						bool needsRestart = installed == null && SteamedWraps.IsWorkshopItemInstalled(id);

						Items.Add(new ModDownloadItem(displayname, metadata["name"], cVersion.ToString(), metadata["author"], metadata["modreferences"], modside, modIconURL, id.m_PublishedFileId.ToString(), (int)downloads, (int)hot, lastUpdate, update, updateIsDowngrade, installed, metadata["modloaderversion"], metadata["homepage"], needsRestart));
					}
					ReleaseWorkshopQuery();
				}

				internal static System.Version CalculateRelevantVersion(string mbDescription, string mbVersionSummary) {
					System.Version selectVersion = new(0,0);
					if (!mbVersionSummary.Contains(':'))
						return new System.Version(mbVersionSummary.Replace("v", ""));

					InnerCalculateRelevantVersion(ref selectVersion, mbVersionSummary);

					// Handle Github Actions metadata from description
					// Nominal string: [quote=GithubActions(Don't Modify)]Version Summary: YYYY.MM:#.#.#.#;YYYY.MM:#.#.#.#;... [/quote]
					Match match = MetadataInDescriptionFallbackRegex.Match(mbDescription);
					if (match.Success) {
						InnerCalculateRelevantVersion(ref selectVersion,(match.Groups[1].Value));
					}

					return selectVersion;
				}

				// This and VersionSummaryToStringArray need a refactor for cleaner code. Not bad for now
				private static void InnerCalculateRelevantVersion(ref System.Version selectVersion, string versionSummary) {
					foreach (var item in VersionSummaryToStringArray(versionSummary)) {
						if (selectVersion < item.Item2 && BuildInfo.tMLVersion.MajorMinor() >= item.Item1.MajorMinor()) {
							selectVersion = item.Item2;
						}
					}
				}

				internal static Tuple<System.Version, System.Version>[] VersionSummaryToStringArray(string versionSummary) {
					return versionSummary.Split(";").Select(s => new Tuple<System.Version, System.Version>(new System.Version(s.Split(":")[0]), new System.Version(s.Split(":")[1]))).ToArray();
				}

				internal bool DoesWorkshopItemNeedUpdate(PublishedFileId_t id, LocalMod installed, System.Version mbVersion) {
					if (installed.properties.version < mbVersion)
						return true;

					if (SteamedWraps.DoesWorkshopItemNeedUpdate(id))
						return true;

					return false;
				}

				internal SteamUGCDetails_t FastQueryItem(ulong publishedId, bool queryChildren = false) {
					_primaryQueryResult = EResult.k_EResultNone;

					_queryHook.Set(SteamedWraps.GenerateSingleItemQuery(publishedId));

					var stopwatch = Stopwatch.StartNew();

					do {
						if (stopwatch.Elapsed.TotalSeconds >= 10) // 10 seconds maximum allotted time before no connection is assumed
							_primaryQueryResult = EResult.k_EResultTimeout;

						SteamedWraps.ForceCallbacks();
					}
					while (_primaryQueryResult == EResult.k_EResultNone);

					var pDetails = SteamedWraps.FetchItemDetails(_primaryUGCHandle, 0);

					if (queryChildren) {
						ugcChildren = SteamedWraps.FetchItemDependencies(_primaryUGCHandle, 0, pDetails.m_unNumChildren).Select(x => x.m_PublishedFileId).ToList();
					}

					ReleaseWorkshopQuery();
					return pDetails;
				}

				private void OnWorkshopQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure) {
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
				private void ReleaseWorkshopQuery() {
					SteamedWraps.ReleaseWorkshopHandle(_primaryUGCHandle);
				}
			}

			internal static string GetDescription(ulong publishedId) {
				var pDetails = new AQueryInstance().FastQueryItem(publishedId);
				return pDetails.m_rgchDescription;
			}

			internal static ulong GetSteamOwner(ulong publishedId) {
				var pDetails = new AQueryInstance().FastQueryItem(publishedId);
				return pDetails.m_ulSteamIDOwner;
			}

			private static List<ulong> GetDependencies(ulong publishedId) {
				var query = new AQueryInstance();
				query.FastQueryItem(publishedId, queryChildren: true);
				return query.ugcChildren;
			}

			internal static void GetDependenciesRecursive(ulong publishedId, ref HashSet<ulong> set) {
				var deps = GetDependencies(publishedId);
				set.UnionWith(deps);

				foreach (ulong dep in deps)
					GetDependenciesRecursive(dep, ref set);
			}

			internal static bool CheckWorkshopConnection() {
				// If populating fails during query, than no connection. Attempt connection if not yet attempted.
				if (ErrorState != EResult.k_EResultOK && !FetchDownloadItems())
					return false;

				// If there are zero items on workshop, than return true.
				if (Items.Count + TotalItemsQueried == 0)
					return true;

				// Otherwise, return the original condition.
				return Items.Count + IncompleteModCount != 0;
			}
		}
	}
}
