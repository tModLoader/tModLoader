namespace Terraria.ModLoader
{
	/// <summary>
	/// A struct that stores information regarding where an NPC is naturally spawning and the player it is spawning around. This serves to reduce the parameter count for ModNPC.CanSpawn and GlobalNPC.EditSpawnPool.
	/// </summary>
	public struct NPCSpawnInfo
	{
		/// <summary>
		/// The x-coordinate of the tile the NPC will spawn above.
		/// </summary>
		public int spawnTileX;
		//num
		/// <summary>
		/// The y-coordinate of the tile the NPC will spawn above.
		/// </summary>
		public int spawnTileY;
		//num2
		public int spawnTileType;
		//num3
		/// <summary>
		/// The player that this NPC is spawning around.
		/// For convenience, here are the player zones, which are also useful for determining NPC spawn:
		/// <list type="bullet">
		/// <item><description>ZoneDungeon</description></item>
		/// <item><description>ZoneCorrupt</description></item>
		/// <item><description>ZoneHoly</description></item>
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
		/// </list>
		/// </summary>
		public Player player;
		//Main.player[j]
		/// <summary>
		/// The x-coordinate of the tile the player is standing on.
		/// </summary>
		public int playerFloorX;
		//num25
		/// <summary>
		/// The y-coordinate of the tile the player is standing on.
		/// </summary>
		public int playerFloorY;
		//num26
		/// <summary>
		/// Whether or not the player is in the sky biome, where harpies and wyverns spawn.
		/// </summary>
		public bool sky;
		//flag
		/// <summary>
		/// Whether or not the player is inside the jungle temple, where Lihzahrds spawn.
		/// </summary>
		public bool lihzahrd;
		//flag2
		/// <summary>
		/// Whether or not the player is in front of a player-placed wall or in a large town. If this is true, enemies that can attack through walls should not spawn (unless an invasion is in progress).
		/// </summary>
		public bool playerSafe;
		//flag3
		/// <summary>
		/// Whether or not there is an invasion going on and the player is near it.
		/// </summary>
		public bool invasion;
		//flag4
		/// <summary>
		/// Whether or not the tile the NPC will spawn in contains water.
		/// </summary>
		public bool water;
		//flag5
		/// <summary>
		/// Whether or not the NPC will spawn on a granite block or the player is near a granite biome.
		/// </summary>
		public bool granite;
		//flag7
		/// <summary>
		/// Whether or not the NPC will spawn on a marble block or the player is near a marble biome.
		/// </summary>
		public bool marble;
		//flag8
		/// <summary>
		/// Whether or not the player is in a spider cave or the NPC will spawn near one.
		/// </summary>
		public bool spiderCave;
		//flag9
		/// <summary>
		/// Whether or not the player is in a town. This is used for spawning critters instead of monsters.
		/// </summary>
		public bool playerInTown;
		//flag10
		/// <summary>
		/// Whether or not the player is in front of a desert wall or the NPC will spawn near one.
		/// </summary>
		public bool desertCave;
		//flag11
		/// <summary>
		/// Whether Plantera is defeated and the world is in hardmode. This isn't needed (it's easy to find by yourself), but it's a local flag in NPC.SpawnNPC, so it is included for completeness.
		/// </summary>
		public bool planteraDefeated;
		//flag12
		/// <summary>
		/// Whether or not the NPC is horizontally within the range near the player in which NPCs cannot spawn. If this is true, it also means that it is vertically outside of the range near the player in which NPCs cannot spawn.
		/// </summary>
		public bool safeRangeX;
		//flag14
	}
}
