using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config.UI;

namespace ExampleMod.Common.Configs.CustomUI;

// TODO: also demo a completely custom UI where IConfigUI is implemented
public class ExampleConfigUIState : UIModConfig
{
	public override void OnActivate() {
		base.OnActivate();

		SetMessage("Hello from Example Mod!", Color.DeepPink);
	}
}
