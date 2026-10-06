using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.DataStructures;

public partial class ArmorSetBonuses
{
	internal static bool ArmorSetsFinishedPopulating = false;

	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, string, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static void Add(int Head, int Body, int Legs, string Identifier, string TextKey, ArmorSetBonus.ArmorSetEffect Effect, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None)
	{
		Create(Identifier, TextKey, Effect, PrimaryPart).Set(Head, Body, Legs).Add();
	}

	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, string, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static void Add<THead, TBody, TLegs>(string Identifier, string TextKey, ArmorSetBonus.ArmorSetEffect Effect, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None)
		where THead : ModItem
		where TBody : ModItem
		where TLegs : ModItem
	{
		Create(Identifier, TextKey, Effect, PrimaryPart).Set<THead, TBody, TLegs>().Add();
	}

	/// <inheritdoc cref="ModLoader.ModItem.CreateArmorSet(LocalizedText, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static ArmorSetBonus.Builder Create(string Identifier, LocalizedText LocalizedText, ArmorSetBonus.ArmorSetEffect Effect, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None) => ArmorSetBonus.Create(Identifier, LocalizedText, Effect, PrimaryPart);

	public static ArmorSetBonus.Builder Create(string Identifier, string TextKey, ArmorSetBonus.ArmorSetEffect Effect, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None) => ArmorSetBonus.Create(Identifier, TextKey, Effect, PrimaryPart);

	// New overloads with LocalizedText

	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, LocalizedText, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static void Add(int Head, int Body, int Legs, string Identifier, LocalizedText LocalizedText, ArmorSetBonus.ArmorSetEffect Effect, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None)
	{
		Create(Identifier, LocalizedText, Effect, PrimaryPart).Set(Head, Body, Legs).Add();
	}

	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, LocalizedText, ArmorSetBonus.PartType, string, ArmorSetBonus.ArmorSetEffect)"/>
	public static void Add<THead, TBody, TLegs>(string Identifier, LocalizedText LocalizedText, ArmorSetBonus.ArmorSetEffect Effect, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None)
		where THead : ModItem
		where TBody : ModItem
		where TLegs : ModItem
	{
		Create(Identifier, LocalizedText, Effect, PrimaryPart).Set<THead, TBody, TLegs>().Add();
	}

	private static void AssignKeysToVanillaArmorSets()
	{
		foreach (var armorSetBonus in All) {
			//armorSetBonus.Identifier = armorSetBonus.Description.Key.Split(".").Last();
		}
	}

	internal static void Unload()
	{
		ArmorSetsFinishedPopulating = false;
		All.Clear();
	}
}
