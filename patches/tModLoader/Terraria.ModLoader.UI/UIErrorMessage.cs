using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIErrorMessage : UIState
	{
		private UIMessageBox message = new UIMessageBox("");
		private int gotoMenu = 0;
		private string file;

		public override void OnInitialize()
		{
			UIElement area = new UIElement();
			area.Width.Set(0f, 0.8f);
			area.Top.Set(200f, 0f);
			area.Height.Set(-240f, 1f);
			area.HAlign = 0.5f;
			message.Width.Set(0f, 1f);
			message.Height.Set(0f, 0.8f);
			message.HAlign = 0.5f;
			area.Append(message);
			UITextPanel<string> button = new UITextPanel<string>("Continue", 0.7f, true);
			button.Width.Set(-10f, 0.5f);
			button.Height.Set(50f, 0f);
			button.VAlign = 1f;
			button.Top.Set(-30f, 0f);
			button.OnMouseOver += UICommon.FadedMouseOver;
			button.OnMouseOut += UICommon.FadedMouseOut;
			button.OnClick += ContinueClick;
			area.Append(button);
			UITextPanel<string> button2 = new UITextPanel<string>("Open Logs", 0.7f, true);
			button2.CopyStyle(button);
			button2.HAlign = 1f;
			button2.OnMouseOver += UICommon.FadedMouseOver;
			button2.OnMouseOut += UICommon.FadedMouseOut;
			button2.OnClick += OpenFile;
			area.Append(button2);
			base.Append(area);
		}

		public override void OnActivate()
		{
			Netplay.disconnect = true;
		}

		internal void SetMessage(string text)
		{
			message.SetText(text);
		}

		internal void SetGotoMenu(int gotoMenu)
		{
			this.gotoMenu = gotoMenu;
		}

		internal void SetFile(string file)
		{
			this.file = file;
		}

		private void ContinueClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
		}

		private void OpenFile(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Process.Start(file);
		}
	}
}
