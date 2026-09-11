using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.DataStructures;

public partial class ArmorSetBonuses
{
	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, string, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static void Add(int Head, int Body, int Legs, string TextKey, ArmorSetBonus.PartType PrimaryPart, string Identifier, ArmorSetBonus.ArmorSetEffect Effect)
	{
		Create(Effect, TextKey, PrimaryPart, Identifier).Set(Head, Body, Legs).Add();
	}

	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, string, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static void Add<THead, TBody, TLegs>(string TextKey, ArmorSetBonus.PartType PrimaryPart, string Identifier, ArmorSetBonus.ArmorSetEffect Effect)
		where THead : ModItem
		where TBody : ModItem
		where TLegs : ModItem
	{
		Create(Effect, TextKey, PrimaryPart, Identifier).Set<THead, TBody, TLegs>().Add();
	}

	/// <inheritdoc cref="ModLoader.ModItem.CreateArmorSet(LocalizedText, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static ArmorSetBonus.Builder Create(ArmorSetBonus.ArmorSetEffect Effect, LocalizedText LocalizedText, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None, string Identifier = null) => ArmorSetBonus.Create(Effect, LocalizedText, PrimaryPart, Identifier);

	// New overloads with LocalizedText

	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, LocalizedText, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static void Add(int Head, int Body, int Legs, LocalizedText LocalizedText, ArmorSetBonus.PartType PrimaryPart, string Identifier, ArmorSetBonus.ArmorSetEffect Effect)
	{
		Create(Effect, LocalizedText, PrimaryPart, Identifier).Set(Head, Body, Legs).Add();
	}

	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, LocalizedText, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static void Add<THead, TBody, TLegs>(LocalizedText LocalizedText, ArmorSetBonus.PartType PrimaryPart, string Identifier, ArmorSetBonus.ArmorSetEffect Effect)
		where THead : ModItem
		where TBody : ModItem
		where TLegs : ModItem
	{
		Create(Effect, LocalizedText, PrimaryPart, Identifier).Set<THead, TBody, TLegs>().Add();
	}

	private static void AssignKeysToVanillaArmorSets()
	{
		foreach (var armorSetBonus in All) {
			armorSetBonus.Identifier = armorSetBonus.Description.Key.Split(".").Last();
		}
	}

	internal static void Unload()
	{
		All.Clear();
	}
}
