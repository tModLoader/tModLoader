using System.Collections.Generic;

namespace Terraria.ID;

partial class ItemID
{
	partial class Sets
	{
		public static bool[] Glowsticks = Factory.CreateBoolSet(282, 286, 3002, 3112, 4776);

		/// <summary>
		/// Set for pre-hardmode boss bags, except it also contains the Queen Slime's Boss Bag. Affects the way dev armor drops function, making it only drop in special world seeds.
		/// <br/> Don't forget to use the <see cref="BossBag"/> set in conjuction with this one.
		/// </summary>
		public static bool[] PreHardmodeLikeBossBag = Factory.CreateBoolSet(
			KingSlimeBossBag, EyeOfCthulhuBossBag, EaterOfWorldsBossBag, BrainOfCthulhuBossBag, QueenBeeBossBag,
			SkeletronBossBag, WallOfFleshBossBag, QueenSlimeBossBag, DeerclopsBossBag
		);

		/// <summary>
		/// Set for catching tools (bug net-type items which can catch critters).<br></br>
		/// If you want your catching tool to be able to catch the Underworld's lava critters, don't forget to use the <see cref="LavaproofCatchingTool"/> set in conjunction with this one.
		/// </summary>
		public static bool[] CatchingTool = Factory.CreateBoolSet(
			BugNet,
			GoldenBugNet,
			FireproofBugNet
		);

		/// <summary>
		/// Set for catching tools which can catch the Underworld's lava critters.<br></br>
		/// Don't forget to use the <see cref="CatchingTool"/> set in conjunction with this one. 
		/// </summary>
		public static bool[] LavaproofCatchingTool = Factory.CreateBoolSet(
			GoldenBugNet,
			FireproofBugNet
		);

		/// <summary>
		/// Set for easily defining weapons as spears.<br/>
		/// Only used for vanilla spears to make sure they still scale with attack speed (though it's encouraged to set this for your spears as well, for cross-mod support).<br/>
		/// </summary>
		public static bool[] Spears = Factory.CreateBoolSet(
			Spear,
			Trident,
			Swordfish,
			ThunderSpear,
			TheRottedFork,
			DarkLance,
			CobaltNaginata,
			PalladiumPike,
			MythrilHalberd,
			OrichalcumHalberd,
			AdamantiteGlaive,
			TitaniumTrident,
			ObsidianSwordfish,
			Gungnir,
			MushroomSpear,
			MonkStaffT2,
			ChlorophytePartisan,
			NorthPole
		);

		/// <summary>
		/// Dictionary for defining what items will drop from a <see cref="ProjectileID.Geode"/> when broken. All items in this dictionary are equally likely to roll, and will drop with a stack size between minStack and maxStack (exclusive).
		/// <br/>Stack sizes with less than 1 or where minStack is not strictly smaller than maxStack will lead to exceptions being thrown.
		/// </summary>
		public static Dictionary<int, (int minStack, int maxStack)> GeodeDrops = new() {
			{ Sapphire, (3, 7) },
			{ Ruby, (3, 7) },
			{ Emerald, (3, 7) },
			{ Topaz, (3, 7) },
			{ Amethyst, (3, 7) },
			{ Diamond, (3, 7) },
			{ Amber, (3, 7) }
		};
	}
}
