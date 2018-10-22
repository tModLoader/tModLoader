using System;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class AltMythril
	{
		public readonly int oreTileType;
		public readonly string oreBlessingText;
		public readonly float abundanceMultiplier;
		private static readonly List<AltMythril> AltMythrils = new List<AltMythril>();
		internal static bool vanillaAltsAdded = false;
		public static AltMythril ChosenAltMythril { get; private set; }

		public AltMythril(int oreTileType, string oreBlessingText, float abundanceMultiplier = 1f)
		{
			this.oreTileType = TileLoader.TileCount > oreTileType ? oreTileType : throw new Exception("oreTileType is an invalid tile");
			this.oreBlessingText = oreBlessingText;
			this.abundanceMultiplier = abundanceMultiplier;
		}

		public static AltMythril GetAltMythril()
		{
			if (!vanillaAltsAdded)
			{
				WorldGen.AddAlt(new AltCobalt(TileID.Mythril, Lang.misc[13].Value));
				WorldGen.AddAlt(new AltCobalt(TileID.Orichalcum, Lang.misc[22].Value, 0.9f));
			}
			return ChosenAltMythril ?? (ChosenAltMythril = AltMythrils[WorldGen.genRand.Next(AltMythrils.Count)]);
		}

		internal static void UnchooseAltMythril()
		{
			ChosenAltMythril = null;
		}

		internal static void Add(AltMythril alt)
		{
			AltMythrils.Add(alt);
		}

		internal static bool unchosen()
		{
			return ChosenAltMythril == null;
		}

		internal static void tryFind(int oreTileType)
		{
			foreach (var altMythril in AltMythrils)
			{
				if (altMythril.oreTileType != oreTileType) continue;
				ChosenAltMythril = altMythril;
				break;
			}
		}
	}
}