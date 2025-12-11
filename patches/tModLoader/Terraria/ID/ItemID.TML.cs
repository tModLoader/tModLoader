using System.Collections.Generic;

namespace Terraria.ID;

partial class ItemID
{
	partial class Sets
	{
		/// <summary>Used in <see cref="SandgunAmmoProjectileData"/>.</summary>
		public class SandgunAmmoInfo
		{
			public SandgunAmmoInfo(int ProjectileType, int BonusDamage = 0)
			{
				this.ProjectileType = ProjectileType;
				this.BonusDamage = BonusDamage;
			}

			public int ProjectileType { get; set; }
			public int BonusDamage { get; set; }
		}
		/// <summary>
		/// The projectile type and associated bonus damage for the specified sandgun ammo item (an item with <c>Item.ammo = AmmoID.Sand;</c>).
		/// <para/> If undefined, the projectile will default to 42 (<see cref = "ProjectileID.SandBallGun" />), as this is the normal sandgun sand projectile. The bonus damage will default to 0.
		/// <para/> The projectile shouldn't be your falling sand projectile - you need to create a second projectile for the sandgun.
		/// </summary>
		public static SandgunAmmoInfo[] SandgunAmmoProjectileData = Factory.CreateCustomSet<SandgunAmmoInfo>(null,
			EbonsandBlock, new SandgunAmmoInfo(ProjectileID.EbonsandBallGun, 5),
			PearlsandBlock, new SandgunAmmoInfo(ProjectileID.PearlSandBallGun, 5),
			CrimsandBlock, new SandgunAmmoInfo(ProjectileID.CrimsandBallGun, 5)
		);

		/// <summary>
		/// If <see langword="true"/> for a given item type (<see cref="Item.type"/>), then that item is a glowstick.
		/// <br/> Glowsticks work underwater and will be auto-selected by Smart Cursor when the cursor is far away from the player.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] Glowsticks = Factory.CreateBoolSet(282, 286, 3002, 3112, 4776);

		/// <summary>
		/// Set for pre-hardmode boss bags, except it also contains the Queen Slime's Boss Bag. Affects the way dev armor drops function, making it only drop in special world seeds.
		/// <br/> Don't forget to use the <see cref="BossBag"/> set in conjunction with this one.
		/// </summary>
		public static bool[] PreHardmodeLikeBossBag = Factory.CreateBoolSet(
			KingSlimeBossBag, EyeOfCthulhuBossBag, EaterOfWorldsBossBag, BrainOfCthulhuBossBag, QueenBeeBossBag,
			SkeletronBossBag, WallOfFleshBossBag, QueenSlimeBossBag, DeerclopsBossBag
		);

		/// <summary> Indicates that an item is to be filtered under the "Tools" filter in Journey Mode's duplication menu.
		/// <br/> Useful for manually setting an item to be filtered under the "Tools" filter, if your item does not meet the automatic criteria for the "Tools" filter.
		/// <br/> See the code of <see cref="GameContent.Creative.ItemFilters.Tools.FitsFilter(Item)"/> to check if your item meets the automatic criteria.
		/// </summary>
		public static bool[] DuplicationMenuToolsFilter = Factory.CreateBoolSet(
			509,
			850,
			851,
			3612,
			3625,
			3611,
			510,
			849,
			3620,
			1071,
			1543,
			1072,
			1544,
			1100,
			1545,
			50,
			3199,
			3124,
			5358,
			5359,
			5360,
			5361,
			5437,
			1326,
			5335,
			3384,
			4263,
			4819,
			4262,
			946,
			4707,
			205,
			206,
			207,
			1128,
			3031,
			4820,
			5302,
			5364,
			4460,
			4608,
			4872,
			3032,
			5303,
			5304,
			1991,
			4821,
			3183,
			779,
			5134,
			1299,
			4711,
			4049,
			114
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
			MonkStaffT1,
			MonkStaffT2,
			MonkStaffT3,
			ChlorophytePartisan,
			NorthPole
		);

		/// <summary>
		/// Set for defining how much coin luck according to its stack this item gives to nearby players when thrown into shimmer (<see cref="Entity.shimmerWet"/>).<br/>
		/// Includes the 4 vanilla coin types by default. The value represents the "price" of the currency in copper coins. For other items, default value is 0, which means it will not give coin luck.<br/>
		/// </summary>
		/// <remarks>Coin luck application takes precedence over other actions related to shimmer.</remarks>
		public static int[] CoinLuckValue = Factory.CreateIntSet(0,
			CopperCoin,   1,
			SilverCoin,	  100,
			GoldCoin,	  10000,
			PlatinumCoin, 1000000
		);

		/// <summary>
		/// If true, the item counts as a specialist weapon.<br/>
		/// Used for Shroomite Helmet damage buffs (and other effects that will affect <see cref="Player.specialistDamage"/>).<br/>
		/// </summary>
		public static bool[] IsRangedSpecialistWeapon = Factory.CreateBoolSet(
			PiranhaGun, PainterPaintballGun, Toxikarp, Harpoon, AleThrowingGlove
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

		/// <summary>
		/// Set to true to ignore this Item when determining Tile or Wall drops automatically from <see cref="Item.createTile"/> and <see cref="Item.createWall"/>. Use this for any item that places the same Tile/Wall as another item, but shouldn't be retrieved when mined. For example, an "infinite" version of a placement item would set this, allowing the non-infinite version to be used reliably as the drop.
		/// <br/> Also use this for any item which places a tile that doesn't return that same item when mined. Herb Seeds, for example, don't necessarily drop from Herb plants.
		/// </summary>
		public static bool[] DisableAutomaticPlaceableDrop = Factory.CreateBoolSet(false);

		/// <summary>
		/// Dictionary for defining what ores can spawn as bonus drop inside slime body. All items in this dictionary are equally likely to roll, and will drop with a stack size between minStack and maxStack (inclusive).
		/// <br/>Stack sizes with less than 1 or where minStack is not strictly smaller than maxStack will lead to exceptions being thrown.
		/// </summary>
		public static Dictionary<int, (int minStack, int maxStack)> OreDropsFromSlime = new() {
			{ CopperOre, (3, 13) },
			{ TinOre, (3, 13) },
			{ IronOre, (3, 13) },
			{ LeadOre, (3, 13) },
			{ SilverOre, (3, 13) },
			{ TungstenOre, (3, 13) },
			{ GoldOre, (3, 13) },
			{ PlatinumOre, (3, 13) },
		};

		/// <summary>
		/// Set to <see langword="true"/> to make this Item set its mana cost to 0 whenever <see cref="Player.spaceGun"/> is set to <see langword="true"/>.
		/// </summary>
		public static bool[] IsSpaceGun = Factory.CreateBoolSet(false, SpaceGun, ZapinatorGray, ZapinatorOrange);

		// Values taken from KillTile_ShouldDropSeeds
		/// <summary>
		/// Set to <see langword="true"/> to have seeds drop whenever grass breaks with this Item in the player's inventory.
		/// </summary>
		public static bool[] DropSeedsIfInInventory = Factory.CreateBoolSet(false, Blowpipe, Blowgun);
	}
}
