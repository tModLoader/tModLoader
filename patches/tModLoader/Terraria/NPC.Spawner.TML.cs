using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;
public partial class NPC
{
	// TODO: Should we make all the fields that don't match 1.4.4's NPCSpawnInfo private and add the "fixed" named properties? Or just have modders change their code?
	public partial class Spawner
	{
		/// <summary>
		/// The x-coordinate of the tile the NPC will spawn above.
		/// </summary>
		public int SpawnTileX;

		/// <summary>
		/// The y-coordinate of the tile the NPC will spawn above.
		/// </summary>
		public int SpawnTileY;

		/// <summary>
		/// The tile type (<see cref="TileID"/> or <see cref="ModContent.TileType{T}"/>) at <see cref="SpawnTileX"/> and Y.
		/// </summary>
		public int SpawnTileType;

		/// <summary>
		/// The wall type (<see cref="WallID"/> or <see cref="ModContent.WallType{T}"/>) at <see cref="SpawnTileX"/> and Y.
		/// </summary>
		public int SpawnWallType;

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
		public Player Player;

		/* TODO: Spawn logic doesn't seem to use Player Floor anymore. pX and py are centered on the player, and many checks now use SpawnTileX/Y instead.
		/// <summary>
		/// The x-coordinate of the tile the player is standing on.
		/// </summary>
		public int PlayerFloorX;

		/// <summary>
		/// The y-coordinate of the tile the player is standing on.
		/// </summary>
		public int PlayerFloorY;
		*/

		/// <summary>
		/// Whether or not the player is in the sky biome, where harpies and wyverns spawn.
		/// </summary>
		public bool Sky => skyMob;

		/// <summary>
		/// Whether or not the player is inside the jungle temple, where Lihzahrds spawn.
		/// </summary>
		public bool Lihzahrd => ZoneLihzhardTemple; // Remove?

		/// <summary>
		/// Whether or not the player is in front of a player-placed wall or in a large town. If this is true, enemies that can attack through walls should not spawn (unless an invasion is in progress).
		/// </summary>
		public bool PlayerSafe => noWorms;

		/// <summary>
		/// Whether or not there is an invasion going on and the player is near it.
		/// </summary>
		public bool Invasion => invaders;

		/// <summary>
		/// Whether or not the tile the NPC will spawn in contains water.
		/// </summary>
		public bool Water => waterTile;

		/// <summary>
		/// Whether or not the NPC will spawn on a granite block or the player is near a granite biome.
		/// </summary>
		public bool Granite => nearGranite;

		/// <summary>
		/// Whether or not the NPC will spawn on a marble block or the player is near a marble biome.
		/// </summary>
		public bool Marble => nearMarble;

		/// <summary>
		/// Whether or not the player is in a spider cave or the NPC will spawn near one.
		/// </summary>
		public bool SpiderCave => spawnSpider;

		/// <summary>
		/// Whether or not the player is in a town. This is used for spawning critters instead of monsters.
		/// </summary>
		public bool PlayerInTown => spawnFriendly;

		/// <summary>
		/// Whether or not the player is in front of a desert wall or the NPC will spawn near one.
		/// </summary>
		public bool DesertCave => spawnUndergroundDesert;

		/// <summary>
		/// Whether or not the NPC is horizontally within the range near the player in which NPCs cannot spawn. If this is true, it also means that it is vertically outside of the range near the player in which NPCs cannot spawn.
		/// </summary>
		public bool SafeRangeX;
	}
}
