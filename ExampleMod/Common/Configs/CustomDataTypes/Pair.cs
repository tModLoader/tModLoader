using Terraria.ModLoader.Config;

// This file defines custom simple data type with two fields - boolean value and integer value.
namespace ExampleMod.Common.Configs.CustomDataTypes
{
	[BackgroundColor(0, 255, 255)]
	public class Pair
	{
		public bool enabled;
		public int boost;

		// If you override ToString, it will show up appended to the Label in the ModConfig UI.
		public override string ToString() {
			return $"Boost: {(enabled ? "" + boost : "disabled")}";
		}

		// Implementing Equals and GetHashCode are critical for any classes you use.
		public override bool Equals(object obj) {
			if (obj is Pair other)
				return enabled == other.enabled && boost == other.boost;
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return new { boost, enabled }.GetHashCode();
		}
	}
}
