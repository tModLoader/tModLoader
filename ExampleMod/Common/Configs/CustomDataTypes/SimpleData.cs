using System.ComponentModel;
using Terraria.ModLoader.Config;

// This file defines custom data type that contains variety of simple data types
// and can be used in ModConfig classes.
namespace ExampleMod.Common.Configs.CustomDataTypes
{
	[BackgroundColor(255, 7, 7)]
	public class SimpleData
	{
		// These Headers annotate members in a non-ModConfig class, so their Header translation keys need to be manually specified and added to the localization files. 
		[Header("$Mods.ExampleMod.Configs.Common.SimpleData.boost.Header")]
		public int boost;
		public float percent;

		[Header("$Mods.ExampleMod.Configs.Common.SimpleData.enabled.Header")]
		public bool enabled;

		[DrawTicks]
		[OptionStrings(new string[] { "Pikachu", "Charmander", "Bulbasaur", "Squirtle" })]
		[DefaultValue("Bulbasaur")]
		public string FavoritePokemon;

		public SimpleData() {
			FavoritePokemon = "Bulbasaur";
		}

		public override bool Equals(object obj) {
			if (obj is SimpleData other)
				return boost == other.boost && percent == other.percent && enabled == other.enabled && FavoritePokemon == other.FavoritePokemon;
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return new { boost, percent, enabled, FavoritePokemon }.GetHashCode();
		}
	}
}
