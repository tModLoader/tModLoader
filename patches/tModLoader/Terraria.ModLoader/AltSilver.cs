using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public class AltSilver
	{
		public readonly int oreTileType;
		public readonly int oreBarItemType;
		private static readonly List<AltSilver> AltSilvers = new List<AltSilver>();

		public static AltSilver ChosenAltSilver;

		public AltSilver(int oreTileType, int oreBarItemType)
		{
			this.oreTileType = TileLoader.TileCount > oreTileType ? oreTileType : throw new Exception("oreTileType is an invalid tile");
			this.oreBarItemType = ItemLoader.ItemCount > oreBarItemType ? oreBarItemType : throw new Exception("oreBarItemType is an invalid item");
		}

		internal static AltSilver GetAltSilver()
		{
			return ChosenAltSilver ?? (ChosenAltSilver = AltSilvers[WorldGen.genRand.Next(AltSilvers.Count)]);
		}

		internal static void Add(AltSilver alt)
		{
			AltSilvers.Add(alt);
		}
	}
}