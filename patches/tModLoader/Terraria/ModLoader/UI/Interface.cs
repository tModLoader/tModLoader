using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader.Config;
using Terraria.Social;
using Terraria.Social.Steam;
using Terraria.UI;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework;
using Terraria.Social.Base;

namespace Terraria.ModLoader.UI;

internal static class Interface
{
	internal const int modsMenuID = 10000;
	internal const int modSourcesID = 10001;
	//set initial Main.menuMode to loadModsID
	internal const int loadModsID = 10002;
	internal const int buildModID = 10003;
	internal const int errorMessageID = 10005;
	internal const int reloadModsID = 10006;
	internal const int modBrowserID = 10007;
	internal const int modInfoID = 10008;
	//internal const int downloadModID = 10009;
	//internal const int modControlsID = 10010;
	//internal const int managePublishedID = 10011;
	internal const int updateMessageID = 10012;
	internal const int infoMessageID = 10013;
	internal const int infoMessageDelayedID = 10014;
	//internal const int enterPassphraseMenuID = 10015;
	internal const int modPacksMenuID = 10016;
	internal const int tModLoaderSettingsID = 10017;
	//internal const int enterSteamIDMenuID = 10018;
	internal const int extractModID = 10019;
	internal const int downloadProgressID = 10020;
	internal const int progressID = 10023;
	internal const int modConfigID = 10024;
	internal const int createModID = 10025;
	internal const int exitID = 10026;
	internal const int modConfigListID = 10027;
	internal const int serverModsDifferMessageID = 10028;
	internal static UIMods modsMenu = new UIMods();
	internal static UILoadMods loadMods = new UILoadMods();
	internal static UIModSources modSources = new UIModSources();
	internal static UIBuildMod buildMod = new UIBuildMod();
	internal static UIErrorMessage errorMessage = new UIErrorMessage();
	internal static UIModBrowser modBrowser = new UIModBrowser(WorkshopBrowserModule.Instance);
	internal static UIModInfo modInfo = new UIModInfo();
	internal static UIForcedDelayInfoMessage infoMessageDelayed = new UIForcedDelayInfoMessage();
	//internal static UIManagePublished managePublished = new UIManagePublished();
	internal static UIUpdateMessage updateMessage = new UIUpdateMessage();
	internal static UIInfoMessage infoMessage = new UIInfoMessage();
	//internal static UIEnterPassphraseMenu enterPassphraseMenu = new UIEnterPassphraseMenu();
	internal static UIModPacks modPacksMenu = new UIModPacks();
	//internal static UIEnterSteamIDMenu enterSteamIDMenu = new UIEnterSteamIDMenu();
	internal static UIExtractMod extractMod = new UIExtractMod();
	internal static UIModConfig modConfig = new UIModConfig();
	internal static UIModConfigList modConfigList = new UIModConfigList();
	internal static UIServerModsDifferMessage serverModsDifferMessage = new UIServerModsDifferMessage();
	internal static UICreateMod createMod = new UICreateMod();
	internal static UIProgress progress = new UIProgress();
	internal static UIDownloadProgress downloadProgress = new UIDownloadProgress();

	/// <summary> Collection of error messages that will be shown one at a time once the main menu is reached. Useful for error messages during player and world saving happening on another thread. </summary>
	internal static Stack<string> pendingErrorMessages = new Stack<string>();

	// adds to Terraria.Main.DrawMenu in Main.menuMode == 0, after achievements
	//Interface.AddMenuButtons(this, this.selectedMenu, array9, array7, ref num, ref num3, ref num10, ref num5);
	internal static void AddMenuButtons(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
	{
		/*
		 * string legacyInfoButton = Language.GetTextValue("tModLoader.HowToAccessLegacytModLoaderButton");
		buttonNames[buttonIndex] = legacyInfoButton;
		if (selectedMenu == buttonIndex) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
		}
		buttonIndex++;
		numButtons++;
		*/
	}

	internal static void ResetData()
	{
		modBrowser.Reset();
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
	}

	//internal static void AddSettingsMenuButtons(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, int[] virticalSpacing, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
	//{
	//	buttonIndex++;
	//	numButtons++;
	//	buttonNames[buttonIndex] = "Mod " + Lang.menu[66];
	//	if (selectedMenu == buttonIndex)
	//	{
	//		SoundEngine.PlaySound(10, -1, -1, 1);
	//		Main.menuMode = modControlsID;
	//	}
	//	for (int k = 0; k < numButtons; k++)
	//	{
	//		buttonScales[k] = 0.73f;
	//		virticalSpacing[k] = 0;
	//	}
	//	virticalSpacing[numButtons - 1] = 8;
	//}

	//add to end of if else chain of Main.menuMode in Terraria.Main.DrawMenu
	//Interface.ModLoaderMenus(this, this.selectedMenu, array9, array7, array4, ref num2, ref num4, ref num5, ref flag5);
	internal static void ModLoaderMenus(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, int[] buttonVerticalSpacing, ref int offY, ref int spacing, ref int numButtons, ref bool backButtonDown)
	{
		if (Main.menuMode == loadModsID) {
			// These must be "else if" because the code should only run when it will actually show. This code will be revisited each time these info messages are closed, to check if any other messages should be shown.
			if (ModLoader.ShowFirstLaunchWelcomeMessage) {
				ModLoader.ShowFirstLaunchWelcomeMessage = false;
				infoMessage.Show(Language.GetTextValue("tModLoader.FirstLaunchWelcomeMessage"), Main.menuMode);
			}

			else if (SteamedWraps.FamilyShared && !ModLoader.WarnedFamilyShare && !ModLoader.WarnedFamilyShareDontShowAgain) {
				ModLoader.WarnedFamilyShare = true;
				infoMessage.Show(Language.GetTextValue("tModLoader.SteamFamilyShareWarning"), Main.menuMode, altButtonText: Language.GetTextValue("tModLoader.DontShowAgain"), altButtonAction: () => { ModLoader.WarnedFamilyShareDontShowAgain = true; Main.SaveSettings(); } );
			}

			/* For Major Updates that span multi-month
			else if (!ModLoader.BetaUpgradeWelcomed144) {
				ModLoader.BetaUpgradeWelcomed144 = true;
				infoMessage.Show(Language.GetTextValue("tModLoader.WelcomeMessageUpgradeBeta"), Main.menuMode);
				Main.SaveSettings();
			}
			*/

			else if (ModLoader.ShowWhatsNew) {
				ModLoader.ShowWhatsNew = false;
				if (File.Exists("RecentGitHubCommits.txt")) {
					bool LastLaunchedShaInRecentGitHubCommits = false;
					var messages = new StringBuilder();
					var recentcommitsfilecontents = File.ReadLines("RecentGitHubCommits.txt");
					foreach (var commitEntry in recentcommitsfilecontents) {
						string[] parts = commitEntry.Split(' ', 2);
						if (parts.Length == 2) {
							string sha = parts[0];
							string message = parts[1];
							if (sha != ModLoader.LastLaunchedTModLoaderAlphaSha)
								messages.Append("\n  " + message);
							if (sha == ModLoader.LastLaunchedTModLoaderAlphaSha) {
								LastLaunchedShaInRecentGitHubCommits = true;
								break;
							}
						}
					}
					string compareUrl = $"{ModLoader.LastLaunchedTModLoaderAlphaSha}...preview";
					if (!LastLaunchedShaInRecentGitHubCommits) {
						// If not seen, then too many commits since the last time user opened Preview
						messages.Append("\n...and more");
						compareUrl = $"stable...preview";
					}

					infoMessage.Show(Language.GetTextValue("tModLoader.WhatsNewMessage") + messages.ToString(), Main.menuMode, null, Language.GetTextValue("tModLoader.ViewOnGitHub"), () => Utils.OpenToURL($"https://github.com/tModLoader/tModLoader/compare/{compareUrl}"));
				}
				else {
					infoMessage.Show(Language.GetTextValue("tModLoader.WhatsNewMessage") + "Unknown, somehow RecentGitHubCommits.txt is missing.", Main.menuMode, null, Language.GetTextValue("tModLoader.ViewOnGitHub"), () => Utils.OpenToURL($"https://github.com/tModLoader/tModLoader/compare/stable...preview"));
				}
			}

			else if (ModLoader.PreviewFreezeNotification) {
				ModLoader.PreviewFreezeNotification = false;
				ModLoader.LastPreviewFreezeNotificationSeen = BuildInfo.tMLVersion.MajorMinor();
				infoMessage.Show(Language.GetTextValue("tModLoader.WelcomeMessagePreview"), Main.menuMode, null, Language.GetTextValue("tModLoader.ModsMoreInfo"),
					() => Utils.OpenToURL($"https://github.com/tModLoader/tModLoader/wiki/tModLoader-Release-Cycle#144"));
				Main.SaveSettings();
			}
			else if (!ModLoader.DownloadedDependenciesOnStartup) { // Keep this at the end of the if/else chain since it doesn't necessarily change Main.menuMode
				ModLoader.DownloadedDependenciesOnStartup = true;

				// Find dependencies that need to be downloaded.
				var missingDeps = ModOrganizer.IdentifyMissingWorkshopDependencies().ToList();

				string message = $"{ModOrganizer.DetectModChangesForInfoMessage(out IEnumerable<string> removedMods)}";
				if (missingDeps.Any()) {
					message += $"{Language.GetTextValue("tModLoader.DependenciesNeededForOtherMods")}\n  {string.Join("\n  ", missingDeps)}";
				}
				message = message.Trim('\n');

				bool anyMissingDependency = missingDeps.Any();
				bool anyRemovedMod = removedMods.Any();
				bool promptDepDownloads = anyMissingDependency || anyRemovedMod;

				string cancelButton = promptDepDownloads ? Language.GetTextValue("tModLoader.ContinueAnyway") : null;
				string continueButton = "";
				if (anyMissingDependency && anyRemovedMod)
					continueButton = Language.GetTextValue("tModLoader.InstallDependenciesAndRedownloadMods");
				else if (anyMissingDependency)
					continueButton = Language.GetTextValue("tModLoader.InstallDependencies");
				else if (anyRemovedMod)
					continueButton = Language.GetTextValue("tModLoader.RedownloadMods");

				Action downloadAction = async () => {
					HashSet<ModDownloadItem> downloads = new();
					foreach (var slug in missingDeps) {
						var state = WorkshopHelper.QueryHelper.AQueryInstance.TryGetModDownloadItem(slug, out var item);
						if (state == WorkshopHelper.WorkshopSearchReturnState.SearchFailed)
							break;

						if (state != WorkshopHelper.WorkshopSearchReturnState.Success) {
							Logging.tML.Error($"Could not find required mod dependency on Workshop: {slug}; Error State {state}");
							continue;
						}

						if (item.Banned) {
							Logging.tML.Error($"The missing dependency {item.DisplayName} with ID:{item.PublishId} is Banned on Workshop.");
							continue;
						}

						downloads.Add(item);
					}

					if (downloads.Any()) {
						await UIModBrowser.DownloadMods(
							downloads,
							loadModsID);
					}

					//TODO: This code was added hastily in response to a popular mod being transferred ownership by reuploading it.
					// Revisit this code at a later date. Its not apparent how well the interaction of both dependencies and removed mods will play out in terms of UX
					HashSet<ModPubId_t> removedDownloads = new();
					foreach (var slug in removedMods) {
						var state = WorkshopHelper.QueryHelper.AQueryInstance.TryGetModDownloadItem(slug, out var item);
						if (state == WorkshopHelper.WorkshopSearchReturnState.SearchFailed)
							break;

						if (state != WorkshopHelper.WorkshopSearchReturnState.Success) {
							Logging.tML.Error($"Could not find removed mod on Workshop: {slug}; Error State {state}");
							continue;
						}

						if (item.Banned) {
							Logging.tML.Error($"The removed mod {item.DisplayName} with ID:{item.PublishId} is Banned on Workshop.");
							continue;
						}

						removedDownloads.Add(item.PublishId);
					}

					if (removedDownloads.Any()) {
						modBrowser.Activate();
						modBrowser.FilterTextBox.Text = "";
						modBrowser.SpecialModPackFilter = removedDownloads.ToList();
						modBrowser.SpecialModPackFilterTitle = Language.GetTextValue("tModLoader.MBFilterModlist");// Too long: " + modListItem.modName.Text;
						modBrowser.UpdateFilterMode = UpdateFilter.All; // Set to 'All' so all mods from ModPack are visible
						modBrowser.ModSideFilterMode = ModSideFilter.All;
						modBrowser.ResetTagFilters();
						SoundEngine.PlaySound(SoundID.MenuOpen);

						modBrowser.reloadOnExit = true;
						modBrowser.PreviousUIState = null;
						Main.menuMode = modBrowserID;
					}
					else {
						Main.QueueMainThreadAction(() => {
							Main.menuMode = Interface.loadModsID;
							Main.MenuUI.SetState(null);
						});
					}
				};

				if (!string.IsNullOrWhiteSpace(message)) {
					Logging.tML.Info($"Mod Changes since last launch:\n{message}");
					infoMessage.Show(message, Main.menuMode, altButtonText: continueButton, altButtonAction: downloadAction, okButtonText: cancelButton);
				}
			}
		}
		if (Main.menuMode == modsMenuID) {
			Main.MenuUI.SetState(modsMenu);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == modSourcesID) {
			Main.menuMode = 888;
			Main.MenuUI.SetState(modSources);
		}
		else if (Main.menuMode == createModID) {
			Main.MenuUI.SetState(createMod);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == loadModsID) {
			Main.menuMode = 888;
			Main.MenuUI.SetState(loadMods);
		}
		else if (Main.menuMode == buildModID) {
			Main.MenuUI.SetState(buildMod);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == errorMessageID) {
			Main.MenuUI.SetState(errorMessage);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == reloadModsID) {
			ModLoader.Reload();
		}
		else if (Main.menuMode == modBrowserID) {
			Main.MenuUI.SetState(modBrowser);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == modInfoID) {
			Main.MenuUI.SetState(modInfo);
			Main.menuMode = 888;
		}
		//else if (Main.menuMode == modControlsID)
		//{
		//	UIModControls.ModLoaderMenus(main, selectedMenu, buttonNames, buttonScales, buttonVerticalSpacing, ref offY, ref spacing, ref numButtons);
		//}
		else if (Main.menuMode == updateMessageID) {
			Main.MenuUI.SetState(updateMessage);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == infoMessageID) {
			Main.MenuUI.SetState(infoMessage);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == modPacksMenuID) {
			Main.MenuUI.SetState(modPacksMenu);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == extractModID) {
			Main.MenuUI.SetState(extractMod);
			Main.menuMode = 888;
		}
		else if(Main.menuMode == progressID) {
			Main.MenuUI.SetState(progress);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == downloadProgressID) {
			Main.MenuUI.SetState(downloadProgress);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == tModLoaderSettingsID) {
			offY = 210;
			spacing = 42;
			numButtons = 8;
			buttonVerticalSpacing[numButtons - 1] = 18;
			for (int i = 0; i < numButtons; i++) {
				buttonScales[i] = 0.75f;
			}
			int buttonIndex = 0;
			buttonNames[buttonIndex] = (ModNet.downloadModsFromServers ? Language.GetTextValue("tModLoader.DownloadFromServersYes") : Language.GetTextValue("tModLoader.DownloadFromServersNo"));
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				ModNet.downloadModsFromServers = !ModNet.downloadModsFromServers;
			}

			/*
			buttonIndex++;
			buttonNames[buttonIndex] = (ModLoader.autoReloadAndEnableModsLeavingModBrowser ? Language.GetTextValue("tModLoader.AutomaticallyReloadAndEnableModsLeavingModBrowserYes") : Language.GetTextValue("tModLoader.AutomaticallyReloadAndEnableModsLeavingModBrowserNo"));
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				ModLoader.autoReloadAndEnableModsLeavingModBrowser = !ModLoader.autoReloadAndEnableModsLeavingModBrowser;
			}
			*/


			buttonIndex++;
			buttonNames[buttonIndex] = (ModLoader.autoReloadRequiredModsLeavingModsScreen ? Language.GetTextValue("tModLoader.AutomaticallyReloadRequiredModsLeavingModsScreenYes") : Language.GetTextValue("tModLoader.AutomaticallyReloadRequiredModsLeavingModsScreenNo"));
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				ModLoader.autoReloadRequiredModsLeavingModsScreen = !ModLoader.autoReloadRequiredModsLeavingModsScreen;
			}

			/*
			buttonIndex++;
			buttonNames[buttonIndex] = (Main.UseExperimentalFeatures ? Language.GetTextValue("tModLoader.ExperimentalFeaturesYes") : Language.GetTextValue("tModLoader.ExperimentalFeaturesNo"));
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.UseExperimentalFeatures = !Main.UseExperimentalFeatures;
			}
			*/

			buttonIndex++;
			buttonNames[buttonIndex] = Language.GetTextValue($"tModLoader.RemoveForcedMinimumZoom{(ModLoader.removeForcedMinimumZoom ? "Yes" : "No")}");
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				ModLoader.removeForcedMinimumZoom = !ModLoader.removeForcedMinimumZoom;
			}

			buttonIndex++;
			buttonNames[buttonIndex] = Language.GetTextValue($"tModLoader.AttackSpeedScalingTooltipVisibility{ModLoader.attackSpeedScalingTooltipVisibility}");
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				ModLoader.attackSpeedScalingTooltipVisibility = (ModLoader.attackSpeedScalingTooltipVisibility + 1) % 3;
			}

			buttonIndex++;
			buttonNames[buttonIndex] = Language.GetTextValue($"tModLoader.ShowModMenuNotifications{(ModLoader.notifyNewMainMenuThemes ? "Yes" : "No")}");
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				ModLoader.notifyNewMainMenuThemes = !ModLoader.notifyNewMainMenuThemes;
			}

			buttonIndex++;
			buttonNames[buttonIndex] = Language.GetTextValue($"tModLoader.ShowNewUpdatedModsInfo{(ModLoader.showNewUpdatedModsInfo ? "Yes" : "No")}");
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				ModLoader.showNewUpdatedModsInfo = !ModLoader.showNewUpdatedModsInfo;
			}

			buttonIndex++;
			buttonNames[buttonIndex] = Language.GetTextValue($"tModLoader.ShowConfirmationWindowWhenEnableDisableAllMods{(ModLoader.showConfirmationWindowWhenEnableDisableAllMods ? "Yes" : "No")}");
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				ModLoader.showConfirmationWindowWhenEnableDisableAllMods = !ModLoader.showConfirmationWindowWhenEnableDisableAllMods;
			}

			/*
			buttonIndex++;
			buttonNames[buttonIndex] = Language.GetTextValue("tModLoader.ClearMBCredentials");
			if (selectedMenu == buttonIndex) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				ModLoader.modBrowserPassphrase = "";
				ModLoader.SteamID64 = "";
			}
			*/

			buttonIndex++;
			buttonNames[buttonIndex] = Lang.menu[5].Value;
			if (selectedMenu == buttonIndex || backButtonDown) {
				backButtonDown = false;
				Main.menuMode = 11;
				SoundEngine.PlaySound(11, -1, -1, 1);
			}
		}
		else if (Main.menuMode == modConfigID)
		{
			Main.MenuUI.SetState(modConfig);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == modConfigListID)
		{
			Main.MenuUI.SetState(modConfigList);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == serverModsDifferMessageID) {
			Main.MenuUI.SetState(serverModsDifferMessage);
			Main.menuMode = 888;
		}
		else if (Main.menuMode == exitID) {
			Environment.Exit(0);
		}
	}

	internal static void ServerModMenu(out bool reloadMods)
	{
		bool exit = false;

		reloadMods = false;

		while (!exit) {
			Console.WriteLine("Terraria Server " + Main.versionNumber2 + " - " + ModLoader.versionedName);
			Console.WriteLine();
			var mods = ModOrganizer.FindMods(logDuplicates: true);
			for (int k = 0; k < mods.Length; k++) {
				Console.Write((k + 1) + "\t\t" + mods[k].DisplayNameClean);
				Console.WriteLine(" (" + (mods[k].Enabled ? "enabled" : "disabled") + ")");
			}
			if (mods.Length == 0) {
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(Language.GetTextValue("tModLoader.ModsNotFoundServer", ModLoader.ModPath));
				Console.ResetColor();
			}
			Console.WriteLine();
			Console.WriteLine("e\t\t" + Language.GetTextValue("tModLoader.ModsEnableAll"));
			Console.WriteLine("d\t\t" + Language.GetTextValue("tModLoader.ModsDisableAll"));
			Console.WriteLine("c <number>\t" + Language.GetTextValue("tModLoader.DedConfigEditServerConfigsForMod"));
			Console.WriteLine("r\t\t" + Language.GetTextValue("tModLoader.ModsReloadAndReturn"));
			Console.WriteLine(Language.GetTextValue("tModLoader.AskForModIndex"));
			Console.WriteLine();
			Console.WriteLine(Language.GetTextValue("tModLoader.AskForCommand"));
			string command = Console.ReadLine() ?? "";
			command = command.ToLower();
			Console.Clear();
			if (command == "e") {
				foreach (var mod in mods) {
					mod.Enabled = true;
				}
			}
			else if (command == "d") {
				foreach (var mod in mods) {
					mod.Enabled = false;
				}
			}
			else if (command == "r") {
				//Do not reload mods here, just to ensure that Main.DedServ_PostModLoad isn't in the call stack during mod reload, to allow hooking into it.
				reloadMods = true;
				exit = true;
			}
			else if (command.StartsWith("c")) {
				Match match = new Regex("c\\s*(\\d+)").Match(command);
				if (!match.Success) {
					continue;
				}
				int modIndex = Convert.ToInt32(match.Groups[1].Value) - 1;
				if (modIndex < 0 || modIndex >= mods.Length) {
					WriteColoredLine(ConsoleColor.Yellow, Language.GetTextValue("tModLoader.DedErrorModOOB"));
				}
				else if (!ModLoader.TryGetMod(mods[modIndex].Name, out Mod mod)) {
					WriteColoredLine(ConsoleColor.Yellow, Language.GetTextValue("tModLoader.DedErrorNotEnabled"));
				}
				else if (!ConfigManager.Configs.TryGetValue(mod, out List<ModConfig> configs) || !configs.Any(config => config.Mode == ConfigScope.ServerSide)) {
					WriteColoredLine(ConsoleColor.Yellow, Language.GetTextValue("tModLoader.DedErrorNoConfig"));
				}
				else {
					// We are acting on the actual configs rather than a clone because a reload will be forced anyway. If changing configs during server play is implemented later this will need to adjust to the clone approach.
					ConfigureMod(mod, configs);
				}
			}
			else if (int.TryParse(command, out int value) && value > 0 && value <= mods.Length) {
				var mod = mods[value - 1];
				mod.Enabled ^= true;

				if (mod.Enabled) {
					var missingRefs = new List<string>();
					EnableDepsRecursive(mod, mods, missingRefs);

					if (missingRefs.Any()) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine(Language.GetTextValue("tModLoader.ModDependencyModsNotFound", string.Join(", ", missingRefs)) + "\n");
						Console.ResetColor();
					}
				}
			}
		}
	}

	internal static void ConfigureMod(Mod mod, List<ModConfig> configs)
	{
		Dictionary<int, (PropertyFieldWrapper, ModConfig)> properties = new();
		int index = 1;
		var sortedConfigs = configs.OrderBy(x => x.DisplayName.Value).ToList();
		foreach (ModConfig config in sortedConfigs) {
			if (config.Mode == ConfigScope.ServerSide) {
				foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(config)) {
					if (variable.IsProperty && variable.Name == "Mode")
						continue;

					if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(ShowDespiteJsonIgnoreAttribute)))
						continue;

					properties.Add(index++, (variable, config));
				}
			}
		}

		while (true) {
			PrintConfigValues(mod, properties);

			Console.WriteLine();
			Console.WriteLine("m <number> <new config> :\t\t\t\t" + Language.GetTextValue("tModLoader.DedConfigEditConfig"));
			WriteColoredLine(ConsoleColor.DarkYellow, Language.GetTextValue("tModLoader.DedConfigEditConfigNote"));
			Console.WriteLine("d :\t\t\t\t\t\t\t" + Language.GetTextValue("tModLoader.DedConfigRestoreConfig"));
			Console.WriteLine("e :\t\t\t\t\t\t\t" + Language.GetTextValue("tModLoader.Exit"));
			Console.WriteLine();
			Console.WriteLine(Language.GetTextValue("tModLoader.AskForCommand"));
			string command = Console.ReadLine();
			Console.Clear();
			command ??= "";

			Match match = new Regex("m\\s*(\\d+) (.*)").Match(command);
			if (match.Success) { // Edit command
				HandleEditConfigValueCommand(properties, match);
			}
			else if (command == "d") {
				foreach (ModConfig config in configs) {
					if (config.Mode == ConfigScope.ServerSide) {
						ConfigManager.Reset(config);
						ConfigManager.Save(config);
						config.OnChanged();
					}
				}
			}
			else if (command == "e") {
				// Note: No need to check for reload required, this returns to mods menu and the only exit from mods menu is "Reload and return to world menu"
				break;
			}
		}
	}

	private static void PrintConfigValues(Mod mod, Dictionary<int, (PropertyFieldWrapper, ModConfig)> properties)
	{
		WriteColoredLine(ConsoleColor.White, mod.DisplayName);
		ModConfig currentConfig = null;
		foreach ((int key, (PropertyFieldWrapper variable, ModConfig config)) in properties) {
			if (currentConfig != config) {
				currentConfig = config;
				WriteColoredLine(ConsoleColor.Green, $"{config.DisplayName}:");
			}

			HeaderAttribute header = ConfigManager.GetLocalizedHeader(variable.MemberInfo);
			if (header != null) {
				WriteColoredLine(ConsoleColor.Yellow, "    " + ConvertChatTagsToText(header.Header)); // are tabs always 8 spaces?
			}

			string text = ConvertChatTagsToText(ConfigManager.GetLocalizedLabel(variable)) + ":";
			int size = text.Length;
			text = (variable.CanWrite ? key : "-") + "\t" + text + new string('\t', Math.Max((55 - size) / 8, 1));
			if (!variable.CanWrite)
				Console.ForegroundColor = (Console.BackgroundColor == ConsoleColor.DarkGray || Console.BackgroundColor == ConsoleColor.Gray) ? ConsoleColor.Blue : ConsoleColor.DarkGray;
			text += JsonConvert.SerializeObject(variable.GetValue(config));
			MethodInfo methodInfo = variable.Type.GetMethod("ToString", Array.Empty<Type>());
			bool hasToString = methodInfo != null && methodInfo.DeclaringType != typeof(object);
			if (!variable.Type.IsPrimitive && hasToString && variable.Type != typeof(string))
				text += "\t\t--> " + variable.GetValue(config);
			Console.WriteLine(text);
			if (!variable.CanWrite)
				Console.ResetColor();

			string tooltip = ConvertChatTagsToText(ConfigManager.GetLocalizedTooltip(variable));
			if (!string.IsNullOrWhiteSpace(tooltip)) {
				WriteColoredLine(ConsoleColor.Cyan, "\t" + tooltip.Replace("\n", "\n\t"));
			}
		}
	}

	private static void HandleEditConfigValueCommand(Dictionary<int, (PropertyFieldWrapper, ModConfig)> properties, Match match)
	{
		int configIndex = Convert.ToInt32(match.Groups[1].Value);
		if (!properties.TryGetValue(configIndex, out (PropertyFieldWrapper, ModConfig) value)) {
			WriteColoredLine(ConsoleColor.Yellow, Language.GetTextValue("tModLoader.DedConfigConfigIndexOOB", configIndex));
			return;
		}
		(PropertyFieldWrapper variable, ModConfig config) = value;
		if (!variable.CanWrite) {
			WriteColoredLine(ConsoleColor.Yellow, Language.GetTextValue("tModLoader.DedConfigReadOnly", ConfigManager.GetLocalizedLabel(variable)));
			return;
		}
		try {
			string inputString = match.Groups[2].Value;
			Type type = variable.Type;
			if (type == typeof(bool))
				inputString = inputString.ToLower();

			// Attempts to simplify the user experience by allowing user to omit "", [], and {}:
			// Some objects are represented as a string in json.
			object originalObject = variable.GetValue(config);
			string originalRepresentation = JsonConvert.SerializeObject(originalObject);
			if ((originalRepresentation.StartsWith('"') || type == typeof(string)) && !inputString.StartsWith('"'))
				inputString = $"\"{inputString}\"";
			else if (type.IsArray || type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(HashSet<>))) {
				if (!inputString.StartsWith("["))
					inputString = $"[{inputString}]";
			}
			else if (type.IsClass && originalRepresentation.StartsWith('{') && !inputString.StartsWith('{')) {
				inputString = $"{{{inputString}}}";
			}
			else if (type.IsEnum && !int.TryParse(inputString, out _)) {
				inputString = $"\"{inputString}\"";
			}

			object newValue = JsonConvert.DeserializeObject(inputString, type);

			// Validate value
			OptionStringsAttribute optionStringsAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<OptionStringsAttribute>(variable, null, null);
			RangeAttribute rangeAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<RangeAttribute>(variable, null, null);
			if (optionStringsAttribute != null &&
				!optionStringsAttribute.OptionLabels.Any(s => s.Equals(newValue))) {
				string text = Language.GetTextValue("tModLoader.DedConfigErrorOutOfOptionStrings", string.Join(", ", optionStringsAttribute.OptionLabels));
				WriteColoredLine(ConsoleColor.Yellow, text);
			}
			else if (rangeAttribute != null && newValue is IComparable comparable &&
						(comparable.CompareTo(rangeAttribute.Min) < 0 ||
						comparable.CompareTo(rangeAttribute.Max) > 0)) {
				WriteColoredLine(ConsoleColor.Yellow, Language.GetTextValue("tModLoader.DedConfigErrorOutOfRange", rangeAttribute.Min, rangeAttribute.Max));
			}
			else if (type.IsArray && originalObject is Array originalArray && newValue is Array newArray && originalArray.Length != newArray.Length) {
				WriteColoredLine(ConsoleColor.Yellow, Language.GetTextValue("tModLoader.DedConfigArrayLengthCantChange", ConfigManager.GetLocalizedLabel(variable)));
			}
			else {
				if (rangeAttribute != null && newValue is not IComparable) {
					WriteColoredLine(ConsoleColor.Yellow, Language.GetTextValue("tModLoader.DedConfigRangeCantBeValidated", ConfigManager.GetLocalizedLabel(variable)));
				}

				variable.SetValue(config, newValue);
				ConfigManager.Save(config);
				config.OnChanged();
			}
		}
		catch {
			WriteColoredLine(ConsoleColor.Yellow, Language.GetTextValue("tModLoader.DedConfigErrorNotParsable"));
		}
	}

	private static void WriteColoredLine(ConsoleColor color, string text)
	{
		Console.ForegroundColor = color;
		Console.WriteLine(text);
		Console.ResetColor();
	}

	internal static string ConvertChatTagsToText(string text)
	{
		return string.Join("", ChatManager.ParseMessage(text, Color.White)
				.Select(x => x.Text));
	}

	private static void EnableDepsRecursive(LocalMod mod, LocalMod[] mods, List<string> missingRefs)
	{
		string[] _modReferences = mod.properties.modReferences.Select(x => x.mod).ToArray();
		foreach (var name in _modReferences) {
			var dep = mods.FirstOrDefault(x => x.Name == name);
			if (dep == null) {
				missingRefs.Add(name);
				continue;
			}
			EnableDepsRecursive(dep, mods, missingRefs);
			if (!dep.Enabled) {
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"Automatically enabling {dep.DisplayNameClean} required by {mod.DisplayNameClean}");
				Console.ResetColor();
			}
			dep.Enabled ^= true;
		}
	}

	internal static void ServerModBrowserMenu()
	{
		//TODO: Broke this, again. I don't think ever really worked in 1.4. To be left broken for later reconsideration if a different host is used.
		//	In Place of fixing, we rely on ModPack Menu for exporting .tmod files and SteamCMD paired with install.txt from ModPack menu

		/*
		if (!SteamedWraps.SteamAvailable) {
			if (!SteamedWraps.TryInitViaGameServer()) {
				Utils.ShowFancyErrorMessage(Language.GetTextValue("tModLoader.NoWorkshopAccess"), 0);
				throw new SocialBrowserException("No Workshop Access");
			}

			// lets wait a few seconds for steam to actually init. It if times out, then another query later will fail, oh well :|
			var stopwatch = Stopwatch.StartNew();
			while (!SteamGameServer.BLoggedOn() && stopwatch.Elapsed.TotalSeconds < 5) {
				await SteamedWraps.ForceCallbacks(token);
			}
		}
		*/

		/*
		bool exit = false;
		Console.Clear();
		while (!exit) {
			Console.WriteLine();
			Console.WriteLine("b\t\t" + Language.GetTextValue("tModLoader.MBServerReturnToMenu"));
			Console.WriteLine();
			Console.WriteLine(Language.GetTextValue("tModLoader.MBServerAskForModName"));
			string command = Console.ReadLine();
			if (command == null) {
				command = "";
			}
			if (command == "b" || command == "B") {
				exit = true;
			}
			else {
				string modname = command;
				try {
					if (!WorkshopHelper.QueryHelper.CheckWorkshopConnection())
						break;

					var info = WorkshopHelper.QueryHelper.FindModDownloadItem(modname);
					if (info == null)
						Console.WriteLine($"No mod with the name {modname} found on the workshop.");
					else
						info.InnerDownloadWithDeps().GetAwaiter().GetResult();
				}
				catch (Exception e) {
					Console.WriteLine(Language.GetTextValue("tModLoader.MBServerDownloadError", modname, e.ToString()));
				}
			}
		}
		//Console.Clear();
		*/
	}
}
