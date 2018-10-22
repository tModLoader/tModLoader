using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public class AltIron
	{
		public readonly int oreTileType;
		public readonly int oreBarItemType;
		private static readonly List<AltIron> AltIrons = new List<AltIron>();

		public static AltIron ChosenAltIron { get; private set; }

		public AltIron(int oreTileType, int oreBarItemType)
		{
			this.oreTileType = TileLoader.TileCount > oreTileType ? oreTileType : throw new Exception("oreTileType is an invalid tile");
			this.oreBarItemType = ItemLoader.ItemCount > oreBarItemType ? oreBarItemType : throw new Exception("oreBarItemType is an invalid item");
		}

		internal static AltIron GetAltIron()
		{
			return ChosenAltIron ?? (ChosenAltIron = AltIrons[WorldGen.genRand.Next(AltIrons.Count)]);
		}

		internal static void UnchooseAltIron()
		{
			ChosenAltIron = null;
		}

		internal static void Add(AltIron alt)
		{
			AltIrons.Add(alt);
		}
	}
}