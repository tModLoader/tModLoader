using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI
{
	internal class UIModInfo : UIState
	{
		internal UIElement uIElement;
		public UIMessageBox modInfo;
		public UITextPanel<string> uITextPanel;
		internal UIAutoScaleTextTextPanel<string> modHomepageButton;
		internal UIAutoScaleTextTextPanel<string> extractButton;
		internal UIAutoScaleTextTextPanel<string> deleteButton;
		private int gotoMenu = 0;
		private LocalMod localMod;
		private string url = "";
		private string info = "";
		private string modDisplayName = "";

		public override void OnInitialize() {
			uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};

			var uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.mainPanelBackground
			};
			uIElement.Append(uIPanel);

			modInfo = new UIMessageBox("This is a test of mod info here.") {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f }
			};
			uIPanel.Append(modInfo);

			var uIScrollbar = new UIScrollbar {
				Height = { Pixels = -20, Percent = 1f },
				VAlign = 0.5f,
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);

			modInfo.SetScrollbar(uIScrollbar);
			uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModInfoHeader"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15f);
			uIElement.Append(uITextPanel);

			modHomepageButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoVisitHomepage")) {
				Width = { Percent = 1f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			}.WithFadedMouseOver();
			modHomepageButton.OnClick += VisitModHomePage;
			uIElement.Append(modHomepageButton);

			var backButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = { Pixels = -10, Percent = 0.333f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			extractButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoExtract")) {
				Width = { Pixels = -10, Percent = 0.333f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				HAlign = 0.5f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			extractButton.OnClick += ExtractClick;
			uIElement.Append(extractButton);

			deleteButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Delete")) {
				Width = { Pixels = -10, Percent = 0.333f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				HAlign = 1f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			deleteButton.OnClick += DeleteClick;
			uIElement.Append(deleteButton);

			Append(uIElement);
		}

		// TODO use Show pattern
		internal void SetModInfo(string text) {
			info = text;
			if (info.Equals("")) {
				info = Language.GetTextValue("tModLoader.ModInfoNoDescriptionAvailable");
			}
		}

		internal void SetModName(string text) {
			modDisplayName = text;
		}

		internal void SetGotoMenu(int gotoMenu) {
			this.gotoMenu = gotoMenu;
		}

		internal void SetURL(string url) {
			this.url = url;
		}

		internal void SetMod(LocalMod mod) {
			localMod = mod;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11);
			Main.menuMode = gotoMenu;
		}

		private void ExtractClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(ID.SoundID.MenuOpen);
			Interface.extractMod.Show(localMod, gotoMenu);
		}

		private void DeleteClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(ID.SoundID.MenuClose);
			File.Delete(localMod.modFile.path);
			Main.menuMode = gotoMenu;
		}

		private void VisitModHomePage(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Process.Start(url);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = this.gotoMenu;
		}

		public override void OnActivate() {
			uITextPanel.SetText(Language.GetTextValue("tModLoader.ModInfoHeader") + modDisplayName, 0.8f, true);
			modInfo.SetText(info);
			if (url.Equals("")) {
				modHomepageButton.Remove();
			}
			else {
				uIElement.Append(modHomepageButton);
			}
			if (localMod != null) {
				uIElement.AddOrRemoveChild(deleteButton, !ModLoader.Mods.Any(x=> x.Name == localMod.Name));
				uIElement.Append(extractButton);
			}
			else {
				deleteButton.Remove();
				extractButton.Remove();
			}
		}
	}
}
