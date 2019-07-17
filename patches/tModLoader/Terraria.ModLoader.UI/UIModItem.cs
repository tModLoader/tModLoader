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

namespace Terraria.ModLoader.UI
{
	internal class UIModItem : UIPanel
	{
		private const float PADDING = 5f;

		private UIImage _moreInfoButton;
		private UIImage _modIcon;
		private UIHoverImage _keyImage;
		private UIImage _configButton;
		private UIText _modName;
		private ModStateText _modStateText;
		private UIHoverImage _modReferenceIcon;
		private readonly LocalMod _mod;

		private bool _configChangesRequireReload;
		private bool _loaded;
		private int _modIconAdjust;
		private string _tooltip;
		private string[] _modReferences;

		private string ToggleModStateText => _mod.Enabled ? Language.GetTextValue("tModLoader.ModsDisable") : Language.GetTextValue("tModLoader.ModsEnable");

		public UIModItem(LocalMod mod) {
			_mod = mod;
			BorderColor = new Color(89, 116, 213) * 0.7f;
			Height.Pixels = 90;
			Width.Percent = 1f;
			SetPadding(6f);
		}

		public override void OnInitialize() {
			base.OnInitialize();

			string text = _mod.DisplayName + " v" + _mod.modFile.version;
			if (_mod.tModLoaderVersion < new Version(0, 10)) {
				text += $" [c/FF0000:({Language.GetTextValue("tModLoader.ModOldWarning")})]";
			}

			if (_mod.modFile.HasFile("icon.png")) {
				try {
					Texture2D modIconTexture;
					using (_mod.modFile.Open())
					using (var s = _mod.modFile.GetStream("icon.png"))
						modIconTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, s);

					if (modIconTexture.Width == 80 && modIconTexture.Height == 80) {
						_modIcon = new UIImage(modIconTexture) {
							Left = { Percent = 0f },
							Top = { Percent = 0f }
						};
						Append(_modIcon);
						_modIconAdjust += 85;
					}
				}
				catch (Exception e) {
					Logging.tML.Error("Unknown error", e);
				}
			}

			_modName = new UIText(text) {
				Left = new StyleDimension(_modIconAdjust, 0f),
				Top = { Pixels = 5 }
			};
			Append(_modName);

			_modStateText = new ModStateText(_mod.Enabled) {
				Top = { Pixels = 40 },
				Left = { Pixels = _modIconAdjust }
			};
			_modStateText.OnClick += ToggleEnabled;
			Append(_modStateText);

			_moreInfoButton = new UIImage(UICommon.buttonModInfoTexture) {
				Width = { Pixels = 36 },
				Height = { Pixels = 36 },
				Left = { Pixels = -36, Precent = 1 },
				Top = { Pixels = 40 }
			};
			_moreInfoButton.OnClick += ShowMoreInfo;
			Append(_moreInfoButton);

			Mod loadedMod = ModLoader.GetMod(_mod.Name);
			if (loadedMod != null && ConfigManager.Configs.ContainsKey(loadedMod)) {
				_configButton = new UIImage(UICommon.buttonModConfigTexture) {
					Width = { Pixels = 36 },
					Height = { Pixels = 36f },
					Left = { Pixels = _moreInfoButton.Left.Pixels - 36 - PADDING, Precent = 1f },
					Top = { Pixels = 40f }
				};
				_configButton.OnClick += OpenConfig;
				Append(_configButton);
				if (ConfigManager.ModNeedsReload(loadedMod)) {
					_configChangesRequireReload = true;
				}
			}

			_modReferences = _mod.properties.modReferences.Select(x => x.mod).ToArray();
			if (_modReferences.Length > 0 && !_mod.Enabled) {
				string refs = string.Join(", ", _mod.properties.modReferences);
				var icon = UICommon.buttonExclamationTexture;
				_modReferenceIcon = new UIHoverImage(icon, Language.GetTextValue("tModLoader.ModDependencyClickTooltip", refs)) {
					Left = new StyleDimension(_modStateText.Left.Pixels + _modStateText.Width.Pixels + PADDING, 0f),
					Top = { Pixels = 42.5f }
				};
				_modReferenceIcon.OnClick += EnableDependencies;
				Append(_modReferenceIcon);
			}
			if (_mod.modFile.ValidModBrowserSignature) {
				_keyImage = new UIHoverImage(Main.itemTexture[ItemID.GoldenKey], Language.GetTextValue("tModLoader.ModsOriginatedFromModBrowser")) {
					Left = { Pixels = -20, Percent = 1f }
				};
				Append(_keyImage);
			}
			if (ModLoader.badUnloaders.Contains(_mod.Name)) {
				_keyImage = new UIHoverImage(UICommon.buttonErrorTexture, "This mod did not fully unload during last unload.") {
					Left = { Pixels = _modIconAdjust + PADDING },
					Top = { Pixels = 3 }
				};
				Append(_keyImage);
				_modName.Left.Pixels += _keyImage.Width.Pixels + PADDING * 2f;
			}
			if (_mod.properties.beta) {
				_keyImage = new UIHoverImage(Main.itemTexture[ItemID.ShadowKey], Language.GetTextValue("tModLoader.BetaModCantPublish")) {
					Left = { Pixels = -10, Percent = 1f }
				};
				Append(_keyImage);
			}

			if (loadedMod != null) {
				_loaded = true;
				int[] values = { loadedMod.items.Count, loadedMod.npcs.Count, loadedMod.tiles.Count, loadedMod.walls.Count, loadedMod.buffs.Count, loadedMod.mountDatas.Count };
				string[] localizationKeys = { "ModsXItems", "ModsXNPCs", "ModsXTiles", "ModsXWalls", "ModsXBuffs", "ModsXMounts" };
				int xOffset = -40;
				for (int i = 0; i < values.Length; i++) {
					if (values[i] > 0) {
						Texture2D iconTexture = Main.instance.infoIconTexture[i];
						_keyImage = new UIHoverImage(iconTexture, Language.GetTextValue($"tModLoader.{localizationKeys[i]}", values[i])) {
							Left = { Pixels = xOffset, Percent = 1f }
						};
						Append(_keyImage);
						xOffset -= 18;
					}
				}
			}

			OnDoubleClick += (e, el) => {
				_modStateText.Click(e);
			};
		}

		// TODO: "Generate Language File Template" button in upcoming "Miscellaneous Tools" menu.
		private void GenerateLangTemplate_OnClick(UIMouseEvent evt, UIElement listeningElement) {
			Mod loadedMod = ModLoader.GetMod(_mod.Name);
			var dictionary = (Dictionary<string, ModTranslation>)loadedMod.translations;
			var result = loadedMod.items.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "=")
				.Concat(loadedMod.items.Where(x => !dictionary.ContainsValue(x.Value.Tooltip)).Select(x => x.Value.Tooltip.Key + "="))
				.Concat(loadedMod.npcs.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "="))
				.Concat(loadedMod.buffs.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "="))
				.Concat(loadedMod.buffs.Where(x => !dictionary.ContainsValue(x.Value.Description)).Select(x => x.Value.Description.Key + "="))
				.Concat(loadedMod.projectiles.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "="));
			//.Concat(loadedMod.tiles.Where(x => !dictionary.ContainsValue(x.Value.)).Select(x => x.Value..Key + "="))
			//.Concat(loadedMod.walls.Where(x => !dictionary.ContainsValue(x.Value.)).Select(x => x.Value..Key + "="));
			int index = $"Mods.{_mod.Name}.".Length;
			result = result.Select(x => x.Remove(0, index));
			ReLogic.OS.Platform.Current.Clipboard = string.Join("\n", result);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			_tooltip = null;
			base.Draw(spriteBatch);
			if (!string.IsNullOrEmpty(_tooltip)) {
				var bounds = GetOuterDimensions().ToRectangle();
				bounds.Height += 16;
				UICommon.DrawHoverStringInBounds(spriteBatch, _tooltip, bounds);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = GetInnerDimensions();
			var drawPos = new Vector2(innerDimensions.X + 5f + _modIconAdjust, innerDimensions.Y + 30f);
			spriteBatch.Draw(UICommon.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - _modIconAdjust) / 8f, 1f), SpriteEffects.None, 0f);
			drawPos = new Vector2(innerDimensions.X + 10f + _modIconAdjust, innerDimensions.Y + 45f);

			// TODO: These should just be UITexts
			if (_mod.properties.side != ModSide.Server && (_mod.Enabled != _loaded || _configChangesRequireReload)) {
				if (_modReferenceIcon != null) {
					drawPos += new Vector2(_modStateText.Width.Pixels + _modReferenceIcon.Width.Pixels + PADDING, 0f);
				}
				else {
					drawPos += new Vector2(_modStateText.Width.Pixels, 0f);
				}
				Utils.DrawBorderString(spriteBatch, _configChangesRequireReload ? Language.GetTextValue("tModLoader.ModReloadForced") : Language.GetTextValue("tModLoader.ModReloadRequired"), drawPos, Color.White, 1f, 0f, 0f, -1);
			}
			if (_mod.properties.side == ModSide.Server) {
				drawPos += new Vector2(90f, -2f);
				spriteBatch.Draw(UICommon.modBrowserIconsTexture, drawPos, new Rectangle(5 * 34, 3 * 34, 32, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				if (new Rectangle((int)drawPos.X, (int)drawPos.Y, 32, 32).Contains(Main.MouseScreen.ToPoint()))
					UICommon.DrawHoverStringInBounds(spriteBatch, "This is a server side mod");
			}

			if (_moreInfoButton?.IsMouseHovering == true) {
				_tooltip = Language.GetTextValue("tModLoader.ModsMoreInfo");
			}
			else if (_modName?.IsMouseHovering == true && _mod?.properties.author.Length > 0) {
				_tooltip = Language.GetTextValue("tModLoader.ModsByline", _mod.properties.author);
			}
			else if (_modStateText?.IsMouseHovering == true) {
				_tooltip = ToggleModStateText;
			} else if (_configButton?.IsMouseHovering == true) {
				_tooltip = Language.GetTextValue("tModLoader.ModsOpenConfig");
			}
		}

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			BackgroundColor = UICommon.defaultUIBlue;
			BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt) {
			base.MouseOut(evt);
			BackgroundColor = new Color(63, 82, 151) * 0.7f;
			BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		private void ToggleEnabled(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(12);
			_mod.Enabled = !_mod.Enabled;
		}

		internal void Enable() {
			Main.PlaySound(12);
			_mod.Enabled = true;
			_modStateText.SetEnabled();
		}

		internal void Disable() {
			Main.PlaySound(12);
			_mod.Enabled = false;
			_modStateText.SetDisabled();
		}

		internal void EnableDependencies(UIMouseEvent evt, UIElement listeningElement) {
			var modList = ModOrganizer.FindMods();
			var missing = new List<string>();
			foreach (var modRef in _modReferences) {
				ModLoader.EnableMod(modRef);
				if (modList.All(m => m.Name != modRef))
					missing.Add(modRef);
			}

			Main.menuMode = Interface.modsMenuID;
			if (missing.Any()) {
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModDependencyModsNotFound", string.Join(",", missing)), Interface.modsMenuID);
			}
		}

		internal void ShowMoreInfo(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Interface.modInfo.Show(_mod.Name, _mod.DisplayName, Interface.modsMenuID, _mod, _mod.properties.description, _mod.properties.homepage);
		}

		internal void OpenConfig(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			Interface.modConfig.SetMod(ModLoader.GetMod(_mod.Name));
			Main.menuMode = Interface.modConfigID;
		}

		public override int CompareTo(object obj) {
			var item = (UIModItem)obj;
			string name = _mod.DisplayName;
			string othername = item._mod.DisplayName;
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

		public bool PassFilters() {
			if (Interface.modsMenu.filter.Length > 0) {
				if (Interface.modsMenu.searchFilterMode == SearchFilter.Author) {
					if (_mod.properties.author.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1) {
						return false;
					}
				}
				else {
					if (_mod.DisplayName.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1 && _mod.Name.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1) {
						return false;
					}
				}
			}
			if (Interface.modsMenu.modSideFilterMode != ModSideFilter.All) {
				if ((int)_mod.properties.side != (int)Interface.modsMenu.modSideFilterMode - 1)
					return false;
			}
			switch (Interface.modsMenu.enabledFilterMode) {
				default:
				case EnabledFilter.All:
					return true;
				case EnabledFilter.EnabledOnly:
					return _mod.Enabled;
				case EnabledFilter.DisabledOnly:
					return !_mod.Enabled;
			}
		}
	}

	internal class ModStateText : UIElement
	{
		private bool _enabled;

		private string DisplayText
			=> _enabled
			? Language.GetTextValue("GameUI.Enabled")
			: Language.GetTextValue("GameUI.Disabled");

		private Color DisplayColor
			=> _enabled ? Color.Green : Color.Red;

		public ModStateText(bool enabled = true) {
			_enabled = enabled;
			PaddingLeft = PaddingRight = 5f;
			PaddingBottom = PaddingTop = 10f;
		}

		public override void OnInitialize() {
			OnClick += (evt, el) => {
				if (_enabled) SetDisabled();
				else SetEnabled();
			};
		}

		public void SetEnabled() {
			_enabled = true;
			Recalculate();
		}

		public void SetDisabled() {
			_enabled = false;
			Recalculate();
		}

		public override void Recalculate() {
			var textSize = new Vector2(Main.fontMouseText.MeasureString(DisplayText).X, 16f);
			Width.Set(textSize.X + PaddingLeft + PaddingRight, 0f);
			Height.Set(textSize.Y + PaddingTop + PaddingBottom, 0f);
			base.Recalculate();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			DrawPanel(spriteBatch);
			DrawEnabledText(spriteBatch);
		}

		private void DrawPanel(SpriteBatch spriteBatch) {
			var position = GetDimensions().Position();
			var width = Width.Pixels;
			spriteBatch.Draw(UICommon.innerPanelTexture, position, new Rectangle(0, 0, 8, UICommon.innerPanelTexture.Height), Color.White);
			spriteBatch.Draw(UICommon.innerPanelTexture, new Vector2(position.X + 8f, position.Y), new Rectangle(8, 0, 8, UICommon.innerPanelTexture.Height), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(UICommon.innerPanelTexture, new Vector2(position.X + width - 8f, position.Y), new Rectangle(16, 0, 8, UICommon.innerPanelTexture.Height), Color.White);
		}

		private void DrawEnabledText(SpriteBatch spriteBatch) {
			var pos = GetDimensions().Position() + new Vector2(PaddingLeft, PaddingTop * 0.5f);
			Utils.DrawBorderString(spriteBatch, DisplayText, pos, DisplayColor);
		}
	}
}
