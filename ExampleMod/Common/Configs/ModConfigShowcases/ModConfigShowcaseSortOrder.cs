using Terraria.ModLoader.Config;

// This file contains fake ModConfig class that showcase creating config section
// by using fields with defined ranges.

// Because this config was designed to show off various UI capabilities,
// this config have no effect on the mod and provides purely teaching example.
namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	// Sort this config to be after ModConfigShowcaseAcceptClientChanges
	[ConfigSort(sortAfter: true, nameof(ModConfigShowcaseAcceptClientChanges))]
	public class ModConfigShowcaseSortOrder : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Empty, since we are only demonstrating how to change the config sort order
	}
}
