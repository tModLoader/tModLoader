using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a type of wall that can be added by a mod. Only one instance of this class will ever exist for each type of wall that is added. Any hooks that are called will be called by the instance corresponding to the wall type.
	/// </summary>
	public class ModWall
	{
		/// <summary>
		/// The mod which has added this type of ModWall.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The name of this type of wall.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The internal ID of this type of wall.
		/// </summary>
		public ushort Type {
			get;
			internal set;
		}

		internal string texture;
		/// <summary>
		/// The default type of sound made when this wall is hit. Defaults to 0.
		/// </summary>
		public int soundType = 0;
		/// <summary>
		/// The default style of sound made when this wall is hit. Defaults to 1.
		/// </summary>
		public int soundStyle = 1;
		/// <summary>
		/// The default type of dust made when this wall is hit. Defaults to 0.
		/// </summary>
		public int dustType = 0;
		/// <summary>
		/// The default type of item dropped when this wall is killed. Defaults to 0, which means no item.
		/// </summary>
		public int drop = 0;

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
		/// Creates a ModTranslation object that you can use in AddMapEntry.
		/// </summary>
		/// <param name="key">The key for the ModTranslation. The full key will be MapObject.ModName.key</param>
		/// <returns></returns>
		public ModTranslation CreateMapEntryName(string key = null) {
			if (string.IsNullOrEmpty(key)) {
				key = Name;
			}
			return mod.GetOrCreateTranslation(string.Format("Mods.{0}.MapObject.{1}", mod.Name, key));
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

		/// <summary>
		/// Allows you to modify the name and texture path of this wall when it is autoloaded. Return true to autoload this wall. When a wall is autoloaded, that means you do not need to manually call Mod.AddWall. By default returns the mod's autoload property.
		/// </summary>
		public virtual bool Autoload(ref string name, ref string texture) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Allows you to set the properties of this wall. Many properties are stored as arrays throughout Terraria's code.
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Allows you to customize which sound you want to play when the wall at the given coordinates is hit. Return false to stop the game from playing its default sound for the wall. Returns true by default.
		/// </summary>
		public virtual bool KillSound(int i, int j) {
			return true;
		}

		/// <summary>
		/// Allows you to change how many dust particles are created when the wall at the given coordinates is hit.
		/// </summary>
		public virtual void NumDust(int i, int j, bool fail, ref int num) {
		}

		/// <summary>
		/// Allows you to modify the default type of dust created when the wall at the given coordinates is hit. Return false to stop the default dust (the type parameter) from being created. Returns true by default.
		/// </summary>
		public virtual bool CreateDust(int i, int j, ref int type) {
			type = dustType;
			return true;
		}

		/// <summary>
		/// Allows you to customize which items the wall at the given coordinates drops. Return false to stop the game from dropping the tile's default item (the type parameter). Returns true by default.
		/// </summary>
		public virtual bool Drop(int i, int j, ref int type) {
			type = drop;
			return true;
		}

		/// <summary>
		/// Allows you to determine what happens when the tile at the given coordinates is killed or hit with a hammer. Fail determines whether the tile is mined (whether it is killed).
		/// </summary>
		public virtual void KillWall(int i, int j, ref bool fail) {
		}

		/// <summary>
		/// Whether or not the wall at the given coordinates can be killed by an explosion (ie. bombs). Returns true by default; return false to stop an explosion from destroying it.
		/// </summary>
		public virtual bool CanExplode(int i, int j) {
			return true;
		}

		/// <summary>
		/// Allows you to choose which minimap entry the wall at the given coordinates will use. 0 is the first entry added by AddMapEntry, 1 is the second entry, etc. Returns 0 by default.
		/// </summary>
		public virtual ushort GetMapOption(int i, int j) {
			return 0;
		}

		/// <summary>
		/// Allows you to determine how much light this wall emits. This can also let you light up the block in front of this wall.
		/// </summary>
		public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
		}

		/// <summary>
		/// Called whenever the world randomly decides to update the tile containing this wall in a given tick. Useful for things such as growing or spreading.
		/// </summary>
		public virtual void RandomUpdate(int i, int j) {
		}

		/// <summary>
		/// Allows you to animate your wall. Use frameCounter to keep track of how long the current frame has been active, and use frame to change the current frame.
		/// </summary>
		public virtual void AnimateWall(ref byte frame, ref byte frameCounter) {
		}

		/// <summary>
		/// Allows you to draw things behind the wall at the given coordinates. Return false to stop the game from drawing the wall normally. Returns true by default.
		/// </summary>
		public virtual bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of the wall at the given coordinates.
		/// </summary>
		public virtual void PostDraw(int i, int j, SpriteBatch spriteBatch) {
		}

		/// <summary>
		/// Called after this wall is placed in the world by way of the item provided.
		/// </summary>
		public virtual void PlaceInWorld(int i, int j, Item item) {
		}
	}
}
