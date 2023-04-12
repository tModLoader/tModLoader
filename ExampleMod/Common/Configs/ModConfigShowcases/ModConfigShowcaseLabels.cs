using System.Collections.Generic;
using Terraria.ModLoader.Config;
using ExampleMod.Common.Configs.CustomDataTypes;
using Terraria.ID;

// This file contains fake ModConfig class that showcase making config fields more readable
// with use of labels, headers and tooltips.

// Because this config was designed to show off various UI capabilities,
// this config have no effect on the mod and provides purely teaching example.
namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	[BackgroundColor(154, 152, 181)]
	public class ModConfigShowcaseLabels : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// By default, all ModConfig fields and properties will have an automatically assigned Label and Tooltip translation key. You'll find these translation keys in your translation files. All of the English translations for the configs in ExampleMod are found in ExampleMod/Localization/en-US_Mods.ExampleMod.Configs.hjson

		// Use Tooltip to convey additional information about the config item.
		// This example shows additional text when hovered.
		[SliderColor(255, 0, 127)]
		public float SomeFloat;

		// Modders can pass in custom localization keys. This can be useful for reusing translations.
		[Label("$Mods.ExampleMod.Configs.Common.LocalizedLabel")]
		[Tooltip("$Mods.ExampleMod.Configs.Common.LocalizedTooltip")]
		public int LocalizedLabel;

		// TODO: Not working. Code missing from ExampleMod.cs
		[Label("$Mods.ExampleMod.Configs.Common.LocalizedLabelDynamic")]
		public int LocalizedLabelDynamic;

		// These 2 examples showcase the power of interpolating values into the translations.
		// Note how both are using the same label key, but are interpolating different values into the label translation, resulting in different text. The same is done for tooltips.
		// Use this approach to reduce unnecessary duplication of text.
		// Note: using nameof can help avoid typos and errors. That would look like: $"$Mods.ExampleMod.Items.{nameof(ExampleYoyo)}.DisplayName"
		// Note: These examples use color and item chat tags. See here for help on using Tags: https://terraria.wiki.gg/wiki/Chat#Tags
		const string InterpolatedLabel = "$Mods.ExampleMod.Configs.Common.InterpolatedLabel";
		const string InterpolatedTooltip = "$Mods.ExampleMod.Configs.Common.InterpolatedTooltip";

		[Label(InterpolatedLabel), Tooltip(InterpolatedTooltip)] // Attributes can also be combined into a single line
		[LabelArgs("ExampleMod/ExampleYoyo", 123, "=>", "$Mods.ExampleMod.Items.ExampleYoyo.DisplayName")]
		[TooltipArgs("$Mods.ExampleMod.Items.ExampleYoyo.DisplayName", "FF55AA", "22a2dd")]
		public bool InterpolatedTextA;

		[Label(InterpolatedLabel), Tooltip(InterpolatedTooltip)]
		[LabelArgs("ExampleMod/ExampleSword", 456, "=>", "$Mods.ExampleMod.Items.ExampleSword.DisplayName")]
		[TooltipArgs("$Mods.ExampleMod.Items.ExampleSword.DisplayName", "77bd8e", "88AADD")]
		public bool InterpolatedTextB;

		[Label(InterpolatedLabel), Tooltip(InterpolatedTooltip)]
		[LabelArgs(ItemID.Meowmere, 789, "=>", $"$ItemName.{nameof(ItemID.Meowmere)}")]
		[TooltipArgs($"$ItemName.{nameof(ItemID.Meowmere)}", "c441c6", "deeb55")]
		public bool InterpolatedTextC;

		// The color of the config entry can be customized. R, G, B
		[BackgroundColor(255, 0, 255)]
		// The corresponding tooltip translation for this entry is empty, so the tooltip shown will be from the Pair class.
		// If the Pair class tooltip had entries for arguments, we could use [TooltipArgs] here to customize it.
		public Pair pairExample = new Pair();

		// List elements also inherit BackgroundColor
		[BackgroundColor(255, 0, 0)]
		public List<Pair> ListOfPair = new List<Pair>();

		// We can also add section headers, separating fields for organization
		// Using [Header("HeaderIdentifier")], Mods.ExampleMod.Configs.ModConfigShowcaseLabels.Headers.HeaderIdentifier will automatically appear in localization files. We have populated the English entry with the value "Headers Section".
		[Header("HeaderIdentifier")]
		public int TypicalHeader;

		// We can also specify a specific translation key, if desired.
		// The "$" character before a name means it should interpret the value as a translation key and use the loaded translation with the same key.
		[Header("$Mods.ExampleMod.Configs.Common.LocalizedHeader")]
		public int LocalizedHeader;

		// Chat tags such as colored text or item icons can help users find config sections quickly
		[Header("ChatTagExample")]
		public int CoolHeader;

		// The class declaration of SimpleData specifies [BackgroundColor(255, 7, 7)]. Field and data structure field annotations override class annotations.
		[BackgroundColor(85, 107, 47)]
		public SimpleData simpleDataExample2 = new SimpleData();
	}
}
