using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ModLoader.Config;


// This file contains fake ModConfig class that showcase creating config section
// by using fields with defined ranges.

// Because this config was designed to show off various UI capabilities,
// this config have no effect on the mod and provides purely teaching example.
namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	[BackgroundColor(99, 180, 209)]
	public class ModConfigShowcaseRanges : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// With no annotations on a float, a range from 0 to 1 with ticks of 0.01 is the default.
		public float NormalFloat;

		// We can specify range, increments, and even whether or not to draw guide ticks with annotations.
		[Range(2f, 3f)]
		[Increment(.25f)]
		[DrawTicks]
		[DefaultValue(2f)]
		public float IncrementalRangedFloat;

		[Range(0f, 5f)]
		[Increment(.11f)]
		public float IncrementByPoint11;

		[Range(2f, 5f)]
		[DefaultValue(2f)]
		public float RangedFloat;

		// With no annotations on an int, a range from 0 to 100 is the default. Ints will be displayed as a text input unless a Slider attribute is present.
		public int NormalInt;

		[Increment(5)]
		[Range(60, 250)]
		[DefaultValue(100)]
		[Slider] // The Slider attribute makes this field be presented with a slider rather than a text input. The default ticks is 1.
		public int RangedInteger;

		// We can annotate a List<int> and the range, ticks, increment, and slider attributes will be used by all elements of the List.
		// We can use DefaultListValue to set the default value for items added to the list. Using DefaultValue here will crash the game.
		[Range(10, 20)]
		[Increment(2)]
		[DrawTicks]
		[DefaultListValue(16)]
		[Slider]
		public List<int> ListOfInts = new List<int>();

		[Range(-20f, 20f)]
		[Increment(5f)]
		[DrawTicks]
		public Vector2 RangedWithIncrementVector2;

		// A method annotated with OnDeserialized will run after deserialization. You can use it for enforcing things like ranges, since Range and Increment are UI suggestions.
		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context) {
			// RangeAttribute is just a suggestion to the UI. If we want to enforce constraints, we need to validate the data here. Users can edit config files manually with values outside the RangeAttribute, so we fix here if necessary.
			// Both enforcing ranges and not enforcing ranges have uses in mods. Make sure you fix config values if values outside the range will mess up your mod.
			RangedFloat = Utils.Clamp(RangedFloat, 2f, 5f);
		}
	}
}
