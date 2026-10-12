using System;
using System.Collections.Generic;
using System.Text;
using ReLogic.Reflection;
using Terraria.ModLoader;

namespace Terraria.ID;

public partial class LiquidID
{
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(LiquidID), typeof(short)); // Added By TML
#if !TMLANALYZER
	public static class Sets
	{
		public static SetFactory Factory = new SetFactory(LiquidLoader.LiquidCount, "LiquidID", Search);

		/// <summary> The Item created when a bucket is used on a liquid selected </summary>
		public static int[] CreateLiquidBucketItem = Factory.CreateIntSet(-1, Water, ItemID.WaterBucket, Lava, ItemID.LavaBucket, Honey, ItemID.HoneyBucket);

		/// <summary> Whats Items a liquid can be absorbed by. Used for defining sponges. </summary>
		public static List<int>[] CanBeAbsorbedBy = Factory.CreateCustomSet<List<int>>(null,
			Water,
			new List<int>() { ItemID.SuperAbsorbantSponge, ItemID.UltraAbsorbantSponge },
			Shimmer,
			new List<int>() { ItemID.SuperAbsorbantSponge, ItemID.UltraAbsorbantSponge },
			Lava,
			new List<int>() { ItemID.LavaAbsorbantSponge, ItemID.UltraAbsorbantSponge },
			Honey,
			new List<int>() { ItemID.HoneyAbsorbantSponge, ItemID.UltraAbsorbantSponge }
		);

		static Sets()
		{
			for (int i = 0; i < CanBeAbsorbedBy.Length; i++) {
				if (CanBeAbsorbedBy[i] == null) {
					CanBeAbsorbedBy[i] = new List<int>() { ItemID.UltraAbsorbantSponge };
				}
			}
		}
	}
#endif
}
