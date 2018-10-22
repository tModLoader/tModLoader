using System;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class AltCobalt
	{
		public readonly int oreTileType;
		public readonly string oreBlessingText;
		public readonly float abundanceMultiplier;
		private static readonly List<AltCobalt> AltCobalts = new List<AltCobalt>();
		internal static bool vanillaAltsAdded = false;
		public static AltCobalt ChosenAltCobalt { get; private set; }

		public AltCobalt(int oreTileType, string oreBlessingText, float abundanceMultiplier = 1f)
		{
			this.oreTileType = TileLoader.TileCount > oreTileType ? oreTileType : throw new Exception("oreTileType is an invalid tile");
			this.oreBlessingText = oreBlessingText;
			this.abundanceMultiplier = abundanceMultiplier;
		}

		public static AltCobalt GetAltCobalt()
		{
			if (!vanillaAltsAdded)
			{
				WorldGen.AddAlt(new AltCobalt(TileID.Cobalt, Lang.misc[12].Value));
				WorldGen.AddAlt(new AltCobalt(TileID.Palladium, Lang.misc[21].Value, 0.9f));
			}
			return ChosenAltCobalt ?? (ChosenAltCobalt = AltCobalts[WorldGen.genRand.Next(AltCobalts.Count)]);
		}

		internal static void UnchooseAltCobalt()
		{
			ChosenAltCobalt = null;
		}

		internal static void Add(AltCobalt alt)
		{
			AltCobalts.Add(alt);
		}

		internal static bool unchosen()
		{
			return ChosenAltCobalt == null;
		}

		internal static void tryFind(int oreTileType)
		{
			foreach (var altCobalt in AltCobalts)
			{
				if (altCobalt.oreTileType != oreTileType) continue;
				ChosenAltCobalt = altCobalt;
				break;
			}
		}
	}
}