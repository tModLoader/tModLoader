using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIErrorMessage : UIState
	{
		private UIMessageBox messageBox;
		private UIElement area;
		private UITextPanel<string> continueButton; // label changes to retry/exit
		private UITextPanel<string> exitAndDisableAllButton;
		private UITextPanel<string> webHelpButton;
		private UITextPanel<string> skipLoadButton;
		
		private string message;
		private int gotoMenu;
		private string webHelpURL;
		private bool showRetry;
		private bool showSkip;

		public override void OnInitialize() {
			area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 200 },
				Height = { Pixels = -210, Percent = 1f },
				HAlign = 0.5f
			};

			messageBox = new UIMessageBox("") {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				HAlign = 0.5f
			};
			area.Append(messageBox);

			continueButton = new UITextPanel<string>("", 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				Top = { Pixels = -108, Percent = 1f }
			};
			continueButton.WithFadedMouseOver();
			continueButton.OnClick += ContinueClick;
			area.Append(continueButton);

			var openLogsButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.OpenLogs"), 0.7f, true);
			openLogsButton.CopyStyle(continueButton);
			openLogsButton.HAlign = 1f;
			openLogsButton.WithFadedMouseOver();
			openLogsButton.OnClick += OpenFile;
			area.Append(openLogsButton);

			webHelpButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.OpenWebHelp"), 0.7f, true);
			webHelpButton.CopyStyle(openLogsButton);
			webHelpButton.Top.Set(-55f, 1f);
			webHelpButton.WithFadedMouseOver();
			webHelpButton.OnClick += VisitRegisterWebpage;
			area.Append(webHelpButton);

			skipLoadButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.SkipToMainMenu"), 0.7f, true);
			skipLoadButton.CopyStyle(continueButton);
			skipLoadButton.Top.Set(-55f, 1f);
			skipLoadButton.WithFadedMouseOver();
			skipLoadButton.OnClick += SkipLoad;
			area.Append(skipLoadButton);

			exitAndDisableAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.ExitAndDisableAll"), 0.7f, true);
			exitAndDisableAllButton.CopyStyle(skipLoadButton);
			exitAndDisableAllButton.TextColor = Color.Red;
			exitAndDisableAllButton.WithFadedMouseOver();
			exitAndDisableAllButton.OnClick += ExitAndDisableAll;

			Append(area);
		}

		public override void OnActivate() {
			Netplay.disconnect = true;

			messageBox.SetText(message);

			var continueKey = gotoMenu < 0 ? "Exit" : showRetry ? "Retry" : "Continue";
			continueButton.SetText(Language.GetTextValue("tModLoader." + continueKey));
			continueButton.TextColor = gotoMenu >= 0 ? Color.White : Color.Red;
			
			area.AddOrRemoveChild(webHelpButton, !string.IsNullOrEmpty(webHelpURL));
			area.AddOrRemoveChild(skipLoadButton, showSkip);
			area.AddOrRemoveChild(exitAndDisableAllButton, gotoMenu < 0);
		}

		internal void Show(string message, int gotoMenu, string webHelpURL = "", bool showRetry = false, bool showSkip = false) {
			this.message = message;
			this.gotoMenu = gotoMenu;
			this.webHelpURL = webHelpURL;
			this.showRetry = showRetry;
			this.showSkip = showSkip;
			Main.gameMenu = true;
			Main.menuMode = Interface.errorMessageID;
		}

		private void ContinueClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			if (gotoMenu < 0)
				Environment.Exit(0);

			Main.menuMode = gotoMenu;
		}

		private void ExitAndDisableAll(UIMouseEvent evt, UIElement listeningElement) {
			foreach (var mod in ModLoader.EnabledMods)
				ModLoader.DisableMod(mod);

			Environment.Exit(0);
		}

		private void OpenFile(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			Process.Start(Logging.LogPath);
		}

		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			Process.Start(webHelpURL);
		}

		private void SkipLoad(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			ModLoader.skipLoad = true;
			Main.menuMode = gotoMenu;
		}
	}
}
