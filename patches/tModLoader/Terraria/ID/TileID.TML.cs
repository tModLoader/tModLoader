namespace Terraria.ID;

partial class TileID
{
	partial class Sets
	{
		public static bool[] CanDropFromRightClick = Factory.CreateBoolSet(4);
		public static bool[] Stone = Factory.CreateBoolSet(1, 117, 25, 203);
		public static bool[] Grass = Factory.CreateBoolSet(2, 23, 109, 199, 477, 492, 633); // Might be incorrect?

		/// <summary> Tiles within this set are allowed to be replaced by generating ore. </summary>
		public static bool[] CanBeClearedDuringOreRunner = Factory.CreateBoolSet(0, 1, 23, 25, 40, 53, 57, 59, 60, 70, 109, 112, 116, 117, 147, 161, 163, 164, 199, 200, 203, 234, 396, 397, 401, 403, 400, 398, 399, 402);

		/// <summary> Allows non-solid tiles to be sloped (solid tiles can always be sloped, regardless of this set). </summary>
		public static bool[] CanBeSloped = Factory.CreateBoolSet();

		/// <summary>
		/// Whether or not the tile will be ignored for automatic step up regarding town NPC collision.
		/// <br>Only checked when <see cref="Collision.StepUp"/> with specialChecksMode set to 1 is called</br>
		/// </summary>
		public static bool[] IgnoredByNpcStepUp = Factory.CreateBoolSet(14, 16, 18, 134, 469);

		/// <summary> Whether or not the smart cursor function is disabled when the cursor hovers above this tile. </summary>
		// Maybe this should be a hook instead?
		public static bool[] DisableSmartCursor = Factory.CreateBoolSet(4, 10, 11, 13, 21, 29, 33, 49, 50, 55, 79, 85, 88, 97, 104, 125, 132, 136, 139, 144, 174, 207, 209, 212, 216, 219, 237, 287, 334, 335, 338, 354, 386, 387, 388, 389, 411, 425, 441, 463, 467, 468, 491, 494, 510, 511, 573, 621, 642);

		/// <summary> Whether or not the smart tile interaction function is disabled when the cursor hovers above this tile. </summary>
		public static bool[] DisableSmartInteract = Factory.CreateBoolSet(4, 33, 334, 395, 410, 455, 471, 480, 509, 520, 657, 658);

		/// <summary> Whether or not this tile is a valid spawn point. </summary>
		public static bool[] IsValidSpawnPoint = Factory.CreateBoolSet(Beds);

		/// <summary> Whether or not this tile behaves like a torch. If you are making a torch tile, then setting this to true is necessary in order for tile placement, tile framing, and the item's smart selection to work properly. </summary>
		public static bool[] Torch = Factory.CreateBoolSet(TileID.Torches);

		/// <summary> Whether or not this tile is a clock. </summary>
		public static bool[] Clock = Factory.CreateBoolSet(GrandfatherClocks);

		/// <summary> Whether or not this tile is a sapling, which can grow into a tree based on the soil it's placed on. Be sure to set <see cref="CommonSapling"/> with this too. </summary>
		public static bool[] TreeSapling = Factory.CreateBoolSet(Saplings);

		/// <summary> Whether or not this tile counts as a water source for crafting purposes. </summary>
		public static bool[] CountsAsWaterSource = Factory.CreateBoolSet(172);

		/// <summary> Whether or not this tile counts as a honey source for crafting purposes. </summary>
		public static bool[] CountsAsHoneySource = Factory.CreateBoolSet();

		/// <summary> Whether or not this tile counts as a lava source for crafting purposes. </summary>
		public static bool[] CountsAsLavaSource = Factory.CreateBoolSet();

		/// <summary> Whether or not this tile counts as a shimmer source for crafting purposes. </summary>
		public static bool[] CountsAsShimmerSource = Factory.CreateBoolSet();

		/// <summary> Whether or not saplings count this tile as empty when trying to grow. </summary>
		public static bool[] IgnoredByGrowingSaplings = Factory.CreateBoolSet(3, 24, 32, 61, 62, 69, 71, 73, 74, 82, 83, 84, 110, 113, 201, 233, 352, 485, 529, 530, 637, 655);

		/// <summary> Whether or not this tile prevents a meteor from landing near it.</summary>
		/// <remarks> Note: Chests and Dungeon tiles are not in this set, but also prevent landing (handled through <see cref="BasicChest"/> and <see cref="Main.tileDungeon"/>)</remarks>
		public static bool[] AvoidedByMeteorLanding = Factory.CreateBoolSet(226, 470, 475, 448, 597);

		/// <summary>
		/// Whether or not this tile will prevent sand/slush from falling beneath it.
		/// </summary>
		/// <remarks>
		///	Note: This Set does not include the values within the Sets <seealso cref="BasicChest"/>, <seealso cref="BasicChestFake"/>,
		/// and <seealso cref="BasicDresser"/>, but Tile IDs within those sets will also prevent sandfall.
		/// </remarks>
		public static bool[] PreventsSandfall = Factory.CreateBoolSet(26, 77, 80, 323, 470, 475, 597);

		/// <summary>
		/// What tiles count as Pylons, which allow the player to teleport to any other valid Pylons on the map.
		/// </summary>
		public static int[] CountsAsPylon = new int[] {
			597
		};

		/// <summary>
		/// Tiles that are interpreted as a wall by nearby walls during framing, causing them to frame as if merging with this adjacent tile. Prevents wall from drawing within bounds for transparant tiles.
		/// </summary>
		public static bool[] WallsMergeWith = Factory.CreateBoolSet(Glass);

		// Values taken from Main.SetupTileMerge
		/// <summary>
		/// The value a tile forces to be set for <see cref="BlockMergesWithMergeAllBlock"/> regardless of default conditions (see its documentation). null by default.
		/// </summary>
		public static bool?[] BlockMergesWithMergeAllBlockOverride = Factory.CreateCustomSet<bool?>(null, 10, false, 387, false, 541, false);

		/// New created sets to facilitate vanilla biome block counting including modded blocks. To replace the current hardcoded counts in SceneMetrics.cs
		public static int[] CorruptBiome = Factory.CreateIntSet(0, 23, 1, 24, 1, 25, 1, 32, 1, 112, 1, 163, 1, 400, 1, 398, 1, 27, -10);
		public static int[] HallowBiome = Factory.CreateIntSet(0, 109, 1, 492, 1, 110, 1, 113, 1, 117, 1, 116, 1, 164, 1, 403, 1, 402, 1);
		public static int[] CrimsonBiome = Factory.CreateIntSet(0, 199, 1, 203, 1, 200, 1, 401, 1, 399, 1, 234, 1, 352, 1, 27, -10);
		public static int[] SnowBiome = Factory.CreateIntSet(0, 147, 1, 148, 1, 161, 1, 162, 1, 164, 1, 163, 1, 200, 1);
		public static int[] JungleBiome = Factory.CreateIntSet(0, 60, 1, 61, 1, 62, 1, 74, 1, 226, 1, 225, 1);
		public static int[] MushroomBiome = Factory.CreateIntSet(0, 70, 1, 71, 1, 72, 1, 528, 1);
		public static int[] SandBiome = Factory.CreateIntSet(0, 53, 1, 112, 1, 116, 1, 234, 1, 397, 1, 398, 1, 402, 1, 399, 1, 396, 1, 400, 1, 403, 1, 401, 1);
		public static int[] DungeonBiome = Factory.CreateIntSet(0, 41, 1, 43, 1, 44, 1, 481, 1, 482, 1, 483, 1);

		public static int[] RemixJungleBiome = Factory.CreateIntSet(0, 60, 1, 61, 1, 62, 1, 74, 1, 225, 1);
		public static int[] RemixCrimsonBiome = Factory.CreateIntSet(0, 199, 1, 203, 1, 200, 1, 401, 1, 399, 1, 234, 1, 352, 1, 27, -10, 195, 1);
		public static int[] RemixCorruptBiome = Factory.CreateIntSet(0, 23, 1, 24, 1, 25, 1, 32, 1, 112, 1, 163, 1, 400, 1, 398, 1, 27, -10, 474, 1);

		/// Functions to simplify modders adding a tile to the crimson, corruption, or jungle regardless of a remix world or not. Can still add manually as needed.
		public static void AddCrimsonTile(ushort type, int strength = 1)
		{
			CrimsonBiome[type] = strength;
			RemixCrimsonBiome[type] = strength;
		}

		public static void AddCorruptionTile(ushort type, int strength = 1)
		{
			CorruptBiome[type] = strength;
			RemixCorruptBiome[type] = strength;
		}

		public static void AddJungleTile(ushort type, int strength = 1)
		{
			JungleBiome[type] = strength;
			RemixJungleBiome[type] = strength;
		}
	}
}
