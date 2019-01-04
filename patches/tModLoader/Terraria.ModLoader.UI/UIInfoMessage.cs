using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIInfoMessage : UIState
	{
		private UIMessageBox messageBox;
		private UIState gotoState;

		private string message;
		private int gotoMenu;

		public override void OnInitialize() {
			var area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 200 },
				Height = { Pixels = -240, Percent = 1f },
				HAlign = 0.5f
			};

			var uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.mainPanelBackground
			};
			area.Append(uIPanel);

			messageBox = new UIMessageBox("") {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f }
			};
			uIPanel.Append(messageBox);

			var uIScrollbar = new UIScrollbar {
				Height = { Pixels = -20, Percent = 1f },
				VAlign = 0.5f,
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);

			messageBox.SetScrollbar(uIScrollbar);

			var button = new UITextPanel<string>(Language.GetTextValue("tModLoader.OK"), 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				Left = { Percent = .25f },
				VAlign = 1f,
				Top = { Pixels = -30 }
			}.WithFadedMouseOver();
			button.OnClick += OKClick;
			area.Append(button);

			Append(area);
		}

		public override void OnActivate() {
			messageBox.SetText(message);
		}

		internal void SetMessage(string text) {
			messageBox.SetText(text);
		}

		internal void SetGotoMenu(int gotoMenu, UIState state = null) {
			this.gotoMenu = gotoMenu;
			this.gotoState = state;
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
			if (gotoState != null)
				Main.MenuUI.SetState(gotoState);
		}
	}

	internal class UIAdvancedInfoMessage : UIState
	{
		private UIMessageBox messageBox;
		private UITextPanel<string> buttonAlt;

		private string text;
		private int gotoMenu;
		private Action altAction;
		private string altText = "???";

		public override void OnInitialize() {
			var area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 200 },
				Height = { Pixels = -240, Percent = 1f },
				HAlign = 0.5f
			};

			var uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.mainPanelBackground
			};
			area.Append(uIPanel);

			messageBox = new UIMessageBox("") {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f }
			};
			uIPanel.Append(messageBox);

			var uIScrollbar = new UIScrollbar {
				Height = { Pixels = -20, Percent = 1f },
				VAlign = 0.5f,
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);

			messageBox.SetScrollbar(uIScrollbar);

			var buttonOk = new UITextPanel<string>(Language.GetTextValue("tModLoader.OK"), 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				Left = { Percent = 0f },
				VAlign = 1f,
				Top = { Pixels = -30 }
			}.WithFadedMouseOver();
			buttonOk.OnClick += OKClick;
			area.Append(buttonOk);

			buttonAlt = new UITextPanel<string>("???", 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				Left = { Percent = .5f },
				VAlign = 1f,
				Top = { Pixels = -30 }
			}.WithFadedMouseOver();
			buttonAlt.OnClick += AltClick;
			area.Append(buttonAlt);

			Append(area);
		}

		public override void OnActivate() {
			messageBox.SetText(text);
			buttonAlt.SetText(altText);
		}

		//TODO: use Show paradigm, and set all parameters together (see UIErrorMessage)
		internal void SetMessage(string text) {
			messageBox.SetText(text);
		}

		internal void SetAltMessage(string text) {
			altText = text;
		}

		internal void SetAltAction(Action action) {
			altAction = action;
		}

		internal void SetGotoMenu(int gotoMenu) {
			this.gotoMenu = gotoMenu;
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
		}

		private void AltClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			altAction?.Invoke();
			Main.menuMode = this.gotoMenu;
		}
	}
}
