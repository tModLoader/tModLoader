using ExampleMod.Common.Configs.CustomUI;
using Terraria.ModLoader.Config;

// This file contains fake ModConfig class that showcase various attributes
// that can be used to customize behavior config fields.

// Because this config was designed to show off various UI capabilities,
// this config have no effect on the mod and provides purely teaching example.
namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	/// <summary>
	/// This config showcases a completely custom config UI.
	/// </summary>
	[SortConfig(after: true, nameof(ModConfigShowcaseMisc))]
	public class ModConfigShowcaseCustomUI : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public bool SomeBool;
		[Slider]
		public int SomeInt;

		public override void OnLoaded() {
			// Here, we can create our custom UI for the config
			CustomUI = new ExampleCustomConfigUI();
		}
	}
}
