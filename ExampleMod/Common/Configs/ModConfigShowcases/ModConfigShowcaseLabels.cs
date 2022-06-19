using System.Collections.Generic;
using Terraria.ModLoader.Config;
using ExampleMod.Common.Configs.CustomDataTypes;

// This file contains fake ModConfig class that showcase making config fields more readable
// with use of labels, headers and tooltips.

// Because this config was designed to show off various UI capabilities,
// this config have no effect on the mod and provides purely teaching example.
namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	[BackgroundColor(154, 152, 181)]
	[Label("ModConfig Showcase C: Labels")]
	public class ModConfigShowcaseLabels : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Without a Label attribute, config items will display as field identifiers. Using Label will make your Config appealing.
		// Use Tooltip to convey additional information about the config item.
		[Label("This is a float")]
		[Tooltip("This text will show when hovered")]
		[SliderColor(255, 0, 127)]
		public float SomeFloat;

		// Using localization keys will help make your config readable in multiple languages. See ExampleMod/Localization/en-US.lang
		[Label("$Mods.ExampleMod.Common.LocalizedLabel")]
		[Tooltip("$Mods.ExampleMod.Common.LocalizedTooltip")]
		public int LocalizedLabel;

		// TODO: Not working. Code missing from ExampleMod.cs
		[Label("$Mods.ExampleMod.Common.LocalizedLabelDynamic")]
		public int LocalizedLabelDynamic;

		// The color of the config entry can be customized. R, G, B
		[BackgroundColor(255, 0, 255)]
		public Pair pairExample = new Pair();

		// List elements also inherit BackgroundColor
		[BackgroundColor(255, 0, 0)]
		public List<Pair> ListOfPair = new List<Pair>();

		// We can also add section headers, separating fields for organization
		[Header("Headers Section")]
		public int Header;

		[Header("$Mods.ExampleMod.Common.LocalizedHeader")]
		public int LocalizedHeader;

		[Header("[i:19][c/00FF00:Green Text][i:ExampleMod/ExampleItem]")]
		public int CoolHeader;

		// The class declaration of SimpleData specifies [BackgroundColor(255, 7, 7)]. Field and data structure field annotations override class annotations.
		[BackgroundColor(85, 107, 47)]
		[Label("OverridenColor SimpleData")]
		public SimpleData simpleDataExample2 = new SimpleData();
	}
}
