using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader;

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
	public void AddToArray(ref int[] array, int type)
	{
		Array.Resize(ref array, array.Length + 1);
		array[array.Length - 1] = type;
	}

	protected sealed override void Register()
	{
		ModTypeLookup<GlobalTile>.Register(this);
		TileLoader.globalTiles.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to modify the chance the tile at the given coordinates has of spawning a certain critter when the tile is killed.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The tile type</param>
	/// <param name="wormChance">Chance for a worm to spawn. Value corresponds to a chance of 1 in X. Vanilla values include: Grass-400, Plants-200, Various Piles-6</param>
	/// <param name="grassHopperChance">Chance for a grass hopper to spawn. Value corresponds to a chance of 1 in X. Vanilla values include: Grass-100, Plants-50</param>
	/// <param name="jungleGrubChance">Chance for a jungle grub to spawn. Value corresponds to a chance of 1 in X. Vanilla values include: JungleVines-250, JunglePlants2-40, PlantDetritus-10</param>
	public virtual void DropCritterChance(int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
	{
	}

	/// <summary>
	/// Allows prevention of item drops from the tile dropping at the given coordinates. Return false to stop the game from dropping the tile's default item. Returns true by default. Use <see cref="Drop"/> to spawn additional items.
	/// </summary>
	public virtual bool CanDrop(int i, int j, int type)
	{
		return true;
	}

	/// <summary>
	/// Allows you to spawn additional items when the tile at the given coordinates drops.
	/// <br/> This hook is called once for multi-tiles. Trees or Cactus call this method for every individual tile.
	/// <br/> For multi-tiles, the coordinates correspond to the tile that triggered this multi-tile to drop, so if checking <see cref="Tile.TileFrameX"/> and <see cref="Tile.TileFrameY"/>, be aware that the coordinates won't necessarily be the top left corner or origin of the multi-tile. Also be aware that some parts of the multi-tile might already be mined out when this method is called, so any math to determine tile style should be done on the tile at the coordinates passed in.
	/// </summary>
	public virtual void Drop(int i, int j, int type)
	{
	}

	/// <summary>
	/// Allows you to determine whether or not the tile at the given coordinates can be hit by anything. Returns true by default. blockDamaged currently has no use.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The tile type</param>
	/// <param name="blockDamaged"></param>
	/// <returns></returns>
	public virtual bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine what happens when the tile at the given coordinates is killed or hit with a pickaxe. If <paramref name="fail"/> is true, the tile will not be mined; <paramref name="effectOnly"/> makes it so that only dust is created; <paramref name="noItem"/> stops items from dropping.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The tile type</param>
	/// <param name="fail">If true, the tile won't be mined</param>
	/// <param name="effectOnly">If true, only the dust visuals will happen</param>
	/// <param name="noItem">If true, the corresponding item won't drop</param>
	public virtual void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
	}

	/// <summary>
	/// Allows you to make things happen when the tile is within a certain range of the player (around the same range water fountains and music boxes work). The closer parameter is whether or not the tile is within the range at which things like campfires and banners work.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The tile type</param>
	/// <param name="closer"></param>
	public virtual void NearbyEffects(int i, int j, int type, bool closer)
	{
	}

	/// <summary>
	/// Allows you to determine whether this tile glows red when the given player has the Dangersense buff.
	/// <br/>Return true to force this behavior, or false to prevent it, overriding vanilla conditions. Returns null by default.
	/// <br/>This is only called on the local client.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The tile type</param>
	/// <param name="player">Main.LocalPlayer</param>
	public virtual bool? IsTileDangerous(int i, int j, int type, Player player)
	{
		return null;
	}

	/// <summary>
	/// Allows you to customize whether this tile can glow yellow while having the Spelunker buff, and is also detected by various pets.
	/// <br/>Return true to force this behavior, or false to prevent it, overriding vanilla conditions. Returns null by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The tile type</param>
	public virtual bool? IsTileSpelunkable(int i, int j, int type)
	{
		return null;
	}

	/// <summary>
	/// Allows you to determine whether or not a tile will draw itself flipped in the world.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The tile type</param>
	/// <param name="spriteEffects"></param>
	public virtual void SetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects)
	{
	}

	/// <summary>
	/// Allows animating tiles that were previously static. Loading a new texture for the tile is required first. Use Main.tileFrameCounter to count game frames and Main.tileFrame to change animation frames.
	/// </summary>
	public virtual void AnimateTile()
	{
	}

	/// <summary>
	/// Allows you to make stuff happen whenever the tile at the given coordinates is drawn. For example, creating dust or changing the color the tile is drawn in.
	/// SpecialDraw will only be called if coordinates are added using Main.instance.TilesRenderer.AddSpecialLegacyPoint here.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The Tile type of the tile being drawn</param>
	/// <param name="spriteBatch">The SpriteBatch that should be used for all draw calls</param>
	/// <param name="drawData">Various information about the tile that is being drawn, such as color, framing, glow textures, etc.</param>
	public virtual void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
	}

	/// <summary>
	/// Special Draw. Only called if coordinates are added using Main.instance.TilesRenderer.AddSpecialLegacyPoint during DrawEffects. Useful for drawing things that would otherwise be impossible to draw due to draw order, such as items in item frames.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The Tile type of the tile being drawn</param>
	/// <param name="spriteBatch">The SpriteBatch that should be used for all draw calls</param>
	public virtual void SpecialDraw(int i, int j, int type, SpriteBatch spriteBatch)
	{
	}

	/// <summary>
	/// Called for every tile that updates due to being placed or being next to a tile that is changed. Return false to stop the game from carrying out its default TileFrame operations. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="resetFrame"></param>
	/// <param name="noBreak"></param>
	/// <returns></returns>
	public virtual bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine which tiles the given tile type can be considered as when looking for crafting stations.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual int[] AdjTiles(int type)
	{
		return new int[0];
	}

	/// <summary>
	/// Allows you to make something happen when any tile is right-clicked by the player.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	public virtual void RightClick(int i, int j, int type)
	{
	}

	/// <summary>
	/// Allows you to make something happen when the mouse hovers over any tile. Useful for showing item icons or text on the mouse.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	public virtual void MouseOver(int i, int j, int type)
	{
	}

	/// <summary>
	/// Allows you to make something happen when the mouse hovers over any tile, even when the player is far away. Useful for showing what's written on signs, etc.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	public virtual void MouseOverFar(int i, int j, int type)
	{
	}

	/// <summary>
	/// Allows you to determine whether the given item can become selected when the cursor is hovering over a tile and the auto selection keybind is pressed.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="item"></param>
	/// <returns></returns>
	public virtual bool AutoSelect(int i, int j, int type, Item item)
	{
		return false;
	}

	/// <summary>
	/// Whether or not the vanilla HitWire code and the HitWire hook is allowed to run. Useful for overriding vanilla behavior by returning false. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual bool PreHitWire(int i, int j, int type)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make something happen when a wire current passes through any tile. Both <see cref="Wiring.SkipWire(int, int)"/> and <see cref="NetMessage.SendTileSquare(int, int, int, int, ID.TileChangeType)"/> are usually required in the logic used in this method to correctly work.
	/// <br/>Only called on the server and single player. All wiring happens on the world, not multiplayer clients. 
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	public virtual void HitWire(int i, int j, int type)
	{
	}

	/// <summary>
	/// Allows you to control how hammers slope any tile. Return true to allow the tile to slope normally. Returns true by default. Called on the local Client and Single Player.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual bool Slope(int i, int j, int type)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make something happen when a player stands on the given type of tile. For example, you can make the player slide as if on ice.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="player"></param>
	public virtual void FloorVisuals(int type, Player player)
	{
	}

	/// <summary>
	/// Allows you to change the style of waterfall that passes through or over any tile.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="style"></param>
	public virtual void ChangeWaterfallStyle(int type, ref int style)
	{
	}

	/// <summary>
	/// Allows you to stop a tile at the given coordinates from being replaced via the block swap feature. The tileTypeBeingPlaced parameter is the tile type that will replace the current tile. The type parameter is the tile type currently at the coordinates.
	/// <br/> This method is called on the local client. This method is only called if the local player has sufficient pickaxe power to mine the existing tile.
	/// <br/> Return false to block the tile from being replaced. Returns true by default.
	/// <br/> Use this for dynamic logic. <see cref="ID.TileID.Sets.DoesntGetReplacedWithTileReplacement"/>, <see cref="ID.TileID.Sets.DoesntPlaceWithTileReplacement"/>, and <see cref="ID.TileID.Sets.PreventsTileReplaceIfOnTopOfIt"/> cover the most common use cases and should be used instead if possible.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <param name="type"></param>
	/// <param name="tileTypeBeingPlaced"></param>
	/// <returns></returns>
	public virtual bool CanReplace(int i, int j, int type, int tileTypeBeingPlaced)
	{
		return true;
	}

	/// <summary>
	/// Can be used to adjust tile merge related things that are not possible to do in <see cref="ModBlockType.SetStaticDefaults"/> due to timing.
	/// </summary>
	public virtual void PostSetupTileMerge()
	{
	}
}
