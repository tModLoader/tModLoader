using System;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class AltAdamantite
	{
		public readonly int oreTileType;
		public readonly string oreBlessingText;
		public readonly float abundanceMultiplier;
		private static readonly List<AltAdamantite> AltAdamantites = new List<AltAdamantite>();
		internal static bool vanillaAltsAdded = false;
		public static AltAdamantite ChosenAltAdamantite { get; private set; }

		public AltAdamantite(int oreTileType, string oreBlessingText, float abundanceMultiplier = 1f)
		{
			this.oreTileType = TileLoader.TileCount > oreTileType ? oreTileType : throw new Exception("oreTileType is an invalid tile");
			this.oreBlessingText = oreBlessingText;
			this.abundanceMultiplier = abundanceMultiplier;
		}

		public static AltAdamantite GetAltAdamantite()
		{
			if (!vanillaAltsAdded)
			{
				WorldGen.AddAlt(new AltCobalt(TileID.Adamantite, Lang.misc[14].Value));
				WorldGen.AddAlt(new AltCobalt(TileID.Titanium, Lang.misc[23].Value, 0.9f));
			}
			return ChosenAltAdamantite ?? (ChosenAltAdamantite = AltAdamantites[WorldGen.genRand.Next(AltAdamantites.Count)]);
		}

		internal static void UnchooseAltAdamantite()
		{
			ChosenAltAdamantite = null;
		}

		internal static void Add(AltAdamantite alt)
		{
			AltAdamantites.Add(alt);
		}

		internal static bool unchosen()
		{
			return ChosenAltAdamantite == null;
		}

		internal static void tryFind(int oreTileType)
		{
			foreach (var altAdamantite in AltAdamantites)
			{
				if (altAdamantite.oreTileType != oreTileType) continue;
				ChosenAltAdamantite = altAdamantite;
				break;
			}
		}
	}
}