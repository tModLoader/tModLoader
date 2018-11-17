using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIErrorMessage : UIState
	{
		private UIMessageBox message;
		private UIElement area;
		private int gotoMenu;
		private UITextPanel<string> continueButton;
		private string webHelpURL;
		private UITextPanel<string> webHelpButton;
		private UITextPanel<string> skipLoadButton;
		private bool showSkipButton;

		public UIErrorMessage() {
			if (Main.dedServ)
				return;

			message = new UIMessageBox("");
			continueButton = new UITextPanel<string>("", 0.7f, true);
		}

		public override void OnInitialize()
		{
			area = new UIElement();
			area.Width.Set(0f, 0.8f);
			area.Top.Set(200f, 0f);
			area.Height.Set(-210f, 1f);
			area.HAlign = 0.5f;
			message.Width.Set(0f, 1f);
			message.Height.Set(-110f, 1f);
			message.HAlign = 0.5f;
			area.Append(message);
			
			continueButton.Width.Set(-10f, 0.5f);
			continueButton.Height.Set(50f, 0f);
			continueButton.Top.Set(-108f, 1f);
			continueButton.OnMouseOver += UICommon.FadedMouseOver;
			continueButton.OnMouseOut += UICommon.FadedMouseOut;
			continueButton.OnClick += ContinueClick;
			area.Append(continueButton);

			UITextPanel<string> openLogsButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.OpenLogs"), 0.7f, true);
			openLogsButton.CopyStyle(continueButton);
			openLogsButton.HAlign = 1f;
			openLogsButton.OnMouseOver += UICommon.FadedMouseOver;
			openLogsButton.OnMouseOut += UICommon.FadedMouseOut;
			openLogsButton.OnClick += OpenFile;
			area.Append(openLogsButton);

			webHelpButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.OpenWebHelp"), 0.7f, true);
			webHelpButton.CopyStyle(openLogsButton);
			webHelpButton.Top.Set(-55f, 1f);
			webHelpButton.OnMouseOver += UICommon.FadedMouseOver;
			webHelpButton.OnMouseOut += UICommon.FadedMouseOut;
			webHelpButton.OnClick += VisitRegisterWebpage;
			area.Append(webHelpButton);

			skipLoadButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.SkipToMainMenu"), 0.7f, true);
			skipLoadButton.CopyStyle(continueButton);
			skipLoadButton.Top.Set(-55f, 1f);
			skipLoadButton.OnMouseOver += UICommon.FadedMouseOver;
			skipLoadButton.OnMouseOut += UICommon.FadedMouseOut;
			skipLoadButton.OnClick += SkipLoad;
			area.Append(skipLoadButton);

			Append(area);
		}

		public override void OnActivate()
		{
			Netplay.disconnect = true;
			if (string.IsNullOrEmpty(webHelpURL))
				area.RemoveChild(webHelpButton);
			else
				area.Append(webHelpButton);
			if (showSkipButton)
				area.Append(skipLoadButton);
			else
				area.RemoveChild(skipLoadButton);
		}

		internal void SetMessage(string text)
		{
			message.SetText(text);
			SetWebHelpURL("");
			showSkipButton = false;
		}

		internal void SetWebHelpURL(string text)
		{
			this.webHelpURL = text;
		}

		internal void ShowSkipModsButton(bool skip = true)
		{
			this.showSkipButton = skip;
		}

		internal void SetGotoMenu(int gotoMenu)
		{
			this.gotoMenu = gotoMenu;
			continueButton.SetText(gotoMenu >= 0 ? Language.GetTextValue("tModLoader.Continue") : Language.GetTextValue("tModLoader.Exit"));
			continueButton.TextColor = gotoMenu >= 0 ? Color.White : Color.Red;
		}

		internal void ContinueIsRetry()
		{
			if(gotoMenu >= 0)
				continueButton.SetText(Language.GetTextValue("tModLoader.Retry"));
		}

		private void ContinueClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10);
			if (gotoMenu < 0)
				Environment.Exit(0);

			Main.menuMode = gotoMenu;
		}

		private void OpenFile(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10);
			Process.Start(Logging.LogPath);
		}

		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			Process.Start(webHelpURL);
		}

		private void SkipLoad(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			ModLoader.skipLoad = true;
			Main.menuMode = gotoMenu;
		}
	}
}
