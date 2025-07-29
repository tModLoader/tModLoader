using ExampleMod.Common.Configs.CustomDataTypes;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;

// This file contains fake ModConfig class that showcase defining subpages
// that can be used to separate config section into sub-configs for easier management.

// Because this config was designed to show off various UI capabilities,
// this config have no effect on the mod and provides purely teaching example.
namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	[BackgroundColor(148, 72, 188)]
	public class ModConfigShowcaseSubpages : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Header("SeparatePageExamples")]
		// Using SeparatePage, an object will be presented to the user as a button. That button will lead to a separate page where the usual UI will be presented. Useful for organization.
		[SeparatePage]
		public Gradient gradient = new Gradient();

		// This example has multiple levels of subpages, check it out. In this example, the SubConfigExample class itself is annotated with [SeparatePage]
		public SubConfigExample subConfigExample = new SubConfigExample();

		[SeparatePage]
		public Dictionary<ItemDefinition, SubConfigExample> DictionaryofSubConfigExample = new Dictionary<ItemDefinition, SubConfigExample>();

		// These 2 examples show how [SeparatePage] works on annotating both a field for a class and annotating a List of a class
		[SeparatePage]
		public List<Pair> SeparateListOfPairs = new List<Pair>();

		[SeparatePage]
		public Pair pair = new Pair();

		// C# allows inner classes (used here), which might be useful for organization if you want.
		[SeparatePage]
		public class SubConfigExample
		{
			[DefaultValue(99)]
			public int boost = 99;
			public float percent;
			public bool enabled;

			[SeparatePage]
			[BackgroundColor(50, 200, 100)]
			public SubSubConfigExample SubA = new SubSubConfigExample();

			[SeparatePage]
			public SubSubConfigExample SubB = new SubSubConfigExample();

			public override string ToString() {
				return $"{boost} {percent} {enabled} {SubA.whoa}/{SubB.whoa}";
			}

			public override bool Equals(object obj) {
				if (obj is SubConfigExample other)
					return boost == other.boost && percent == other.percent && enabled == other.enabled && SubA.Equals(other.SubA) && SubB.Equals(other.SubB);
				return base.Equals(obj);
			}

			public override int GetHashCode() {
				return new { boost, percent, enabled, SubA, SubB }.GetHashCode();
			}
		}

		public class SubSubConfigExample
		{
			public int whoa;
			public override bool Equals(object obj) {
				if (obj is SubSubConfigExample other)
					return whoa == other.whoa;
				return base.Equals(obj);
			}

			public override int GetHashCode() => whoa.GetHashCode();
		}
	}
}
