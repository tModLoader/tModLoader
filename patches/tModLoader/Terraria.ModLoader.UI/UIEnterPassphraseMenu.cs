using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader.IO;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq;

namespace Terraria.ModLoader.UI
{
	internal class UIEnterPassphraseMenu : UIState
	{
		string registerURL = "http://javid.ddns.net/tModLoader/register.php";
		public UITextPanel<string> uITextPanel;
		internal UIInputTextField passcodeTextField;
		private int gotoMenu = 0;

		public override void OnInitialize()
		{
			UIElement uIElement = new UIElement();
			uIElement.Width.Set(0f, 0.8f);
			uIElement.MaxWidth.Set(600f, 0f);
			uIElement.Top.Set(220f, 0f);
			uIElement.Height.Set(-220f, 1f);
			uIElement.HAlign = 0.5f;

			UIPanel uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			uIPanel.PaddingTop = 0f;
			uIElement.Append(uIPanel);

			uITextPanel = new UITextPanel<string>("Please Enter Your Passcode", 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);

			UITextPanel<string> button = new UITextPanel<string>("Back", 1f, false);
			button.Width.Set(-10f, 0.5f);
			button.Height.Set(25f, 0f);
			button.VAlign = 1f;
			button.Top.Set(-65f, 0f);
			button.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button.OnClick += new UIElement.MouseEvent(BackClick);
			uIElement.Append(button);

			UITextPanel<string> button2 = new UITextPanel<string>("Submit", 1f, false);
			button2.CopyStyle(button);
			button2.HAlign = 1f;
			button2.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button2.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button2.OnClick += new UIElement.MouseEvent(OKClick);
			uIElement.Append(button2);

			UITextPanel<string> button3 = new UITextPanel<string>("Visit Website to Generate Passphrase", 1f, false);
			button3.CopyStyle(button);
			button3.Width.Set(0f, 1f);
			button3.Top.Set(-20f, 0f);
			button3.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button3.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button3.OnClick += new UIElement.MouseEvent(VisitRegisterWebpage);
			uIElement.Append(button3);

			passcodeTextField = new UIInputTextField("Paste Passphrase (ctrl-v)");
			passcodeTextField.HAlign = 0.5f;
			passcodeTextField.VAlign = 0.5f;
			passcodeTextField.Left.Set(-100, 0);
			passcodeTextField.OnTextChange += new UIInputTextField.EventHandler(OnTextChange);
			uIPanel.Append(passcodeTextField);

			base.Append(uIElement);
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.modBrowserPassphrase = passcodeTextField.currentString;
			Main.SaveSettings();
			Main.menuMode = this.gotoMenu;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
		}

		private void VisitRegisterWebpage(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Process.Start(registerURL);
		}

		private void OnTextChange(object sender, EventArgs e)
		{
		}

		private static void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
		}

		private static void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
		{
			((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.7f;
		}

		internal void SetGotoMenu(int gotoMenu)
		{
			this.gotoMenu = gotoMenu;
		}
	}
}
