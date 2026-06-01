using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.Audio;
using ReLogic.Content;
using ReLogic.OS;

namespace Terraria.ModLoader.UI;

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
	private readonly UIAutoScaleTextTextPanel<string> _playInstanceButton;
	private readonly UIAutoScaleTextTextPanel<string> _exportPackInstanceButton;
	private readonly UIAutoScaleTextTextPanel<string> _removePackInstanceButton;
	private readonly UIAutoScaleTextTextPanel<string> _importFromPackLocalButton;
	private readonly UIAutoScaleTextTextPanel<string> _removePackLocalButton;
	private readonly UIImageButton _deleteButton;
	private readonly UIImageButton _fakeDeleteButton;
	private readonly string _filename;
	private readonly string _filepath;
	private readonly bool _legacy;
	private string _tooltip;
	private bool IsLocalModPack => ModOrganizer.ModPackActive == _filepath;

	public UIModPackItem(string name, string[] mods, bool legacy, IEnumerable<LocalMod> localMods)
	{
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
		if (IsLocalModPack)
			BackgroundColor = Color.MediumPurple * 0.7f;
		_dividerTexture = UICommon.DividerTexture;
		_innerPanelTexture = UICommon.InnerPanelTexture;
		Height.Pixels = _legacy ? 126 : 210;
		Width.Percent = 1f;
		SetPadding(6f);

		// The below doesn't care about legacy
		_modName = new UIText(_filename) {
			Left = { Pixels = 10 },
			Top = { Pixels = 5 }
		};
		Append(_modName);

		// View Pack (1-R)
		var viewListButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackViewList")) {
			Width = { Pixels = 100 },
			Height = { Pixels = 36 },
			Left = { Pixels = 407 },
			Top = { Pixels = 40 }
		}.WithFadedMouseOver();
		viewListButton.PaddingTop -= 2f;
		viewListButton.PaddingBottom -= 2f;
		viewListButton.OnLeftClick += ViewListInfo;
		Append(viewListButton);

		// Enable (1-L)
		_enableListButton = new UIAutoScaleTextTextPanel<string>(
			Language.GetTextValue("tModLoader.ModPackEnableThisList")) {
			Width = { Pixels = 151 },
			Height = { Pixels = 36 },
			Left = { Pixels = 248 },
			Top = { Pixels = 40 }
		}.WithFadedMouseOver();
		_enableListButton.PaddingTop -= 2f;
		_enableListButton.PaddingBottom -= 2f;
		_enableListButton.OnLeftClick += EnableList;
		Append(_enableListButton);

		// Enable List Only (2-L)
		_enableListOnlyButton = new UIAutoScaleTextTextPanel<string>(
			Language.GetTextValue("tModLoader.ModPackEnableOnlyThisList")) {
			Width = { Pixels = 190 },
			Height = { Pixels = 36 },
			Left = { Pixels = 50 },
			Top = { Pixels = 40 }
		}.WithFadedMouseOver();
		_enableListOnlyButton.PaddingTop -= 2f;
		_enableListOnlyButton.PaddingBottom -= 2f;
		_enableListOnlyButton.OnLeftClick += EnabledListOnly;
		Append(_enableListOnlyButton);

		// View on Browser (2-R)
		_viewInModBrowserButton = new UIAutoScaleTextTextPanel<string>(
			Language.GetTextValue("tModLoader.ModPackViewModsInModBrowser")) {
			Width = { Pixels = 246 },
			Height = { Pixels = 36 },
			Left = { Pixels = 50 },
			Top = { Pixels = 80 }
		}.WithFadedMouseOver();
		_viewInModBrowserButton.PaddingTop -= 2f;
		_viewInModBrowserButton.PaddingBottom -= 2f;
		_viewInModBrowserButton.OnLeftClick += DownloadMissingMods;
		Append(_viewInModBrowserButton);

		// Update From Local (3-L)
		_updateListWithEnabledButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackUpdateListWithEnabled")) {
			Width = { Pixels = 225 },
			Height = { Pixels = 36 },
			Left = { Pixels = 304 },
			Top = { Pixels = 80 }
		}.WithFadedMouseOver();
		_updateListWithEnabledButton.PaddingTop -= 2f;
		_updateListWithEnabledButton.PaddingBottom -= 2f;
		_updateListWithEnabledButton.OnLeftClick += UpdateModPack;
		Append(_updateListWithEnabledButton);

		// Delete button
		_deleteButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete")) {
			Top = { Pixels = 40 }
		};
		_deleteButton.OnLeftClick += DeleteButtonClick;
		this.AddOrRemoveChild(_deleteButton, !IsLocalModPack);

		_fakeDeleteButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete")) {
			Top = { Pixels = 40 }
		};
		_fakeDeleteButton.SetVisibility(0.4f, 0.4f);
		this.AddOrRemoveChild(_fakeDeleteButton, IsLocalModPack);

		if (_legacy)
			return;

		// The new stuff

		// Import From Pack (Local) (3-L)
		_importFromPackLocalButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.InstallPackLocal")) {
			Width = { Pixels = 225 },
			Height = { Pixels = 36 },
			Left = { Pixels = 50 },
			Top = { Pixels = 120 }
		}.WithFadedMouseOver();
		_importFromPackLocalButton.PaddingTop -= 2f;
		_importFromPackLocalButton.PaddingBottom -= 2f;
		_importFromPackLocalButton.OnLeftClick += ImportModPackLocal;
		Append(_importFromPackLocalButton);

		// Remove Pack (Local) (3-R)
		_removePackLocalButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.RemovePackLocal")) {
			Width = { Pixels = 225 },
			Height = { Pixels = 36 },
			Left = { Pixels = 280 },
			Top = { Pixels = 120 }
		}.WithFadedMouseOver();
		_removePackLocalButton.PaddingTop -= 2f;
		_removePackLocalButton.PaddingBottom -= 2f;
		_removePackLocalButton.OnLeftClick += RemoveModPackLocal;
		Append(_removePackLocalButton);

		// Export Pack Instance (4-L)
		_exportPackInstanceButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ExportPackInstance")) {
			Width = { Pixels = 200 },
			Height = { Pixels = 36 },
			Left = { Pixels = 10 },
			Top = { Pixels = 160 }
		}.WithFadedMouseOver();
		_exportPackInstanceButton.PaddingTop -= 2f;
		_exportPackInstanceButton.PaddingBottom -= 2f;
		_exportPackInstanceButton.OnLeftClick += ExportInstance;
		Append(_exportPackInstanceButton);

		string instancePath = Path.Combine(Directory.GetCurrentDirectory(), _filename);
		if (Directory.Exists(instancePath)) {
			// Delete Instance (4-R)
			_removePackInstanceButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.DeletePackInstance")) {
				Width = { Pixels = 140 },
				Height = { Pixels = 36 },
				Left = { Pixels = 370 },
				Top = { Pixels = 160 }
			}.WithFadedMouseOver();
			_removePackInstanceButton.PaddingTop -= 2f;
			_removePackInstanceButton.PaddingBottom -= 2f;
			_removePackInstanceButton.OnLeftClick += DeleteInstance;
			Append(_removePackInstanceButton);

			//TODO: Play Instance (4-M)
			/*
			_playInstanceButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("Play Instance")) {
				Width = { Pixels = 140 },
				Height = { Pixels = 36 },
				Left = { Pixels = 220 },
				Top = { Pixels = 160 }
			}.WithFadedMouseOver();
			_playInstanceButton.PaddingTop -= 2f;
			_playInstanceButton.PaddingBottom -= 2f;
			_playInstanceButton.OnLeftClick += PlayInstance;
			Append(_playInstanceButton);
			*/
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_tooltip = null;
		base.Draw(spriteBatch);
		if (!string.IsNullOrEmpty(_tooltip)) {
			UICommon.TooltipMouseText(_tooltip);
		}
	}

	private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width)
	{
		spriteBatch.Draw(_innerPanelTexture.Value, position, new Rectangle(0, 0, 8, _innerPanelTexture.Height()), Color.White);
		spriteBatch.Draw(_innerPanelTexture.Value, new Vector2(position.X + 8f, position.Y), new Rectangle(8, 0, 8, _innerPanelTexture.Height()), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
		spriteBatch.Draw(_innerPanelTexture.Value, new Vector2(position.X + width - 8f, position.Y), new Rectangle(16, 0, 8, _innerPanelTexture.Height()), Color.White);
	}

	private void DrawEnabledText(SpriteBatch spriteBatch, Vector2 drawPos)
	{
		string text = Language.GetTextValue("tModLoader.ModPackModsAvailableStatus", _numMods, _numModsEnabled, _numModsDisabled, _missing.Count);
		Color color = (_missing.Count > 0 ? Color.Red : (_numModsDisabled > 0 ? Color.Yellow : (ModLoader.EnabledMods.Count > _mods.Count() ? Color.LimeGreen : Color.Green)));

		Utils.DrawBorderString(spriteBatch, text, drawPos, color);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
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

		if (_enableListOnlyButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.ModPackEnableOnlyThisListDesc");
		}
		else if (_enableListButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.ModPackEnableThisListDesc");
		}
		else if (_exportPackInstanceButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.ExportPackInstanceDesc");
		}
		else if (_removePackInstanceButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.DeletePackInstanceDesc");
		}
		else if (_playInstanceButton?.IsMouseHovering == true) {
			_tooltip = "Play tModLoader using InstallDirectory/<ModPackName>";
		}
		else if (_importFromPackLocalButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.InstallPackLocalDesc");
		}
		else if (_removePackLocalButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.RemovePackLocalDesc");
		}
		else if (_viewInModBrowserButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.ModPackViewModsInModBrowserDesc");
		}
		else if (_updateListWithEnabledButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.ModPackUpdateListWithEnabledDesc");
		}
		else if (_deleteButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.ModPackDelete");
		}
		else if (_fakeDeleteButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.ModPackDisableToDelete");
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		if (Path.GetFileNameWithoutExtension(ModOrganizer.ModPackActive) == _filename)
			BackgroundColor = Color.MediumPurple * 0.4f;
		else
			BackgroundColor = UICommon.DefaultUIBlue;

		BorderColor = new Color(89, 116, 213);
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);
		if (Path.GetFileNameWithoutExtension(ModOrganizer.ModPackActive) == _filename)
			BackgroundColor = Color.MediumPurple * 0.7f;
		else
			BackgroundColor = UICommon.DefaultUIBlueMouseOver;

		BorderColor = new Color(89, 116, 213) * 0.7f;
	}

	private void DeleteButtonClick(UIMouseEvent evt, UIElement listeningElement)
	{
		if (IsLocalModPack) {
			Logging.tML.Warn("Tried to delete active modpack somehow");
			return;
		}

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

		Logging.tML.Info($"Deleted Mod Pack {modPackItem._filename}");
		Interface.modPacksMenu.OnDeactivate(); // should reload
		Interface.modPacksMenu.OnActivate(); // should reload
	}

	private static void EnableList(UIMouseEvent evt, UIElement listeningElement)
	{
		UIModPackItem modListItem = (UIModPackItem)listeningElement.Parent;
		foreach (var mod in ModOrganizer.FindMods()) {
			mod.Enabled = mod.Enabled || modListItem._mods.Contains(mod.Name);
		}

		if (modListItem._missing.Count > 0) {
			Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModPackModsMissing", string.Join("\n", modListItem._missing)), Interface.modPacksMenuID);
		}

		Logging.tML.Info($"Enabled Collection of mods defined in  Mod Pack {modListItem._filename}");
		ModLoader.OnSuccessfulLoad += () => Main.menuMode = Interface.modPacksMenuID;
		ModLoader.Reload();
	}

	private List<ModPubId_t> GetModPackBrowserIds()
	{
		if (!_legacy) {
			string path = UIModPacks.ModPackModsPath(_filename);
			var ids = File.ReadAllLines(Path.Combine(path, "install.txt"));
			return Array.ConvertAll(ids, x => new ModPubId_t() { m_ModPubId = x }).ToList();
		}

		var query = new QueryParameters() { searchModSlugs = _mods };
		if (!WorkshopHelper.QueryHelper.AQueryInstance.TryGetGroupPublishIdsByInternalName(query, out var modIds))
			return new List<ModPubId_t>(); // query failed. TODO, actually show an error UI instead

		var output = new List<ModPubId_t>();
		foreach (var item in modIds) {
			if (item != "0")
				output.Add(new ModPubId_t() { m_ModPubId = item });
		}

		return output;
	}

	private static void DownloadMissingMods(UIMouseEvent evt, UIElement listeningElement)
	{
		UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);
		Interface.modBrowser.Activate();
		Interface.modBrowser.FilterTextBox.Text = "";
		Interface.modBrowser.SpecialModPackFilter = modpack.GetModPackBrowserIds();
		Interface.modBrowser.SpecialModPackFilterTitle = Language.GetTextValue("tModLoader.MBFilterModlist");// Too long: " + modListItem.modName.Text;
		Interface.modBrowser.UpdateFilterMode = UpdateFilter.All; // Set to 'All' so all mods from ModPack are visible
		Interface.modBrowser.ModSideFilterMode = ModSideFilter.All;
		Interface.modBrowser.ResetTagFilters();
		SoundEngine.PlaySound(SoundID.MenuOpen);

		Interface.modBrowser.PreviousUIState = Interface.modPacksMenu;
		Main.menuMode = Interface.modBrowserID;
	}

	private static void EnabledListOnly(UIMouseEvent evt, UIElement listeningElement)
	{
		UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);
		ModLoader.DisableAllMods();
		EnableList(evt, listeningElement);

		Logging.tML.Info($"Enabled only mods defined in Collection {modpack._filename}");
	}

	private static void UpdateModPack(UIMouseEvent evt, UIElement listeningElement)
	{
		UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);
		UIModPacks.SaveModPack(modpack._filename);

		if (modpack._filepath == ModOrganizer.ModPackActive) {
			ModLoader.DisableAllMods();
			Logging.tML.Info($"Cleaning up removed tmods {modpack._filename}");
			ModLoader.OnSuccessfulLoad += () => {
				foreach (var file in Directory.EnumerateFiles(UIModPacks.ModPackModsPath(modpack._filename), "*.tmod"))
					if (!modpack._mods.Contains(Path.GetFileNameWithoutExtension(file)))
						File.Delete(file);

				EnableList(evt, listeningElement);
			};
			ModLoader.Reload();
		}
		else {
			foreach (var file in Directory.EnumerateFiles(UIModPacks.ModPackModsPath(modpack._filename), "*.tmod"))
				if (!modpack._mods.Contains(Path.GetFileNameWithoutExtension(file)))
					File.Delete(file);
		}

		Interface.modPacksMenu.OnDeactivate(); // should reload
		Interface.modPacksMenu.OnActivate(); // should reload
	}

	private static void ImportModPackLocal(UIMouseEvent evt, UIElement listeningElement)
	{
		UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);
		ModOrganizer.ModPackActive = modpack._filepath;
		Main.SaveSettings();

		//TODO: Add code to utilize the saved configs

		Logging.tML.Info($"Enabled Frozen Mod Pack {modpack._filename}");
		EnabledListOnly(evt, listeningElement);
	}

	private static void RemoveModPackLocal(UIMouseEvent evt, UIElement listeningElement)
	{
		// Clear active Mod Pack
		UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);
		ModOrganizer.ModPackActive = null;
		Main.SaveSettings();

		//TODO: Add code to utilize the saved configs

		ModLoader.DisableAllMods();
		Logging.tML.Info($"Disabled Frozen Mod Pack {modpack._filename}");
		ModLoader.OnSuccessfulLoad += () => Main.menuMode = Interface.modPacksMenuID;
		ModLoader.Reload();
	}

	private static void ExportInstance(UIMouseEvent evt, UIElement listeningElement)
	{
		UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);

		UIModPacks.ExportSnapshot(modpack._filename);
		Interface.modPacksMenu.OnDeactivate(); // should reload
		Interface.modPacksMenu.OnActivate(); // should reload
	}

	private static void PlayInstance(UIMouseEvent evt, UIElement listeningElement)
	{
		UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);

		string instancePath = Path.Combine(Directory.GetCurrentDirectory(), modpack._filename);

		string launchScript = Path.Combine(instancePath, Platform.IsWindows ? "start-tModLoader.bat" : "start-tModLoader.sh");

		Process.Start(new ProcessStartInfo() {
			FileName = launchScript,
			UseShellExecute = true
		});
	}

	private static void DeleteInstance(UIMouseEvent evt, UIElement listeningElement)
	{
		UIModPackItem modpack = ((UIModPackItem)listeningElement.Parent);
		string instancePath = Path.Combine(Directory.GetCurrentDirectory(),modpack._filename);

		Directory.Delete(instancePath, true);
		Interface.modPacksMenu.OnDeactivate(); // should reload
		Interface.modPacksMenu.OnActivate(); // should reload
	}

	private static void ViewListInfo(UIMouseEvent evt, UIElement listeningElement)
	{
		UIModPackItem modListItem = ((UIModPackItem)listeningElement.Parent);
		SoundEngine.PlaySound(10);
		string message = "";
		foreach (string mod in modListItem._mods) {
			message += mod + (modListItem._missing.Contains(mod) ? Language.GetTextValue("tModLoader.ModPackMissing") : ModLoader.IsEnabled(mod) ? "" : Language.GetTextValue("tModLoader.ModPackDisabled")) + "\n";
		}
		//Interface.infoMessage.SetMessage($"This list contains the following mods:\n{String.Join("\n", ((UIModListItem)listeningElement.Parent).mods)}");
		Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModPackModsContained", message), Interface.modPacksMenuID);
	}

	public override int CompareTo(object obj)
	{
		if (!(obj is UIModPackItem item)) {
			return base.CompareTo(obj);
		}
		return string.Compare(_filename, item._filename, StringComparison.Ordinal);
	}
}
