using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIInfoMessage : UIState
	{
		private UIMessageBox message = new UIMessageBox("");
		private int gotoMenu = 0;
		private UIState gotoState;

		public override void OnInitialize()
		{
			UIElement area = new UIElement();
			area.Width.Set(0f, 0.8f);
			area.Top.Set(200f, 0f);
			area.Height.Set(-240f, 1f);
			area.HAlign = 0.5f;

			UIPanel uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			area.Append(uIPanel);

			message.Width.Set(-25f, 1f);
			message.Height.Set(0f, 1f);
			uIPanel.Append(message);

			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(-20, 1f);
			uIScrollbar.VAlign = 0.5f;
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);

			message.SetScrollbar(uIScrollbar);

			UITextPanel<string> button = new UITextPanel<string>(Language.GetTextValue("tModLoader.OK"), 0.7f, true);
			button.Width.Set(-10f, 0.5f);
			button.Height.Set(50f, 0f);
			button.Left.Set(0, .25f);
			button.VAlign = 1f;
			button.Top.Set(-30f, 0f);
			button.OnMouseOver += UICommon.FadedMouseOver;
			button.OnMouseOut += UICommon.FadedMouseOut;
			button.OnClick += OKClick;
			area.Append(button);

			Append(area);
		}

		internal void SetMessage(string text)
		{
			message.SetText(text);
		}

		internal void SetGotoMenu(int gotoMenu, UIState state = null)
		{
			this.gotoMenu = gotoMenu;
			this.gotoState = state;
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
			if (gotoState != null)
				Main.MenuUI.SetState(gotoState);
		}
	}

	internal class UIAdvancedInfoMessage : UIState
	{
		private UIMessageBox message = new UIMessageBox("");
		private int gotoMenu = 0;
		private Action altAction;
		private UITextPanel<string> button2;
		private string button2Text = "???";

		public override void OnInitialize()
		{
			UIElement area = new UIElement();
			area.Width.Set(0f, 0.8f);
			area.Top.Set(200f, 0f);
			area.Height.Set(-240f, 1f);
			area.HAlign = 0.5f;

			UIPanel uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			area.Append(uIPanel);

			message.Width.Set(-25f, 1f);
			message.Height.Set(0f, 1f);
			uIPanel.Append(message);

			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(-20, 1f);
			uIScrollbar.VAlign = 0.5f;
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);

			message.SetScrollbar(uIScrollbar);

			UITextPanel<string> button1 = new UITextPanel<string>(Language.GetTextValue("tModLoader.OK"), 0.7f, true);
			button1.Width.Set(-10f, 0.5f);
			button1.Height.Set(50f, 0f);
			button1.Left.Set(0, 0f);
			button1.VAlign = 1f;
			button1.Top.Set(-30f, 0f);
			button1.OnMouseOver += UICommon.FadedMouseOver;
			button1.OnMouseOut += UICommon.FadedMouseOut;
			button1.OnClick += OKClick;
			area.Append(button1);

			button2 = new UITextPanel<string>("???", 0.7f, true);
			button2.Width.Set(-10f, 0.5f);
			button2.Height.Set(50f, 0f);
			button2.Left.Set(0, .5f);
			button2.VAlign = 1f;
			button2.Top.Set(-30f, 0f);
			button2.OnMouseOver += UICommon.FadedMouseOver;
			button2.OnMouseOut += UICommon.FadedMouseOut;
			button2.OnClick += AltClick;
			area.Append(button2);

			Append(area);
		}

		public override void OnActivate()
		{
			button2.SetText(button2Text);
		}

		internal void SetMessage(string text)
		{
			message.SetText(text);
		}

		internal void SetAltMessage(string text)
		{
			button2Text = text;
		}

		internal void SetAltAction(Action action)
		{
			altAction = action;
		}

		internal void SetGotoMenu(int gotoMenu)
		{
			this.gotoMenu = gotoMenu;
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
		}

		private void AltClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			altAction?.Invoke();
			Main.menuMode = this.gotoMenu;
		}
	}
}
