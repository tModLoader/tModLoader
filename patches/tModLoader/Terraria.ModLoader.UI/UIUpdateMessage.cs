using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIUpdateMessage : UIState
	{
		private UIMessageBox message = new UIMessageBox("");
		private int gotoMenu = 0;
		private string url;

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
			UITextPanel<string> button = new UITextPanel<string>("Ignore", 0.7f, true);
			button.Width.Set(-10f, 0.5f);
			button.Height.Set(50f, 0f);
			button.VAlign = 1f;
			button.Top.Set(-30f, 0f);
			button.OnMouseOver += UICommon.FadedMouseOver;
			button.OnMouseOut += UICommon.FadedMouseOut;
			button.OnClick += IgnoreClick;
			area.Append(button);
			UITextPanel<string> button2 = new UITextPanel<string>("Download", 0.7f, true);
			button2.CopyStyle(button);
			button2.HAlign = 1f;
			button2.OnMouseOver += UICommon.FadedMouseOver;
			button2.OnMouseOut += UICommon.FadedMouseOut;
			button2.OnClick += OpenURL;
			area.Append(button2);
			base.Append(area);
		}

		internal void SetMessage(string text)
		{
			message.SetText(text);
		}

		internal void SetGotoMenu(int gotoMenu)
		{
			this.gotoMenu = gotoMenu;
		}

		internal void SetURL(string url)
		{
			this.url = url;
		}

		private void IgnoreClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
		}

		private void OpenURL(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Process.Start(url);
		}
	}
}
