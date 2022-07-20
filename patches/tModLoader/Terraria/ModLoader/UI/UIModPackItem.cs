using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;
using Terraria.Utilities;
using Terraria.Audio;
using ReLogic.Content;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.UI
{
	//TODO common 'Item' code
	internal class UIModPackItem : UIPanel
	{
		// Name -- > x in list disabled
		// Super List             5 Total, 3 Loaded, 1 Disabled, 1 Missing
		// Enable Only this List, Add mods to enabled, See mods in list

		// X mods, 3 enabled, 2 disabled.      Enable only, Add Mods
		// More info? see list of mods.
		// user will reload if needed (added)

		// TODO update this list button.

		private readonly Asset<Texture2D> _dividerTexture;
		private readonly Asset<Texture2D> _innerPanelTexture;
		private readonly UIText _modName;
		private readonly string[] _mods;
		private readonly List<string> _missing = new();
		private readonly int _numMods;
		private readonly int _numModsEnabled;
		private readonly int _numModsDisabled;
		private readonly UIAutoScaleTextTextPanel<string> _enableListButton;
		private readonly UIAutoScaleTextTextPanel<string> _enableListOnlyButton;
		private readonly UIAutoScaleTextTextPanel<string> _viewInModBrowserButton;
		private readonly UIAutoScaleTextTextPanel<string> _updateListWithEnabledButton;
		private readonly UIImageButton _deleteButton;
		private readonly string _filename;
		private readonly string _filepath;
		private readonly bool _legacy;

		private static string ConfigBackups => Path.Combine(Main.SavePath, "ModConfigsBackups");

		public UIModPackItem(string name, string[] mods, bool legacy, IEnumerable<LocalMod> localMods) {
			_legacy = legacy;
			_filename = _legacy ? name : Path.GetFileNameWithoutExtension(name);
			_filepath = name;

			_numModsEnabled = 0;
			_numModsDisabled = 0;

			_mods = mods;
			_numMods = mods.Length;

			foreach (string mod in mods) {
				if (localMods.SingleOrDefault(m => m.Name == mod) is LocalMod localMod) {
					if (localMod.Enabled) {
						_numModsEnabled++;
					}
					else {
						_numModsDisabled++;
					}
				}
				else {
					_missing.Add(mod);
				}
			}

			BorderColor = new Color(89, 116, 213) * 0.7f;
			_dividerTexture = UICommon.DividerTexture;
			_innerPanelTexture = UICommon.InnerPanelTexture;
			Height.Pixels = 126;
			Width.Percent = 1f;
			SetPadding(6f);

			// The below doesn't care about legacy
			_modName = new UIText(_filename) {
				Left = { Pixels = 10 },
				Top = { Pixels = 5 }
			};
			Append(_modName);

			var viewListButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackViewList")) {
				Width = { Pixels = 100 },
				Height = { Pixels = 36 },
				Left = { Pixels = 407 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			viewListButton.PaddingTop -= 2f;
			viewListButton.PaddingBottom -= 2f;
			viewListButton.OnClick += ViewListInfo;
			Append(viewListButton);

			_enableListButton = new UIAutoScaleTextTextPanel<string>(
				Language.GetTextValue(_legacy ? "tModLoader.ModPackEnableThisList" : "tModLoader.DeactivateModPack")) {
				Width = { Pixels = 151 },
				Height = { Pixels = 36 },
				Left = { Pixels = 248 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			_enableListButton.PaddingTop -= 2f;
			_enableListButton.PaddingBottom -= 2f;
			_enableListButton.OnClick += _legacy ? EnableList : DeactivateModPack;
			Append(_enableListButton);

			_enableListOnlyButton = new UIAutoScaleTextTextPanel<string>(
				Language.GetTextValue(_legacy ? "tModLoader.ModPackEnableOnlyThisList": "tModLoader.ActivateModPack")) {
				Width = { Pixels = 190 },
				Height = { Pixels = 36 },
				Left = { Pixels = 50 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			_enableListOnlyButton.PaddingTop -= 2f;
			_enableListOnlyButton.PaddingBottom -= 2f;
			_enableListOnlyButton.OnClick += ActivateModPack;
			Append(_enableListOnlyButton);

			_viewInModBrowserButton = new UIAutoScaleTextTextPanel<string>(
				Language.GetTextValue(_legacy ? "tModLoader.ModPackViewModsInModBrowser" : "tModLoader.DownloadMissingMods")) {
				Width = { Pixels = 246 },
				Height = { Pixels = 36 },
				Left = { Pixels = 50 },
				Top = { Pixels = 80 }
			}.WithFadedMouseOver();
			_viewInModBrowserButton.PaddingTop -= 2f;
			_viewInModBrowserButton.PaddingBottom -= 2f;
			_viewInModBrowserButton.OnClick += DownloadMissingMods;
			Append(_viewInModBrowserButton);

			_updateListWithEnabledButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackUpdateListWithEnabled")) {
				Width = { Pixels = 225 },
				Height = { Pixels = 36 },
				Left = { Pixels = 304 },
				Top = { Pixels = 80 }
			}.WithFadedMouseOver();
			_updateListWithEnabledButton.PaddingTop -= 2f;
			_updateListWithEnabledButton.PaddingBottom -= 2f;
			_updateListWithEnabledButton.OnClick += (a, b) => UIModPacks.SaveModPack(_filename);
			Append(_updateListWithEnabledButton);

			_deleteButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete")) {
				Top = { Pixels = 40 }
			};
			_deleteButton.OnClick += DeleteButtonClick;
			Append(_deleteButton);
		}

		private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width) {
			spriteBatch.Draw(_innerPanelTexture.Value, position, new Rectangle(0, 0, 8, _innerPanelTexture.Height()), Color.White);
			spriteBatch.Draw(_innerPanelTexture.Value, new Vector2(position.X + 8f, position.Y), new Rectangle(8, 0, 8, _innerPanelTexture.Height()), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(_innerPanelTexture.Value, new Vector2(position.X + width - 8f, position.Y), new Rectangle(16, 0, 8, _innerPanelTexture.Height()), Color.White);
		}

		private void DrawEnabledText(SpriteBatch spriteBatch, Vector2 drawPos) {
			string text = Language.GetTextValue("tModLoader.ModPackModsAvailableStatus", _numMods, _numModsEnabled, _numModsDisabled, _missing.Count);
			Color color = (_missing.Count > 0 ? Color.Red : (_numModsDisabled > 0 ? Color.Yellow : Color.Green));

			Utils.DrawBorderString(spriteBatch, text, drawPos, color);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(_dividerTexture.Value, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
			drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 355, innerDimensions.Y);
			DrawPanel(spriteBatch, drawPos, 350f);
			DrawEnabledText(spriteBatch, drawPos + new Vector2(10f, 5f));

			//if (this.enabled != ModLoader.ModLoaded(mod.name))
			//{
			//	drawPos += new Vector2(120f, 5f);
			//	Utils.DrawBorderString(spriteBatch, "Reload Required", drawPos, Color.White, 1f, 0f, 0f, -1);
			//}
			//string text = this.enabled ? "Click to Disable" : "Click to Enable";
			//drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 150f, innerDimensions.Y + 50f);
			//Utils.DrawBorderString(spriteBatch, text, drawPos, Color.White, 1f, 0f, 0f, -1);
		}

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			BackgroundColor = UICommon.DefaultUIBlue;
			BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt) {
			base.MouseOut(evt);
			BackgroundColor = new Color(63, 82, 151) * 0.7f;
			BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		private void DeleteButtonClick(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modPackItem = ((UIModPackItem)listeningElement.Parent);

			if (_legacy) {
				string path = UIModPacks.ModPacksDirectory + Path.DirectorySeparatorChar + modPackItem._filename + ".json";
				if (File.Exists(path)) {
					File.Delete(path);
				}
			}
			else {
				string path = Path.Combine(UIModPacks.ModPacksDirectory, _filename);
				if (Directory.Exists(path))
					Directory.Delete(path, true);
			}

			Interface.modPacksMenu.OnDeactivate(); // should reload
			Interface.modPacksMenu.OnActivate(); // should reload
		}

		private static void EnableList(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = (UIModPackItem)listeningElement.Parent;
			foreach (var mod in ModOrganizer.FindMods()) {
				mod.Enabled = modListItem._mods.Contains(mod.Name);
			}

			if (modListItem._missing.Count > 0) {
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModPackModsMissing", string.Join("\n", modListItem._missing)), Interface.modPacksMenuID);
			}

			ModLoader.OnSuccessfulLoad += () => Main.menuMode = Interface.modPacksMenuID;
			ModLoader.Reload();
		}

		private static void DownloadMissingMods(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);

			if (modpack._missing.Count == 0)
				return;

			if (modpack._legacy) {
				Interface.modBrowser.Activate();
				Interface.modBrowser.FilterTextBox.Text = "";
				Interface.modBrowser.SpecialModPackFilter = modpack._mods.ToList();
				Interface.modBrowser.SpecialModPackFilterTitle = Language.GetTextValue("tModLoader.MBFilterModlist");// Too long: " + modListItem.modName.Text;
				Interface.modBrowser.UpdateFilterMode = UpdateFilter.All; // Set to 'All' so all mods from ModPack are visible
				Interface.modBrowser.ModSideFilterMode = ModSideFilter.All;
				Interface.modBrowser.UpdateFilterToggle.SetCurrentState((int)Interface.modBrowser.UpdateFilterMode);
				Interface.modBrowser.ModSideFilterToggle.SetCurrentState((int)Interface.modBrowser.ModSideFilterMode);
				Interface.modBrowser.UpdateNeeded = true;
				SoundEngine.PlaySound(SoundID.MenuOpen);
				Main.menuMode = Interface.modBrowserID;
				return;
			}

			// TODO: what do we do if a different version of the mod is present (compared to the copy in the modpack)?
			// What if it's loaded from the dev path? You can't always close the file handle without unloading it.
			int missingModsLocallyRestored = 0;
			foreach (var missing in modpack._missing) {
				var copyPath = Path.Combine(modpack._filepath, "mods", missing + ".tmod");
				if (File.Exists(copyPath)) {
					File.Copy(copyPath, Path.Combine(ModLoader.ModPath, Path.GetFileName(copyPath)));
					missingModsLocallyRestored++;
				}
			}

			if (missingModsLocallyRestored < modpack._missing.Count) {
				string steamInstall = Path.Combine(modpack._filepath, "mods", "install.txt");
				string[] workshopIds = File.ReadAllLines(steamInstall);

				Social.Steam.WorkshopHelper.ModManager.DownloadBatch(workshopIds, Interface.modPacksMenu);
			}
		}

		private static void ActivateModPack(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);

			if (modpack._legacy) {
				EnableList(evt, listeningElement);
				return;
			}

			// Deploy Configs
			string deployedConfigs = Config.ConfigManager.ModConfigPath;
			string modpackConfigs = Path.Combine(modpack._filepath, "configs");

			if (!Directory.Exists(ConfigBackups))
				Directory.CreateDirectory(ConfigBackups);

			foreach (var file in Directory.EnumerateFiles(ConfigBackups))
				File.Delete(file);

			FileUtilities.CopyFolder(deployedConfigs, ConfigBackups);

			foreach (var file in Directory.EnumerateFiles(deployedConfigs))
				File.Delete(file);

			FileUtilities.CopyFolder(modpackConfigs, deployedConfigs);

			// Deploy Mods
			DownloadMissingMods(evt, listeningElement);

			// Enable Mods
			EnableList(evt, listeningElement);
		}

		private static void DeactivateModPack(UIMouseEvent evt, UIElement listeningElement) {
			ModLoader.DisableAllMods();

			UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);

			// Restore configs
			string deployedConfigs = Config.ConfigManager.ModConfigPath;

			foreach (var file in Directory.EnumerateFiles(deployedConfigs))
				File.Delete(file);
			
			if (!Directory.Exists(ConfigBackups))
				Directory.CreateDirectory(ConfigBackups);
			
			FileUtilities.CopyFolder(ConfigBackups, deployedConfigs);

			foreach (var file in Directory.EnumerateFiles(ConfigBackups))
				File.Delete(file);

			// Delete non-workshop mods
			string modpackMods = Path.Combine(modpack._filepath, "mods");
			foreach (var mod in Directory.EnumerateFiles(modpackMods, "*.tmod")) {
				File.Copy(mod, Path.Combine(ModLoader.ModPath, Path.GetFileName(mod)));
			}

			Interface.modPacksMenu.OnDeactivate(); // should reload
			Interface.modPacksMenu.OnActivate(); // should reload
		}

		private static void ViewListInfo(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = ((UIModPackItem)listeningElement.Parent);
			SoundEngine.PlaySound(10);
			string message = "";
			foreach (string mod in modListItem._mods) {
				message += mod + (modListItem._missing.Contains(mod) ? Language.GetTextValue("tModLoader.ModPackMissing") : ModLoader.IsEnabled(mod) ? "" : Language.GetTextValue("tModLoader.ModPackDisabled")) + "\n";
			}
			//Interface.infoMessage.SetMessage($"This list contains the following mods:\n{String.Join("\n", ((UIModListItem)listeningElement.Parent).mods)}");
			Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModPackModsContained", message), Interface.modPacksMenuID);
		}

		public override int CompareTo(object obj) {
			if (!(obj is UIModPackItem item)) {
				return base.CompareTo(obj);
			}
			return string.Compare(_filename, item._filename, StringComparison.Ordinal);
		}
	}
}
