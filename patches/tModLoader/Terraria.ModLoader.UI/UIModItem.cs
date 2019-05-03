using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;

namespace Terraria.ModLoader.UI
{
	//TODO common code with UIModDownloadItem etc
	internal class UIModItem : UIPanel
	{
		private readonly LocalMod mod;
		private readonly Texture2D dividerTexture;
		private int modIconAdjust;
		private readonly Texture2D innerPanelTexture;
		private readonly UIText modName;
		private readonly UIAutoScaleTextTextPanel<string> toggleModEnabledButton;
		private UIImage modIcon;
		readonly UIHoverImage keyImage;
		private readonly UITextPanel<string> configButton;
		private bool configChangesRequireReload;
		private bool loaded;

		public UIModItem(LocalMod mod) {
			this.mod = mod;
			BorderColor = new Color(89, 116, 213) * 0.7f;
			dividerTexture = TextureManager.Load("Images/UI/Divider");
			innerPanelTexture = TextureManager.Load("Images/UI/InnerPanelBackground");
			Height.Pixels = 90;
			Width.Percent = 1f;
			SetPadding(6f);
			//base.OnClick += this.ToggleEnabled;
			string text = mod.DisplayName + " v" + mod.modFile.version;
			if (mod.tModLoaderVersion < new Version(0, 10)) {
				text += $" [c/FF0000:({Language.GetTextValue("tModLoader.ModOldWarning")})]";
			}

			if (mod.modFile.HasFile("icon.png")) {
				try {
					Texture2D modIconTexture;
					using (mod.modFile.Open())
					using (var s = mod.modFile.GetStream("icon.png"))
						modIconTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, s);

					if (modIconTexture.Width == 80 && modIconTexture.Height == 80) {
						modIcon = new UIImage(modIconTexture) {
							Left = { Percent = 0f },
							Top = { Percent = 0f }
						};
						Append(modIcon);
						modIconAdjust += 85;
					}
				}
				catch { }
			}
			modName = new UIText(text) {
				Left = new StyleDimension(modIconAdjust + 10f, 0f),
				Top = { Pixels = 5 }
			};
			Append(modName);

			var moreInfoButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModsMoreInfo")) {
				Width = { Pixels = 100 },
				Height = { Pixels = 36 },
				Left = { Pixels = 430 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			moreInfoButton.PaddingTop -= 2f;
			moreInfoButton.PaddingBottom -= 2f;
			moreInfoButton.OnClick += Moreinfo;
			Append(moreInfoButton);

			toggleModEnabledButton = new UIAutoScaleTextTextPanel<string>(mod.Enabled ? Language.GetTextValue("tModLoader.ModsDisable") : Language.GetTextValue("tModLoader.ModsEnable")) {
				Width = { Pixels = 100 },
				Height = { Pixels = 36 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			toggleModEnabledButton.Left.Pixels = moreInfoButton.Left.Pixels - toggleModEnabledButton.Width.Pixels - 5f;
			toggleModEnabledButton.PaddingTop -= 2f;
			toggleModEnabledButton.PaddingBottom -= 2f;
			toggleModEnabledButton.OnClick += ToggleEnabled;
			Append(toggleModEnabledButton);
			
			Mod loadedMod = ModLoader.GetMod(mod.Name);
			if (loadedMod != null && ConfigManager.Configs.ContainsKey(loadedMod)) // and has config
			{
				configButton = new UITextPanel<string>("Config", 1f, false);
				configButton.Width.Set(100f, 0f);
				configButton.Height.Set(30f, 0f);
				configButton.Left.Set(toggleModEnabledButton.Left.Pixels - configButton.Width.Pixels - 5f, 0f);
				configButton.Top.Set(40f, 0f);
				configButton.PaddingTop -= 2f;
				configButton.PaddingBottom -= 2f;
				configButton.WithFadedMouseOver();
				configButton.OnClick += this.OpenConfig;
				Append(configButton);
				if (ConfigManager.ModNeedsReload(loadedMod))
				{
					configChangesRequireReload = true;
				}
			}

			var modRefs = mod.properties.modReferences.Select(x => x.mod).ToArray();
			if (modRefs.Length > 0 && !mod.Enabled) {
				string refs = string.Join(", ", mod.properties.modReferences);
				var icon = Texture2D.FromStream(Main.instance.GraphicsDevice,
					Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.ButtonExclamation.png"));
				var modReferenceIcon = new UIHoverImage(icon, Language.GetTextValue("tModLoader.ModDependencyClickTooltip", refs)) {
					Left = new StyleDimension(toggleModEnabledButton.Left.Pixels - 24f, 0f),
					Top = { Pixels = 47 }
				};
				modReferenceIcon.OnClick += (a, b) => {
					var modList = ModOrganizer.FindMods();
					var missing = new List<string>();
					foreach (var modRef in modRefs) {
						ModLoader.EnableMod(modRef);
						if (!modList.Any(m => m.Name == modRef))
							missing.Add(modRef);
					}

					Main.menuMode = Interface.modsMenuID;
					if (missing.Any()) {
						Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModDependencyModsNotFound", String.Join(",", missing)), Interface.modsMenuID);
					}
				};
				Append(modReferenceIcon);
			}
			if (mod.modFile.ValidModBrowserSignature) {
				keyImage = new UIHoverImage(Main.itemTexture[ID.ItemID.GoldenKey], Language.GetTextValue("tModLoader.ModsOriginatedFromModBrowser")) {
					Left = { Pixels = -20, Percent = 1f }
				};
				Append(keyImage);
			}
			if (ModLoader.badUnloaders.Contains(mod.Name)) {
				keyImage = new UIHoverImage(Texture2D.FromStream(Main.instance.GraphicsDevice,
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.ButtonError.png")), "This mod did not fully unload during last unload.") {
					Left = { Pixels = modIconAdjust + 4 },
					Top = { Pixels = 3 }
				};
				Append(keyImage);
				modName.Left.Pixels += 20;
			}
			if (mod.properties.beta) {
				keyImage = new UIHoverImage(Main.itemTexture[ID.ItemID.ShadowKey], Language.GetTextValue("tModLoader.BetaModCantPublish")) {
					Left = { Pixels = -10, Percent = 1f }
				};
				Append(keyImage);
			}
			if (loadedMod != null) {
				loaded = true;
				int[] values = { loadedMod.items.Count, loadedMod.npcs.Count, loadedMod.tiles.Count, loadedMod.walls.Count, loadedMod.buffs.Count, loadedMod.mountDatas.Count };
				string[] localizationKeys = { "ModsXItems", "ModsXNPCs", "ModsXTiles", "ModsXWalls", "ModsXBuffs", "ModsXMounts" };
				int xOffset = -40;
				for (int i = 0; i < values.Length; i++) {
					if (values[i] > 0) {
						Texture2D iconTexture = Main.instance.infoIconTexture[i];
						keyImage = new UIHoverImage(iconTexture, Language.GetTextValue($"tModLoader.{localizationKeys[i]}", values[i])) {
							Left = { Pixels = xOffset, Percent = 1f }
						};
						Append(keyImage);
						xOffset -= 18;
					}
				}
			}
		}

		// TODO: "Generate Language File Template" button in upcoming "Miscellaneous Tools" menu.
		private void GenerateLangTemplate_OnClick(UIMouseEvent evt, UIElement listeningElement) {
			Mod loadedMod = ModLoader.GetMod(mod.Name);
			var dictionary = (Dictionary<string, ModTranslation>)loadedMod.translations;
			var result = loadedMod.items.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "=")
				.Concat(loadedMod.items.Where(x => !dictionary.ContainsValue(x.Value.Tooltip)).Select(x => x.Value.Tooltip.Key + "="))
				.Concat(loadedMod.npcs.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "="))
				.Concat(loadedMod.buffs.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "="))
				.Concat(loadedMod.buffs.Where(x => !dictionary.ContainsValue(x.Value.Description)).Select(x => x.Value.Description.Key + "="))
				.Concat(loadedMod.projectiles.Where(x => !dictionary.ContainsValue(x.Value.DisplayName)).Select(x => x.Value.DisplayName.Key + "="));
			//.Concat(loadedMod.tiles.Where(x => !dictionary.ContainsValue(x.Value.)).Select(x => x.Value..Key + "="))
			//.Concat(loadedMod.walls.Where(x => !dictionary.ContainsValue(x.Value.)).Select(x => x.Value..Key + "="));
			int index = $"Mods.{mod.Name}.".Length;
			result = result.Select(x => x.Remove(0, index));
			ReLogic.OS.Platform.Current.Clipboard = string.Join("\n", result);
		}

		private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width) {
			spriteBatch.Draw(innerPanelTexture, position, new Rectangle?(new Rectangle(0, 0, 8, innerPanelTexture.Height)), Color.White);
			spriteBatch.Draw(innerPanelTexture, new Vector2(position.X + 8f, position.Y), new Rectangle?(new Rectangle(8, 0, 8, innerPanelTexture.Height)), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(innerPanelTexture, new Vector2(position.X + width - 8f, position.Y), new Rectangle?(new Rectangle(16, 0, 8, innerPanelTexture.Height)), Color.White);
		}

		private void DrawEnabledText(SpriteBatch spriteBatch, Vector2 drawPos) {
			string text = mod.Enabled ? Language.GetTextValue("GameUI.Enabled") : Language.GetTextValue("GameUI.Disabled");
			Color color = mod.Enabled ? Color.Green : Color.Red;
			Utils.DrawBorderString(spriteBatch, text, drawPos, color, 1f, 0f, 0f, -1);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = GetInnerDimensions();
			var drawPos = new Vector2(innerDimensions.X + 5f + modIconAdjust, innerDimensions.Y + 30f);
			spriteBatch.Draw(dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - modIconAdjust) / 8f, 1f), SpriteEffects.None, 0f);
			drawPos = new Vector2(innerDimensions.X + 10f + modIconAdjust, innerDimensions.Y + 45f);
			DrawPanel(spriteBatch, drawPos, 85f);
			DrawEnabledText(spriteBatch, drawPos + new Vector2(10f, 5f));
			if (mod.Enabled != loaded || configChangesRequireReload) {
				drawPos += new Vector2(90f, 5f);
				Utils.DrawBorderString(spriteBatch, configChangesRequireReload ? Language.GetTextValue("tModLoader.ModReloadForced") : Language.GetTextValue("tModLoader.ModReloadRequired"), drawPos, Color.White, 1f, 0f, 0f, -1);
			}
			//string text = this.enabled ? "Click to Disable" : "Click to Enable";
			//drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 150f, innerDimensions.Y + 50f);
			//Utils.DrawBorderString(spriteBatch, text, drawPos, Color.White, 1f, 0f, 0f, -1);
		}

		protected override void DrawChildren(SpriteBatch spriteBatch) {
			base.DrawChildren(spriteBatch);

			// show authors on mod title hover, after everything else
			// main.hoverItemName isn't drawn in UI
			if (this.modName.IsMouseHovering && mod.properties.author.Length > 0) {
				string text = Language.GetTextValue("tModLoader.ModsByline", mod.properties.author);
				UICommon.DrawHoverStringInBounds(spriteBatch, text);
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

		internal void ToggleEnabled(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(12, -1, -1, 1);
			mod.Enabled = !mod.Enabled;
			toggleModEnabledButton.SetText(mod.Enabled ? Language.GetTextValue("tModLoader.ModsDisable") : Language.GetTextValue("tModLoader.ModsEnable"));
		}

		internal void Enable() {
			if (!mod.Enabled)
				ToggleEnabled(null, null);
		}

		internal void Disable() {
			if (mod.Enabled)
				ToggleEnabled(null, null);
		}

		internal void Moreinfo(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Interface.modInfo.SetModName(mod.DisplayName);
			Interface.modInfo.SetModInfo(mod.properties.description);
			Interface.modInfo.SetMod(mod);
			Interface.modInfo.SetGotoMenu(Interface.modsMenuID);
			Interface.modInfo.SetUrl(mod.properties.homepage);
			Main.menuMode = Interface.modInfoID;
		}

		internal void OpenConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuOpen);
			Interface.modConfig.SetMod(ModLoader.GetMod(mod.Name));
			Main.menuMode = Interface.modConfigID;
		}

		public override int CompareTo(object obj) {
			var item = (UIModItem)obj;
			string name = mod.DisplayName;
			string othername = item.mod.DisplayName;
			switch (Interface.modsMenu.sortMode) {
				default:
					return base.CompareTo(obj);
				case ModsMenuSortMode.RecentlyUpdated:
					return -1 * mod.lastModified.CompareTo(item.mod.lastModified);
				case ModsMenuSortMode.DisplayNameAtoZ:
					return string.Compare(name, othername, StringComparison.Ordinal);
				case ModsMenuSortMode.DisplayNameZtoA:
					return -1 * string.Compare(name, othername, StringComparison.Ordinal);
			}
		}

		public bool PassFilters() {
			if (Interface.modsMenu.filter.Length > 0) {
				if (Interface.modsMenu.searchFilterMode == SearchFilter.Author) {
					if (mod.properties.author.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1) {
						return false;
					}
				}
				else {
					if (mod.DisplayName.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1 && mod.Name.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1) {
						return false;
					}
				}
			}
			if (Interface.modsMenu.modSideFilterMode != ModSideFilter.All) {
				if ((int)mod.properties.side != (int)Interface.modsMenu.modSideFilterMode - 1)
					return false;
			}
			switch (Interface.modsMenu.enabledFilterMode) {
				default:
				case EnabledFilter.All:
					return true;
				case EnabledFilter.EnabledOnly:
					return mod.Enabled;
				case EnabledFilter.DisabledOnly:
					return !mod.Enabled;
			}
		}
	}
}
