using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Enums;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to modify the behavior of any tile in the game, both vanilla and modded.
/// <br/> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
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
	/// Allows prevention of item drops from the tile dropping at the given coordinates. Return false to stop the game from dropping the tile's item(s). Returns true by default. Use <see cref="Drop"/> to spawn additional items.
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

	/// <inheritdoc cref="ModTile.NearbyEffects(int, int, bool)"/>
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
	/// Allows you to customize whether this tile glows <paramref name="sightColor"/> while the local player has the <see href="https://terraria.wiki.gg/wiki/Biome_Sight_Potion">Biome Sight buff</see>.
	/// <br/>Return true to force this behavior, or false to prevent it, overriding vanilla conditions and colors. Returns null by default.
	/// <br/>This is only called on the local client.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The tile type</param>
	/// <param name="sightColor">The color this tile should glow with, which defaults to <see cref="Color.White"/>.</param>
	public virtual bool? IsTileBiomeSightable(int i, int j, int type, ref Color sightColor)
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

	/// <inheritdoc cref="ModTile.DrawEffects(int, int, SpriteBatch, ref TileDrawInfo)"/>
	public virtual void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
	}

	/// <inheritdoc cref="ModTile.EmitParticles(int, int, Tile, short, short, Color, bool)"/>
	public virtual void EmitParticles(int i, int j, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
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
	/// Allows you to draw behind this multi-tile's regular placement preview rendering, or change relevant drawing parameters. This is ran for each rendered section of the multi-tile.
	/// <br/><br/> Make sure to use <paramref name="frame"/> for logic rather than the TileFrameX/Y values of the tile at the provided coordinates, this tile isn't placed yet.
	/// <br/><br/> Return false to stop this section from drawing normally. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The Tile type of the preview that will be drawn.</param>
	/// <param name="spriteBatch"></param>
	/// <param name="frame">The source rectangle that this section will use for rendering.</param>
	/// <param name="position">The position at which this section will be drawn.</param>
	/// <param name="color">The color with which this section will be drawn. This is red when overlapping with another tile.</param>
	/// <param name="validPlacement">Indicates if the tile can occupy this location.</param>
	/// <param name="spriteEffects">The <see cref="SpriteEffects"/> that will be used to draw this section.</param>
	public virtual bool PreDrawPlacementPreview(int i, int j, int type, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw in front of this multi-tile's placement preview rendering. This is ran for each rendered section of the multi-tile.
	/// <br/><br/> Make sure to use <paramref name="frame"/> for logic rather than the TileFrameX/Y values of the tile at the provided coordinates, this tile isn't placed yet.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The Tile type of the preview that was drawn.</param>
	/// <param name="spriteBatch"></param>
	/// <param name="frame">The source rectangle that was used for rendering this section.</param>
	/// <param name="position">The position at which this section was drawn.</param>
	/// <param name="color">The color with which this section was drawn.</param>
	/// <param name="validPlacement">Indicates if the tile can occupy this location.</param>
	/// <param name="spriteEffects">The <see cref="SpriteEffects"/> that were used to draw this section.</param>
	public virtual void PostDrawPlacementPreview(int i, int j, int type, SpriteBatch spriteBatch, Rectangle frame, Vector2 position, Color color, bool validPlacement, SpriteEffects spriteEffects)
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
	/// Called when <see cref="Wiring.HitSwitch"/> is called on the tile. Can be used to skip or override the vanilla behavior by returning false.
	/// <br/><br/> Returns true by default.
	/// </summary>
	public virtual bool HitSwitch(int i, int j, int type)
	{
		return true;
	}

	/// <inheritdoc cref="ModTile.SwitchTiles(int, int, Entity, Vector2, int, int, Vector2, int)"/>
	public virtual bool SwitchTiles(int i, int j, int type, Entity entity, Vector2 position, int width, int height, Vector2 oldPosition, int objType)
	{
		return false;
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

	/// <inheritdoc cref="ModTile.ReplaceTile(int, int, int, int)"/>
	public virtual void ReplaceTile(int i, int j, int type, int targetType, int targetStyle)
	{
	}

	/// <summary>
	/// Can be used to adjust tile merge related things that are not possible to do in <see cref="ModBlockType.SetStaticDefaults"/> due to timing.
	/// </summary>
	public virtual void PostSetupTileMerge()
	{
	}

	/// <summary>
	/// This hook runs before <see cref="ShakeTree(int, int, TreeTypes)"/> and is intended to be used to spawn bonus tree shaking drops and prevent existing drops using <see cref="NPCLoader.blockLoot"/>.
	/// <para/> The tile coordinates provided indicates the leafy top of the tree where entities should be spawned.
	/// <para/> Runs on the server or singleplayer.
	/// </summary>
	/// <param name="x">The x tile coordinate of the tree.</param>
	/// <param name="y">The y tile coordinate of the top of the tree.</param>
	/// <param name="treeType">The type of tree that is being shaken. Modded trees will be <see cref="TreeTypes.Custom"/> by default.</param>
	public virtual void PreShakeTree(int x, int y, TreeTypes treeType)
	{
	}

	/// <summary>
	/// This hook runs when any tree is shaken (See the <see href="https://terraria.wiki.gg/wiki/Trees#Shaking">Tree Shaking wiki page</see>). It is intended to be used to drop the primary item (or NPC). Use <see cref="PreShakeTree(int, int, TreeTypes)"/> to implement bonus drops instead, since this method isn't guaranteed to be called if another mod rolls an item drop.
	/// <para/> If a drop happens, return true to signify that a primary drop has been spawned and to prevent other mods and vanilla code from also attempting to drop the primary item. When spawning the drop be sure to use <see cref="EntitySource_ShakeTree"/> as the source.
	/// <para/> The tile coordinates provided indicates the leafy top of the tree where entities should be spawned.
	/// <para/> Returns false by default. Runs on the server or singleplayer.
	/// </summary>
	/// <param name="x">The x tile coordinate of the tree.</param>
	/// <param name="y">The y tile coordinate of the top of the tree.</param>
	/// <param name="treeType">The type of tree that is being shaken. Modded trees will be <see cref="TreeTypes.Custom"/> by default.</param>
	public virtual bool ShakeTree(int x, int y, TreeTypes treeType)
	{
		return false;
	}

	/// <inheritdoc cref="ModTile.OnTileConverted(int, int, int, int, int)"/>
	public virtual void OnTileConverted(int i, int j, int fromType, int toType, int conversionType)
	{
	}
}
