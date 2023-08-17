using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace Terraria.ModLoader.UI;

internal class UIForcedDelayInfoMessage : UIInfoMessage
{
	int timeLeft = -1;
	private UITextPanel<string> waitPanel;

	internal void Delay(int seconds)
	{
		Main.menuMode = Interface.infoMessageDelayedID;
		if (timeLeft == -1)
			timeLeft = seconds * 60;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		waitPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.WaitXSeconds", timeLeft / 60), 0.7f, true) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 50 },
			Left = { Percent = 0 },
			VAlign = 1f,
			Top = { Pixels = -30 },
			BackgroundColor = Color.Orange
		};
	}

	public override void OnActivate()
	{
		base.OnActivate();

		_area.AddOrRemoveChild(_button, false);
		_area.Append(waitPanel);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (timeLeft > 0) {
			timeLeft--;
			waitPanel.SetText(Language.GetTextValue("tModLoader.WaitXSeconds", timeLeft / 60 + 1));

			if (timeLeft == 0) {
				// Leftover from 1.4.3-Legacy to 1.4.4 transition
				//ModLoader.SeenMigrateTo143BranchMessage = true; // altbutton will trigger the state refresh, unfortunately.

				_area.AddOrRemoveChild(waitPanel, false);
				_area.AddOrRemoveChild(_button, true);
			}
		}
	}
}
