using System;
using System.Diagnostics;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO is this meaningfully different to EnterPassphraseMenu?
	internal class UIEnterSteamIDMenu : UIState
	{
		private const string REGISTER_URL = "http://javid.ddns.net/tModLoader/register.php";

		public UITextPanel<string> UiTextPanel;
		internal UIInputTextField SteamIdTextField;
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

			UiTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.EnterSteamID"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.DefaultUIBlue
			}.WithPadding(15f);
			uIElement.Append(UiTextPanel);

			var button = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			};
			button.WithFadedMouseOver();
			button.OnClick += BackClick;
			uIElement.Append(button);

			button = new UITextPanel<string>(Language.GetTextValue("UI.Submit"));
			button.CopyStyle(button);
			button.HAlign = 1f;
			button.WithFadedMouseOver();
			button.OnClick += SubmitSteamID;
			uIElement.Append(button);

			// TODO Commented code, yuck
			//UITextPanel<string> button3 = new UITextPanel<string>("Visit Website to Generate Passphrase");
			//button3.CopyStyle(button);
			//button3.Width.Set(0f, 1f);
			//button3.Top.Set(-20f, 0f);
			//button3.OnMouseOver += UICommon.FadedMouseOver;
			//button3.OnMouseOut += UICommon.FadedMouseOut;
			//button3.OnClick += VisitRegisterWebpage;
			//uIElement.Append(button3);

			SteamIdTextField = new UIInputTextField(Language.GetTextValue("tModLoader.PasteSteamID")) {
				HAlign = 0.5f,
				VAlign = 0.5f,
				Left = { Pixels = -100, Percent = 0 }
			};
			SteamIdTextField.OnTextChange += OnTextChange;
			uIPanel.Append(SteamIdTextField);

			Append(uIElement);
		}

		private void SubmitSteamID(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			ModLoader.SteamID64 = SteamIdTextField.Text.Trim();
			Main.SaveSettings();
			Main.menuMode = _gotoMenu;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11);
			Main.menuMode = _gotoMenu;
		}

		//TODO unused
		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Process.Start(REGISTER_URL);
		}

		//TODO unused
		private void OnTextChange(object sender, EventArgs e) {
		}
		
		//TODO unused
		internal void SetGotoMenu(int gotoMenu) {
			_gotoMenu = gotoMenu;
		}
	}
}
