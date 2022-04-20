namespace Terraria.ID
{
	partial class TileID
	{
		partial class Sets
		{
			public static bool[] CanDropFromRightClick = Factory.CreateBoolSet(Torches);
			public static bool[] Stone = Factory.CreateBoolSet(TileID.Stone, Pearlstone, Ebonstone, Crimstone);
			public static bool[] Grass = Factory.CreateBoolSet(TileID.Grass, CorruptGrass, HallowedGrass, CrimsonGrass, GolfGrass, GolfGrassHallowed);
			public static bool[] CanBeClearedDuringOreRunner = Factory.CreateBoolSet(Dirt, TileID.Stone, CorruptGrass, Ebonstone, ClayBlock, Sand, Ash, TileID.Mud, JungleGrass, MushroomGrass, HallowedGrass, Ebonsand, Pearlsand, Pearlstone, SnowBlock, IceBlock, CorruptIce, HallowedIce, CrimsonGrass, FleshIce, Crimstone, Crimsand);

			/// <summary>
			/// Whether or not the tile will be ignored for automatic step up regarding town NPC collision.
			/// <br>Only checked when <see cref="Collision.StepUp"/> with specialChecksMode set to 1 is called</br>
			/// </summary>
			public static bool[] IgnoredByNpcStepUp = Factory.CreateBoolSet(Tables, Anvils, WorkBenches, MythrilAnvil, Tables2);

			/// <summary> Whether or not the smart cursor function is disabled when the cursor hovers above this tile. </summary>
			// Maybe this should be a hook instead?
			public static bool[] DisableSmartCursor = Factory.CreateBoolSet(Torches, ClosedDoor, OpenDoor, Bottles, Containers, PiggyBank, Candles, WaterCandle, Books, Signs, Beds, Tombstones, Dressers, Safes, GrandfatherClocks, CrystalBall, Lever, Switches, MusicBoxes, Timers, PlatinumCandle, WaterFountain, Cannon, SnowballLauncher, Firework, Extractinator, LihzahrdAltar, AmmoBox, WeaponsRack, FireworksBox, FireworkFountain, BewitchingTable, TrapdoorOpen, TrapdoorClosed, TallGateClosed, TallGateOpen, Detonator, AnnouncementBox, FakeContainers, DefendersForge, Containers2, FakeContainers2, VoidVault, GolfTee, ArrowSign, PaintedArrowSign, TatteredWoodSign, SliceOfCake);

			/// <summary> Whether or not the smart tile interaction function is disabled when the cursor hovers above this tile. </summary>
			public static bool[] DisableSmartInteract = Factory.CreateBoolSet(Torches, Candles, WeaponsRack, ItemFrame, LunarMonolith, PartyMonolith, WeaponsRack2, BloodMoonMonolith, VoidMonolith, FoodPlatter);

			/// <summary> Whether or not this tile is a valid spawn point. </summary>
			public static bool[] IsValidSpawnPoint = Factory.CreateBoolSet(Beds);

			/// <summary> Whether or not this tile behaves like a torch. If you are making a torch tile, then setting this to true is necessary in order for tile placement, tile framing, and the item's smart selection to work properly. </summary>
			public static bool[] Torch = Factory.CreateBoolSet(Torches);

			/// <summary> Whether or not this tile is a clock. </summary>
			public static bool[] Clock = Factory.CreateBoolSet(GrandfatherClocks);

			/// <summary> Whether or not this tile is a sapling, which can grow into a tree based on the soil it's placed on. Be sure to set <see cref="CommonSapling"/> with this too. </summary>
			public static bool[] TreeSapling = Factory.CreateBoolSet(Saplings);

			/// <summary> Whether or not this tile counts as a water source for crafting purposes. </summary>
			public static bool[] CountsAsWaterSource = Factory.CreateBoolSet(Sinks);

			/// <summary> Whether or not this tile counts as a honey source for crafting purposes. </summary>
			public static bool[] CountsAsHoneySource = Factory.CreateBoolSet();

			/// <summary> Whether or not this tile counts as a lava source for crafting purposes. </summary>
			public static bool[] CountsAsLavaSource = Factory.CreateBoolSet();

			/// <summary> Whether or not saplings count this tile as empty when trying to grow. </summary>
			public static bool[] IgnoredByGrowingSaplings = Factory.CreateBoolSet(Plants, CorruptPlants, CorruptThorns, JunglePlants, JungleVines, JungleThorns, MushroomPlants, Plants2, JunglePlants2, ImmatureHerbs, MatureHerbs, BloomingHerbs, HallowedPlants, HallowedPlants2, CrimsonPlants, PlantDetritus, CrimsonThorns, AntlionLarva, SeaOats, OasisPlants);
		}
	}
}
