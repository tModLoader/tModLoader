using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.ModLoader.IO;
using Terraria.UI;
using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader.UI
{
	internal class UIModItem : UIPanel
	{
		private readonly TmodFile mod;
		private readonly Texture2D dividerTexture;
		private readonly Texture2D innerPanelTexture;
		private readonly UIText modName;
		internal bool enabled;
		private readonly BuildProperties properties;
		private readonly UITextPanel<string> button2;
		private UIImage modIcon;
		readonly UIHoverImage keyImage;

		public UIModItem(TmodFile mod)
		{
			this.mod = mod;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.innerPanelTexture = TextureManager.Load("Images/UI/InnerPanelBackground");
			this.Height.Set(90f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);
			//base.OnClick += this.ToggleEnabled;
			properties = BuildProperties.ReadModFile(mod);
			string text = properties.displayName.Length > 0 ? properties.displayName : mod.name;
			text += " v" + mod.version;
			if (mod.tModLoaderVersion < new Version(0, 10))
			{
				text += " [c/FF0000:(Old mod, enable at own risk)]";
			}
			int modIconAdjust = 0;
			if (mod.HasFile("icon.png"))
			{
				var modIconTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, new MemoryStream(mod.GetFile("icon.png")));
				if (modIconTexture.Width == 80 && modIconTexture.Height == 80)
				{
					modIcon = new UIImage(modIconTexture);
					modIcon.Left.Set(0f, 0f);
					modIcon.Top.Set(0f, 0f);
					Append(modIcon);
					modIconAdjust += 85;
				}
			}
			this.modName = new UIText(text, 1f, false);
			this.modName.Left.Set(modIconAdjust + 10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);
			this.enabled = ModLoader.IsEnabled(mod);
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
			button2 = new UITextPanel<string>(this.enabled ? Language.GetTextValue("tModLoader.ModsDisable") : Language.GetTextValue("tModLoader.ModsEnable"), 1f, false);
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
			if (properties.modReferences.Length > 0 && !enabled)
			{
				string refs = String.Join(", ", properties.modReferences.Select(x => x.mod));
				UIHoverImage modReferenceIcon = new UIHoverImage(Main.quicksIconTexture, "This mod depends on: " + refs + "\n (click to enable)");
				modReferenceIcon.Left.Set(button2.Left.Pixels - 10f, 0f);
				modReferenceIcon.Top.Set(50f, 0f);
				modReferenceIcon.OnClick += (a, b) =>
				{
					var referencedMods = properties.modReferences.Select(x => x.mod);
					var foundMods = ModLoader.FindMods();
					var referencedtModFiles = foundMods.Where(x => referencedMods.Contains(x.name));
					foreach (var referencedMod in referencedtModFiles)
					{
						ModLoader.EnableMod(referencedMod);
					}
					Main.menuMode = Interface.modsMenuID;
					var missingMods = referencedMods.Where(modstring => foundMods.All(modfile => modfile.name != modstring));
					if (missingMods.Count() > 0)
					{
						Interface.infoMessage.SetMessage("The following mods were not found: " + String.Join(",", missingMods));
						Interface.infoMessage.SetGotoMenu(Interface.modsMenuID);
						Main.menuMode = Interface.infoMessageID;
					}
				};
				base.Append(modReferenceIcon);
			}
			if (mod.ValidModBrowserSignature)
			{
				keyImage = new UIHoverImage(Main.itemTexture[ID.ItemID.GoldenKey], Language.GetTextValue("tModLoader.ModsOriginatedFromModBrowser"));
				keyImage.Left.Set(-20, 1f);
				base.Append(keyImage);
			}
			if (ModLoader.ModLoaded(mod.name))
			{
				Mod loadedMod = ModLoader.GetMod(mod.name);
				int[] values = { loadedMod.items.Count, loadedMod.npcs.Count, loadedMod.tiles.Count, loadedMod.walls.Count, loadedMod.buffs.Count, loadedMod.mountDatas.Count };
				string[] strings = { " items", " NPCs", " tiles", " walls", " buffs", " mounts" };
				int xOffset = -40;
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i] > 0)
					{
						Texture2D iconTexture = Main.instance.infoIconTexture[i];
						keyImage = new UIHoverImage(iconTexture, values[i] + strings[i]);
						keyImage.Left.Set(xOffset, 1f);
						base.Append(keyImage);
						xOffset -= 18;
					}
				}
			}
		}

		private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width)
		{
			spriteBatch.Draw(this.innerPanelTexture, position, new Rectangle?(new Rectangle(0, 0, 8, this.innerPanelTexture.Height)), Color.White);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + 8f, position.Y), new Rectangle?(new Rectangle(8, 0, 8, this.innerPanelTexture.Height)), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + width - 8f, position.Y), new Rectangle?(new Rectangle(16, 0, 8, this.innerPanelTexture.Height)), Color.White);
		}

		private void DrawEnabledText(SpriteBatch spriteBatch, Vector2 drawPos)
		{
			string text = this.enabled ? "Enabled" : "Disabled";
			Color color = this.enabled ? Color.Green : Color.Red;
			Utils.DrawBorderString(spriteBatch, text, drawPos, color, 1f, 0f, 0f, -1);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			int modIconAdjust = modIcon != null ? 85 : 0;
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f + modIconAdjust, innerDimensions.Y + 30f);
			spriteBatch.Draw(this.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - modIconAdjust) / 8f, 1f), SpriteEffects.None, 0f);
			drawPos = new Vector2(innerDimensions.X + 10f + modIconAdjust, innerDimensions.Y + 45f);
			this.DrawPanel(spriteBatch, drawPos, 85f);
			this.DrawEnabledText(spriteBatch, drawPos + new Vector2(10f, 5f));
			if (this.enabled != ModLoader.ModLoaded(mod.name))
			{
				drawPos += new Vector2(90f, 5f);
				Utils.DrawBorderString(spriteBatch, "Reload Required", drawPos, Color.White, 1f, 0f, 0f, -1);
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
			if (this.modName.IsMouseHovering && properties.author.Length > 0)
			{
				string text = "By: " + properties.author;
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
			this.enabled = !this.enabled;
			button2.SetText(this.enabled ? Language.GetTextValue("tModLoader.ModsDisable") : Language.GetTextValue("tModLoader.ModsEnable"), 1f, false);
			ModLoader.SetModActive(this.mod, this.enabled);
		}

		internal void Moreinfo(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Interface.modInfo.SetModName(properties.displayName);
			Interface.modInfo.SetModInfo(properties.description);
			Interface.modInfo.SetMod(mod);
			Interface.modInfo.SetGotoMenu(Interface.modsMenuID);
			Interface.modInfo.SetURL(properties.homepage);
			Main.menuMode = Interface.modInfoID;
		}

		public override int CompareTo(object obj)
		{
			var item = obj as UIModItem;
			string name = properties.displayName.Length > 0 ? properties.displayName : mod.name;
			string othername = item.properties.displayName.Length > 0 ? item.properties.displayName : item.mod.name;
			switch (Interface.modsMenu.sortMode)
			{
				default:
					return base.CompareTo(obj);
				case ModsMenuSortMode.RecentlyUpdated:
					return -1 * mod.lastModifiedTime.CompareTo(item?.mod.lastModifiedTime);
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
				string name = properties.displayName.Length > 0 ? properties.displayName : mod.name;
				if (Interface.modsMenu.searchFilterMode == SearchFilter.Author)
				{
					if (properties.author.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1)
					{
						return false;
					}
				}
				else
				{
					if (name.IndexOf(Interface.modsMenu.filter, StringComparison.OrdinalIgnoreCase) == -1)
					{
						return false;
					}
				}
			}
			switch (Interface.modsMenu.enabledFilterMode)
			{
				default:
				case EnabledFilter.All:
					return true;
				case EnabledFilter.EnabledOnly:
					return enabled;
				case EnabledFilter.DisabledOnly:
					return !enabled;
			}
		}
	}
}
