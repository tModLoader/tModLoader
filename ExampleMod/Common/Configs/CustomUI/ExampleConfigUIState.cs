using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;

namespace ExampleMod.Common.Configs.CustomUI;

// TODO: also demo a completely custom UI where IConfigUI is implemented
// TODO: for this, just make everything pink
public class ExampleConfigUIState : UIModConfig, ILoadable
{
	private Color rootColor = Color.DeepPink;

	public void Load(Mod modUnused) {
	}

	public void Unload() {
	}

	public override void OnActivate() {
		base.OnActivate();

		SetMessage("Hello from Example Mod!", rootColor);
		ColorAllUI(); // Theme the entire UI
	}

	private void ColorAllUI() {
		headerTextPanel.BackgroundColor = rootColor;
	}
}
