using ReLogic.OS;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
		private static readonly Regex MetadataInDescriptionFallbackRegex = new Regex(@"\[quote=GithubActions\(Don't Modify\)\]Version (.*) built for (tModLoader v.*)\[/quote\]", RegexOptions.Compiled);

		public struct ItemInstallInfo
		{
			public string installPath;
			public uint lastUpdatedTime;
		}

		public class ModPublisherInstance : UGCBased.APublisherInstance
		{
			protected override string GetHeaderText() => ModWorkshopEntry.GetHeaderTextFor(_publishedFileID.m_PublishedFileId, _entryData.Tags, _publicity, _entryData.PreviewImagePath);

			protected override void PrepareContentForUpdate() { }
		}

		internal static void OnGameExitCleanup() {
			if (ModManager.SteamUser) {
				SteamAPI.Shutdown();
				return;
			}

			GameServer.Shutdown();
		}

		internal static void ForceCallbacks() {
			Thread.Sleep(5);

			if (ModManager.SteamUser)
				SteamAPI.RunCallbacks();
			else
				GameServer.RunCallbacks();
		}

		// Used to get the right token for fetching/setting localized descriptions from/to Steam Workshop
		internal static string GetCurrentSteamLangKey() {
			//TODO: Unhardcode this whenever the language roster is unhardcoded for modding.
			return (GameCulture.CultureName)LanguageManager.Instance.ActiveCulture.LegacyId switch {
				GameCulture.CultureName.German => "german",
				GameCulture.CultureName.Italian => "italian",
				GameCulture.CultureName.French  => "french",
				GameCulture.CultureName.Spanish  => "spanish",
				GameCulture.CultureName.Russian => "russian",
				GameCulture.CultureName.Chinese => "schinese",
				GameCulture.CultureName.Portuguese => "portuguese",
				GameCulture.CultureName.Polish => "polish",
				_ => "english",
			};
		}

		internal static void ReportCheckSteamLogs() {
			string workshopLogLoc = "";
			if (Platform.IsWindows)
				workshopLogLoc = "C:/Program Files (x86)/Steam/logs/workshop_log.txt";
			else if (Platform.IsOSX)
				workshopLogLoc = "~/Library/Application Support/Steam/logs/workshop_log.txt";
			else if (Platform.IsLinux)
				workshopLogLoc = "/home/user/.local/share/Steam/logs/workshop_log.txt";

			Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.ConsultSteamLogs", workshopLogLoc));
		}

		public static string GetWorkshopFolder(AppId_t app) {
			if (Program.LaunchParameters.TryGetValue("-steamworkshopfolder", out string workshopLocCustom)) {
				if (Directory.Exists(workshopLocCustom))
					return workshopLocCustom;

				Logging.tML.Warn("-steamworkshopfolder path not found: " + workshopLocCustom);
			}

			string installLoc = null;
			if (ModManager.SteamUser) // get app install dir if possible
				SteamApps.GetAppInstallDir(app, out installLoc, 1000);

			installLoc ??= "."; // GetAppInstallDir may return null (#2491). Also the default location for dedicated servers and such

			var workshopLoc = Path.Combine(installLoc, "..", "..", "workshop");
			if (Directory.Exists(workshopLoc))
				return workshopLoc;

			// Load mods installed by GoG / Manually copied steamapps\workshop directories.
			return Path.Combine("steamapps", "workshop");
		}

		internal class ModManager
		{
			internal const uint thisApp = ModLoader.Engine.Steam.TMLAppID;

			internal static bool SteamUser { get; set; } = false;
			internal static bool SteamAvailable { get; set; } = true;

			protected Callback<DownloadItemResult_t> m_DownloadItemResult;

			private PublishedFileId_t itemID;

			internal static void Initialize() {
				string apptxt = "steam_appid.txt";

				// Non-steam tModLoader will use the SteamGameServer to perform Browsing & Downloading
				if (SocialMode.Steam == SocialAPI.Mode) {
					SteamUser = true;
					if (File.Exists(apptxt))
						File.Delete(apptxt);

					return;
				}

				File.WriteAllText(apptxt, thisApp.ToString());

				if (RuntimeInformation.ProcessArchitecture is Architecture.Arm or Architecture.Arm64) {
					Logging.tML.Warn("Steam Game Server currently unsupported on ARM");
					SteamAvailable = false;
				}
				else if (GameServer.Init(0x7f000001, 7775, 7774, EServerMode.eServerModeNoAuthentication, "0.11.9.0")) {
					SteamGameServer.SetGameDescription("tModLoader Mod Browser");
					SteamGameServer.SetProduct(thisApp.ToString());
					SteamGameServer.LogOnAnonymous();
				}
				else {
					Logging.tML.Error("Steam Game Server failed to Init. Steam Workshop downloading on GoG is unavailable. Make sure Steam is installed");
					SteamAvailable = false;
				}
			}

			internal static bool GetPublishIdLocal(TmodFile modFile, out ulong publishId) {
				publishId = 0;
				if (modFile == null || !ModOrganizer.TryReadManifest(ModOrganizer.GetParentDir(modFile.path), out var info))
					return false;

				publishId = info.workshopEntryId;
				return true;
			}

			internal ModManager(PublishedFileId_t itemID) {
				this.itemID = itemID;
			}

			private static List<LocalMod> enabledItems;

			/// <summary>
			/// Downloads all UIModDownloadItems provided.
			/// </summary>
			public static void Download(List<ModDownloadItem> items) {
				//Set UIWorkshopDownload
				UIWorkshopDownload uiProgress = null;

				// Can't update enabled items due to in-use file access constraints
				enabledItems = new List<LocalMod>();
				foreach (var item in items) {
					if (item.Installed != null && item.Installed.Enabled) {
						enabledItems.Add(item.Installed);
						item.Installed.Enabled = false;
					}
				}

				if (enabledItems.Count > 0) {
					ModLoader.ModLoader.Unload();
				}

				if (!Main.dedServ) {
					uiProgress = new UIWorkshopDownload(Interface.modBrowser);
					Main.MenuUI.SetState(uiProgress);
				}

				int counter = 0;

				Task.Run(() => TaskDownload(counter, uiProgress, items));
			}

			internal static void DownloadBatch(string[] workshopIds, UI.UIState returnMenu) {
				//Set UIWorkshopDownload
				UIWorkshopDownload uiProgress = null;		

				if (!Main.dedServ) {
					uiProgress = new UIWorkshopDownload(Interface.modPacksMenu);
					Main.MenuUI.SetState(uiProgress);
				}

				int counter = 0;

				Task.Run(() => TaskDownload(counter, uiProgress, ids: workshopIds));
			}

			private static void TaskDownload(int counter, UIWorkshopDownload uiProgress, List<ModDownloadItem> items = null, string[] ids = null) {
				string name = "";
				bool hasUpdate = false;
				ModManager mod = null;
				int endCount = 0;

				if (items == null) {
					var id = ids[counter++];
					mod = new ModManager(new PublishedFileId_t(ulong.Parse(id)));

					hasUpdate = mod.NeedsUpdate() || !mod.IsInstalled();
					name = id;
					endCount = ids.Length;
				}

				if (ids == null) {
					var item = items[counter++];
					mod = new ModManager(new PublishedFileId_t(ulong.Parse(item.PublishId)));

					hasUpdate = item.HasUpdate;
					name = item.DisplayName;
					endCount = items.Count;
				}

				uiProgress?.PrepUIForDownload(name);
				Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.BeginDownload", name));
				mod.InnerDownload(uiProgress, hasUpdate);

				if (counter == endCount) {
					uiProgress?.Leave(items != null);

					// Restore Enabled items.
					if (enabledItems?.Count > 0) {
						foreach (var localMod in enabledItems) {
							localMod.Enabled = true;
						}
						ModLoader.ModLoader.Reload();
					}
				}
				else
					Task.Run(() => TaskDownload(counter, uiProgress, items, ids));
			}

			private EResult downloadResult;

			/// <summary>
			/// Updates and/or Downloads the Item specified when generating the ModManager Instance.
			/// </summary>
			internal bool InnerDownload(UIWorkshopDownload uiProgress, bool mbHasUpdate) {
				downloadResult = EResult.k_EResultOK;

				if (NeedsUpdate() || mbHasUpdate) {
					downloadResult = EResult.k_EResultNone;
					Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.SteamDownloader"));

					if (SteamUser)
						SteamDownload(uiProgress);
					else
						GoGDownload(uiProgress);

					Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.EndDownload"));
				}
				else {
					// A warning here that you will need to restart the game for item to be removed completely from Steam's runtime cache.
					Utils.LogAndConsoleErrorMessage(Language.GetTextValue("tModLoader.SteamRejectUpdate"));
				}

				return downloadResult == EResult.k_EResultOK;
			}

			private void SteamDownload(UIWorkshopDownload uiProgress) {
				if (!SteamUGC.DownloadItem(itemID, true)) {
					ReportCheckSteamLogs();
					throw new ArgumentException("Downloading Workshop Item failed due to unknown reasons");
				}

				InnerDownloadQueue(uiProgress);
				SteamUGC.SubscribeItem(itemID);
			}

			private void GoGDownload(UIWorkshopDownload uiProgress) {
				if (!SteamGameServerUGC.DownloadItem(itemID, true)) {
					ReportCheckSteamLogs();
					throw new ArgumentException("GoG: Downloading Workshop Item failed due to unknown reasons");
				}

				InnerDownloadQueue(uiProgress);
			}

			private void InnerDownloadQueue(UIWorkshopDownload uiProgress) {
				ulong dlBytes, totalBytes;

				const int LogEveryXPercent = 10;

				int nextPercentageToLog = LogEveryXPercent;

				while (!IsInstalled()) {
					if (SteamUser)
						SteamUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
					else
						SteamGameServerUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);

					if (uiProgress != null)
						uiProgress.UpdateDownloadProgress((float)dlBytes / Math.Max(totalBytes, 1), (long)dlBytes, (long)totalBytes);

					int percentage = (int)MathF.Round(dlBytes / (float)totalBytes * 100f);

					if (percentage >= nextPercentageToLog) {
						string str = Language.GetTextValue("tModLoader.DownloadProgress", percentage);

						Utils.LogAndConsoleInfoMessage(str);

						nextPercentageToLog = percentage + LogEveryXPercent;

						if (nextPercentageToLog > 100 && nextPercentageToLog != 100 + LogEveryXPercent) {
							nextPercentageToLog = 100;
						}
					}
				}

				// We don't receive a callback, so we manually set the success.
				downloadResult = EResult.k_EResultOK;
			}

			internal void Uninstall(string installPath = null) {
				if (string.IsNullOrEmpty(installPath))
					installPath = GetInstallInfo().installPath;

				if (!Directory.Exists(installPath))
					return;

				// Unsubscribe
				if (SteamUser)
					SteamUGC.UnsubscribeItem(itemID);

				// Remove the files
				Directory.Delete(installPath, true);

				if (!SteamUser)
					UninstallACF();
			}

			private void UninstallACF() {
				// Cleanup acf file by removing info on this itemID
				string acfPath;

				if (!SteamUser)
					acfPath = Path.Combine(Directory.GetCurrentDirectory(), "steamapps", "workshop", "appworkshop_" + thisApp.ToString() + ".acf");
				else
					acfPath = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(GetInstallInfo().installPath).ToString()).ToString()).ToString(), "appworkshop_" + thisApp.ToString() + ".acf");

				string[] acf = File.ReadAllLines(acfPath);
				using StreamWriter w = new StreamWriter(acfPath);

				int blockLines = 5;
				int skip = 0;

				for (int i = 0; i < acf.Length; i++) {
					if (acf[i].Contains(itemID.ToString())) {
						skip = blockLines;
						continue;
					}
					else if (skip > 0) {
						skip--;
						continue;
					}

					w.WriteLine(acf[i]);
				}
			}

			public ItemInstallInfo GetInstallInfo() {
				string installPath;
				uint lastUpdatedTime;

				if (SteamUser)
					SteamUGC.GetItemInstallInfo(itemID, out var installSize, out installPath, 1000, out lastUpdatedTime);
				else
					SteamGameServerUGC.GetItemInstallInfo(itemID, out var installSize, out installPath, 1000, out lastUpdatedTime);

				return new ItemInstallInfo() { installPath = installPath, lastUpdatedTime = lastUpdatedTime };
			}

			private uint GetState() {
				if (SteamUser)
					return SteamUGC.GetItemState(itemID);
				else
					return SteamGameServerUGC.GetItemState(itemID);
			}

			public bool IsInstalled() {
				var currState = GetState();

				bool installed = (currState & (uint)(EItemState.k_EItemStateInstalled)) != 0;
				bool downloading = (currState & ((uint)EItemState.k_EItemStateDownloading + (uint)EItemState.k_EItemStateDownloadPending)) != 0;
				return installed && !downloading;
			}

			public bool NeedsUpdate() {
				var currState = GetState();

				return (currState & (uint)EItemState.k_EItemStateNeedsUpdate) != 0 ||
					(currState == (uint)EItemState.k_EItemStateNone) ||
					(currState & (uint)EItemState.k_EItemStateDownloadPending) != 0;
			}

			private const int PlaytimePagingConst = 100; //https://partner.steamgames.com/doc/api/ISteamUGC#StartPlaytimeTracking

			public static void BeginPlaytimeTracking() {
				if (!SteamAvailable)
					return;

				List<PublishedFileId_t> list = new List<PublishedFileId_t>();
				foreach (var mod in ModLoader.ModLoader.Mods) {
					if (GetPublishIdLocal(mod.File, out ulong publishId))
						list.Add(new PublishedFileId_t(publishId));
				}

				int count = list.Count;
				if (count == 0)
					return;

				int pg = count / PlaytimePagingConst;
				int rem = count % PlaytimePagingConst;

				for (int i = 0; i < pg + 1; i++) {
					var pgList = list.GetRange(i * PlaytimePagingConst, (i == pg) ? rem : PlaytimePagingConst);

					// Call the appropriate variant, may need performance optimization.
					if (SteamUser)
						SteamUGC.StartPlaytimeTracking(pgList.ToArray(), (uint)pgList.Count);
					else
						SteamGameServerUGC.StartPlaytimeTracking(pgList.ToArray(), (uint)pgList.Count);
				}
			}

			public static void StopPlaytimeTracking() {
				// Call the appropriate variant
				if (SteamUser)
					SteamUGC.StopPlaytimeTrackingForAllItems();
				else if (SteamAvailable)
					SteamGameServerUGC.StopPlaytimeTrackingForAllItems();
			}
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

				if (!ModManager.SteamAvailable) {
					//TODO: Replace with a localization text
					Utils.ShowFancyErrorMessage("Error: Unable to access Steam Workshop." +
						"\n\nYou must have a valid Steam install on your system in order to use the in-game Mod Browser. " +
						"\n\nNOTE: GoG users - once Steam is installed, you can use the ingame mod browser to download mods", 0);

					return false;
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
					ReportCheckSteamLogs();
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
						QueryForPage();

						if (!HandleError(ErrorState))
							return false;
					} while (TotalItemsQueried != Items.Count + IncompleteModCount + HiddenModCount);
					return true;
				}

				internal void QueryForPage() {
					_primaryQueryResult = EResult.k_EResultNone;

					SteamAPICall_t call;
					if (ModManager.SteamUser) {
						UGCQueryHandle_t qHandle;
						qHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(ModManager.thisApp), new AppId_t(ModManager.thisApp), _nextCursor);

						SteamUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
						SteamUGC.SetReturnKeyValueTags(qHandle, true);
						//SteamUGC.SetReturnLongDescription(qHandle, true);
						SteamUGC.SetReturnPlaytimeStats(qHandle, 30); // Last 30 days of playtime statistics
						SteamUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

						call = SteamUGC.SendQueryUGCRequest(qHandle);
					}
					else {
						UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(ModManager.thisApp), new AppId_t(ModManager.thisApp), _nextCursor);

						SteamGameServerUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
						SteamGameServerUGC.SetReturnKeyValueTags(qHandle, true);
						//SteamGameServerUGC.SetReturnLongDescription(qHandle, true);
						SteamGameServerUGC.SetReturnPlaytimeStats(qHandle, 30); // Last 30 days of playtime statistics
						SteamGameServerUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

						call = SteamGameServerUGC.SendQueryUGCRequest(qHandle);
					}

					_queryHook.Set(call);

					var stopwatch = Stopwatch.StartNew();

					do {
						if (stopwatch.Elapsed.TotalSeconds >= 10) // 10 seconds maximum allotted time before no connection is assumed
							_primaryQueryResult = EResult.k_EResultTimeout;

						ForceCallbacks();
					}
					while (_primaryQueryResult == EResult.k_EResultNone);

					if (_primaryQueryResult != EResult.k_EResultOK) {
						ErrorState = _primaryQueryResult;
						ReleaseWorkshopQuery();
						return;
					}

					QueryPageResult();
				}

				private void QueryPageResult() {
					for (uint i = 0; i < _queryReturnCount; i++) {
						// Item Result call data
						SteamUGCDetails_t pDetails;

						if (ModManager.SteamUser)
							SteamUGC.GetQueryUGCResult(_primaryUGCHandle, i, out pDetails);
						else
							SteamGameServerUGC.GetQueryUGCResult(_primaryUGCHandle, i, out pDetails);

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
						uint keyCount;
						var metadata = new NameValueCollection();

						if (ModManager.SteamUser)
							keyCount = SteamUGC.GetQueryUGCNumKeyValueTags(_primaryUGCHandle, i);
						else
							keyCount = SteamGameServerUGC.GetQueryUGCNumKeyValueTags(_primaryUGCHandle, i);

						if (keyCount < MetadataKeys.Length) {
							Logging.tML.Warn("Mod is missing required metadata: " + displayname);
							IncompleteModCount++;
							continue;
						}

						for (uint j = 0; j < keyCount; j++) {
							string key, val;

							if (ModManager.SteamUser)
								SteamUGC.GetQueryUGCKeyValueTag(_primaryUGCHandle, i, j, out key, byte.MaxValue, out val, byte.MaxValue);
							else
								SteamGameServerUGC.GetQueryUGCKeyValueTag(_primaryUGCHandle, i, j, out key, byte.MaxValue, out val, byte.MaxValue);

							metadata[key] = val;
						}

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

						// Calculate the Mod Browser Version
						System.Version cVersion = new System.Version(metadata["version"].Replace("v", ""));

						string description = pDetails.m_rgchDescription;

						// Handle Github Actions metadata from description
						// Nominal string: [quote=GithubActions(Don't Modify)]Version #.#.#.# built for tModLoader v#.#.#.#[/quote]
						Match match = MetadataInDescriptionFallbackRegex.Match(description);
						if (match.Success) {
							System.Version descriptionVersion = new System.Version(match.Groups[1].Value);
							if (descriptionVersion > cVersion) {
								cVersion = descriptionVersion;
								metadata["version"] = "v" + match.Groups[1].Value;
								metadata["modloaderversion"] = match.Groups[2].Value;
							}
						}

						// Assign ModSide Enum
						ModSide modside = ModSide.Both;

						if (metadata["modside"] == "Client")
							modside = ModSide.Client;

						if (metadata["modside"] == "Server")
							modside = ModSide.Server;

						if (metadata["modside"] == "NoSync")
							modside = ModSide.NoSync;

						// Preview Image url
						string modIconURL;

						if (ModManager.SteamUser)
							SteamUGC.GetQueryUGCPreviewURL(_primaryUGCHandle, i, out modIconURL, 1000);
						else
							SteamGameServerUGC.GetQueryUGCPreviewURL(_primaryUGCHandle, i, out modIconURL, 1000);

						// Item Statistics
						ulong hot, downloads;

						if (ModManager.SteamUser) {
							SteamUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out downloads);
							SteamUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumSecondsPlayedDuringTimePeriod, out hot); //Temp: based on how often being played lately?
						}
						else {
							SteamGameServerUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out downloads);
							SteamGameServerUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumSecondsPlayedDuringTimePeriod, out hot); //Temp: based on how often being played lately?
						}

						// Check against installed mods
						bool update = false;
						bool updateIsDowngrade = false;
						bool needsRestart = false;
						var installed = InstalledMods.FirstOrDefault(m => m.Name == metadata["name"]);
						var check = new ModManager(id);

						if (installed != null) {
							//exists = true;
							if (check.NeedsUpdate()) {
								update = true;

								/*
								string location = installed.modFile.path;
								string repo = ModOrganizer.GetParentDir(location);
								string oldest = ModOrganizer.FindOldest(repo);

								if (!oldest.Contains(".tmod"))
									oldest = Directory.GetFiles(oldest, "*.tmod")[0];

								var sModFile = new TmodFile(oldest);
								LocalMod sMod;
								using (sModFile.Open())
									sMod = new LocalMod(sModFile);

								var installedVer = sMod.properties.version;
								if (cVersion > installedVer)
									update = true;
								else if (cVersion < installedVer)
									update = updateIsDowngrade = true;
								*/
							}
						}
						else if (check.IsInstalled()) {
							needsRestart = true;
						}

						Items.Add(new ModDownloadItem(displayname, metadata["name"], cVersion.ToString(), metadata["author"], metadata["modreferences"], modside, modIconURL, id.m_PublishedFileId.ToString(), (int)downloads, (int)hot, lastUpdate, update, updateIsDowngrade, installed, metadata["modloaderversion"], metadata["homepage"], needsRestart));
					}
					ReleaseWorkshopQuery();
				}

				internal SteamUGCDetails_t FastQueryItem(ulong publishedId) {
					_primaryQueryResult = EResult.k_EResultNone;

					SteamAPICall_t call;
					if (ModManager.SteamUser) {
						UGCQueryHandle_t qHandle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { new PublishedFileId_t(publishedId) }, 1);

						SteamUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
						SteamUGC.SetReturnLongDescription(qHandle, true);
						SteamUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

						call = SteamUGC.SendQueryUGCRequest(qHandle);
					}
					else {
						UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { new PublishedFileId_t(publishedId) }, 1);

						SteamGameServerUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
						SteamGameServerUGC.SetReturnLongDescription(qHandle, true);
						SteamGameServerUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

						call = SteamGameServerUGC.SendQueryUGCRequest(qHandle);
					}

					_queryHook.Set(call);

					var stopwatch = Stopwatch.StartNew();

					do {
						if (stopwatch.Elapsed.TotalSeconds >= 10) // 10 seconds maximum allotted time before no connection is assumed
							_primaryQueryResult = EResult.k_EResultTimeout;

						ForceCallbacks();
					}
					while (_primaryQueryResult == EResult.k_EResultNone);

					SteamUGCDetails_t pDetails;
					if (ModManager.SteamUser)
						SteamUGC.GetQueryUGCResult(_primaryUGCHandle, 0, out pDetails);
					else
						SteamGameServerUGC.GetQueryUGCResult(_primaryUGCHandle, 0, out pDetails);

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
					if (ModManager.SteamUser)
						SteamUGC.ReleaseQueryUGCRequest(_primaryUGCHandle);
					else
						SteamGameServerUGC.ReleaseQueryUGCRequest(_primaryUGCHandle);
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
