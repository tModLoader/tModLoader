using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public class AltCopper
	{
		public readonly int oreTileType;
		public readonly int oreBarItemType;
		private static readonly List<AltCopper> AltCoppers = new List<AltCopper>();

		public static AltCopper ChosenAltCopper;

		public AltCopper(int oreTileType, int oreBarItemType)
		{
			this.oreTileType = TileLoader.TileCount > oreTileType ? oreTileType : throw new Exception("oreTileType is an invalid tile");
			this.oreBarItemType = ItemLoader.ItemCount > oreBarItemType ? oreBarItemType : throw new Exception("oreBarItemType is an invalid item");
		}

		internal static AltCopper GetAltCopper()
		{
			return ChosenAltCopper ?? (ChosenAltCopper = AltCoppers[WorldGen.genRand.Next(AltCoppers.Count)]);
		}

		internal static void Add(AltCopper alt)
		{
			AltCoppers.Add(alt);
		}
	}
}