using Microsoft.Xna.Framework;
using System.ComponentModel;

// This file defines custom data type that represents Gradient data type that can be used in ModConfig classes.
namespace ExampleMod.Common.Configs.CustomDataTypes
{
	public class Gradient
	{
		[DefaultValue(typeof(Color), "0, 0, 255, 255")]
		public Color start = Color.Blue; // For sub-objects, you'll want to make sure to set defaults in constructor or field initializer.
		[DefaultValue(typeof(Color), "255, 0, 0, 255")]
		public Color end = Color.Red;

		public override bool Equals(object obj) {
			if (obj is Gradient other)
				return start == other.start && end == other.end;
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return new { start, end }.GetHashCode();
		}
	}
}
