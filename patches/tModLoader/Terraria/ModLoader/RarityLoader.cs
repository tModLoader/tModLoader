using System;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class RarityLoader
	{
		public static int RarityCount { get; private set; } = ItemRarityID.Count;

		internal static readonly List<ModRarity> rarities = new List<ModRarity>();

		internal static void Add(ModRarity rarity) {
			rarities.Add(rarity);
		}

		internal static int ReserveRarityID() {
			if (ModNet.AllowVanillaClients)
				throw new Exception("Adding rarities breaks vanilla client compatibility");

			int reserveID = RarityCount;
			RarityCount++;
			return reserveID;
		}

		internal static ModRarity GetRarity(int type) {
			return type >= ItemRarityID.Count && type < RarityCount ? rarities[type - ItemRarityID.Count] : null;
		}

		internal static void Unload() {
			rarities.Clear();
			RarityCount = ItemRarityID.Count;
		}
	}
}
