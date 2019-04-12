using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;

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

		private readonly Texture2D dividerTexture;
		private readonly Texture2D innerPanelTexture;
		private readonly UIText modName;
		private readonly string[] mods;
		private readonly bool[] modMissing;
		private readonly int numMods;
		private readonly int numModsEnabled;
		private readonly int numModsDisabled;
		private readonly int numModsMissing;
		readonly UIAutoScaleTextTextPanel<string> enableListButton;
		readonly UIAutoScaleTextTextPanel<string> enableListOnlyButton;
		readonly UIAutoScaleTextTextPanel<string> viewInModBrowserButton;
		readonly UIAutoScaleTextTextPanel<string> updateListWithEnabledButton;
		private readonly UIImageButton deleteButton;
		private readonly string filename;

		public UIModPackItem(string name, string[] mods) {
			this.filename = name;
			this.mods = mods;
			this.numMods = mods.Length;
			modMissing = new bool[mods.Length];
			numModsEnabled = 0;
			numModsDisabled = 0;
			numModsMissing = 0;
			for (int i = 0; i < mods.Length; i++) {
				if (UIModPacks.mods.Contains(mods[i])) {
					if (ModLoader.IsEnabled(mods[i])) {
						numModsEnabled++;
					}
					else {
						numModsDisabled++;
					}
				}
				else {
					modMissing[i] = true;
					numModsMissing++;
				}
			}

			BorderColor = new Color(89, 116, 213) * 0.7f;
			dividerTexture = TextureManager.Load("Images/UI/Divider");
			innerPanelTexture = TextureManager.Load("Images/UI/InnerPanelBackground");
			Height.Pixels = 126;
			Width.Percent = 1f;
			SetPadding(6f);

			modName = new UIText(name) {
				Left = { Pixels = 10 },
				Top = { Pixels = 5 }
			};
			Append(modName);

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

			enableListButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackEnableThisList")) {
				Width = { Pixels = 151 },
				Height = { Pixels = 36 },
				Left = { Pixels = 248 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			enableListButton.PaddingTop -= 2f;
			enableListButton.PaddingBottom -= 2f;
			enableListButton.OnClick += EnableList;
			Append(enableListButton);

			enableListOnlyButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackEnableOnlyThisList")) {
				Width = { Pixels = 190 },
				Height = { Pixels = 36 },
				Left = { Pixels = 50 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			enableListOnlyButton.PaddingTop -= 2f;
			enableListOnlyButton.PaddingBottom -= 2f;
			enableListOnlyButton.OnClick += EnableListOnly;
			Append(enableListOnlyButton);

			viewInModBrowserButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackViewModsInModBrowser")) {
				Width = { Pixels = 246 },
				Height = { Pixels = 36 },
				Left = { Pixels = 50 },
				Top = { Pixels = 80 }
			}.WithFadedMouseOver();
			viewInModBrowserButton.PaddingTop -= 2f;
			viewInModBrowserButton.PaddingBottom -= 2f;
			viewInModBrowserButton.OnClick += ViewInModBrowser;
			Append(viewInModBrowserButton);

			updateListWithEnabledButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackUpdateListWithEnabled")) {
				Width = { Pixels = 225 },
				Height = { Pixels = 36 },
				Left = { Pixels = 304 },
				Top = { Pixels = 80 }
			}.WithFadedMouseOver();
			updateListWithEnabledButton.PaddingTop -= 2f;
			updateListWithEnabledButton.PaddingBottom -= 2f;
			updateListWithEnabledButton.OnClick += (a, b) => UIModPacks.SaveModList(filename);
			Append(updateListWithEnabledButton);

			deleteButton = new UIImageButton(TextureManager.Load("Images/UI/ButtonDelete")) {
				Top = { Pixels = 40 }
			};
			deleteButton.OnClick += DeleteButtonClick;
			Append(deleteButton);
		}

		private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width) {
			spriteBatch.Draw(this.innerPanelTexture, position, new Rectangle?(new Rectangle(0, 0, 8, this.innerPanelTexture.Height)), Color.White);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + 8f, position.Y), new Rectangle?(new Rectangle(8, 0, 8, this.innerPanelTexture.Height)), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + width - 8f, position.Y), new Rectangle?(new Rectangle(16, 0, 8, this.innerPanelTexture.Height)), Color.White);
		}

		private void DrawEnabledText(SpriteBatch spriteBatch, Vector2 drawPos) {

			string text = Language.GetTextValue("tModLoader.ModPackModsAvailableStatus", numMods, numModsEnabled, numModsDisabled, numModsMissing);
			Color color = (numModsMissing > 0 ? Color.Red : (numModsDisabled > 0 ? Color.Yellow : Color.Green));

			Utils.DrawBorderString(spriteBatch, text, drawPos, color, 1f, 0f, 0f, -1);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(this.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
			drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 355, innerDimensions.Y);
			this.DrawPanel(spriteBatch, drawPos, 350f);
			this.DrawEnabledText(spriteBatch, drawPos + new Vector2(10f, 5f));
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
			this.BackgroundColor = UICommon.defaultUIBlue;
			this.BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt) {
			base.MouseOut(evt);
			this.BackgroundColor = new Color(63, 82, 151) * 0.7f;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		private void DeleteButtonClick(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modPackItem = ((UIModPackItem)listeningElement.Parent);
			Directory.CreateDirectory(UIModPacks.ModListSaveDirectory);
			string path = UIModPacks.ModListSaveDirectory + Path.DirectorySeparatorChar + modPackItem.filename + ".json";
			if (File.Exists(path)) {
				File.Delete(path);
			}
			Main.menuMode = Interface.modPacksMenuID;// should reload
		}

		private static void EnableList(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = (UIModPackItem)listeningElement.Parent;
			foreach (string modname in modListItem.mods) {
				if (UIModPacks.mods.Contains(modname))
					ModLoader.EnableMod(modname);
			}
			Main.menuMode = Interface.modPacksMenuID; // should reload, which should refresh enabled counts

			if (modListItem.numModsMissing > 0) {
				string missing = "";
				for (int i = 0; i < modListItem.mods.Length; i++) {
					if (modListItem.modMissing[i]) {
						missing += modListItem.mods[i] + "\n";
					}
				}
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModPackModsMissing", missing), Interface.modPacksMenuID);
			}
		}

		private static void ViewInModBrowser(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = ((UIModPackItem)listeningElement.Parent);
			Interface.modBrowser.Activate();
			Interface.modBrowser.filterTextBox.Text = "";
			Interface.modBrowser.SpecialModPackFilter = modListItem.mods.ToList();
			Interface.modBrowser.SpecialModPackFilterTitle = Language.GetTextValue("tModLoader.MBFilterModlist");// Too long: " + modListItem.modName.Text;
			Interface.modBrowser.updateFilterMode = UpdateFilter.All; // Set to 'All' so all mods from ModPack are visible
			Interface.modBrowser.modSideFilterMode = ModSideFilter.All;
			Interface.modBrowser.UpdateFilterToggle.setCurrentState((int)Interface.modBrowser.updateFilterMode);
			Interface.modBrowser.ModSideFilterToggle.setCurrentState((int)Interface.modBrowser.modSideFilterMode);
			Interface.modBrowser.updateNeeded = true;
			Main.PlaySound(SoundID.MenuOpen);
			Main.menuMode = Interface.modBrowserID;
		}

		private static void EnableListOnly(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = (UIModPackItem)listeningElement.Parent;
			foreach (var item in UIModPacks.mods) {
				ModLoader.DisableMod(item);
			}
			foreach (string modname in modListItem.mods) {
				if (UIModPacks.mods.Contains(modname))
					ModLoader.EnableMod(modname);
			}
			Main.menuMode = Interface.reloadModsID; // should reload, which should refresh enabled counts

			if (modListItem.numModsMissing > 0) {
				string missing = "";
				for (int i = 0; i < modListItem.mods.Length; i++) {
					if (modListItem.modMissing[i]) {
						missing += modListItem.mods[i] + "\n";
					}
				}
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModPackModsMissing", missing), Interface.reloadModsID);
			}
		}

		private static void ViewListInfo(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = ((UIModPackItem)listeningElement.Parent);
			Main.PlaySound(10, -1, -1, 1);
			string message = "";
			for (int i = 0; i < modListItem.mods.Length; i++) {
				message += modListItem.mods[i] + (modListItem.modMissing[i] ? Language.GetTextValue("tModLoader.ModPackMissing") : ModLoader.IsEnabled(modListItem.mods[i]) ? "" : Language.GetTextValue("tModLoader.ModPackDisabled")) + "\n";
			}
			//Interface.infoMessage.SetMessage($"This list contains the following mods:\n{String.Join("\n", ((UIModListItem)listeningElement.Parent).mods)}");
			Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModPackModsContained", message), Interface.modPacksMenuID);
		}

		public override int CompareTo(object obj) {
			var item = obj as UIModPackItem;
			if (item == null) {
				return base.CompareTo(obj);
			}
			return filename.CompareTo(item.filename);
		}
	}
}
