using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.IO;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
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
		private UITextPanel<string> retryButton;
		
		private string message;
		private int gotoMenu;
		private string webHelpURL;
		private bool continueIsRetry;
		private bool showSkip;
		private Action retryAction;

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

			retryButton = new UITextPanel<string>("Retry", 0.7f, true);
			retryButton.CopyStyle(continueButton);
			retryButton.Top.Set(-50f, 1f);
			retryButton.WithFadedMouseOver();
			retryButton.OnClick += (evt, elem) => retryAction();

			Append(area);
		}

		public override void OnActivate() {
			Netplay.Disconnect = true;

			messageBox.SetText(message);

			string continueKey = gotoMenu < 0 ? "Exit" : continueIsRetry ? "Retry" : "Continue";
			continueButton.SetText(Language.GetTextValue("tModLoader." + continueKey));
			continueButton.TextColor = gotoMenu >= 0 ? Color.White : Color.Red;
			
			area.AddOrRemoveChild(webHelpButton, !string.IsNullOrEmpty(webHelpURL));
			area.AddOrRemoveChild(skipLoadButton, showSkip);
			area.AddOrRemoveChild(exitAndDisableAllButton, gotoMenu < 0);
			area.AddOrRemoveChild(retryButton, retryAction != null);
		}

		public override void OnDeactivate() {
			retryAction = null; //release references for the GC
		}

		internal void Show(string message, int gotoMenu, string webHelpURL = "", bool continueIsRetry = false, bool showSkip = false, Action retryAction = null) {
			this.message = message;
			this.gotoMenu = gotoMenu;
			this.webHelpURL = webHelpURL;
			this.continueIsRetry = continueIsRetry;
			this.showSkip = showSkip;
			this.retryAction = retryAction;
			Main.gameMenu = true;
			Main.menuMode = Interface.errorMessageID;
		}

		private void ContinueClick(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10);
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
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Process.Start(Logging.LogPath);
		}

		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Process.Start(webHelpURL);
		}

		private void SkipLoad(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			ModLoader.skipLoad = true;
			Main.menuMode = gotoMenu;
		}
	}
}
