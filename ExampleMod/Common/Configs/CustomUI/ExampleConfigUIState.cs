using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config.UI;

namespace ExampleMod.Common.Configs.CustomUI;

// TODO: also demo a completely custom UI where IConfigUI is implemented
public class ExampleConfigUIState : UIModConfig
{
	// TODO: actually change stuff

	public override void OnActivate() {
		base.OnActivate();

		modNamePanel.DefaultState.PanelColor = Color.Red;
	}
}
