namespace Terraria.ID
{
	partial class TileID
	{
		partial class Sets
		{
			public static bool[] CanDropFromRightClick = Factory.CreateBoolSet(4);
			public static bool[] Stone = Factory.CreateBoolSet(1, 117, 25, 203);
			public static bool[] Grass = Factory.CreateBoolSet(2, 23, 109, 199, 477, 492);
			public static bool[] CanBeClearedDuringOreRunner = Factory.CreateBoolSet(0, 1, 23, 25, 40, 53, 57, 59, 60, 70, 109, 112, 116, 117, 147, 161, 163, 164, 199, 200, 203, 234);

			/// <summary> Whether or not the smart cursor function is disabled when the cursor hovers above this tile. </summary>
			// Maybe this should be a hook instead?
			public static bool[] DisableSmartCursor = Factory.CreateBoolSet(4, 10, 11, 13, 21, 29, 33, 49, 50, 55, 79, 85, 88, 97, 104, 125, 132, 136, 139, 144, 174, 207, 209, 212, 216, 219, 237, 287, 334, 335, 338, 354, 386, 387, 388, 389, 411, 425, 441, 463, 467, 468, 491, 494, 510, 511, 573, 621);

			/// <summary> Whether or not the smart tile interaction function is disabled when the cursor hovers above this tile. </summary>
			public static bool[] DisableSmartInteract = Factory.CreateBoolSet(4, 33, 334, 395, 410, 455, 471, 480, 509, 520);

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

			/// <summary> Whether or not saplings count this tile as empty when trying to grow. </summary>
			public static bool[] IgnoredByGrowingSaplings = Factory.CreateBoolSet(3, 24, 32, 61, 62, 69, 71, 73, 74, 82, 83, 84, 110, 113, 201, 233, 352, 485, 529, 530);
		}
	}
}
