namespace Terraria.ModLoader;

/// <summary>
/// A struct that stores information regarding where an NPC is naturally spawning and the player it is spawning around. This serves to reduce the
/// parameter count for ModNPC.CanSpawn and GlobalNPC.EditSpawnPool.
/// </summary>
public record class NPCSpawnInfo
{
	/// <summary> The x-coordinate of the tile the NPC will spawn above. </summary>
	public int SpawnTileX { get; internal set; }

	/// <summary> The y-coordinate of the tile the NPC will spawn above. </summary>
	public int SpawnTileY { get; internal set; }

	public int SpawnTileType { get; internal set; }

	/// <summary> Shorthand for "Main.tile[info.SpawnTileX, info.SpawnTileY]" </summary>
	public Tile GetTile()
		=> Main.tile[SpawnTileX, SpawnTileY];

	/// <summary>
	/// The player that this NPC is spawning around. For convenience, here are the player zones, which are also useful for determining NPC spawn:
	/// (ZoneGranite, ZoneMarble, ZoneHive, ZoneGemCave are not actually proper spawning related checks, they are for visuals only (RGB), determined
	/// by the backwall type)
	/// <list type="bullet">
	/// <item>
	/// <description> ZoneDungeon </description>
	/// </item>
	/// <item>
	/// <description> ZoneCorrupt </description>
	/// </item>
	/// <item>
	/// <description> ZoneHallow </description>
	/// </item>
	/// <item>
	/// <description> ZoneMeteor </description>
	/// </item>
	/// <item>
	/// <description> ZoneJungle </description>
	/// </item>
	/// <item>
	/// <description> ZoneSnow </description>
	/// </item>
	/// <item>
	/// <description> ZoneCrimson </description>
	/// </item>
	/// <item>
	/// <description> ZoneWaterCandle </description>
	/// </item>
	/// <item>
	/// <description> ZonePeaceCandle </description>
	/// </item>
	/// <item>
	/// <description> ZoneTowerSolar </description>
	/// </item>
	/// <item>
	/// <description> ZoneTowerVortex </description>
	/// </item>
	/// <item>
	/// <description> ZoneTowerNebula </description>
	/// </item>
	/// <item>
	/// <description> ZoneTowerStardust </description>
	/// </item>
	/// <item>
	/// <description> ZoneDesert </description>
	/// </item>
	/// <item>
	/// <description> ZoneGlowshroom </description>
	/// </item>
	/// <item>
	/// <description> ZoneUndergroundDesert </description>
	/// </item>
	/// <item>
	/// <description> ZoneSkyHeight </description>
	/// </item>
	/// <item>
	/// <description> ZoneOverworldHeight </description>
	/// </item>
	/// <item>
	/// <description> ZoneDirtLayerHeight </description>
	/// </item>
	/// <item>
	/// <description> ZoneRockLayerHeight </description>
	/// </item>
	/// <item>
	/// <description> ZoneUnderworldHeight </description>
	/// </item>
	/// <item>
	/// <description> ZoneBeach </description>
	/// </item>
	/// <item>
	/// <description> ZoneRain </description>
	/// </item>
	/// <item>
	/// <description> ZoneSandstorm </description>
	/// </item>
	/// <item>
	/// <description> ZoneOldOneArmy </description>
	/// </item>
	/// <item>
	/// <description> ZoneGraveyard </description>
	/// </item>
	/// </list>
	/// </summary>
	public Player Player { get; internal set; }

	/// <summary> The x-coordinate of the tile the player is standing on. </summary>
	public int PlayerFloorX => (int)Player.Center.X / 16;

	/// <summary> The y-coordinate of the tile the player is standing on. </summary>
	public int PlayerFloorY => (int)(Player.Bottom.Y + 8f) / 16;

	public Tile GetPlayerCentreTile()
		=> Main.tile[(int)Player.Center.X / 16, (int)Player.Center.Y / 16];

	/// <summary> Whether or not the player is in the sky biome, where harpies and wyverns spawn. </summary>
	public bool Sky { get; internal set; }

	/// <summary> Whether or not the player is inside the jungle temple, where Lihzahrds spawn. </summary>
	public bool Lihzahrd { get; internal set; }

	/// <summary>
	/// Whether or not the player is in front of a player-placed wall or in a large town. If this is true, enemies that can attack through walls
	/// should not spawn (unless an invasion is in progress).
	/// </summary>
	public bool PlayerSafe { get; internal set; }

	/// <summary> Whether or not there is an invasion going on and the player is near it. </summary>
	public bool Invasion { get; internal set; }

	/// <summary> Whether or not the tile the NPC will spawn in contains water. </summary>
	public bool Water { get; internal set; }
	/// <summary> Whether or not the tile the NPC will spawn in contains honey. </summary>
	public bool Honey { get; internal set; }

	/// <summary> Whether or not the NPC will spawn on a granite block or the player is near a granite biome. </summary>
	public bool Granite { get; internal set; }

	/// <summary> Whether or not the NPC will spawn on a marble block or the player is near a marble biome. </summary>
	public bool Marble { get; internal set; }

	/// <summary> Whether or not the player is in a spider cave or the NPC will spawn near one. </summary>
	public bool SpiderCave { get; internal set; }

	/// <summary> Whether or not the player is in a town. This is used for spawning critters instead of monsters. </summary>
	public bool PlayerInTown { get; internal set; }

	/// <summary> Whether or not the player is in front of a desert wall or the NPC will spawn near one. </summary>
	public bool DesertCave { get; internal set; }

	/// <summary>
	/// Whether or not the NPC is horizontally within the range near the player in which NPCs cannot spawn. If this is true, it also means that it is
	/// vertically outside of the range near the player in which NPCs cannot spawn.
	/// </summary>
	public bool SafeRangeX { get; internal set; }

	public bool OverWorld { get; internal set; }

	public bool UnderGround { get; internal set; }

	public bool Ocean { get; internal set; }

	public bool Beach { get; internal set; }

	public bool Caverns { get; internal set; }

	public int ProperGroundTileType { get; internal set; }

	public int WallTileType { get; internal set; }

	/// <summary> Set as Math.Abs(Main.windSpeedTarget) &gt;= 0.4f;. Affects stinkbugs, butterflies, </summary>
	public bool TooWindyForButterflies { get; internal set; }

	/// <summary>
	/// Set as (double)Main.windSpeedTarget &lt; -0.4 || (double)Main.windSpeedTarget &gt; 0.4; Affects Water striders, Gold Water Striders, Grebes.
	/// </summary>
	/// <remarks>
	/// Almost identical to <see cref="TooWindyForButterflies"/>. Only produces a different result when Main.windSpeedTarget is exactly 0.4 or -0.4
	/// </remarks>
	public bool TooWindyForWaterStriders { get; internal set; }
	public static bool TrueDayTime => NPC.dayTimeHax;
	public static bool TrueRaining => NPC.rainingHax;
	public static float TrueCloudAlpha => NPC.cloudAlphaHax;
}