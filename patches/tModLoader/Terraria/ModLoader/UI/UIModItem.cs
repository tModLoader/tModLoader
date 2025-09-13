using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using Terraria.Social.Base;

namespace Terraria.ModLoader.UI;

internal class UIModItem : UIPanel
{
	private const float PADDING = 5f;
	private float left2ndLine = 0;

	private UIImage _moreInfoButton;
	private UIImage _modIcon;
	private UIImageFramed updatedModDot;
	private Version previousVersionHint;
	private UIHoverImage _keyImage;
	private UIImage _configButton;
	private UIText _modName;
	private UIModStateText _uiModStateText;
	internal UIAutoScaleTextTextPanel<string> tMLUpdateRequired;
	private UIImage _modReferenceIcon;
	private UIImage _translationModIcon;
	private UIImage _deleteModButton;
	private UIAutoScaleTextTextPanel<string> _dialogYesButton;
	private UIAutoScaleTextTextPanel<string> _dialogNoButton;
	private UIText _dialogText;
	private readonly LocalMod _mod;
	private bool modFromLocalModFolder;

	private bool _configChangesRequireReload;
	private bool _loaded;
	private int _modIconAdjust;
	private string _tooltip;
	private string[] _modReferences;
	private string[] _modDependents; // Note: Recursive
	private string[] _modDependencies; // Note: Recursive
	private string _modRequiresTooltip;
	public readonly string DisplayNameClean; // No chat tags: for search and sort functionality.

	private string ToggleModStateText {
		get {
			if (_mod.Enabled) {
				if(_modDependents.Any())
					return Language.GetTextValue("tModLoader.ModsDisableAndDependents", _mod.DisplayName, _modDependents.Length);
				return Language.GetTextValue("tModLoader.ModsDisable");
			}
			else {
				if (_modDependencies.Any())
					return Language.GetTextValue("tModLoader.ModsEnableAndDependencies", _mod.DisplayName, _modDependencies.Length);
				return Language.GetTextValue("tModLoader.ModsEnable");
			}
		}
	}

	public string ModName => _mod.Name;
	public bool NeedsReload => _mod.properties.side != ModSide.Server && (_mod.Enabled != _loaded || _configChangesRequireReload);

	public UIModItem(LocalMod mod)
	{
		_mod = mod;
		BorderColor = new Color(89, 116, 213) * 0.7f;
		Height.Pixels = 92;
		Width.Percent = 1f;
		SetPadding(6f);
		DisplayNameClean = _mod.DisplayNameClean;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		string text = _mod.DisplayName + " v" + _mod.modFile.Version;
		var modIcon = Main.Assets.Request<Texture2D>("Images/UI/DefaultResourcePackIcon", AssetRequestMode.ImmediateLoad);
		_modIconAdjust += 85;

		if (_mod.modFile.HasFile("icon.png")) {
			try {
				using (_mod.modFile.Open())
				using (var s = _mod.modFile.GetStream("icon.png")) {
					var iconTexture = Main.Assets.CreateUntracked<Texture2D>(s, ".png");

					if (iconTexture.Width() == 80 && iconTexture.Height() == 80) {
						modIcon = iconTexture;
					}
				}
			}
			catch (Exception e) {
				Logging.tML.Error("Unknown error", e);
			}
		}

		_modIcon = new UIImage(modIcon) {
			Left = { Percent = 0f },
			Top = { Percent = 0f },
			Width = { Pixels = 80 },
			Height = { Pixels = 80 },
			ScaleToFit = true,
		};
		Append(_modIcon);

		_modName = new UIText(text) {
			Left = new StyleDimension(_modIconAdjust, 0f),
			Top = { Pixels = 5 }
		};
		Append(_modName);

		_uiModStateText = new UIModStateText(_mod.Enabled) {
			Top = { Pixels = 40 },
			Left = { Pixels = _modIconAdjust }
		};
		_uiModStateText.OnLeftClick += ToggleEnabled;

		// Don't show the Enable/Disable button if there is no loadable version
		string updateVersion = null;
		string updateURL = "https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-players#beta-branches";
		Color updateColor = Color.Orange;

		// Detect if it's for a preview or stable version ahead of our time
		if (BuildInfo.tMLVersion.MajorMinorBuild() < _mod.tModLoaderVersion.MajorMinorBuild()) {
			updateVersion = $"v{_mod.tModLoaderVersion}";

			if (_mod.tModLoaderVersion.Build == 2)
				updateVersion = $"Preview {updateVersion}";
		}

		// Detect if it's for a different browser version entirely
		if (!CheckIfPublishedForThisBrowserVersion(out var modBrowserVersion)) {
			updateVersion = $"{modBrowserVersion} v{_mod.tModLoaderVersion}";
			updateColor = Color.Yellow;
		}

		// Hide the Enabled button if it's not for this built version
		if (updateVersion != null) {
			tMLUpdateRequired = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MBRequiresTMLUpdate", updateVersion)).WithFadedMouseOver(updateColor, updateColor * 0.7f);
			tMLUpdateRequired.BackgroundColor = updateColor * 0.7f;
			tMLUpdateRequired.Top.Pixels = 40;
			tMLUpdateRequired.Width.Pixels = 280;
			tMLUpdateRequired.Height.Pixels = 36;
			tMLUpdateRequired.Left.Pixels += _uiModStateText.Width.Pixels + _uiModStateText.Left.Pixels + PADDING;
			tMLUpdateRequired.OnLeftClick += (a, b) => {
				Utils.OpenToURL(updateURL);
			};
			Append(tMLUpdateRequired);
		}
		else
			Append(_uiModStateText);

		int bottomRightRowOffset = -36;
		_moreInfoButton = new UIImage(UICommon.ButtonModInfoTexture) {
			RemoveFloatingPointsFromDrawPosition = true,
			Width = { Pixels = 36 },
			Height = { Pixels = 36 },
			Left = { Pixels = bottomRightRowOffset, Precent = 1 },
			Top = { Pixels = 40 }
		};
		_moreInfoButton.OnLeftClick += ShowMoreInfo;
		Append(_moreInfoButton);

		if (ModLoader.TryGetMod(ModName, out var loadedMod) && ConfigManager.Configs.ContainsKey(loadedMod)) {
			bottomRightRowOffset -= 36;
			_configButton = new UIImage(UICommon.ButtonModConfigTexture) {
				RemoveFloatingPointsFromDrawPosition = true,
				Width = { Pixels = 36 },
				Height = { Pixels = 36f },
				Left = { Pixels = bottomRightRowOffset - PADDING, Precent = 1f },
				Top = { Pixels = 40f }
			};
			_configButton.OnLeftClick += OpenConfig;
			Append(_configButton);
			if (ConfigManager.ModNeedsReload(loadedMod)) {
				_configChangesRequireReload = true;
			}
		}

		_modReferences = _mod.properties.modReferences.Select(x => x.mod).ToArray();

		var availableMods = ModOrganizer.RecheckVersionsToLoad();
		_modRequiresTooltip = "";
		HashSet<string> allDependencies = new();
		GetDependencies(_mod.Name, allDependencies);
		_modDependencies = allDependencies.ToArray();
		if (_modDependencies.Any()) {
			string refs = string.Join("\n", _modDependencies.Select(x => "- " + (Interface.modsMenu.FindUIModItem(x)?._mod.DisplayName ?? x + Language.GetTextValue("tModLoader.ModPackMissing"))));
			_modRequiresTooltip += Language.GetTextValue("tModLoader.ModDependencyTooltip", refs);
		}
		void GetDependencies(string modName, HashSet<string> allDependencies)
		{
			var modItem = Interface.modsMenu.FindUIModItem(modName);
			if (modItem == null)
				return; // Mod isn't downloaded, can't determine further recursive dependencies

			var dependencies = modItem._mod.properties.modReferences.Select(x => x.mod).ToArray();
			foreach (var dependency in dependencies) {
				if (allDependencies.Add(dependency)) {
					GetDependencies(dependency, allDependencies);
				}
			}
		}

		HashSet<string> allDependents = new();
		GetDependents(_mod.Name, allDependents);
		_modDependents = allDependents.ToArray();
		if (_modDependents.Any()) {
			if (!string.IsNullOrWhiteSpace(_modRequiresTooltip))
				_modRequiresTooltip += "\n\n";
			string refs = string.Join("\n", _modDependents.Select(x => "- " + (Interface.modsMenu.FindUIModItem(x)?._mod.DisplayName ?? x + Language.GetTextValue("tModLoader.ModPackMissing"))));
			_modRequiresTooltip += Language.GetTextValue("tModLoader.ModDependentsTooltip", refs);
		}
		void GetDependents(string modName, HashSet<string> allDependents)
		{
			var dependents = availableMods
				.Where(m => m.properties.RefNames(includeWeak: false).Any(refName => refName.Equals(modName)))
				.Select(m => m.Name).ToArray();
			foreach (var dependent in dependents) {
				if (allDependents.Add(dependent))
					GetDependents(dependent, allDependents);
			}
		}

		if (!string.IsNullOrWhiteSpace(_modRequiresTooltip)) {
			var icon = UICommon.ButtonDepsTexture;
			_modReferenceIcon = new UIImage(icon) {
				RemoveFloatingPointsFromDrawPosition = true,
				Left = new StyleDimension(_uiModStateText.Left.Pixels + _uiModStateText.Width.Pixels + PADDING + left2ndLine, 0f),
				Top = { Pixels = 42.5f }
			};
			left2ndLine += 28;
			// _modReferenceIcon.OnLeftClick += EnableDependencies;

			Append(_modReferenceIcon);
		}

		if (_mod.properties.RefNames(true).Any() && _mod.properties.translationMod) {
			var icon = UICommon.ButtonTranslationModTexture;
			_translationModIcon = new UIImage(icon) {
				RemoveFloatingPointsFromDrawPosition = true,
				Left = new StyleDimension(_uiModStateText.Left.Pixels + _uiModStateText.Width.Pixels + PADDING + left2ndLine, 0f),
				Top = { Pixels = 42.5f }
			};
			left2ndLine += 28;
			Append(_translationModIcon);
		}

		// TODO: Keep this feature locked to Dev for now until we are sure modders are at fault for this warning.
		if (BuildInfo.IsDev && ModCompile.DeveloperMode && ModLoader.IsUnloadedModStillAlive(ModName)) {
			_keyImage = new UIHoverImage(UICommon.ButtonErrorTexture, Language.GetTextValue("tModLoader.ModDidNotFullyUnloadWarning")) {
				RemoveFloatingPointsFromDrawPosition = true,
				Left = { Pixels = _modIconAdjust + PADDING },
				Top = { Pixels = 3 }
			};

			Append(_keyImage);

			_modName.Left.Pixels += _keyImage.Width.Pixels + PADDING * 2f;
			_modName.Recalculate();
		}


		if (ModOrganizer.CheckStableBuildOnPreview(_mod)) {
			_keyImage = new UIHoverImage(Main.Assets.Request<Texture2D>(TextureAssets.Item[ItemID.LavaSkull].Name), Language.GetTextValue("tModLoader.ModStableOnPreviewWarning")) {
				Left = { Pixels = 4, Percent = 0.2f },
				Top = { Pixels = 0, Percent = 0.5f }
			};

			Append(_keyImage);
		}

		var modLocationIconTexture = _mod.location switch {
			ModLocation.Workshop => TextureAssets.Extra[243],
			ModLocation.Modpack => UICommon.ModLocationModPackIcon,
			ModLocation.Local => UICommon.ModLocationLocalIcon,
			_ => throw new NotImplementedException(),
		};
		var modLocationIcon = new UIHoverImage(modLocationIconTexture, Language.GetTextValue("tModLoader.ModFrom" + _mod.location)) {
			RemoveFloatingPointsFromDrawPosition = true,
			UseTooltipMouseText = true,
			Left = { Pixels = -22, Percent = 1f }
		};
		Append(modLocationIcon);

		if (loadedMod != null) {
			_loaded = true;
			// TODO: refactor and add nicer icons (and maybe not iterate 6 times)
			int[] values = { loadedMod.GetContent<ModItem>().Count(), loadedMod.GetContent<ModNPC>().Count(), loadedMod.GetContent<ModTile>().Count(), loadedMod.GetContent<ModWall>().Count(), loadedMod.GetContent<ModBuff>().Count(), loadedMod.GetContent<ModMount>().Count() };
			string[] localizationKeys = { "ModsXItems", "ModsXNPCs", "ModsXTiles", "ModsXWalls", "ModsXBuffs", "ModsXMounts" };
			int xOffset = -40;

			for (int i = 0; i < values.Length; i++) {
				if (values[i] > 0) {
					_keyImage = new UIHoverImage(Main.Assets.Request<Texture2D>(TextureAssets.InfoIcon[i].Name), Language.GetTextValue($"tModLoader.{localizationKeys[i]}", values[i])) {
						RemoveFloatingPointsFromDrawPosition = true,
						Left = { Pixels = xOffset, Percent = 1f }
					};

					Append(_keyImage);
					xOffset -= 18;
				}
			}
		}

		OnLeftDoubleClick += (e, el) => {
			if (tMLUpdateRequired != null)
				return;
			// Only trigger if we didn't target the ModStateText, otherwise we trigger this behavior twice
			if (e.Target.GetType() != typeof(UIModStateText))
				_uiModStateText.LeftClick(e);
		};

		if (!_loaded && ModOrganizer.CanDeleteFrom(_mod.location)) {
			bottomRightRowOffset -= 36;
			_deleteModButton = new UIImage(TextureAssets.Trash) {
				RemoveFloatingPointsFromDrawPosition = true,
				Width = { Pixels = 36 },
				Height = { Pixels = 36 },
				Left = { Pixels = bottomRightRowOffset - PADDING, Precent = 1 },
				Top = { Pixels = 42.5f }
			};
			_deleteModButton.OnLeftClick += QuickModDelete;
			Append(_deleteModButton);
		}

		var oldModVersionData = ModOrganizer.modsThatUpdatedSinceLastLaunch.FirstOrDefault(x => x.ModName == ModName);
		if (oldModVersionData != default) {
			previousVersionHint = oldModVersionData.previousVersion;
			var toggleImage = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");
			updatedModDot = new UIImageFramed(toggleImage, toggleImage.Frame(2, 1, 1, 0)) {
				Left = { Pixels = _modName.GetInnerDimensions().ToRectangle().Right + 8 /* _modIconAdjust*/, Percent = 0f },
				Top = { Pixels = 5, Percent = 0f },
				Color = previousVersionHint == null ? Color.Green : new Color(6, 95, 212)
			};
			//_modName.Left.Pixels += 18; // use these 2 for left of the modname

			Append(updatedModDot);
		}

		if (loadedMod != null && (_mod.modFile.path != loadedMod.File.path)) {
			var serverDiffMessage = new UITextPanel<string>($"v{loadedMod.Version} currently loaded due to multiplayer game session") {
				Left = new StyleDimension(0, 0f),
				Width = new StyleDimension(0, 1f),
				Height = new StyleDimension(30, 0f),
				BackgroundColor = Color.Orange,
				Top = { Pixels = 82 }
			};
			Append(serverDiffMessage);

			Height.Pixels = 130;
		}

		SetHoverColors(hovered: false);
	}

	// TODO: "Generate Language File Template" button in upcoming "Miscellaneous Tools" menu.
	/*private void GenerateLangTemplate_OnClick(UIMouseEvent evt, UIElement listeningElement) {
		Mod loadedMod = ModLoader.GetMod(ModName);
		var dictionary = (Dictionary<string, ModTranslation>)loadedMod.translations;
		var result = loadedMod.items.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "=")
			.Concat(loadedMod.items.Where(x => !dictionary.ContainsValue(x.Value.Tooltip)).Select(x => x.Value.Tooltip.Key + "="))
			.Concat(loadedMod.npcs.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "="))
			.Concat(loadedMod.buffs.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "="))
			.Concat(loadedMod.buffs.Where(x => !dictionary.ContainsValue(x.Value.Description)).Select(x => x.Value.Description.Key + "="))
			.Concat(loadedMod.projectiles.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "="));
		//.Concat(loadedMod.tiles.Where(x => !dictionary.ContainsValue(x.Value.)).Select(x => x.Value..Key + "="))
		//.Concat(loadedMod.walls.Where(x => !dictionary.ContainsValue(x.Value.)).Select(x => x.Value..Key + "="));

		result = result.Select(x => x.Remove(0, $"Mods.{ModName}.".Length));

		Platform.Get<IClipboard>().Value = string.Join("\n", result);

		// TODO: ITranslatable or something?
	}*/

	public override void Draw(SpriteBatch spriteBatch)
	{
		_tooltip = null;
		base.Draw(spriteBatch);
		if (!string.IsNullOrEmpty(_tooltip)) {
			//var bounds = GetOuterDimensions().ToRectangle();
			//bounds.Height += 16;
			UICommon.TooltipMouseText(_tooltip);
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		CalculatedStyle innerDimensions = GetInnerDimensions();
		var drawPos = new Vector2(innerDimensions.X + 5f + _modIconAdjust, innerDimensions.Y + 30f);
		spriteBatch.Draw(UICommon.DividerTexture.Value, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - _modIconAdjust) / 8f, 1f), SpriteEffects.None, 0f);
		drawPos = new Vector2(innerDimensions.X + 10f + _modIconAdjust, innerDimensions.Y + 45f);

		// TODO: These should just be UITexts
		if (_mod.properties.side != ModSide.Server && (_mod.Enabled != _loaded || _configChangesRequireReload)) {
			drawPos += new Vector2(_uiModStateText.Width.Pixels + left2ndLine, 0f);
			Utils.DrawBorderString(spriteBatch, _configChangesRequireReload ? Language.GetTextValue("tModLoader.ModReloadForced") : Language.GetTextValue("tModLoader.ModReloadRequired"), drawPos, Color.White, 1f, 0f, 0f, -1);
		}
		if (_mod.properties.side == ModSide.Server) {
			drawPos += new Vector2(90f, -2f);
			spriteBatch.Draw(UICommon.ModBrowserIconsTexture.Value, drawPos, new Rectangle(5 * 34, 3 * 34, 32, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			if (new Rectangle((int)drawPos.X, (int)drawPos.Y, 32, 32).Contains(Main.MouseScreen.ToPoint()))
				UICommon.DrawHoverStringInBounds(spriteBatch, Language.GetTextValue("tModLoader.ModIsServerSide"));
		}

		if (_moreInfoButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.ModsMoreInfo");
		}
		else if (_deleteModButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("UI.Delete");
		}
		else if (_modName?.IsMouseHovering == true && _mod?.properties.author.Length > 0) {
			_tooltip = Language.GetTextValue("tModLoader.ModsByline", _mod.properties.author);
		}
		else if (_uiModStateText?.IsMouseHovering == true) {
			_tooltip = ToggleModStateText;
		}
		else if (_configButton?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.ModsOpenConfig");
		}
		else if (updatedModDot?.IsMouseHovering == true) {
			if (previousVersionHint == null)
				_tooltip = Language.GetTextValue("tModLoader.ModAddedSinceLastLaunchMessage");
			else
				_tooltip = Language.GetTextValue("tModLoader.ModUpdatedSinceLastLaunchMessage", previousVersionHint);
		}
		else if (tMLUpdateRequired?.IsMouseHovering == true) {
			_tooltip = Language.GetTextValue("tModLoader.SwitchVersionInfoButton");
		}
		else if (_modReferenceIcon?.IsMouseHovering == true) {
			_tooltip = _modRequiresTooltip;
		}
		else if (_translationModIcon?.IsMouseHovering == true) {
			string refs = string.Join(", ", _mod.properties.RefNames(true)); // Translation mods can be strong or weak references.
			_tooltip = Language.GetTextValue("tModLoader.TranslationModTooltip", refs);
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		SetHoverColors(hovered: true);
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);
		SetHoverColors(hovered: false);
	}

	private void SetHoverColors(bool hovered)
	{
		BorderColor = hovered ? new Color(89, 116, 213) : new Color(89, 116, 213) * 0.7f;
		if (_mod.location == ModLocation.Local)
			BackgroundColor = hovered ? Color.MediumPurple : Color.MediumPurple * 0.7f;
		else if (_mod.location == ModLocation.Modpack)
			BackgroundColor = hovered ? Color.SkyBlue : Color.SkyBlue * 0.7f;
		else
			BackgroundColor = hovered ? UICommon.DefaultUIBlueMouseOver : UICommon.DefaultUIBlue;
	}

	private void ToggleEnabled(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuTick);
		_mod.Enabled = !_mod.Enabled;

		UpdateUIForEnabledChange();

		if (!_mod.Enabled) {
			DisableDependents();
			return;
		}

		EnableDependencies();
	}

	internal void Enable()
	{
		if (_mod.Enabled) { return; }
		_mod.Enabled = true;
		UpdateUIForEnabledChange();
	}

	internal void Disable()
	{
		if (!_mod.Enabled) { return; }
		_mod.Enabled = false;
		UpdateUIForEnabledChange();
	}

	private void UpdateUIForEnabledChange()
	{
		if (_mod.Enabled)
			_uiModStateText.SetEnabled();
		else
			_uiModStateText.SetDisabled();
	}

	internal void EnableDependencies()
	{
		var missingRefs = new List<string>();
		EnableDepsRecursive(missingRefs);

		if (missingRefs.Any()) {
			Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModDependencyModsNotFound", string.Join(", ", missingRefs)), Interface.modsMenuID);
		}
	}

	private void EnableDepsRecursive(List<string> missingRefs)
	{
		foreach (var name in _modReferences) {
			var dep = Interface.modsMenu.FindUIModItem(name);
			if (dep == null) {
				missingRefs.Add(name);
				continue;
			}
			dep.EnableDepsRecursive(missingRefs);
			dep.Enable();
		}
	}

	private void DisableDependents()
	{
		DisableDependentsRecursive();
	}

	private void DisableDependentsRecursive()
	{
		foreach (var name in _modDependents) {
			var dep = Interface.modsMenu.FindUIModItem(name);
			if (dep == null) {
				continue;
			}
			dep.DisableDependentsRecursive();
			dep.Disable();
		}
	}

	internal void ShowMoreInfo(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		Interface.modInfo.Show(ModName, _mod.DisplayName, Interface.modsMenuID, _mod, _mod.properties.description, _mod.properties.homepage);
	}

	internal void OpenConfig(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		Interface.modConfigList.ModToSelectOnOpen = ModLoader.GetMod(ModName);
		Main.menuMode = Interface.modConfigListID;
	}

	public override int CompareTo(object obj)
	{
		var item = obj as UIModItem;
		if (item == null)
			return 1;
		string name = DisplayNameClean;
		string othername = item.DisplayNameClean;
		switch (Interface.modsMenu.sortMode) {
			default:
				return base.CompareTo(obj);
			case ModsMenuSortMode.RecentlyUpdated:
				return -1 * _mod.lastModified.CompareTo(item._mod.lastModified);
			case ModsMenuSortMode.DisplayNameAtoZ:
				return string.Compare(name, othername, StringComparison.Ordinal);
			case ModsMenuSortMode.DisplayNameZtoA:
				return -1 * string.Compare(name, othername, StringComparison.Ordinal);
		}
	}

	public bool PassFilters(UIModsFilterResults filterResults)
	{
		if (Interface.modsMenu.filter.Length > 0) {
			if (Interface.modsMenu.searchFilterMode == SearchFilter.Author) {
				if (_mod.properties.author.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1) {
					filterResults.filteredBySearch++;
					return false;
				}
			}
			else {
				if (DisplayNameClean.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1 && ModName.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1) {
					filterResults.filteredBySearch++;
					return false;
				}
			}
		}
		if (Interface.modsMenu.modSideFilterMode != ModSideFilter.All) {
			if ((int)_mod.properties.side != (int)Interface.modsMenu.modSideFilterMode - 1) {
				filterResults.filteredByModSide++;
				return false;
			}
		}
		switch (Interface.modsMenu.enabledFilterMode) {
			default:
			case EnabledFilter.All:
				return true;
			case EnabledFilter.EnabledOnly:
				if (!_mod.Enabled)
					filterResults.filteredByEnabled++;
				return _mod.Enabled;
			case EnabledFilter.DisabledOnly:
				if (_mod.Enabled)
					filterResults.filteredByEnabled++;
				return !_mod.Enabled;
		}
	}

	private void QuickModDelete(UIMouseEvent evt, UIElement listeningElement)
	{
		bool shiftPressed = Main.keyState.PressingShift();

		if (!shiftPressed) {
			SoundEngine.PlaySound(10, -1, -1, 1);
			var _deleteModDialog = new UIPanel() {
				Width = { Percent = .30f },
				Height = { Percent = .30f },
				HAlign = .5f,
				VAlign = .5f,
				BackgroundColor = new Color(63, 82, 151),
				BorderColor = Color.Black
			};
			_deleteModDialog.SetPadding(6f);
			Interface.modsMenu.ShowConfirmDialog(_deleteModDialog);

			_dialogYesButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("LegacyMenu.104")) {
				TextColor = Color.White,
				Width = new StyleDimension(-10f, 1f / 3f),
				Height = { Pixels = 40 },
				VAlign = .85f,
				HAlign = .15f
			}.WithFadedMouseOver();
			_dialogYesButton.OnLeftClick += DeleteMod;
			_deleteModDialog.Append(_dialogYesButton);

			_dialogNoButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("LegacyMenu.105")) {
				TextColor = Color.White,
				Width = new StyleDimension(-10f, 1f / 3f),
				Height = { Pixels = 40 },
				VAlign = .85f,
				HAlign = .85f
			}.WithFadedMouseOver();
			_dialogNoButton.OnLeftClick += Interface.modsMenu.CloseConfirmDialog;
			_deleteModDialog.Append(_dialogNoButton);

			_dialogText = new UIText(Language.GetTextValue("tModLoader.DeleteModConfirm")) {
				Width = { Percent = .75f },
				HAlign = .5f,
				VAlign = .3f,
				IsWrapped = true
			};
			_deleteModDialog.Append(_dialogText);

			Interface.modsMenu.Recalculate();
		}
		else {
			DeleteMod(evt, listeningElement);
		}
	}

	private void DeleteMod(UIMouseEvent evt, UIElement listeningElement)
	{
		ModOrganizer.DeleteMod(_mod);

		Interface.modsMenu.CloseConfirmDialog(evt, listeningElement);
		Interface.modsMenu.StoreCurrentScrollPosition();
		Interface.modsMenu.Activate();
	}

	private bool CheckIfPublishedForThisBrowserVersion(out string recommendedModBrowserVersion)
	{
		recommendedModBrowserVersion = SocialBrowserModule.GetBrowserVersionNumber(_mod.tModLoaderVersion);
		return recommendedModBrowserVersion == SocialBrowserModule.GetBrowserVersionNumber(BuildInfo.tMLVersion);
	}
}
