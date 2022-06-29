using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Terraria.ModLoader.Config;

// This file defines custom data type that contains variety of other data types and can be used in ModConfig classes.
namespace ExampleMod.Common.Configs.CustomDataTypes
{
	public class ComplexData
	{
		public List<int> ListOfInts = new List<int>();

		public SimpleData nestedSimple = new SimpleData();

		[Range(2f, 3f)]
		[Increment(.25f)]
		[DrawTicks]
		[DefaultValue(2f)]
		public float IncrementalFloat = 2f;
		public override bool Equals(object obj) {
			if (obj is ComplexData other)
				return ListOfInts.SequenceEqual(other.ListOfInts) && IncrementalFloat == other.IncrementalFloat && nestedSimple.Equals(other.nestedSimple);
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return new { ListOfInts, nestedSimple, IncrementalFloat }.GetHashCode();
		}
	}
}
