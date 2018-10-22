using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public class AltGold
	{
		public readonly int oreTileType;
		public readonly int oreBarItemType;
		private static readonly List<AltGold> AltGolds = new List<AltGold>();

		public static AltGold ChosenAltGold { get; private set; }

		public AltGold(int oreTileType, int oreBarItemType)
		{
			this.oreTileType = TileLoader.TileCount > oreTileType ? oreTileType : throw new Exception("oreTileType is an invalid tile");
			this.oreBarItemType = ItemLoader.ItemCount > oreBarItemType ? oreBarItemType : throw new Exception("oreBarItemType is an invalid item");
		}

		internal static AltGold GetAltGold()
		{
			return ChosenAltGold ?? (ChosenAltGold = AltGolds[WorldGen.genRand.Next(AltGolds.Count)]);
		}

		internal static void UnchooseAltGold()
		{
			ChosenAltGold = null;
		}

		internal static void Add(AltGold alt)
		{
			AltGolds.Add(alt);
		}
	}
}