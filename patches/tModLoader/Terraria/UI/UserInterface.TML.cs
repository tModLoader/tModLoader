using Microsoft.Xna.Framework;
using System;

#nullable enable

namespace Terraria.UI;

partial class UserInterface
{
	private struct ButtonData
	{
		public bool IsDown; // The value may be lying, this field is used like a local.
		public bool WasDown;
		public double LastDownTime;
		public UIElement? LastElementDown;
		public UIElement? LastElementClicked;

		public void ResetState()
		{
			LastDownTime = 0d;

			ResetLasts();
		}

		public void ResetLasts()
		{
			LastElementDown = null;
			LastElementClicked = null;
		}
	}

	private ButtonData mouseRightData;
	private ButtonData mouseMiddleData;
	private ButtonData mouseXButton1Data;
	private ButtonData mouseXButton2Data;

	// TODO: This code is a recent rewrite of something really old, ensure that everything's working.
	private void UpdateButtonState(GameTime time, UIElement uiElement, bool clicksDisabled, ref ButtonData data, Action<UIMouseEvent> downAction, Action<UIMouseEvent> upAction, Action<UIMouseEvent> clickAction, Action<UIMouseEvent> doubleClickAction)
	{
		if (clicksDisabled) {
			return;
		}

		if (data.IsDown && !data.WasDown && uiElement != null) {
			data.LastElementDown = uiElement;

			downAction(new UIMouseEvent(uiElement, MousePosition));

			if (data.LastElementClicked == uiElement && time.TotalGameTime.TotalMilliseconds - data.LastDownTime < 500.0) {
				doubleClickAction(new UIMouseEvent(uiElement, MousePosition));

				data.LastElementClicked = null;
			}

			data.LastDownTime = time.TotalGameTime.TotalMilliseconds;
		}
		else if (!data.IsDown && data.WasDown && data.LastElementDown != null && !clicksDisabled) {
			var lastElementDown = data.LastElementDown;

			if (lastElementDown.ContainsPoint(MousePosition)) {
				clickAction(new UIMouseEvent(lastElementDown, MousePosition));

				data.LastElementClicked = lastElementDown;
			}

			upAction(new UIMouseEvent(lastElementDown, MousePosition));

			data.LastElementDown = null;
		}
	}
}
