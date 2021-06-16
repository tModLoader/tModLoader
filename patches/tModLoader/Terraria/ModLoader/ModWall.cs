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
			SetDefaults();
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
		/// Allows you to animate your wall. Use frameCounter to keep track of how long the current frame has been active, and use frame to change the current frame.
		/// </summary>
		public virtual void AnimateWall(ref byte frame, ref byte frameCounter) {
		}
	}
}
