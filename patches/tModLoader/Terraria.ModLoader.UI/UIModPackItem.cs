using System;
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

		private readonly Texture2D _dividerTexture;
		private readonly Texture2D _innerPanelTexture;
		private readonly UIText _modName;
		private readonly string[] _mods;
		private readonly bool[] _modMissing;
		private readonly int _numMods;
		private readonly int _numModsEnabled;
		private readonly int _numModsDisabled;
		private readonly int _numModsMissing;
		private readonly UIAutoScaleTextTextPanel<string> _enableListButton;
		private readonly UIAutoScaleTextTextPanel<string> _enableListOnlyButton;
		private readonly UIAutoScaleTextTextPanel<string> _viewInModBrowserButton;
		private readonly UIAutoScaleTextTextPanel<string> _updateListWithEnabledButton;
		private readonly UIImageButton _deleteButton;
		private readonly string _filename;

		public UIModPackItem(string name, string[] mods) {
			_filename = name;
			_mods = mods;
			_numMods = mods.Length;
			_modMissing = new bool[mods.Length];
			_numModsEnabled = 0;
			_numModsDisabled = 0;
			_numModsMissing = 0;
			for (int i = 0; i < mods.Length; i++) {
				if (UIModPacks.Mods.Contains(mods[i])) {
					if (ModLoader.IsEnabled(mods[i])) {
						_numModsEnabled++;
					}
					else {
						_numModsDisabled++;
					}
				}
				else {
					_modMissing[i] = true;
					_numModsMissing++;
				}
			}

			BorderColor = new Color(89, 116, 213) * 0.7f;
			_dividerTexture = UICommon.DividerTexture;
			_innerPanelTexture = UICommon.InnerPanelTexture;
			Height.Pixels = 126;
			Width.Percent = 1f;
			SetPadding(6f);

			_modName = new UIText(name) {
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

			_enableListButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackEnableThisList")) {
				Width = { Pixels = 151 },
				Height = { Pixels = 36 },
				Left = { Pixels = 248 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			_enableListButton.PaddingTop -= 2f;
			_enableListButton.PaddingBottom -= 2f;
			_enableListButton.OnClick += EnableList;
			Append(_enableListButton);

			_enableListOnlyButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackEnableOnlyThisList")) {
				Width = { Pixels = 190 },
				Height = { Pixels = 36 },
				Left = { Pixels = 50 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			_enableListOnlyButton.PaddingTop -= 2f;
			_enableListOnlyButton.PaddingBottom -= 2f;
			_enableListOnlyButton.OnClick += EnableListOnly;
			Append(_enableListOnlyButton);

			_viewInModBrowserButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackViewModsInModBrowser")) {
				Width = { Pixels = 246 },
				Height = { Pixels = 36 },
				Left = { Pixels = 50 },
				Top = { Pixels = 80 }
			}.WithFadedMouseOver();
			_viewInModBrowserButton.PaddingTop -= 2f;
			_viewInModBrowserButton.PaddingBottom -= 2f;
			_viewInModBrowserButton.OnClick += ViewInModBrowser;
			Append(_viewInModBrowserButton);

			_updateListWithEnabledButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackUpdateListWithEnabled")) {
				Width = { Pixels = 225 },
				Height = { Pixels = 36 },
				Left = { Pixels = 304 },
				Top = { Pixels = 80 }
			}.WithFadedMouseOver();
			_updateListWithEnabledButton.PaddingTop -= 2f;
			_updateListWithEnabledButton.PaddingBottom -= 2f;
			_updateListWithEnabledButton.OnClick += (a, b) => UIModPacks.SaveModList(_filename);
			Append(_updateListWithEnabledButton);

			_deleteButton = new UIImageButton(TextureManager.Load("Images/UI/ButtonDelete")) {
				Top = { Pixels = 40 }
			};
			_deleteButton.OnClick += DeleteButtonClick;
			Append(_deleteButton);
		}

		private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width) {
			spriteBatch.Draw(_innerPanelTexture, position, new Rectangle(0, 0, 8, _innerPanelTexture.Height), Color.White);
			spriteBatch.Draw(_innerPanelTexture, new Vector2(position.X + 8f, position.Y), new Rectangle(8, 0, 8, _innerPanelTexture.Height), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(_innerPanelTexture, new Vector2(position.X + width - 8f, position.Y), new Rectangle(16, 0, 8, _innerPanelTexture.Height), Color.White);
		}

		private void DrawEnabledText(SpriteBatch spriteBatch, Vector2 drawPos) {
			string text = Language.GetTextValue("tModLoader.ModPackModsAvailableStatus", _numMods, _numModsEnabled, _numModsDisabled, _numModsMissing);
			Color color = (_numModsMissing > 0 ? Color.Red : (_numModsDisabled > 0 ? Color.Yellow : Color.Green));

			Utils.DrawBorderString(spriteBatch, text, drawPos, color);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(_dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
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
			Directory.CreateDirectory(UIModPacks.ModPacksDirectory);
			string path = UIModPacks.ModPacksDirectory + Path.DirectorySeparatorChar + modPackItem._filename + ".json";
			if (File.Exists(path)) {
				File.Delete(path);
			}
			Main.menuMode = Interface.modPacksMenuID;// should reload
		}

		private static void EnableList(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = (UIModPackItem)listeningElement.Parent;
			foreach (string modname in modListItem._mods) {
				if (UIModPacks.Mods.Contains(modname))
					ModLoader.EnableMod(modname);
			}
			Main.menuMode = Interface.modPacksMenuID; // should reload, which should refresh enabled counts

			if (modListItem._numModsMissing > 0) {
				string missing = "";
				for (int i = 0; i < modListItem._mods.Length; i++) {
					if (modListItem._modMissing[i]) {
						missing += modListItem._mods[i] + "\n";
					}
				}
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModPackModsMissing", missing), Interface.modPacksMenuID);
			}
		}

		private static void ViewInModBrowser(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = ((UIModPackItem)listeningElement.Parent);
			Interface.modBrowser.Activate();
			Interface.modBrowser.FilterTextBox.Text = "";
			Interface.modBrowser.SpecialModPackFilter = modListItem._mods.ToList();
			Interface.modBrowser.SpecialModPackFilterTitle = Language.GetTextValue("tModLoader.MBFilterModlist");// Too long: " + modListItem.modName.Text;
			Interface.modBrowser.UpdateFilterMode = UpdateFilter.All; // Set to 'All' so all mods from ModPack are visible
			Interface.modBrowser.ModSideFilterMode = ModSideFilter.All;
			Interface.modBrowser.UpdateFilterToggle.SetCurrentState((int)Interface.modBrowser.UpdateFilterMode);
			Interface.modBrowser.ModSideFilterToggle.SetCurrentState((int)Interface.modBrowser.ModSideFilterMode);
			Interface.modBrowser.UpdateNeeded = true;
			Main.PlaySound(SoundID.MenuOpen);
			Main.menuMode = Interface.modBrowserID;
		}

		private static void EnableListOnly(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = (UIModPackItem)listeningElement.Parent;
			foreach (var item in UIModPacks.Mods) {
				ModLoader.DisableMod(item);
			}
			foreach (string modname in modListItem._mods) {
				if (UIModPacks.Mods.Contains(modname))
					ModLoader.EnableMod(modname);
			}
			Main.menuMode = Interface.reloadModsID; // should reload, which should refresh enabled counts

			if (modListItem._numModsMissing > 0) {
				string missing = "";
				for (int i = 0; i < modListItem._mods.Length; i++) {
					if (modListItem._modMissing[i]) {
						missing += modListItem._mods[i] + "\n";
					}
				}
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ModPackModsMissing", missing), Interface.reloadModsID);
			}
		}

		private static void ViewListInfo(UIMouseEvent evt, UIElement listeningElement) {
			UIModPackItem modListItem = ((UIModPackItem)listeningElement.Parent);
			Main.PlaySound(10);
			string message = "";
			for (int i = 0; i < modListItem._mods.Length; i++) {
				message += modListItem._mods[i] + (modListItem._modMissing[i] ? Language.GetTextValue("tModLoader.ModPackMissing") : ModLoader.IsEnabled(modListItem._mods[i]) ? "" : Language.GetTextValue("tModLoader.ModPackDisabled")) + "\n";
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
