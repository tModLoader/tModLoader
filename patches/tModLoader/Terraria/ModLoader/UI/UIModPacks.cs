﻿using Microsoft.Xna.Framework;
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
using Terraria.Audio;

namespace Terraria.ModLoader.UI
{
	internal class UIModPacks : UIState, IHaveBackButtonCommand
	{
		internal const string MODPACK_REGEX = "[^a-zA-Z0-9_.-]+";
		internal static string ModPacksDirectory = Path.Combine(ModLoader.ModPath, "ModPacks");
		internal static string[] Mods;

		private UIList _modPacks;
		private UILoaderAnimatedImage _uiLoader;
		private UIPanel _scrollPanel;
		private CancellationTokenSource _cts;
		public UIState PreviousUIState { get; set; }

		public override void OnInitialize() {
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

			var titleTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModPacksHeader"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.DefaultUIBlue
			}.WithPadding(15f);
			uIElement.Append(titleTextPanel);

			var folderButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.OpenModPackFolder")) {
				Width = new StyleDimension(-10f, 1f / 2f),
				Height = { Pixels = 40 },
				VAlign = 0.9f,
				HAlign = 0f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			folderButton.OnClick += OpenFolder;
			uIElement.Append(folderButton);

			var backButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = new StyleDimension(-10f, 1f / 2f),
				Height = { Pixels = 40 },
				VAlign = 1f,
				HAlign = 0f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			var saveNewButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPacksSaveEnabledAsNewPack"));
			saveNewButton.CopyStyle(backButton);
			saveNewButton.TextColor = Color.Green;
			saveNewButton.VAlign = 1f;
			saveNewButton.HAlign = 1f;
			saveNewButton.WithFadedMouseOver();
			saveNewButton.OnClick += SaveNewModList;
			uIElement.Append(saveNewButton);

			Append(uIElement);
		}

		private static UIVirtualKeyboard _virtualKeyboard;
		private static UIVirtualKeyboard VirtualKeyboard =>
			_virtualKeyboard ?? (_virtualKeyboard = new UIVirtualKeyboard(
				Language.GetTextValue("tModLoader.ModPacksEnterModPackName"), "", SaveModPack, () => Main.menuMode = Interface.modPacksMenuID));

		private static void SaveNewModList(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(11);
			VirtualKeyboard.Text = "";
			Main.MenuUI.SetState(VirtualKeyboard);
			Main.menuMode = 888;
		}

		public static void SaveModPack(string filename) {
			// Sanitize input if not valid
			if (!IsValidModpackName(filename)) {
				VirtualKeyboard.Text = SanitizeModpackName(filename);
				return;
			}

			if (!Directory.Exists(Config.ConfigManager.ModConfigPath))
				Directory.CreateDirectory(Config.ConfigManager.ModConfigPath);

			string configsPath = Path.Combine(ModPacksDirectory, filename, "configs");
			Directory.CreateDirectory(configsPath);
			var configsAll = Directory.EnumerateFiles(Config.ConfigManager.ModConfigPath);

			string modsPath = Path.Combine(ModPacksDirectory, filename, "mods");
			Directory.CreateDirectory(modsPath);

			var modsSteamAll = ModOrganizer.FindMods(true);
			var workshopIds = new List<string>();

			var modsManualAll = Directory.EnumerateFiles(ModOrganizer.modPath, "*.tmod");

			foreach (var enabled in ModLoader.EnabledMods) {
				// Export Config Files
				foreach (var config in configsAll) {
					if (!config.Contains(enabled))
						continue;

					File.Copy(config, Path.Combine(configsPath, Path.GetFileName(config)));
				}

				// Prep work for export workshop mods to a download manager list
				foreach (var mod in modsSteamAll) {
					if (mod.Name != enabled)
						continue;

					string parentDir = ModOrganizer.GetParentDir(mod.modFile.path);
					if (!ModOrganizer.TryReadManifest(parentDir, out var info))
						continue;

					workshopIds.Add(info.workshopEntryId.ToString());
					break;
				}

				// Export non-workshop mods to the modpack
				foreach (var mod in modsManualAll) {
					if (!mod.Contains(enabled))
						continue;

					File.Copy(mod, Path.Combine(modsPath, Path.GetFileName(mod)));
					break;
				}
			}

			// Export enabled.json to the modpack
			string enabledJson = Path.Combine(ModOrganizer.modPath, "enabled.json");
			File.Copy(enabledJson, Path.Combine(modsPath, "enabled.json"), true);

			// Write the required workshop mods to install.txt
			File.Delete(Path.Combine(modsPath, "install.txt"));
			File.WriteAllLines(Path.Combine(modsPath, "install.txt"), workshopIds);


			// Legacy code we need to port forward???
			/*
			Directory.CreateDirectory(ModPacksDirectory);

			string path = Path.Combine(ModPacksDirectory, filename + ".json");
			var foundMods = ModOrganizer.FindMods().Select(x => x.Name).Intersect(ModLoader.EnabledMods).ToList();
			string json = JsonConvert.SerializeObject(foundMods, Formatting.Indented);
			File.WriteAllText(path, json);
			*/

			Main.menuMode = Interface.modPacksMenuID; // should reload
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(11);
			(this as IHaveBackButtonCommand).HandleBackButtonUsage();
		}

		private void OpenFolder(UIMouseEvent evt, UIElement listeningElement) {
			Utils.OpenFolder(ModPacksDirectory);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modsMenuID;
		}

		internal static string SanitizeModpackName(string name)
			=> Regex.Replace(name, MODPACK_REGEX, string.Empty, RegexOptions.Compiled);

		internal static bool IsValidModpackName(string name)
			=> !Regex.Match(name, MODPACK_REGEX, RegexOptions.Compiled).Success && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

		public override void OnDeactivate() {
			_cts?.Cancel(false);
			_cts?.Dispose();
			_cts = null;
		}

		public override void OnActivate() {
			_cts = new CancellationTokenSource();
			_scrollPanel.Append(_uiLoader);
			_modPacks.Clear();

			Task.Factory
				.StartNew(delegate {
					Mods = ModOrganizer.FindMods().Select(m => m.Name).ToArray();
					Directory.CreateDirectory(ModPacksDirectory);
					var dirs = Directory.GetDirectories(ModPacksDirectory, "*", SearchOption.TopDirectoryOnly);
					var files = Directory.GetFiles(ModPacksDirectory, "*.json", SearchOption.TopDirectoryOnly);
					return files.Concat(dirs);
				}, _cts.Token)

				.ContinueWith(task => {
					foreach (string modPackPath in task.Result) {
						try {
							if (!IsValidModpackName(Path.GetFileNameWithoutExtension(modPackPath))) {
								throw new Exception();
							}

							if (Directory.Exists(modPackPath)) {
								string enabledJson = Path.Combine(modPackPath, "mods", "enabled.json");
								string[] modPackMods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(enabledJson));
								_modPacks.Add(new UIModPackItem(modPackPath, modPackMods, false));
							}
								
							else {
								string[] modPackMods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(modPackPath));
								_modPacks.Add(new UIModPackItem(Path.GetFileNameWithoutExtension(modPackPath), modPackMods, true));
							}
						}
						catch {
							var badModPackMessage = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackMalformed", Path.GetFileName(modPackPath))) {
								Width = { Percent = 1 },
								Height = { Pixels = 50, Percent = 0 }
							};
							_modPacks.Add(badModPackMessage);
						}
					}
					_scrollPanel.RemoveChild(_uiLoader);
				}, _cts.Token, TaskContinuationOptions.None, TaskScheduler.Current);
		}
	}
}
