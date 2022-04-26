namespace Terraria.ID
{
	partial class ItemID
	{
		partial class Sets
		{
			public static bool[] Glowsticks = Factory.CreateBoolSet(282, 286, 3002, 3112, 4776);

			/// <summary>
			/// Set for all boss bags. Causes bags to drop dev armor.
			/// <br/> If your bag is pre-hardmode, don't forget to use the <see cref="PreHardmodeLikeBossBag"/> set in conjuction with this one.
			/// </summary>
			public static bool[] BossBag = Factory.CreateBoolSet(
				KingSlimeBossBag, EyeOfCthulhuBossBag, EaterOfWorldsBossBag, BrainOfCthulhuBossBag, QueenBeeBossBag,
				SkeletronBossBag, WallOfFleshBossBag, DestroyerBossBag, TwinsBossBag, SkeletronPrimeBossBag,
				PlanteraBossBag, GolemBossBag, FishronBossBag, CultistBossBag, MoonLordBossBag,
				BossBagBetsy, FairyQueenBossBag, QueenSlimeBossBag
			);

			/// <summary>
			/// Set for pre-hardmode boss bags, except it also contains the Queen Slime's Boss Bag. Affects the way dev armor drops function, making it only drop in special world seeds.
			/// <br/> Don't forget to use the <see cref="BossBag"/> set in conjuction with this one.
			/// </summary>
			public static bool[] PreHardmodeLikeBossBag = Factory.CreateBoolSet(
				KingSlimeBossBag, EyeOfCthulhuBossBag, EaterOfWorldsBossBag, BrainOfCthulhuBossBag, QueenBeeBossBag,
				SkeletronBossBag, WallOfFleshBossBag, QueenSlimeBossBag
			);

			public static bool[] CanCatchLavaCritters = Factory.CreateBoolSet(
				GoldenBugNet,
				FireproofBugNet
			);
		}
	}
}
