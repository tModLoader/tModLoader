using ExampleMod.Common.Configs.ModConfigShowcases;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace ExampleMod.Common.Configs.CustomUI
{
	/// <summary>
	/// This is a completely custom UI for ModConfigShowcaseCustomUI that doesn't use UIModConfig.
	/// See ModConfigShowcaseCustomUI for how it is set.
	/// </summary>
	public class ExampleCustomConfigUI : UIState, IConfigUI
	{
		// These 2 fields are required by IConfigUI, but are unused by the UI
		public int UpdateCount { get; }
		public string ConfigElementTooltip { get; set; }

		private ModConfigShowcaseCustomUI config; // The actual config
		private ModConfigShowcaseCustomUI pendingConfig; // The clone we make changes to

		public override void OnInitialize() {
			CreateUI();
		}

		public override void OnActivate() {
			config = ModContent.GetInstance<ModConfigShowcaseCustomUI>();
			pendingConfig = (ModConfigShowcaseCustomUI)ConfigManager.GeneratePopulatedClone(config);

			RefreshUI();
		}

		private void CreateUI() {
			// TODO
		}

		private void RefreshUI() {
			// TODO: just manually add some ConfigElements to the screen
		}
	}
}
