using System;
using System.Diagnostics;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIEnterPassphraseMenu : UIState
	{
		private const string REGISTER_URL = "http://javid.ddns.net/tModLoader/register.php";

		public UITextPanel<string> UITextPanel;
		internal UIInputTextField PasscodeTextField;
		private int _gotoMenu;

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
				BackgroundColor = UICommon.MainPanelBackground,
				PaddingTop = 0f
			};
			uIElement.Append(uIPanel);

			UITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBPublishEnterPassphrase"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.DefaultUIBlue
			}.WithPadding(15);
			uIElement.Append(UITextPanel);
			
			var buttonBack = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			}.WithFadedMouseOver();
			buttonBack.OnClick += BackClick;
			uIElement.Append(buttonBack);

			var buttonSubmit = new UITextPanel<string>(Language.GetTextValue("UI.Submit"));
			buttonSubmit.CopyStyle(buttonBack);
			buttonSubmit.HAlign = 1f;
			buttonSubmit.WithFadedMouseOver();
			buttonSubmit.OnClick += SubmitPassphrase;
			uIElement.Append(buttonSubmit);

			var buttonVisitWebsite = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBPublishVisitWebsiteForPassphrase"));
			buttonVisitWebsite.CopyStyle(buttonBack);
			buttonVisitWebsite.Width.Percent = 1f;
			buttonVisitWebsite.Top.Pixels = -20;
			buttonVisitWebsite.WithFadedMouseOver();
			buttonVisitWebsite.OnClick += VisitRegisterWebpage;
			uIElement.Append(buttonVisitWebsite);

			PasscodeTextField = new UIInputTextField(Language.GetTextValue("tModLoader.MBPublishPastePassphrase")) {
				HAlign = 0.5f,
				VAlign = 0.5f,
				Left = { Pixels = -100, Percent = 0 }
			};
			PasscodeTextField.OnTextChange += OnTextChange;
			uIPanel.Append(PasscodeTextField);

			Append(uIElement);
		}

		private void SubmitPassphrase(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			ModLoader.modBrowserPassphrase = PasscodeTextField.Text.Trim();
			Main.SaveSettings();
			if (Engine.GoGVerifier.IsGoG) {
				Main.menuMode = Interface.enterSteamIDMenuID;
				Interface.enterSteamIDMenu.SetGotoMenu(_gotoMenu);
			}
			else
				Main.menuMode = _gotoMenu;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11);
			Main.menuMode = _gotoMenu;
		}

		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Process.Start(REGISTER_URL);
		}

		// TODO unused?
		private void OnTextChange(object sender, EventArgs e) {
		}

		internal void SetGotoMenu(int gotoMenu) {
			_gotoMenu = gotoMenu;
		}
	}
}
