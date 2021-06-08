using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to modify the behavior of any tile in the game. Create an instance of an overriding class then call Mod.AddGlobalTile to use this.
	/// </summary>
	public abstract class GlobalTile : GlobalBlockType
	{
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

		protected sealed override void Register() {
			ModTypeLookup<GlobalTile>.Register(this);
			TileLoader.globalTiles.Add(this);
		}

		public sealed override void SetupContent() => SetDefaults();

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
		/// Allows you to determine what happens when the tile at the given coordinates is killed or hit with a pickaxe. If <paramref name="fail"/> is true, the tile will not be mined; <paramref name="effectOnly"/> makes it so that only dust is created; <paramref name="noItem"/> stops items from dropping.
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
		/// Allows you to make things happen when the tile is within a certain range of the player (around the same range water fountains and music boxes work). The closer parameter is whether or not the tile is within the range at which things like campfires and banners work.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="type"></param>
		/// <param name="closer"></param>
		public virtual void NearbyEffects(int i, int j, int type, bool closer) {
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
		/// Allows you to make stuff happen whenever the tile at the given coordinates is drawn. For example, creating dust or changing the color the tile is drawn in.
		/// SpecialDraw will only be called if coordinates are added using Main.instance.TilesRenderer.AddSpecialLegacyPoint here.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		/// <param name="drawData">Various information about the tile that is being drawn, such as color, framing, glow textures, etc.</param>
		public virtual void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
		}

		/// <summary>
		/// Special Draw. Only called if coordinates are added using Main.instance.TilesRenderer.AddSpecialLegacyPoint during DrawEffects. Useful for drawing things that would otherwise be impossible to draw due to draw order, such as items in item frames.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void SpecialDraw(int i, int j, int type, SpriteBatch spriteBatch) {
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
		/// Allows you to determine whether the given item can become selected when the cursor is hovering over a tile and the auto selection keybind is pressed.
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
	}
}
