using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI;

internal class UIInfoMessage : UIState, IHaveBackButtonCommand
{
	protected UIElement _area;
	private UIMessageBox _messageBox;
	protected UITextPanel<string> _button;
	private UIAutoScaleTextTextPanel<string> _buttonAlt;
	private UIState _gotoState;
	private string _message;
	private int _gotoMenu;
	private Action _altAction;
	private string _altText;
	private string _okText;

	public UIState PreviousUIState { get; set; }

	public override void OnInitialize()
	{
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
			Height = { Pixels = -12, Percent = 1f },
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
		_button.OnLeftClick += OKClick;
		_area.Append(_button);

		_buttonAlt = new UIAutoScaleTextTextPanel<string>("???", 0.7f, true) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 50 },
			Left = { Percent = .5f },
			VAlign = 1f,
			Top = { Pixels = -30 }
		}.WithFadedMouseOver();
		_buttonAlt.OnLeftClick += AltClick;
		_area.Append(_buttonAlt);

		Append(_area);
	}

	public override void OnActivate()
	{
		_messageBox.SetText(_message);
		_buttonAlt.SetText(_altText);
		_button.SetText(_okText ?? Language.GetTextValue("tModLoader.OK"));
		bool showAlt = !string.IsNullOrEmpty(_altText);
		_button.Left.Percent = showAlt ? 0 : .25f;
		_area.AddOrRemoveChild(_buttonAlt, showAlt);
	}

	internal void Show(string message, int gotoMenu, UIState gotoState = null, string altButtonText = "", Action altButtonAction = null, string okButtonText = null)
	{
		if (!Program.IsMainThread) {
			// in some cases it would be better to block on this, but in other cases that might be a deadlock. Better to assume that letting the thread continue is the right choice
			Main.QueueMainThreadAction(() => Show(message, gotoMenu, gotoState, altButtonText, altButtonAction, okButtonText));
			return;
		}

		_message = message;
		_gotoMenu = gotoMenu;
		_gotoState = gotoState;
		_altText = altButtonText;
		_altAction = altButtonAction;
		_okText = okButtonText;
		Main.menuMode = Interface.infoMessageID;
		Main.MenuUI.SetState(null); // New SetState code ignores setting to current state, so this is necessary to ensure OnActivate is called.
	}

	private void OKClick(UIMouseEvent evt, UIElement listeningElement)
	{
		HandleBackButtonUsage();
	}

	public void HandleBackButtonUsage()
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		Main.menuMode = _gotoMenu;
		if (_gotoState != null)
			Main.MenuUI.SetState(_gotoState);
	}

	private void AltClick(UIMouseEvent evt, UIElement listeningElement)
	{
		HandleBackButtonUsage();
		_altAction();
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
	}
}
