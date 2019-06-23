using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIInfoMessage : UIState
	{
		private UIElement area;
		private UIMessageBox messageBox;
		private UITextPanel<string> button;
		private UITextPanel<string> buttonAlt;
		private UIState gotoState;

		private string message;
		private int gotoMenu;

		private Action altAction;
		private string altText;

		public override void OnInitialize() {
			area = new UIElement {
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

			button = new UITextPanel<string>(Language.GetTextValue("tModLoader.OK"), 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				Left = { Percent = .25f },
				VAlign = 1f,
				Top = { Pixels = -30 }
			}.WithFadedMouseOver();
			button.OnClick += OKClick;
			area.Append(button);

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
			messageBox.SetText(message);
			buttonAlt.SetText(altText);
			bool showAlt = !string.IsNullOrEmpty(altText);
			button.Left.Percent = showAlt ? 0 : .25f;
			area.AddOrRemoveChild(buttonAlt, showAlt);
		}

		internal void Show(string message, int gotoMenu, UIState state = null, string altButtonText = "", Action altButtonAction = null) {
			this.message = message;
			this.gotoMenu = gotoMenu;
			this.gotoState = state;
			this.altText = altButtonText;
			this.altAction = altButtonAction;
			Main.menuMode = Interface.infoMessageID;
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = this.gotoMenu;
			if (gotoState != null)
				Main.MenuUI.SetState(gotoState);
		}

		private void AltClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			altAction?.Invoke();
			Main.menuMode = this.gotoMenu;
		}
	}
}
