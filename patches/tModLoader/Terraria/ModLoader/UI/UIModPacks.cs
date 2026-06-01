using Ionic.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Terraria.Utilities;
using Terraria.Audio;

namespace Terraria.ModLoader.UI;

internal class UIModPacks : UIState, IHaveBackButtonCommand
{
	internal static readonly Regex ModPackRegex = new(@"(?:[^a-zA-Z0-9_.-]+)|(?:^(con|prn|aux|nul|com[1-9]|lpt[1-9])$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	internal static string ModPacksDirectory = Path.Combine(ModLoader.ModPath, "ModPacks");

	private UIList _modPacks;
	private UILoaderAnimatedImage _uiLoader;
	private UIPanel _scrollPanel;
	private CancellationTokenSource _cts;
	public UIState PreviousUIState { get; set; }

	public static string ModPackModsPath(string packName) => Path.Combine(ModPacksDirectory, packName, "Mods");
	public static string ModPackConfigPath(string packName) => Path.Combine(ModPacksDirectory, packName, "ModConfigs");

	public override void OnInitialize()
	{
		var uIElement = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 220 },
			Height = { Pixels = -220, Percent = 1f },
			HAlign = 0.5f
		};

		_uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f);

		_scrollPanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -65, Percent = 0.9f },
			BackgroundColor = UICommon.MainPanelBackground
		};
		uIElement.Append(_scrollPanel);

		_modPacks = new UIList {
			Width = { Pixels = -25, Percent = 1f },
			Height = { Percent = 0.9f },
			ListPadding = 5f
		};
		_scrollPanel.Append(_modPacks);

		var uIScrollbar = new UIScrollbar {
			Height = { Percent = 0.9f },
			HAlign = 1f
		}.WithView(100f, 1000f);
		_scrollPanel.Append(uIScrollbar);
		_modPacks.SetScrollbar(uIScrollbar);

		var titleTextPanel = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModPacksHeader"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35 },
			BackgroundColor = UICommon.DefaultUIBlue
		}.WithPadding(15f);
		uIElement.Append(titleTextPanel);

		var folderButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.OpenModPackFolder")) {
			Width = new StyleDimension(-10f, 1f / 2f),
			Height = { Pixels = 40 },
			VAlign = 0.9f,
			HAlign = 0f,
			Top = { Pixels = -20 }
		}.WithFadedMouseOver();
		folderButton.OnLeftClick += OpenFolder;
		uIElement.Append(folderButton);

		var backButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("UI.Back")) {
			Width = new StyleDimension(-10f, 1f / 2f),
			Height = { Pixels = 40 },
			VAlign = 1f,
			HAlign = 0f,
			Top = { Pixels = -20 }
		}.WithFadedMouseOver();
		backButton.OnLeftClick += BackClick;
		uIElement.Append(backButton);

		var saveNewButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("tModLoader.ModPacksSaveEnabledAsNewPack"));
		saveNewButton.CopyStyle(backButton);
		saveNewButton.TextColor = Color.Green;
		saveNewButton.VAlign = 1f;
		saveNewButton.HAlign = 1f;
		saveNewButton.WithFadedMouseOver();
		saveNewButton.OnLeftClick += SaveNewModList;
		uIElement.Append(saveNewButton);

		Append(uIElement);
	}

	private static UIVirtualKeyboard _virtualKeyboard;
	private static UIVirtualKeyboard VirtualKeyboard =>
		_virtualKeyboard ?? (_virtualKeyboard = new UIVirtualKeyboard(
			Language.GetTextValue("tModLoader.ModPacksEnterModPackName"), "", SaveModPack, () => Main.menuMode = Interface.modPacksMenuID));

	private static void SaveNewModList(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(11);
		Main.clrInput();
		VirtualKeyboard.Text = "";
		Main.MenuUI.SetState(VirtualKeyboard);
		Main.menuMode = 888;
	}

	public static void SaveModPack(string filename)
	{
		// Sanitize input if not valid
		if (!IsValidModpackName(filename)) {
			VirtualKeyboard.Text = SanitizeModpackName(filename);
			return;
		}

		//string modsPath = Path.Combine(ModPacksDirectory, filename, "mods");
		string modsPath = ModPackModsPath(filename);
		//string configsPath = Path.Combine(ModPacksDirectory, filename, "configs"); need to port this?
		string configsPath = ModPackConfigPath(filename);

		SaveSnapshot(configsPath, modsPath);

		Main.menuMode = Interface.modPacksMenuID;
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(11);
		(this as IHaveBackButtonCommand).HandleBackButtonUsage();
	}

	private void OpenFolder(UIMouseEvent evt, UIElement listeningElement)
	{
		Utils.OpenFolder(ModPacksDirectory);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
	}

	internal static string SanitizeModpackName(string name)
		=> ModPackRegex.Replace(name, string.Empty);

	internal static bool IsValidModpackName(string name)
		=> !ModPackRegex.Match(name).Success && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

	public override void OnDeactivate()
	{
		_cts?.Cancel(false);
		_cts?.Dispose();
		_cts = null;
	}

	public override void OnActivate()
	{
		_cts = new CancellationTokenSource();
		_scrollPanel.Append(_uiLoader);
		_modPacks.Clear();

		Task.Run(() => {
			Directory.CreateDirectory(ModPacksDirectory);
			var dirs = Directory.GetDirectories(ModPacksDirectory, "*", SearchOption.TopDirectoryOnly);
			var files = Directory.GetFiles(ModPacksDirectory, "*.json", SearchOption.TopDirectoryOnly);
			var ModPacksToAdd = new List<UIElement>();
			foreach (string modPackPath in files.Concat(dirs)) {
				try {
					if (!IsValidModpackName(Path.GetFileNameWithoutExtension(modPackPath)))
						throw new Exception();
					else if (Directory.Exists(modPackPath))
						ModPacksToAdd.Add(LoadModernModPack(modPackPath));
					else
						ModPacksToAdd.Add(LoadLegacyModPack(modPackPath));
				}
				catch {
					var badModPackMessage = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackMalformed", Path.GetFileName(modPackPath))) {
						Width = { Percent = 1 },
						Height = { Pixels = 50, Percent = 0 }
					};
					ModPacksToAdd.Add(badModPackMessage);
				}
			}

			// Rather than use complicated lock and Monitor.TryEnter code in many places to gate interations over Children, we can do all UI updates at once at a time guaranteed to not be running any UI code which would otherwise cause Collection was modified exceptions
			Main.QueueMainThreadAction(() => {
				_modPacks.AddRange(ModPacksToAdd);
				_scrollPanel.RemoveChild(_uiLoader);
			});
		});
	}

	public UIModPackItem LoadModernModPack(string folderPath)
	{
		string enabledJson = Path.Combine(folderPath, "Mods", "enabled.json");

		string[] modPackMods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(enabledJson));
		if (modPackMods == null) {
			Utils.LogAndConsoleInfoMessage($"No contents in enabled.json at: {folderPath}. Is this correct?");
			modPackMods = new string[0];
		}

		var localMods = ModOrganizer.FindMods();

		return new UIModPackItem(folderPath, modPackMods, false, localMods);
	}

	public UIModPackItem LoadLegacyModPack(string jsonPath)
	{
		string[] modPackMods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(jsonPath));

		var localMods = ModOrganizer.FindMods();
		return new UIModPackItem(Path.GetFileNameWithoutExtension(jsonPath), modPackMods, true, localMods);
	}

	public static void SaveSnapshot(string configsPath, string modsPath)
	{
		if (!Directory.Exists(Config.ConfigManager.ModConfigPath))
			Directory.CreateDirectory(Config.ConfigManager.ModConfigPath);

		Directory.CreateDirectory(configsPath);
		Directory.CreateDirectory(modsPath);

		var configsAll = Directory.EnumerateFiles(Config.ConfigManager.ModConfigPath);

		// Export enabled.json to the modpack
		File.Copy(Path.Combine(ModOrganizer.modPath, "enabled.json"), Path.Combine(modsPath, "enabled.json"), true);

		File.WriteAllText(Path.Combine(modsPath, "tmlversion.txt"), BuildInfo.tMLVersion.ToString());

		// Export Mods Utilized
		var workshopIds = new List<string>();
		foreach (var mod in ModLoader.Mods) {
			if (mod.File == null)
				continue; // internal ModLoader mod

			// Export Config Files
			/*
			foreach (var config in configsAll.Where(c => Path.GetFileName(c).StartsWith(mod.Name + '_'))) {
				// Overwrite existing config file to fix config collisions (#2661)
				File.Copy(config, Path.Combine(configsPath, Path.GetFileName(config)), true);
			}
			*/

			// Export Publish ID information from Steam Workshop mods for easy re-downloading/downloading
			if (ModOrganizer.TryReadManifest(ModOrganizer.GetParentDir(mod.File.path), out var info)) {
				workshopIds.Add(info.workshopEntryId.ToString());
			}

			// Copy the frozen mod to the Mod Pack if its different/new
			if (mod.File.path != Path.Combine(modsPath, mod.Name + ".tmod"))
				File.Copy(mod.File.path, Path.Combine(modsPath, mod.Name + ".tmod"), true);
		}

		// Write the required workshop mods to install.txt
		File.WriteAllLines(Path.Combine(modsPath, "install.txt"), workshopIds);
	}

	public static void ExportSnapshot(string modPackName)
	{
		string instancePath = Path.Combine(Directory.GetCurrentDirectory(), modPackName);

		Directory.CreateDirectory(instancePath);
		Directory.CreateDirectory(Path.Combine(instancePath, "SaveData"));

		//TODO: When implementing ModConfig as part of Mod Pack, update
		string modsPath =  ModPackModsPath(modPackName);
		string configPath = Config.ConfigManager.ModConfigPath; //ModPackConfigPath(modPackName);

		// Deploy Mods, Configs to instance
		FileUtilities.CopyFolder(modsPath, Path.Combine(instancePath, "SaveData", "Mods"));
		FileUtilities.CopyFolder(configPath, Path.Combine(instancePath, "SaveData", "ModConfigs"));

		// Customize the instance to look at the correct folder
		File.WriteAllText(Path.Combine(instancePath, "cli-argsConfig.txt"), $"-tmlsavedirectory {Path.Combine(instancePath, "SaveData")}\n-steamworkshopfolder none");

		//TODO: Install the correct tModLoader version
		/*
		string tmlVersion = File.ReadAllText(Path.Combine(modsPath, "tmlversion.txt"));
		string downloadFrom = $"https://github.com/tModLoader/tModLoader/releases/download/v{tmlVersion}/tModLoader.zip";
		var downloadFile = new DownloadManager.DownloadFile(downloadFrom, Path.Combine(instancePath, "tModLoader.zip"), $"Installing tModLoader {tmlVersion}");

		downloadFile.OnComplete += () => ExtractTmlInstall(instancePath);
		Interface.downloadProgress.HandleDownloads(downloadFile);
		*/

		Logging.tML.Info($"Exported instance of Frozen Mod Pack {modPackName} to {instancePath}");
		Utils.OpenFolder(instancePath);
	}

	public static void ExtractTmlInstall(string instancePath)
	{
		string zipFilePath = Path.Combine(instancePath, "tModLoader.zip");

		using (var zip = ZipFile.Read(zipFilePath))
			zip.ExtractAll(instancePath);

		File.Delete(zipFilePath);
	}
}
