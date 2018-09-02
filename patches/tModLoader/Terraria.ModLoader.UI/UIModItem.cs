using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.UI;
using System.Linq;
using Terraria.Localization;
using System.Reflection;

namespace Terraria.ModLoader.UI
{
	internal class UIModItem : UIPanel
	{
		private readonly LocalMod mod;
		private readonly Texture2D dividerTexture;
		private int modIconAdjust;
		private readonly Texture2D innerPanelTexture;
		private readonly UIText modName;
		private readonly UITextPanel<string> button2;
		private UIImage modIcon;
		readonly UIHoverImage keyImage;
		private bool loaded;

		public UIModItem(LocalMod mod)
		{
			this.mod = mod;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.innerPanelTexture = TextureManager.Load("Images/UI/InnerPanelBackground");
			this.Height.Set(90f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);
			//base.OnClick += this.ToggleEnabled;
			string text = mod.DisplayName + " v" + mod.modFile.version;
			if (mod.modFile.tModLoaderVersion < new Version(0, 10))
			{
				text += $" [c/FF0000:({Language.GetTextValue("tModLoader.ModOldWarning")})]";
			}

			if (mod.modFile.HasFile("icon.png"))
			{
				try
				{
					var modIconTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, new MemoryStream(mod.modFile.GetFile("icon.png")));
					if (modIconTexture.Width == 80 && modIconTexture.Height == 80)
					{
						modIcon = new UIImage(modIconTexture);
						modIcon.Left.Set(0f, 0f);
						modIcon.Top.Set(0f, 0f);
						Append(modIcon);
						modIconAdjust += 85;
					}
				}
				catch { }
			}
			this.modName = new UIText(text, 1f, false);
			this.modName.Left.Set(modIconAdjust + 10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);
			UITextPanel<string> button = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModsMoreInfo"), 1f, false);
			button.Width.Set(100f, 0f);
			button.Height.Set(30f, 0f);
			button.Left.Set(430f, 0f);
			button.Top.Set(40f, 0f);
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			button.OnMouseOver += UICommon.FadedMouseOver;
			button.OnMouseOut += UICommon.FadedMouseOut;
			button.OnClick += this.Moreinfo;
			base.Append(button);
			button2 = new UITextPanel<string>(mod.Enabled ? Language.GetTextValue("tModLoader.ModsDisable") : Language.GetTextValue("tModLoader.ModsEnable"), 1f, false);
			button2.Width.Set(100f, 0f);
			button2.Height.Set(30f, 0f);
			button2.Left.Set(button.Left.Pixels - button2.Width.Pixels - 5f, 0f);
			button2.Top.Set(40f, 0f);
			button2.PaddingTop -= 2f;
			button2.PaddingBottom -= 2f;
			button2.OnMouseOver += UICommon.FadedMouseOver;
			button2.OnMouseOut += UICommon.FadedMouseOut;
			button2.OnClick += this.ToggleEnabled;
			base.Append(button2);

			var modRefs = mod.properties.modReferences.Select(x => x.mod).ToArray();
			if (modRefs.Length > 0 && !mod.Enabled)
			{
				string refs = String.Join(", ", mod.properties.modReferences);
				Texture2D icon = Texture2D.FromStream(Main.instance.GraphicsDevice,
					Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.ButtonExclamation.png"));
				UIHoverImage modReferenceIcon = new UIHoverImage(icon, Language.GetTextValue("tModLoader.ModDependencyClickTooltip", refs));
				modReferenceIcon.Left.Set(button2.Left.Pixels - 24f, 0f);
				modReferenceIcon.Top.Set(47f, 0f);
				modReferenceIcon.OnClick += (a, b) =>
				{
					var modList = ModLoader.FindMods();
					var missing = new List<string>();
					foreach (var modRef in modRefs)
					{
						ModLoader.EnableMod(modRef);
						if (!modList.Any(m => m.Name == modRef))
							missing.Add(modRef);
					}

					Main.menuMode = Interface.modsMenuID;
					if (missing.Any())
					{
						Interface.infoMessage.SetMessage(Language.GetTextValue("tModLoader.ModDependencyModsNotFound", String.Join(",", missing)));
						Interface.infoMessage.SetGotoMenu(Interface.modsMenuID);
						Main.menuMode = Interface.infoMessageID;
					}
				};
				base.Append(modReferenceIcon);
			}
			if (mod.modFile.ValidModBrowserSignature)
			{
				keyImage = new UIHoverImage(Main.itemTexture[ID.ItemID.GoldenKey], Language.GetTextValue("tModLoader.ModsOriginatedFromModBrowser"));
				keyImage.Left.Set(-20, 1f);
				base.Append(keyImage);
			}
			if (mod.properties.beta)
			{
				keyImage = new UIHoverImage(Main.itemTexture[ID.ItemID.ShadowKey], Language.GetTextValue("tModLoader.BetaModCantPublish"));
				keyImage.Left.Set(-10, 1f);
				Append(keyImage);
			}
			Mod loadedMod = ModLoader.GetMod(mod.Name);
			if (loadedMod != null)
			{
				loaded = true;
				int[] values = { loadedMod.items.Count, loadedMod.npcs.Count, loadedMod.tiles.Count, loadedMod.walls.Count, loadedMod.buffs.Count, loadedMod.mountDatas.Count };
				string[] localizationKeys = { "ModsXItems", "ModsXNPCs", "ModsXTiles", "ModsXWalls", "ModsXBuffs", "ModsXMounts" };
				int xOffset = -40;
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i] > 0)
					{
						Texture2D iconTexture = Main.instance.infoIconTexture[i];
						keyImage = new UIHoverImage(iconTexture, Language.GetTextValue($"tModLoader.{localizationKeys[i]}", values[i]));
						keyImage.Left.Set(xOffset, 1f);
						base.Append(keyImage);
						xOffset -= 18;
					}
				}
			}
		}

		// TODO: "Generate Language File Template" button in upcoming "Miscellaneous Tools" menu.
		private void GenerateLangTemplate_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Mod loadedMod = ModLoader.GetMod(mod.Name);
			Dictionary<string, ModTranslation> dictionary = (Dictionary<string, ModTranslation>)loadedMod.translations;
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

		private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width)
		{
			spriteBatch.Draw(this.innerPanelTexture, position, new Rectangle?(new Rectangle(0, 0, 8, this.innerPanelTexture.Height)), Color.White);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + 8f, position.Y), new Rectangle?(new Rectangle(8, 0, 8, this.innerPanelTexture.Height)), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + width - 8f, position.Y), new Rectangle?(new Rectangle(16, 0, 8, this.innerPanelTexture.Height)), Color.White);
		}

		private void DrawEnabledText(SpriteBatch spriteBatch, Vector2 drawPos)
		{
			string text = mod.Enabled ? Language.GetTextValue("GameUI.Enabled") : Language.GetTextValue("GameUI.Disabled");
			Color color = mod.Enabled ? Color.Green : Color.Red;
			Utils.DrawBorderString(spriteBatch, text, drawPos, color, 1f, 0f, 0f, -1);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f + modIconAdjust, innerDimensions.Y + 30f);
			spriteBatch.Draw(this.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - modIconAdjust) / 8f, 1f), SpriteEffects.None, 0f);
			drawPos = new Vector2(innerDimensions.X + 10f + modIconAdjust, innerDimensions.Y + 45f);
			this.DrawPanel(spriteBatch, drawPos, 85f);
			this.DrawEnabledText(spriteBatch, drawPos + new Vector2(10f, 5f));
			if (mod.Enabled != loaded)
			{
				drawPos += new Vector2(90f, 5f);
				Utils.DrawBorderString(spriteBatch, Language.GetTextValue("tModLoader.ModReloadRequired"), drawPos, Color.White, 1f, 0f, 0f, -1);
			}
			//string text = this.enabled ? "Click to Disable" : "Click to Enable";
			//drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 150f, innerDimensions.Y + 50f);
			//Utils.DrawBorderString(spriteBatch, text, drawPos, Color.White, 1f, 0f, 0f, -1);
		}

		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			base.DrawChildren(spriteBatch);

			// show authors on mod title hover, after everything else
			// main.hoverItemName isn't drawn in UI
			if (this.modName.IsMouseHovering && mod.properties.author.Length > 0)
			{
				string text = Language.GetTextValue("tModLoader.ModsByline", mod.properties.author);
				float x = Main.fontMouseText.MeasureString(text).X;
				Vector2 vector = Main.MouseScreen + new Vector2(16f);
				if (vector.Y > (float)(Main.screenHeight - 30))
				{
					vector.Y = (float)(Main.screenHeight - 30);
				}
				if (vector.X > (float)Main.screenWidth - x)
				{
					vector.X = (float)(Main.screenWidth - x - 30);
				}
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, text, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
			}
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			this.BackgroundColor = new Color(73, 94, 171);
			this.BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			base.MouseOut(evt);
			this.BackgroundColor = new Color(63, 82, 151) * 0.7f;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		internal void ToggleEnabled(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			mod.Enabled = !mod.Enabled;
			button2.SetText(mod.Enabled ? Language.GetTextValue("tModLoader.ModsDisable") : Language.GetTextValue("tModLoader.ModsEnable"), 1f, false);
		}

		internal void Enable()
		{
			if (!mod.Enabled)
				ToggleEnabled(null, null);
		}

		internal void Disable()
		{
			if (mod.Enabled)
				ToggleEnabled(null, null);
		}

		internal void Moreinfo(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Interface.modInfo.SetModName(mod.DisplayName);
			Interface.modInfo.SetModInfo(mod.properties.description);
			Interface.modInfo.SetMod(mod);
			Interface.modInfo.SetGotoMenu(Interface.modsMenuID);
			Interface.modInfo.SetURL(mod.properties.homepage);
			Main.menuMode = Interface.modInfoID;
		}

		public override int CompareTo(object obj)
		{
			var item = (UIModItem)obj;
			string name = mod.DisplayName;
			string othername = item.mod.DisplayName;
			switch (Interface.modsMenu.sortMode)
			{
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

		public override bool PassFilters()
		{
			if (Interface.modsMenu.filter.Length > 0)
			{
				if (Interface.modsMenu.searchFilterMode == SearchFilter.Author)
				{
					if (mod.properties.author.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1)
					{
						return false;
					}
				}
				else
				{
					if (mod.DisplayName.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1)
					{
						return false;
					}
				}
			}
			if (Interface.modsMenu.modSideFilterMode != ModSideFilter.All)
			{
				if ((int)mod.properties.side != (int)Interface.modsMenu.modSideFilterMode - 1)
					return false;
			}
			switch (Interface.modsMenu.enabledFilterMode)
			{
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
