using System;
using System.Collections.Generic;
using System.Text;
using ReLogic.Reflection;
using Terraria.ModLoader;

namespace Terraria.ID;

public partial class LiquidID
{
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

		/// <summary>
		/// If <see langword="true"/>, certain npc and player can drown in it 
		/// </summary>
		public static bool[] CheckForDrowning = Factory.CreateBoolSet(false, Water, Honey);

		/// <summary>
		/// The higher the value, longer a liquid will take to fall (maximum of 10)
		/// </summary>
		public static int[] FallDelay = Factory.CreateIntSet(0, Lava, 5, Honey, 10);

		/// <summary>
		/// If <see langword="true"/>, player and npc that can drown will emit bubble if they are in a liquid
		/// </summary>
		public static bool[] AllowEmitBreathBubbles = Factory.CreateBoolSet(false, Water, Honey);

		/// <summary>
		/// Liquid's stopwatch MPH multiplier whether that be modded or vanilla liquids.
		/// </summary>
		public static float[] StopWatchMPHMultiplier = Factory.CreateFloatSet(0.5f, Honey, 0.25f, Shimmer, 0.375f);

		/// <summary>
		/// Liquids movement multiplier for when a player is moving in it
		/// </summary>
		public static float[] PlayerMovementMultiplier = Factory.CreateFloatSet(0.5f);

		/// <summary>
		/// Default NPCs movement speed multiplier before any change for each liquid
		/// </summary>
		public static float[] NPCMovementMultiplierDefault = Factory.CreateFloatSet(0.5f, Honey, 0.25f, Shimmer, 0.375f);

		/// <summary>
		/// Default projectiles movement speed multiplier before any change for each liquid
		/// </summary>
		public static float[] ProjectileMovementMultiplier = Factory.CreateFloatSet(0.5f, Honey, 0.25f, Shimmer, 0.375f);

		/// <summary>
		/// Liquids by default extinguish On Fire and HellFire debuff, unless it's lava.
		/// </summary>
		public static bool[] DoesNotExtinguishesOnFireDebuffs = Factory.CreateBoolSet(true, Lava);

		/// <summary>
		/// Minimum amount of block required for a liquid to make it viable for fishing
		/// </summary>
		public static int[] FishingPoolMinimumSize = Factory.CreateIntSet(75, Honey, 50);

		/// <summary>
		/// Multiplier for liquid ripple when an entity come in contact with a liquid
		/// </summary>
		public static float[] LiquidRippleMultiplier = Factory.CreateFloatSet(1f, Lava, 0.3f, Honey, 0.3f);

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
