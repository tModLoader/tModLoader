using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIInfoMessage : UIState
	{
		private UIElement _area;
		private UIMessageBox _messageBox;
		private UITextPanel<string> _button;
		private UITextPanel<string> _buttonAlt;
		private UIState _gotoState;
		private string _message;
		private int _gotoMenu;
		private Action _altAction;
		private string _altText;

		public override void OnInitialize() {
			_area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 200 },
				Height = { Pixels = -240, Percent = 1f },
				HAlign = 0.5f
			};

			var uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.MainPanelBackground
			};
			_area.Append(uIPanel);

			_messageBox = new UIMessageBox(string.Empty) {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f }
			};
			uIPanel.Append(_messageBox);

			var uIScrollbar = new UIScrollbar {
				Height = { Pixels = -20, Percent = 1f },
				VAlign = 0.5f,
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);

			_messageBox.SetScrollbar(uIScrollbar);

			_button = new UITextPanel<string>(Language.GetTextValue("tModLoader.OK"), 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				Left = { Percent = .25f },
				VAlign = 1f,
				Top = { Pixels = -30 }
			}.WithFadedMouseOver();
			_button.OnClick += OKClick;
			_area.Append(_button);

			_buttonAlt = new UITextPanel<string>("???", 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				Left = { Percent = .5f },
				VAlign = 1f,
				Top = { Pixels = -30 }
			}.WithFadedMouseOver();
			_buttonAlt.OnClick += AltClick;
			_area.Append(_buttonAlt);

			Append(_area);
		}

		public override void OnActivate() {
			_messageBox.SetText(_message);
			_buttonAlt.SetText(_altText);
			bool showAlt = !string.IsNullOrEmpty(_altText);
			_button.Left.Percent = showAlt ? 0 : .25f;
			_area.AddOrRemoveChild(_buttonAlt, showAlt);
		}

		internal void Show(string message, int gotoMenu, UIState state = null, string altButtonText = "", Action altButtonAction = null) {
			_message = message;
			_gotoMenu = gotoMenu;
			_gotoState = state;
			_altText = altButtonText;
			_altAction = altButtonAction;
			Main.menuMode = Interface.infoMessageID;
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Main.menuMode = _gotoMenu;
			if (_gotoState != null)
				Main.MenuUI.SetState(_gotoState);
		}

		private void AltClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			_altAction?.Invoke();
			Main.menuMode = _gotoMenu;
		}
	}
}
