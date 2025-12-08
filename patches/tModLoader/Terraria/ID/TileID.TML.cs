using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace Terraria.ID;

partial class TileID
{
	partial class Sets
	{
		/// <summary> Will cause the tile to be killed when right clicked, like <see cref="Torches"/> or <see cref="Bottles"/>. No need to set this if using <see cref="Torch"/>. </summary>
		public static bool[] CanDropFromRightClick = Factory.CreateBoolSet(4);
		public static bool[] Stone = Factory.CreateBoolSet(1, 117, 25, 203);
		public static bool[] Grass = Factory.CreateBoolSet(2, 23, 109, 199, 477, 492, 633); // Might be incorrect?
		/// <summary> Tiles within this set are multi-tiles that don't have a TileObjectData. This is only used to prevent TileLoader.Drop from being called multiple times when breaking these tiles, as might be expected. Trees and Cactus are not included in this, since each of those tiles drop items. </summary>
		public static bool[] IsMultitile = Factory.CreateBoolSet(Pots, ShadowOrbs, PlantDetritus, LifeFruit, PlanteraBulb, OasisPlants); // 165, 185, 201: Have 1x1 and multitiles in same tile, ignore.

		/// <summary> Tiles within this set are allowed to be replaced by generating ore. </summary>
		public static bool[] CanBeClearedDuringOreRunner = Factory.CreateBoolSet(0, 1, 23, 25, 40, 53, 57, 59, 60, 70, 109, 112, 116, 117, 147, 161, 163, 164, 199, 200, 203, 234, 396, 397, 401, 403, 400, 398, 399, 402);

		/// <summary> Allows non-solid tiles to be sloped (solid tiles can always be sloped, regardless of this set). </summary>
		public static bool[] CanBeSloped = Factory.CreateBoolSet();

		/// <summary> Whether or not this tile can be infected by the natural spreading of biomes such as corruption, crimson, or hallow. </summary>
		public static bool[] Infectable = Factory.CreateBoolSet(1, 2, 53, 60, 69, 161, 179, 180, 181, 182, 183, 381, 396, 397, 534, 536, 539, 625, 627);

		/// <summary>
		/// Prevents a tile immediately below a tile of this type from being hammered (sloped). Since a sloped tile would break a typical bottom tile anchor, this prevents such tiles from being broken in this manner. Anything in <see cref="BasicChest"/> or <see cref="BasicDresser"/> are also protected in the same manner. This is typically used for tiles that shouldn't break as easily as other tiles, such as tiles containing Tile Entities holding items.
		/// <para/> Some examples include DemonAltar, Teleporter, Mannequins, and HatRack.
		/// <para/> See also <see cref="PreventsTileRemovalIfOnTopOfIt"/>, which is frequently set in tandem with this.
		/// </summary>
		public static bool[] PreventsTileHammeringIfOnTopOfIt = Factory.CreateBoolSet(false, 21, 26, 77, 88, 235, 237, 441, 467, 468, 470, 475, 488, 597);

		/// <summary>Used in <see cref="FallingBlockProjectile"/>.</summary>
		public class FallingBlockProjectileInfo
		{
			public FallingBlockProjectileInfo(int FallingProjectileType, int FallingProjectileDamage = 10)
			{
				this.FallingProjectileType = FallingProjectileType;
				this.FallingProjectileDamage = FallingProjectileDamage;
			}

			public int FallingProjectileType { get; set; }
			public int FallingProjectileDamage { get; set; }
		}
		/// <summary>
		/// Maps tile type for <see cref="Falling"/> tiles to their corresponding falling block projectile and associated falling projectile damage. Falling coins use 0 damage while all other tiles use 10 damage.
		/// </summary>
		public static FallingBlockProjectileInfo[] FallingBlockProjectile = Factory.CreateCustomSet<FallingBlockProjectileInfo>(null,
			Sand, new FallingBlockProjectileInfo(ProjectileID.SandBallFalling),
			Ebonsand, new FallingBlockProjectileInfo(ProjectileID.EbonsandBallFalling),
			TileID.Mud, new FallingBlockProjectileInfo(ProjectileID.MudBall),
			Pearlsand, new FallingBlockProjectileInfo(ProjectileID.PearlSandBallFalling),
			Silt, new FallingBlockProjectileInfo(ProjectileID.SiltBall),
			Slush, new FallingBlockProjectileInfo(ProjectileID.SlushBall),
			Crimsand, new FallingBlockProjectileInfo(ProjectileID.CrimsandBallFalling),
			CopperCoinPile, new FallingBlockProjectileInfo(ProjectileID.CopperCoinsFalling, 0),
			SilverCoinPile, new FallingBlockProjectileInfo(ProjectileID.SilverCoinsFalling, 0),
			GoldCoinPile, new FallingBlockProjectileInfo(ProjectileID.GoldCoinsFalling, 0),
			PlatinumCoinPile, new FallingBlockProjectileInfo(ProjectileID.PlatinumCoinsFalling, 0),
			ShellPile, new FallingBlockProjectileInfo(ProjectileID.ShellPileFalling)
		);

		/// <summary>
		/// Whether or not the tile will be ignored for automatic step up regarding town NPC collision.
		/// <br>Only checked when <see cref="Collision.StepUp"/> with specialChecksMode set to 1 is called</br>
		/// </summary>
		public static bool[] IgnoredByNpcStepUp = Factory.CreateBoolSet(14, 16, 18, 134, 469);

		/// <summary>
		/// Whether or not the smart cursor function is disabled when the cursor hovers above this tile. Used by tiles frequently right click interacted with to help prevent accidental tile placement when the player accidentally left clicks on it with smart cursor enabled, such as doors and containers.
		/// <para/> Defaults to <see langword="false"/>.
		/// </summary>
		// Maybe this should be a hook instead?
		public static bool[] DisableSmartCursor = Factory.CreateBoolSet(4, 10, 11, 13, 21, 29, 33, 49, 50, 55, 79, 85, 88, 97, 104, 125, 132, 136, 139, 144, 174, 207, 209, 212, 216, 219, 237, 287, 334, 335, 338, 354, 386, 387, 388, 389, 411, 425, 441, 463, 467, 468, 491, 494, 510, 511, 573, 621, 642);

		/// <summary>
		/// Whether or not the smart tile interaction function is disabled when the cursor hovers above this tile. Used by tiles interactable by right click that do not use smart interact, such as torches and candles.
		/// <para/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] DisableSmartInteract = Factory.CreateBoolSet(4, 33, 334, 395, 410, 455, 471, 480, 509, 520, 657, 658);

		/// <summary> Whether or not this tile is a valid spawn point. </summary>
		public static bool[] IsValidSpawnPoint = Factory.CreateBoolSet(Beds);

		/// <summary> Whether or not this tile behaves like a torch. If you are making a torch tile, then setting this to true is necessary in order for tile placement, tile framing, and the item's smart selection to work properly. Each item that places torch tiles should also set <see cref="ItemID.Sets.Torches"/>.</summary>
		public static bool[] Torch = Factory.CreateBoolSet(TileID.Torches);

		/// <summary> Whether or not this tile behaves like a campfire. Campfires must be 3x2 and need to follow the vanilla layout with the on state being at the top of the texture. Padding must also be present in the same manner, resulting in a 54x36 section for each style. The animation, however, can be done with a separate flame texture if desired. <br/>
		/// Necessary for block swap and Marshmallow on a Stick features.</summary>
		public static bool[] Campfire = Factory.CreateBoolSet(TileID.Campfire);

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
		public static bool[] IgnoredByGrowingSaplings = Factory.CreateBoolSet(3, 24, 32, 61, 62, 69, 71, 73, 74, 82, 83, 84, 110, 113, 184, 201, 233, 352, 485, 529, 530, 637, 655);

		/// <summary> Whether or not this tile prevents a meteor from landing near it.
		/// <para/> Contains LihzahrdBrick, DisplayDoll, HatRack, FallenLog, and TeleportationPylon.
		/// </summary>
		/// <remarks> Note: Chests and Dungeon tiles are not in this set, but also prevent landing (handled through <see cref="BasicChest"/> and <see cref="Main.tileDungeon"/>)</remarks>
		public static bool[] AvoidedByMeteorLanding = Factory.CreateBoolSet(226, 470, 475, 488, 597);

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
		/// Tiles that are interpreted as a wall by nearby walls during framing, causing them to frame as if merging with this adjacent tile. Prevents wall from drawing within bounds for transparent tiles.
		/// </summary>
		public static bool[] WallsMergeWith = Factory.CreateBoolSet(Glass);

		// Values taken from Main.SetupTileMerge
		/// <summary>
		/// The value a tile forces to be set for <see cref="BlockMergesWithMergeAllBlock"/> regardless of default conditions (see its documentation). null by default.
		/// </summary>
		public static bool?[] BlockMergesWithMergeAllBlockOverride = Factory.CreateCustomSet<bool?>(null, 10, false, 387, false, 541, false);

		// Values taken from Player.PlaceThing_Tiles_BlockPlacementForAssortedThings()
		/// <summary>
		/// Allows tiles to be placed next to any tile or wall. (Tiles normally need a <see cref="Main.tileSolid"/> tile, <see cref="Rope"/> tile, <see cref="IsBeam"/> tile, or a wall adjacent to the target position to be placeable.)
		/// <br>Used by: Cobweb, Coin Piles, Living Fire Blocks, Smoke Blocks, Bubble Blocks</br>
		/// </summary>
		public static bool[] CanPlaceNextToNonSolidTile = Factory.CreateBoolSet(false, Cobweb, CopperCoinPile, SilverCoinPile, GoldCoinPile, PlatinumCoinPile, LivingFire, LivingCursedFire, LivingDemonFire, LivingFrostFire, LivingIchor, LivingUltrabrightFire, ChimneySmoke, Bubble);

		/// New created sets to facilitate vanilla biome block counting including modded blocks. To replace the current hardcoded counts in SceneMetrics.cs
		public static int[] CorruptBiome = Factory.CreateIntSet(0, 23, 1, 661, 1, 24, 1, 25, 1, 32, 1, 112, 1, 163, 1, 400, 1, 398, 1, 27, -10);
		public static int[] HallowBiome = Factory.CreateIntSet(0, 109, 1, 492, 1, 110, 1, 113, 1, 117, 1, 116, 1, 164, 1, 403, 1, 402, 1);
		public static int[] CrimsonBiome = Factory.CreateIntSet(0, 199, 1, 662, 1, 203, 1, 200, 1, 401, 1, 399, 1, 234, 1, 352, 1, 27, -10, 201, 1);
		public static int[] SnowBiome = Factory.CreateIntSet(0, 147, 1, 148, 1, 161, 1, 162, 1, 164, 1, 163, 1, 200, 1);
		public static int[] JungleBiome = Factory.CreateIntSet(0, 60, 1, 61, 1, 62, 1, 74, 1, 226, 1, 225, 1);
		public static int[] MushroomBiome = Factory.CreateIntSet(0, 70, 1, 71, 1, 72, 1, 528, 1);
		public static int[] SandBiome = Factory.CreateIntSet(0, 53, 1, 112, 1, 116, 1, 234, 1, 397, 1, 398, 1, 402, 1, 399, 1, 396, 1, 400, 1, 403, 1, 401, 1);
		public static int[] DungeonBiome = Factory.CreateIntSet(0, 41, 1, 43, 1, 44, 1, 481, 1, 482, 1, 483, 1);

		public static int[] RemixJungleBiome = Factory.CreateIntSet(0, 60, 1, 61, 1, 62, 1, 74, 1, 225, 1);
		public static int[] RemixCrimsonBiome = Factory.CreateIntSet(0, 199, 1, 662, 1, 203, 1, 200, 1, 401, 1, 399, 1, 234, 1, 352, 1, 27, -10, 195, 1, 201, 1);
		public static int[] RemixCorruptBiome = Factory.CreateIntSet(0, 23, 1, 661, 1, 24, 1, 25, 1, 32, 1, 112, 1, 163, 1, 400, 1, 398, 1, 27, -10, 474, 1);

		/// <summary>
		/// The ID of the tile that a given closed door transforms into when it becomes OPENED. Defaults to -1, which means said tile isn't a closed door.
		/// </summary>
		public static int[] OpenDoorID = Factory.CreateIntSet(-1);

		/// <summary>
		/// The ID of the tile that a given open door transforms into when it becomes CLOSED. Defaults to -1, which means said tile isn't an open door.
		/// </summary>
		public static int[] CloseDoorID = Factory.CreateIntSet(-1);

		/// <summary>
		/// A version of <see cref="TileID.Sets.SwaysInWindBasic"/> that functions with multitiles. Causes the tile to sway along with the wind and player interaction.
		/// <para/> <see cref="ModTile.AdjustMultiTileVineParameters(int, int, ref float?, ref float, ref float, ref bool, ref float, ref Microsoft.Xna.Framework.Graphics.Texture2D, ref Microsoft.Xna.Framework.Color)"/> can be used to customize how the tile sways with wind and player interaction.
		/// <para/> <b>NOTE:</b> Requires calling <see cref="TileDrawing.AddSpecialPoint"/> in <c>ModTile.PreDraw</c> for the coordinates of the top left tile of the multitile. Use either
		/// <see cref="TileDrawing.TileCounterType.MultiTileVine"/> or <see cref="TileDrawing.TileCounterType.MultiTileGrass"/>, depending on what kind of sway interaction you want.
		/// </summary>
		public static bool[] MultiTileSway = Factory.CreateBoolSet(false);

		/// <summary>
		/// If true, players landing on these tiles will not suffer <see href="https://terraria.wiki.gg/wiki/Fall_damage#Tiles">fall damage</see>. Vanilla entries include Cloud, RainCloud, SnowCloud, and PoopBlock. Defaults to false.
		/// <para/> See also <see cref="Main.tileBouncy"/>.
		/// </summary>
		public static bool[] NegatesFallDamage = Factory.CreateBoolSet(Cloud, RainCloud, SnowCloud, PoopBlock);

		/// <summary>
		/// Indicates that this tile is a pressure plate, and which entities should trigger it when they collide with it. Custom tiles will still need to implement <see cref="ModTile.HitSwitch(int, int)"/> to act on the trigger. The <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExamplePressurePlate.cs">ExamplePressurePlate.cs</see> example demonstrates this.
		/// <br/><br/> Positive values and 0 indicate that this pressure plate acts like a specific <see cref="PressurePlates"/> style, inheriting the specific set of entity interactions of that style. (See the <see href="https://terraria.wiki.gg/wiki/Pressure_Plates">Pressure Plates wiki page</see>.) For example: 2 for players only, 0 for all entities, 5 for NPC, and 7 for player only but the pressure plate breaks.
		/// <br/><br/> A -2 value indicates that this pressure plate acts exactly like <see cref="PressurePlates"/>, with each style corresponding to the same entity interactions sets of the <see cref="PressurePlates"/> styles.
		/// <br/><br/> A -3 value indicates that this is a <see cref="WeightedPressurePlate"/>. It can only be interacted by players but will trigger when stepped on and off.
		/// <br/><br/> A -4 value indicates that this is a <see cref="ProjectilePressurePad"/>. It can only be interacted by projectiles support all 4 placement orientations.
		/// <br/><br/> Note that for each of these the tile sprite dimensions should match the vanilla sprite.
		/// <br/><br/> <see cref="ModTile.SwitchTiles"/> can be used if these options are insufficient and custom collision calculations are needed. The <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExampleSlopeTile.cs">ExampleSlopeTile.cs</see> example demonstrates this.
		/// <br/><br/> Defaults to -1.
		/// </summary>
		public static int[] PressurePlate = Factory.CreateIntSet(-1, PressurePlates, -2, WeightedPressurePlate, -3, ProjectilePressurePad, -4);

		// Values taken from WorldFile.ClearTempTiles
		/// <summary>
		/// If true, the tile will be destroyed after the world is loaded, before it is entered. Can be used to get rid of temporary tiles such as the block created by <see href="https://terraria.wiki.gg/wiki/Ice_Rod">Ice Rod</see>.
		/// <br/><br/> The tile will NOT drop it's associated item, but it will trigger any effects invoked through <see cref="GlobalTile.KillTile(int, int, int, ref bool, ref bool, ref bool)"/> and <see cref="ModTile.KillTile(int, int, ref bool, ref bool, ref bool)"/>.
		/// <br/><br/> The tile will NOT get destroyed in cases where certain tiles - such as non-empty chests - anchor onto it.
		/// <br/><br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] ClearedOnWorldLoad = Factory.CreateBoolSet(MagicalIceBlock, MysticSnakeRope);

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
