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
		string registerURL = "http://javid.ddns.net/tModLoader/register.php";
		public UITextPanel<string> uITextPanel;
		internal UIInputTextField steamIDTextField;
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

			uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.EnterSteamID"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15f);
			uIElement.Append(uITextPanel);

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
			button.OnClick += OKClick;
			uIElement.Append(button);

			//UITextPanel<string> button3 = new UITextPanel<string>("Visit Website to Generate Passphrase");
			//button3.CopyStyle(button);
			//button3.Width.Set(0f, 1f);
			//button3.Top.Set(-20f, 0f);
			//button3.OnMouseOver += UICommon.FadedMouseOver;
			//button3.OnMouseOut += UICommon.FadedMouseOut;
			//button3.OnClick += VisitRegisterWebpage;
			//uIElement.Append(button3);

			steamIDTextField = new UIInputTextField(Language.GetTextValue("tModLoader.PasteSteamID")) {
				HAlign = 0.5f,
				VAlign = 0.5f,
				Left = { Pixels = -100, Percent = 0 }
			};
			steamIDTextField.OnTextChange += OnTextChange;
			uIPanel.Append(steamIDTextField);

			Append(uIElement);
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.SteamID64 = steamIDTextField.Text.Trim();
			Main.SaveSettings();
			Main.menuMode = this.gotoMenu;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
		}

		//TODO unused
		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Process.Start(registerURL);
		}

		//TODO unused
		private void OnTextChange(object sender, EventArgs e) {
		}
		
		//TODO unused
		internal void SetGotoMenu(int gotoMenu) {
			this.gotoMenu = gotoMenu;
		}
	}
}
