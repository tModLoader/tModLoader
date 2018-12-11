using System;
using System.Diagnostics;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIEnterPassphraseMenu : UIState
	{
		readonly string registerURL = "http://javid.ddns.net/tModLoader/register.php";
		public UITextPanel<string> uITextPanel;
		internal UIInputTextField passcodeTextField;
		private int gotoMenu;

		public override void OnInitialize() {
			var uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};

			var uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.mainPanelBackground,
				PaddingTop = 0f
			};
			uIElement.Append(uIPanel);

			uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBPublishEnterPassphrase"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15);
			uIElement.Append(uITextPanel);
			
			var button = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			}.WithFadedMouseOver();
			button.OnClick += BackClick;
			uIElement.Append(button);

			button = new UITextPanel<string>(Language.GetTextValue("UI.Submit"));
			button.CopyStyle(button);
			button.HAlign = 1f;
			button.WithFadedMouseOver();
			button.OnClick += OKClick;
			uIElement.Append(button);

			button = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBPublishVisitWebsiteForPassphrase"));
			button.CopyStyle(button);
			button.Width.Percent = 1f;
			button.Top.Pixels = -20;
			button.WithFadedMouseOver();
			button.OnClick += VisitRegisterWebpage;
			uIElement.Append(button);

			passcodeTextField = new UIInputTextField(Language.GetTextValue("tModLoader.MBPublishPastePassphrase")) {
				HAlign = 0.5f,
				VAlign = 0.5f,
				Left = { Pixels = -100, Percent = 0 }
			};
			passcodeTextField.OnTextChange += OnTextChange;
			uIPanel.Append(passcodeTextField);

			Append(uIElement);
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.modBrowserPassphrase = passcodeTextField.Text.Trim();
			Main.SaveSettings();
#if GOG
			Main.menuMode = Interface.enterSteamIDMenuID;
			Interface.enterSteamIDMenu.SetGotoMenu(this.gotoMenu);
#else
			Main.menuMode = this.gotoMenu;
#endif
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
		}

		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Process.Start(registerURL);
		}

		private void OnTextChange(object sender, EventArgs e) {
		}

		internal void SetGotoMenu(int gotoMenu) {
			this.gotoMenu = gotoMenu;
		}
	}
}
