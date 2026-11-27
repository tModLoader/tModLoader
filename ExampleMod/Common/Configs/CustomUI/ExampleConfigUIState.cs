using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;

namespace ExampleMod.Common.Configs.CustomUI;

// TODO: also demo a completely custom UI where IConfigUI is implemented
public class ExampleConfigUIState : UIModConfig
{
	private Color mainColor;
	private Color fadedColor;

	public void Load(Mod modUnused) {
		mainColor = Color.DeepPink;
		fadedColor = mainColor * 0.7f;
	}

	public void Unload() {
	}

	public override void OnActivate() {
		base.OnActivate();

		SetMessage("Hello from Example Mod!", mainColor);
		ColorAllUI();
	}

	// Theme the entire UI (not just the main panel!)
	private void ColorAllUI() {
		headerTextPanel.BackgroundColor = mainColor;

		modNamePanel.DefaultState.PanelColor = fadedColor * 0.5f;
		modNamePanel.DefaultState.BorderColor = fadedColor;

		configSideIndicatorPanel.BackgroundColor = fadedColor * 0.5f;
		configSideIndicatorPanel.BorderColor = fadedColor;

		backButton.DefaultState.PanelColor = fadedColor;
		backButton.DefaultState.HoverPanelColor = mainColor;
		saveConfigButton.AlternateState.PanelColor = fadedColor;
		saveConfigButton.AlternateState.HoverPanelColor = mainColor;
		revertConfigButton.AlternateState.PanelColor = fadedColor;
		revertConfigButton.AlternateState.HoverPanelColor = mainColor;
		restoreDefaultsConfigButton.AlternateState.PanelColor = fadedColor;
		restoreDefaultsConfigButton.AlternateState.HoverPanelColor = mainColor;

		// TODO: subpage UI

		// Currently,
		((UIPanel)listHeaderContainer.Children.First()).BackgroundColor = fadedColor;

		// Currently, not everything can be themed yet. These still need to be done:
		// - text input field
		// - scrollbar
		// - config elements
		// - config notifications
		// - confirm dialog
	}
}
