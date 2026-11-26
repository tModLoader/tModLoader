using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;

namespace ExampleMod.Common.Configs.CustomUI;

// TODO: also demo a completely custom UI where IConfigUI is implemented
// TODO: for this, just make everything pink
public class ExampleConfigUIState : UIModConfig, ILoadable
{
	public override void OnActivate() {
		base.OnActivate();

		SetMessage("Hello from Example Mod!", Color.DeepPink);
	}

	public void Load(Mod modUnused) {
	}

	public void Unload() {
	}
}
