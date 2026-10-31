using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.DataStructures;

public partial class ArmorSetBonuses
{
	internal static bool ArmorSetsFinishedPopulating = false;

	// New overloads with LocalizedText

	/// <inheritdoc cref="ModItem.CreateArmorSet(LocalizedText, string, ArmorSetBonus.ArmorSetEffect, ArmorSetBonus.PartType)"/>
	public static ArmorSetBonus.Builder Create(string Identifier, LocalizedText LocalizedText, ArmorSetBonus.ArmorSetEffect Effect, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None) => new ArmorSetBonus.Builder(Identifier, LocalizedText, Effect, PrimaryPart);

	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, LocalizedText, string, ArmorSetBonus.ArmorSetEffect, ArmorSetBonus.PartType)"/>
	public static void Add(int Head, int Body, int Legs, string Identifier, LocalizedText LocalizedText, ArmorSetBonus.ArmorSetEffect Effect, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None)
	{
		Create(Identifier, LocalizedText, Effect, PrimaryPart).Set(Head, Body, Legs).Add();
	}

	/// <inheritdoc cref="ModItem.AddArmorSet(int, int, int, LocalizedText, string, ArmorSetBonus.ArmorSetEffect, ArmorSetBonus.PartType)"/>
	public static void Add<THead, TBody, TLegs>(string Identifier, LocalizedText LocalizedText, ArmorSetBonus.ArmorSetEffect Effect, ArmorSetBonus.PartType PrimaryPart = ArmorSetBonus.PartType.None)
		where THead : ModItem
		where TBody : ModItem
		where TLegs : ModItem
	{
		Create(Identifier, LocalizedText, Effect, PrimaryPart).Set<THead, TBody, TLegs>().Add();
	}

	internal static void Unload()
	{
		ArmorSetsFinishedPopulating = false;
		All.Clear();
	}
}
