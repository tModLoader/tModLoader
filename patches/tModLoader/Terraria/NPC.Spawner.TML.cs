using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;
public partial class NPC
{
	public partial class Spawner
	{
		/// <summary>
		/// The x-coordinate of the tile the NPC will spawn above.
		/// </summary>
		public int SpawnTileX { get; private set; }

		/// <summary>
		/// The y-coordinate of the tile the NPC will spawn above.
		/// </summary>
		public int SpawnTileY { get; private set; }

		/// <summary>
		/// The tile type (<see cref="TileID"/> or <see cref="ModContent.TileType{T}"/>) at <see cref="SpawnTileX"/> and Y.
		/// </summary>
		public int SpawnTileType { get; private set; }

		/// <summary>
		/// The wall type (<see cref="WallID"/> or <see cref="ModContent.WallType{T}"/>) at <see cref="SpawnTileX"/> and Y.
		/// </summary>
		public int SpawnWallType { get; private set; }

		/// <summary>
		/// The player that this NPC is spawning around.
		/// For convenience, here are the player zones, which are also useful for determining NPC spawn:
		/// (ZoneGranite, ZoneMarble, ZoneHive, ZoneGemCave are not actually proper spawning related checks, they are for visuals only (RGB), determined by the backwall type)
		/// <list type="bullet">
		/// <item><description>ZoneDungeon</description></item>
		/// <item><description>ZoneCorrupt</description></item>
		/// <item><description>ZoneHallow</description></item>
		/// <item><description>ZoneMeteor</description></item>
		/// <item><description>ZoneJungle</description></item>
		/// <item><description>ZoneSnow</description></item>
		/// <item><description>ZoneCrimson</description></item>
		/// <item><description>ZoneWaterCandle</description></item>
		/// <item><description>ZonePeaceCandle</description></item>
		/// <item><description>ZoneTowerSolar</description></item>
		/// <item><description>ZoneTowerVortex</description></item>
		/// <item><description>ZoneTowerNebula</description></item>
		/// <item><description>ZoneTowerStardust</description></item>
		/// <item><description>ZoneDesert</description></item>
		/// <item><description>ZoneGlowshroom</description></item>
		/// <item><description>ZoneUndergroundDesert</description></item>
		/// <item><description>ZoneSkyHeight</description></item>
		/// <item><description>ZoneOverworldHeight</description></item>
		/// <item><description>ZoneDirtLayerHeight</description></item>
		/// <item><description>ZoneRockLayerHeight</description></item>
		/// <item><description>ZoneUnderworldHeight</description></item>
		/// <item><description>ZoneBeach</description></item>
		/// <item><description>ZoneRain</description></item>
		/// <item><description>ZoneSandstorm</description></item>
		/// <item><description>ZoneOldOneArmy</description></item>
		/// <item><description>ZoneGraveyard</description></item>
		/// </list>
		/// </summary>
		public Player Player { get; private set; }

		/* TODO: Spawn logic doesn't seem to use Player Floor anymore. pX and py are centered on the player, and many checks now use SpawnTileX/Y instead.
		/// <summary>
		/// The x-coordinate of the tile the player is standing on.
		/// </summary>
		public int PlayerFloorX { get; private set; }

		/// <summary>
		/// The y-coordinate of the tile the player is standing on.
		/// </summary>
		public int PlayerFloorY { get; private set; }
		*/

		/// <summary>
		/// Whether or not the NPC is horizontally within the range near the player in which NPCs cannot spawn. If this is true, it also means that it is vertically outside of the range near the player in which NPCs cannot spawn.
		/// </summary>
		public bool SafeRangeX { get; private set; }
	}
}
