using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader;

public static class BiomeConversionLoader
{
	internal static int BiomeConversionCount { get; private set; } = BiomeConversionID.Count;
	internal static readonly IList<ModBiomeConversion> conversions = new List<ModBiomeConversion>();

	/// <summary>
	/// Gets the ModBiomeConversion instance with the given type. Returns null if no ModBiomeConversion with the given type exists.
	/// </summary>
	public static ModBiomeConversion GetBiomeConversion(int type)
		=> type >= BiomeConversionID.Count && type < BiomeConversionCount ? conversions[type - BiomeConversionID.Count] : null;


	internal static void ResizeArrays()
	{
		//Nothing for now
	}

	internal static void PostSetupContent()
	{
		foreach (ModBiomeConversion conversion in conversions) {
			conversion.PostSetupContent();
		}
	}

	internal static void Unload()
	{
		conversions.Clear();
		BiomeConversionCount = BiomeConversionID.Count;
	}

	public static int Register(ModBiomeConversion conversion)
	{
		int type = BiomeConversionCount++;
		conversions.Add(conversion);
		return type;
	}
}
