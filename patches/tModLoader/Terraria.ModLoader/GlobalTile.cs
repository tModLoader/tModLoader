using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to modify the behavior of any tile in the game. Create an instance of an overriding class then call Mod.AddGlobalTile to use this.
	/// </summary>
	public class GlobalTile
	{
		/// <summary>
		/// The mod to which this GlobalTile belongs to.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The name of this GlobalTile instance.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// A convenient method for adding an integer to the end of an array. This can be used with the arrays in TileID.Sets.RoomNeeds.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="type"></param>
		public void AddToArray(ref int[] array, int type) {
			Array.Resize(ref array, array.Length + 1);
			array[array.Length - 1] = type;
		}

		/// <summary>
		/// Allows the given type of tile to grow the given modded tree.
		/// </summary>
		/// <param name="soilType"></param>
		/// <param name="tree"></param>
		public void AddModTree(int soilType, ModTree tree) {
			TileLoader.trees[soilType] = tree;
		}

		/// <summary>
		/// Allows the given type of tile to grow the given modded palm tree.
		/// </summary>
		/// <param name="soilType"></param>
		/// <param name="palmTree"></param>
		public void AddModPalmTree(int soilType, ModPalmTree palmTree) {
			TileLoader.palmTrees[soilType] = palmTree;
		}

		/// <summary>
		/// Allows the given type of tile to grow the given modded cactus.
		/// </summary>
		/// <param name="soilType"></param>
		/// <param name="cactus"></param>
		public void AddModCactus(int soilType, ModCactus cactus) {
			TileLoader.cacti[soilType] = cactus;
		}

		/// <summary>
		/// Allows you to automatically load a GlobalTile instead of using Mod.AddGlobalTile. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name. Use this method to either force or stop an autoload or to control the internal name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual bool Autoload(ref string name) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Allows you to modify the properties of any tile in the game. Most properties are stored as arrays throughout the Terraria code.
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Allows you to customize which sound you want to play when the tile at the given coordinates is hit. Return false to stop the game from playing its default sound for the tile. Returns true by default.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual bool KillSound(int i, int j, int type) {
			return true;
		}

		/// <summary>
		/// Allows you to change how many dust particles are created when the tile at the given coordinates is hit.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="fail"></param>
		/// <param name="num"></param>
		public virtual void NumDust(int i, int j, int type, bool fail, ref int num) {
		}

		/// <summary>
		/// Allows you to modify the default type of dust created when the tile at the given coordinates is hit. Return false to stop the default dust (the dustType parameter) from being created. Returns true by default.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="dustType"></param>
		/// <returns></returns>
		public virtual bool CreateDust(int i, int j, int type, ref int dustType) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the chance the tile at the given coordinates has of spawning a certain critter when the tile is killed.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="wormChance"></param>
		/// <param name="grassHopperChance"></param>
		/// <param name="jungleGrubChance"></param>
		public virtual void DropCritterChance(int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
		}

		/// <summary>
		/// Allows you to customize which items the tile at the given coordinates drops. Return false to stop the game from dropping the tile's default item. Returns true by default.
		/// </summary>
		public virtual bool Drop(int i, int j, int type) {
			return true;
		}

		/// <summary>
		/// Allows you to determine whether or not the tile at the given coordinates can be hit by anything. Returns true by default. blockDamaged currently has no use.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="blockDamaged"></param>
		/// <returns></returns>
		public virtual bool CanKillTile(int i, int j, int type, ref bool blockDamaged) {
			return true;
		}

		/// <summary>
		/// Allows you to determine what happens when the tile at the given coordinates is killed or hit with a pickaxe. Fail determines whether the tile is mined, effectOnly makes it so that only dust is created, and noItem stops items from dropping.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="fail"></param>
		/// <param name="effectOnly"></param>
		/// <param name="noItem"></param>
		public virtual void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
		}

		/// <summary>
		/// Whether or not the tile at the given coordinates can be killed by an explosion (ie. bombs). Returns true by default; return false to stop an explosion from destroying it.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual bool CanExplode(int i, int j, int type) {
			return true;
		}

		/// <summary>
		/// Allows you to make things happen when the tile is within a certain range of the player (around the same range water fountains and music boxes work). The closer parameter is whether or not the tile is within the range at which things like campfires and banners work.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="closer"></param>
		public virtual void NearbyEffects(int i, int j, int type, bool closer) {
		}

		/// <summary>
		/// Allows you to determine how much light the block emits. Make sure you set Main.tileLighted[type] to true in SetDefaults for this to work.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		public virtual void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
		}

		/// <summary>
		/// Allows you to determine whether the block glows red when the given player has the Dangersense buff.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public virtual bool Dangersense(int i, int j, int type, Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to determine whether or not a tile will draw itself flipped in the world.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="spriteEffects"></param>
		public virtual void SetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects) {
		}

		/// <summary>
		/// Allows animating tiles that were previously static. Loading a new texture for the tile is required first. Use Main.tileFrameCounter to count game frames and Main.tileFrame to change animation frames.
		/// </summary>
		public virtual void AnimateTile() {
		}

		/// <summary>
		/// Allows you to draw things behind the tile at the given coordinates. Return false to stop the game from drawing the tile normally. Returns true by default.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="spriteBatch"></param>
		/// <returns></returns>
		public virtual bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			return true;
		}

		/// <summary>
		/// Allows you to make stuff happen whenever the tile at the given coordinates is drawn. For example, creating dust or changing the color the tile is drawn in.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="spriteBatch"></param>
		/// <param name="drawColor"></param>
		/// <param name="nextSpecialDrawIndex">The special draw count. Use with Main.specX and Main.specY and then increment to draw special things after the main tile drawing loop is complete via DrawSpecial.</param>
		public virtual void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
		}

		/// <summary>
		/// Allows you to draw things in front of the tile at the given coordinates. This can also be used to do things such as creating dust. Called on active tiles. See also ModWorld.PostDrawTiles.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="spriteBatch"></param>
		public virtual void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
		}

		/// <summary>
		/// Special Draw. Only called if coordinates are placed in Main.specX/Y during DrawEffects. Useful for drawing things that would otherwise be impossible to draw due to draw order, such as items in item frames.
		/// </summary>
		/// <param name="i">The i.</param>
		/// <param name="j">The j.</param>
		public virtual void SpecialDraw(int i, int j, int type, SpriteBatch spriteBatch) {
		}

		/// <summary>
		/// Called for every tile the world randomly decides to update in a given tick. Useful for things such as growing or spreading.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		public virtual void RandomUpdate(int i, int j, int type) {
		}

		/// <summary>
		/// Called for every tile that updates due to being placed or being next to a tile that is changed. Return false to stop the game from carrying out its default TileFrame operations. Returns true by default.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="resetFrame"></param>
		/// <param name="noBreak"></param>
		/// <returns></returns>
		public virtual bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak) {
			return true;
		}

		/// <summary>
		/// Allows you to stop a tile from being placed at the given coordinates. Return false to block the tile from being placed. Returns true by default.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual bool CanPlace(int i, int j, int type) {
			return true;
		}

		/// <summary>
		/// Allows you to determine which tiles the given tile type can be considered as when looking for crafting stations.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual int[] AdjTiles(int type) {
			return new int[0];
		}

		/// <summary>
		/// Allows you to make something happen when any tile is right-clicked by the player.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		public virtual void RightClick(int i, int j, int type) {
		}

		/// <summary>
		/// Allows you to make something happen when the mouse hovers over any tile. Useful for showing item icons or text on the mouse.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		public virtual void MouseOver(int i, int j, int type) {
		}

		/// <summary>
		/// Allows you to make something happen when the mouse hovers over any tile, even when the player is far away. Useful for showing what's written on signs, etc.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		public virtual void MouseOverFar(int i, int j, int type) {
		}

		/// <summary>
		/// Allows you to determine whether the given item can become selected when the cursor is hovering over a tile and the auto selection hotkey is pressed.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool AutoSelect(int i, int j, int type, Item item) {
			return false;
		}

		/// <summary>
		/// Whether or not the vanilla HitWire code and the HitWire hook is allowed to run. Useful for overriding vanilla behavior by returning false. Returns true by default.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual bool PreHitWire(int i, int j, int type) {
			return true;
		}

		/// <summary>
		/// Allows you to make something happen when a wire current passes through any tile.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		public virtual void HitWire(int i, int j, int type) {
		}

		/// <summary>
		/// Allows you to control how hammers slope any tile. Return true to allow the tile to slope normally. Returns true by default. Called on the local Client and Single Player.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual bool Slope(int i, int j, int type) {
			return true;
		}

		/// <summary>
		/// Allows you to make something happen when a player stands on the given type of tile. For example, you can make the player slide as if on ice.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="player"></param>
		public virtual void FloorVisuals(int type, Player player) {
		}

		/// <summary>
		/// Allows you to change the style of waterfall that passes through or over any tile.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="style"></param>
		public virtual void ChangeWaterfallStyle(int type, ref int style) {
		}

		/// <summary>
		/// Allows a tile to support a sapling that can eventually grow into a tree. The type of the sapling should be returned here. Returns -1 by default. The style parameter will determine which sapling is chosen if multiple sapling types share the same ID; even if you only have a single sapling in an ID, you must still set this to 0.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="style"></param>
		/// <returns></returns>
		public virtual int SaplingGrowthType(int type, ref int style) {
			return -1;
		}

		/// <summary>
		/// Allows you to do something when this tile is placed. Called on the local Client and Single Player.
		/// </summary>
		/// <param name="i">The x position in tile coordinates. Equal to Player.tileTargetX</param>
		/// <param name="j">The y position in tile coordinates. Equal to Player.tileTargetY</param>
		/// <param name="item">The item used to place this tile.</param>
		public virtual void PlaceInWorld(int i, int j, Item item) {
		}
	}
}
