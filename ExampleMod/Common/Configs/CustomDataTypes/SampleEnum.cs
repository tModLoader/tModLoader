﻿using Terraria.ModLoader.Config;

// This file defines an enum data type that can be used in ModConfig classes.
namespace ExampleMod.Common.Configs.CustomDataTypes
{
	public enum SampleEnum
	{
		Weird,
		Odd,
		// Enum members can be individually labeled as well
		[Label("$Mods.ExampleMod.Configs.Common.SampleEnumLabels.Strange")]
		Strange,
		[Label("$Mods.ExampleMod.Configs.Common.SampleEnumLabels.Peculiar")]
		Peculiar
	}
}
