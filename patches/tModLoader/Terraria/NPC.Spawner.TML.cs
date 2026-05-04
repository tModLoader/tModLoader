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
		/// <para/> Note that there are many fields in <see cref="NPC.Spawner"/> like <see cref="ZoneJungle"/> that appear to be duplicates of Player fields, such as <see cref="Player.ZoneJungle"/>, but you should always used the <see cref="NPC.Spawner"/> fields if present for NPC spawning logic to properly account for custom NPC spawning logic. For example, the dual dungeons secret world seed affects those fields for custom spawning logic separately from the logic used for the Player fields, which would affect visuals and other things.
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
