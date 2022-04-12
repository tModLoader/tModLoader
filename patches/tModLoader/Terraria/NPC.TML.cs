using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class NPC : IEntityWithGlobals<GlobalNPC>
	{
		public ModNPC ModNPC { get; internal set; }

		internal Instanced<GlobalNPC>[] globalNPCs = Array.Empty<Instanced<GlobalNPC>>();

		public RefReadOnlyArray<Instanced<GlobalNPC>> Globals => new RefReadOnlyArray<Instanced<GlobalNPC>>(globalNPCs);

		/// <summary> Provides access to (static) happiness data associated with this NPC's type. </summary>
		public NPCHappiness Happiness => NPCHappiness.Get(type);

		/// <summary>
		/// Assign a special boss bar, vanilla or modded. Not used by vanilla.
		/// <para>To assign a modded boss bar, use NPC.BossBar = ModContent.GetInstance&lt;ExampleBossBar&gt;(); where ExampleBossBar is a ModBossBar</para>
		/// <para>To assign a vanilla boss bar for whatever reason, fetch it first through the NPC type using Main.BigBossProgressBar.TryGetSpecialVanillaBossBar</para>
		/// </summary>
		public IBigProgressBar BossBar { get; set; }

		private int _defenseDiff = 0;
		public int DefenseModifier {
			get => _defenseDiff;
			set => _defenseDiff = value;
		}

		public int GetTotalDefense() => defense + DefenseModifier;

		/// <summary> Returns whether or not this NPC currently has a (de)buff of the provided type. </summary>
		public bool HasBuff(int type) => FindBuffIndex(type) != -1;

		/// <inheritdoc cref="HasBuff(int)" />
		public bool HasBuff<T>() where T : ModBuff
			=> HasBuff(ModContent.BuffType<T>());

		// Get

		/// <summary> Gets the instance of the specified GlobalNPC type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetGlobalNPC<T>(bool exactType = true) where T : GlobalNPC
			=> GlobalType.GetGlobal<NPC, GlobalNPC, T>(globalNPCs, exactType);

		/// <summary> Gets the local instance of the type of the specified GlobalNPC instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetGlobalNPC<T>(T baseInstance) where T : GlobalNPC
			=> GlobalType.GetGlobal<NPC, GlobalNPC, T>(globalNPCs, baseInstance);

		/// <summary> Gets the instance of the specified GlobalNPC type. </summary>
		public bool TryGetGlobalNPC<T>(out T result, bool exactType = true) where T : GlobalNPC
			=> GlobalType.TryGetGlobal<GlobalNPC, T>(globalNPCs, exactType, out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified GlobalNPC instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetGlobalNPC<T>(T baseInstance, out T result) where T : GlobalNPC
			=> GlobalType.TryGetGlobal<GlobalNPC, T>(globalNPCs, baseInstance, out result);

		/// <summary>
		/// Helper method for getting the parameters for seating a town NPC. Assumes the tile at <paramref name="anchorTilePosition"/> is a valid tile for sitting
		/// </summary>
		public void SitDown(Point anchorTilePosition, out int direction, out Vector2 bottom) {
			Tile tile = Main.tile[anchorTilePosition.X, anchorTilePosition.Y];
			if (tile.type < TileID.Count)
				anchorTilePosition.Y -= 1; // Vanilla compatibility with new hook

			TileRestingInfo info = new TileRestingInfo(this, anchorTilePosition, Vector2.Zero, ((tile.frameX != 0) ? 1 : (-1)), 2);
			TileLoader.ModifySittingTargetInfo(anchorTilePosition.X, anchorTilePosition.Y, tile.type, ref info);
			int anchorX = info.AnchorTilePosition.X;
			int anchorY = info.AnchorTilePosition.Y;
			int directionOffset = info.DirectionOffset;
			direction = info.TargetDirection;
			Vector2 finalOffset = info.FinalOffset;

			bottom = new Point(anchorX, anchorY).ToWorldCoordinates(8f, 16f);
			bottom.X += direction * directionOffset; // Added to match PlayerSittingHelper
			bottom += finalOffset; // Added to match PlayerSittingHelper
		}
	}
}
