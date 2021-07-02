using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is the superclass for ModTile and ModWall, combining common code
	/// </summary>
	public abstract class ModBlockType : ModTexturedType
	{
		/// <summary> The internal ID of this type of tile/wall. </summary>
		public ushort Type { get; internal set; }

		/// <summary> The default type of sound made when this tile/wall is hit. Defaults to 0. </summary>
		public int SoundType { get; set; }

		/// <summary> The default style of sound made when this tile/wall is hit. Defaults to 1. </summary>
		public int SoundStyle { get; set; } = 1;

		/// <summary> The default type of dust made when this tile/wall is hit. Defaults to 0. </summary>
		public int DustType { get; set; }

		/// <summary> The default type of item dropped when this tile/wall is killed. Defaults to 0, which means no item. </summary>
		public int ItemDrop { get; set; }

		/// <summary> The vanilla ID of what should replace the instance when a user unloads and subsequently deletes data from your mod in their save file. Defaults to 0. </summary>
		public ushort VanillaFallbackOnModDeletion { get; set; } = 0;

		/// <summary>
		/// Creates a ModTranslation object that you can use in AddMapEntry.
		/// </summary>
		/// <param name="key">The key for the ModTranslation. The full key will be MapObject.ModName.key</param>
		/// <returns></returns>
		public ModTranslation CreateMapEntryName(string key = null) {
			if (string.IsNullOrEmpty(key)) {
				key = Name;
			}
			return LocalizationLoader.GetOrCreateTranslation(Mod, $"MapObject.{key}");
		}

		/// <summary>
		/// Allows you to choose which minimap entry the tile/wall at the given coordinates will use. 0 is the first entry added by AddMapEntry, 1 is the second entry, etc. Returns 0 by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual ushort GetMapOption(int i, int j) {
			return 0;
		}

		/// <summary>
		/// Allows you to set the properties of this tile/wall. Many properties are stored as arrays throughout Terraria's code.
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Allows you to customize which sound you want to play when the tile/wall at the given coordinates is hit. Return false to stop the game from playing its default sound for the tile/wall. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool KillSound(int i, int j) {
			return true;
		}

		/// <summary>
		/// Allows you to change how many dust particles are created when the tile/wall at the given coordinates is hit.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void NumDust(int i, int j, bool fail, ref int num) {
		}

		/// <summary>
		/// Allows you to modify the default type of dust created when the tile/wall at the given coordinates is hit. Return false to stop the default dust (the type parameter) from being created. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool CreateDust(int i, int j, ref int type) {
			type = DustType;
			return true;
		}

		/// <summary>
		/// Allows you to stop this tile/wall from being placed at the given coordinates. Return false to stop the tile/wall from being placed. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool CanPlace(int i, int j) {
			return true;
		}

		/// <summary>
		/// Whether or not the tile/wall at the given coordinates can be killed by an explosion (ie. bombs). Returns true by default; return false to stop an explosion from destroying it.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool CanExplode(int i, int j) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things behind the tile/wall at the given coordinates. Return false to stop the game from drawing the tile normally. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of the tile/wall at the given coordinates. This can also be used to do things such as creating dust.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void PostDraw(int i, int j, SpriteBatch spriteBatch) {
		}

		/// <summary>
		/// Called whenever the world randomly decides to update this tile/wall in a given tick. Useful for things such as growing or spreading.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void RandomUpdate(int i, int j) {
		}

		/// <summary>
		/// Allows you to do something when this tile/wall is placed. Called on the local Client and Single Player.
		/// </summary>
		/// <param name="i">The x position in tile coordinates. Equal to Player.tileTargetX</param>
		/// <param name="j">The y position in tile coordinates. Equal to Player.tileTargetY</param>
		/// <param name="item">The item used to place this tile/wall.</param>
		public virtual void PlaceInWorld(int i, int j, Item item) {
		}

		/// <summary>
		/// Allows you to determine how much light this tile/wall emits.
		/// If it is a tile, make sure you set Main.tileLighted[Type] to true in SetDefaults for this to work.
		/// If it is a wall, it can also let you light up the block in front of this wall.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
		}
	}
}
