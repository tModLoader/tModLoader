using System;
using System.Collections.Generic;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace Terraria.ModLoader;

public static class RarityLoader
{
	public static int RarityCount => ItemRarityID.Count + rarities.Count;
	internal static readonly List<ModRarity> rarities = new List<ModRarity>();

	internal static int Add(ModRarity rarity)
	{
		rarities.Add(rarity);
		return RarityCount - 1;
	}

	internal static void FinishSetup()
	{
		ItemRarity.Initialize();
	}

	public static ModRarity GetRarity(int type)
	{
		return type >= ItemRarityID.Count && type < RarityCount ? rarities[type - ItemRarityID.Count] : null;
	}

	internal static void Unload()
	{
		rarities.Clear();
		ItemRarity.Initialize();
	}
}
