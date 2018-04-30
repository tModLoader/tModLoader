using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIErrorMessage : UIState
	{
		private UIMessageBox message = new UIMessageBox("");
		private UIElement area;
		private int gotoMenu = 0;
		private string file;
		private string webHelpURL;
		private UITextPanel<string> webHelpButton;
		private Action continueAction;

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

			UITextPanel<string> button = new UITextPanel<string>(Language.GetTextValue("tModLoader.Continue"), 0.7f, true);
			button.Width.Set(-10f, 0.5f);
			button.Height.Set(50f, 0f);
			button.Top.Set(-108f, 1f);
			button.OnMouseOver += UICommon.FadedMouseOver;
			button.OnMouseOut += UICommon.FadedMouseOut;
			button.OnClick += ContinueClick;
			area.Append(button);

			UITextPanel<string> button2 = new UITextPanel<string>(Language.GetTextValue("tModLoader.OpenLogs"), 0.7f, true);
			button2.CopyStyle(button);
			button2.HAlign = 1f;
			button2.OnMouseOver += UICommon.FadedMouseOver;
			button2.OnMouseOut += UICommon.FadedMouseOut;
			button2.OnClick += OpenFile;
			area.Append(button2);

			webHelpButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.OpenWebHelp"), 0.7f, true);
			webHelpButton.CopyStyle(button2);
			webHelpButton.Top.Set(-55f, 1f);
			webHelpButton.OnMouseOver += UICommon.FadedMouseOver;
			webHelpButton.OnMouseOut += UICommon.FadedMouseOut;
			webHelpButton.OnClick += VisitRegisterWebpage;
			area.Append(webHelpButton);

			Append(area);
		}

		public override void OnActivate()
		{
			Netplay.disconnect = true;
			if (string.IsNullOrEmpty(webHelpURL))
				area.RemoveChild(webHelpButton);
			else
				area.Append(webHelpButton);
		}

		internal void SetMessage(string text)
		{
			message.SetText(text);
			SetWebHelpURL("");
		}

		internal void SetWebHelpURL(string text)
		{
			this.webHelpURL = text;
		}

		internal void SetGotoMenu(int gotoMenu)
		{
			this.gotoMenu = gotoMenu;
			continueAction = null;
		}

		internal void OverrideContinueAction(Action action)
		{
			continueAction = action;
			gotoMenu = Interface.errorMessageID;
		}

		internal void SetFile(string file)
		{
			this.file = file;
		}

		private void ContinueClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10);

			continueAction?.Invoke();
			Main.menuMode = gotoMenu;
		}

		private void OpenFile(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10);
			Process.Start(file);
		}

		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			Process.Start(webHelpURL);
		}
	}
}
