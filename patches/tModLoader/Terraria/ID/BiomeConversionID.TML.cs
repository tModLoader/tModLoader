using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.ID;

public static partial class BiomeConversionID
{
	internal static readonly Dictionary<string, int> nameToId = new();
	internal static readonly Dictionary<int, string> idToName = new();

	internal static int nextConversion = Count;
	/// <summary>
	/// Gives the total amount of biome conversions, including modded ones
	/// </summary>
	public static int BiomeConversionCount => nextConversion;

	public static void Unload()
	{
		nameToId.Clear();
		idToName.Clear();
		nextConversion = Count;
	}

	public static int RegisterBiomeConversionID(Mod mod, string conversionName)
	{
		if (!mod.loading)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorLoadOnlyMethod", "RegisterBiomeConversionID"));

		string conversionFullName = $"{mod.Name}/{conversionName}";
		if (nameToId.ContainsKey(conversionFullName))
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorDuplicateName", "BiomeConversionID", conversionFullName));

		nameToId[conversionFullName] = nextConversion;
		return nextConversion++;
	}

	/// <summary>
	/// Attempts to retrieve the modded conversion ID associated with the given name from the specified mod. Caching the result is recommended.<br/>
	/// </summary>
	/// <param name="mod">Mod the conversion was registered from</param>
	/// <param name="conversionName">Name of the registered conversion</param>
	/// <param name="conversionID">The conversion ID matching the mod and name provided (if it was found)</param>
	/// <returns>Whether or not a conversion was found with the matching mod and name</returns>
	public static bool TryGetConversionID(Mod mod, string conversionName, out int conversionID) => TryGetConversionID(mod.Name, conversionName, out conversionID);

	/// <summary>
	/// Attempts to retrieve the modded conversion ID associated with the given name and mod name. Caching the result is recommended.<br/>
	/// </summary>
	/// <param name="modName">Name of the mod the conversion was registered from</param>
	/// <param name="conversionName">Name of the registered conversion</param>
	/// <param name="conversionID">The conversion ID matching the mod and name provided (if it was found)</param>
	/// <returns>Whether or not a conversion was found with the matching mod and name</returns>
	public static bool TryGetConversionID(string modName, string conversionName, out int conversionID)
	{
		string conversionFullName = $"{modName}/{conversionName}";
		return TryGetConversionID(conversionFullName, out conversionID);
	}

	/// <summary>
	/// Attempts to retrieve the modded conversion ID associated with the given full name. Caching the result is recommended.<br/>
	/// </summary>
	/// <param name="conversionFullName">The full name of the registered conversion ("ModName/ConversionName")</param>
	/// <param name="conversionID">The conversion ID matching the mod and name provided (if it was found)</param>
	/// <returns>Whether or not a conversion was found with the matching full name</returns>
	public static bool TryGetConversionID(string conversionFullName, out int conversionID)
	{
		return nameToId.TryGetValue(conversionFullName, out conversionID);
	}

	/// <summary>
	/// Attempts to find the full name for the given modded conversion ID. Vanilla conversion IDs and invalid IDs will return false<br/>
	/// </summary>
	/// <param name="conversionID">The biome conversion ID to find the full name for</param>
	/// <param name="conversionName">The conversion name associated to this ID (if found)</param>
	/// <returns>Whether or not a modded conversion name was found with the matching ID</returns>
	public static bool TryGetConversionName(int conversionID, out string conversionName)
	{
		conversionName = "";
		//Vanilla conversions
		if (conversionID < Count)
			return false;
		return idToName.TryGetValue(conversionID, out conversionName);
	}
}
