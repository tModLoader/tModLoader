using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a type of wall that can be added by a mod. Only one instance of this class will ever exist for each type of wall that is added. Any hooks that are called will be called by the instance corresponding to the wall type.
	/// </summary>
	public abstract class ModWall : ModBlockType
	{
		/// <summary>
		/// Adds an entry to the minimap for this wall with the given color and display name. This should be called in SetDefaults.
		/// </summary>
		public void AddMapEntry(Color color, LocalizedText name = null) {
			if (!MapLoader.initialized) {
				MapEntry entry = new MapEntry(color, name);
				if (!MapLoader.wallEntries.Keys.Contains(Type)) {
					MapLoader.wallEntries[Type] = new List<MapEntry>();
				}
				MapLoader.wallEntries[Type].Add(entry);
			}
		}

		/// <summary>
		/// Adds an entry to the minimap for this wall with the given color and display name. This should be called in SetDefaults.
		/// </summary>
		public void AddMapEntry(Color color, ModTranslation name) {
			if (!MapLoader.initialized) {
				MapEntry entry = new MapEntry(color, name);
				if (!MapLoader.wallEntries.Keys.Contains(Type)) {
					MapLoader.wallEntries[Type] = new List<MapEntry>();
				}
				MapLoader.wallEntries[Type].Add(entry);
			}
		}

		/// <summary>
		/// Adds an entry to the minimap for this wall with the given color, default display name, and display name function. The parameters for the function are the default display name, x-coordinate, and y-coordinate. This should be called in SetDefaults.
		/// </summary>
		public void AddMapEntry(Color color, LocalizedText name, Func<string, int, int, string> nameFunc) {
			if (!MapLoader.initialized) {
				MapEntry entry = new MapEntry(color, name, nameFunc);
				if (!MapLoader.wallEntries.Keys.Contains(Type)) {
					MapLoader.wallEntries[Type] = new List<MapEntry>();
				}
				MapLoader.wallEntries[Type].Add(entry);
			}
		}

		/// <summary>
		/// Adds an entry to the minimap for this wall with the given color, default display name, and display name function. The parameters for the function are the default display name, x-coordinate, and y-coordinate. This should be called in SetDefaults.
		/// </summary>
		public void AddMapEntry(Color color, ModTranslation name, Func<string, int, int, string> nameFunc) {
			if (!MapLoader.initialized) {
				MapEntry entry = new MapEntry(color, name, nameFunc);
				if (!MapLoader.wallEntries.Keys.Contains(Type)) {
					MapLoader.wallEntries[Type] = new List<MapEntry>();
				}
				MapLoader.wallEntries[Type].Add(entry);
			}
		}

		protected override sealed void Register() {
			Type = (ushort)WallLoader.ReserveWallID();

			ModTypeLookup<ModWall>.Register(this);
			WallLoader.walls.Add(this);
		}

		public sealed override void SetupContent() {
			TextureAssets.Wall[Type] = ModContent.Request<Texture2D>(Texture);

			SetStaticDefaults();

			WallID.Search.Add(FullName, Type);
		}

		/// <summary>
		/// Allows you to customize which items the wall at the given coordinates drops. Return false to stop the game from dropping the tile's default item (the type parameter). Returns true by default.
		/// </summary>
		public virtual bool Drop(int i, int j, ref int type) {
			type = ItemDrop;
			return true;
		}

		/// <summary>
		/// Allows you to determine what happens when the tile at the given coordinates is killed or hit with a hammer. Fail determines whether the tile is mined (whether it is killed).
		/// </summary>
		public virtual void KillWall(int i, int j, ref bool fail) {
		}

		/// <summary>
		/// Allows you to animate your wall. Use frameCounter to keep track of how long the current frame has been active, and use frame to change the current frame. Walls are drawn every 4 frames.
		/// </summary>
		public virtual void AnimateWall(ref byte frame, ref byte frameCounter) {
		}

		/// <summary>
		/// Called whenever this wall updates due to being placed or being next to a wall that is changed. Return false to stop the game from carrying out its default WallFrame operations. If you return false, make sure to set <see cref="Tile.WallFrameNumber"/>, <see cref="Tile.WallFrameX"/>, and <see cref="Tile.WallFrameY"/> according to the your desired custom framing design. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		/// <param name="randomizeFrame">True if the calling code intends that the frameNumber be randomly changed, such as when placing the wall initially or loading the world, but not when updating due to nearby tile or wall placements</param>
		/// <param name="style">The style or orientation that will be applied</param>
		/// <param name="frameNumber">The random style that will be applied</param>
		public virtual bool WallFrame(int i, int j, bool randomizeFrame, ref int style, ref int frameNumber) {
			return true;
		}
	}
}
