using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.ID;

public static partial class BiomeConversionID
{
	internal static readonly Dictionary<string, int> nameToId = new();
	private static readonly Dictionary<string, Dictionary<string, int>> tieredDict = new();
	internal static readonly Dictionary<int, string> idToName = new();

	internal static int nextConversion = Count;
	/// <summary>
	/// Gives the total amount of biome conversions, including modded ones
	/// </summary>
	public static int BiomeConversionCount => nextConversion;

	public static int RegisterBiomeConversionID(Mod mod, string conversionName)
	{
		if (!mod.loading)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorLoadOnlyMethod", "RegisterBiomeConversionID"));

		string conversionFullName = $"{mod.Name}/{conversionName}";
		if (nameToId.ContainsKey(conversionFullName))
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorDuplicateName", "BiomeConversionID", conversionFullName));

		nameToId[conversionFullName] = nextConversion;
		if (!tieredDict.TryGetValue(mod.Name, out var subDictionary))
			tieredDict[mod.Name] = subDictionary = new();

		subDictionary[conversionName] = nextConversion;

		return nextConversion++;
	}

	/// <summary>
	/// Attempts to retrieve the modded conversion ID associated with the given name and mod name. Caching the result is recommended.<br/>
	/// Returns -1 if no conversion was found registered under that name.
	/// </summary>
	/// <param name="modName">Name of the mod the conversion was registered from</param>
	/// <param name="conversionName">Name of the registered conversion</param>
	/// <returns>The numerical ID of the biome conversion using the given name and mod name</returns>
	public static int TryGetConversionID(string modName, string conversionName)
	{
		if (!tieredDict.TryGetValue(modName, out var subDictionary))
			return -1;

		return subDictionary.TryGetValue(conversionName, out int conversionID) ? conversionID : -1;
	}

	/// <summary>
	/// Attempts to retrieve the modded conversion ID associated with the given name from the specified mod. Caching the result is recommended.<br/>
	/// Returns -1 if no conversion was found registered under that name.
	/// </summary>
	/// <param name="mod">Mod the conversion was registered from</param>
	/// <param name="conversionName">Name of the registered conversion</param>
	/// <returns>The numerical ID of the biome conversion using the given name and mod name</returns>
	public static int TryGetConversionID(Mod mod, string conversionName)
	{
		if (!tieredDict.TryGetValue(mod.Name, out var subDictionary))
			return -1;

		return subDictionary.TryGetValue(conversionName, out int conversionID) ? conversionID : -1;
	}

	/// <summary>
	/// Attempts to retrieve the modded conversion ID associated with the given full name. Caching the result is recommended.<br/>
	/// Returns -1 if no conversion was found registered under that name.
	/// </summary>
	/// <param name="conversionFullName">The full name of the registered conversion ("ModName/ConversionName")</param>
	/// <returns>The numerical ID of the biome conversion using the given name</returns>
	public static int TryGetConversionID(string conversionFullName)
	{
		return nameToId.TryGetValue(conversionFullName, out int conversionID) ? conversionID : -1;
	}

	/// <summary>
	/// Attempts to find the full name for the given modded conversion ID. Vanilla conversion IDs and invalid IDs will return an empty string<br/>
	/// </summary>
	/// <param name="conversionID">The biome conversion ID to find the full name for</param>
	/// <returns>The full name of the conversion ID ("ModName/ConversionName")</returns>
	public static string GetConversionName(int conversionID)
	{
		//Vanilla conversions
		if (conversionID < Count)
			return "";

		return idToName.TryGetValue(conversionID, out string conversionName) ? conversionName : "";
	}
}
