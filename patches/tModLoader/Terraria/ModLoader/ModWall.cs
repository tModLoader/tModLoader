using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Interfaces;

namespace Terraria.ModLoader;

/// <summary>
/// This class represents a type of wall that can be added by a mod. Only one instance of this class will ever exist for each type of wall that is added. Any hooks that are called will be called by the instance corresponding to the wall type.
/// <br/><br/> The <see href="https://github.com/tModLoader/tModLoader/wiki/Wall">Wall Guide</see> teaches the basics of making a modded wall.
/// </summary>
public abstract class ModWall : ModBlockType, IWorldUpdateBlock, IPlaceableBlock, IMineableBlock
{
	public override string LocalizationCategory => "Walls";

	/// <summary>
	/// Adds an entry to the minimap for this wall with the given color and display name. This should be called in SetDefaults.
	/// </summary>
	public override void AddMapEntry(Color color, LocalizedText name = null)
	{
		if (!MapLoader.initialized) {
			MapEntry entry = new MapEntry(color, name);
			if (!MapLoader.wallEntries.Keys.Contains(Type)) {
				MapLoader.wallEntries[Type] = new List<MapEntry>();
			}
			MapLoader.wallEntries[Type].Add(entry);
		}
	}

	/// <summary>
	/// <inheritdoc cref="AddMapEntry(Color, LocalizedText)"/>
	/// <br/><br/> <b>Overload specific:</b> This overload has an additional <paramref name="nameFunc"/> parameter. This function will be used to dynamically adjust the hover text. The parameters for the function are the default display name, x-coordinate, and y-coordinate.
	/// </summary>
	public override void AddMapEntry(Color color, LocalizedText name, Func<string, int, int, string> nameFunc)
	{
		if (!MapLoader.initialized) {
			MapEntry entry = new MapEntry(color, name, nameFunc);
			if (!MapLoader.wallEntries.Keys.Contains(Type)) {
				MapLoader.wallEntries[Type] = new List<MapEntry>();
			}
			MapLoader.wallEntries[Type].Add(entry);
		}
	}

	/// <summary>
	/// Manually registers the item to drop for this wall.<br/>
	/// Only necessary if there is no item which places this wall, such as with unsafe walls dropping safe variants. Otherwise, the item placing this wall will be dropped automatically.<br/>
	/// Use <see cref="ModWall.Drop(int, int, ref int)"/> to conditionally prevent or change drops.
	/// </summary>
	/// <param name="itemType"></param>
	public void RegisterItemDrop(int itemType)
	{
		WallLoader.wallTypeToItemType[Type] = itemType;
	}

	protected override sealed void Register()
	{
		Type = (ushort)WallLoader.ReserveWallID();

		ModTypeLookup<ModWall>.Register(this);
		WallLoader.walls.Add(this);
	}

	public sealed override void SetupContent()
	{
		TextureAssets.Wall[Type] = ModContent.Request<Texture2D>(Texture);

		SetStaticDefaults();

		WallID.Search.Add(FullName, Type);
	}

	/// <summary>
	/// Allows you to customize which items the wall at the given coordinates drops. Return false to stop the game from dropping the tile's default item (the type parameter). Returns true by default.
	/// <br/> The <paramref name="type"/> passed in is the item type of the loaded item with <see cref="Item.createWall"/> matching the type of this Wall. If <see cref="RegisterItemDrop(int)"/> was used, that item will be passed in instead.
	/// </summary>
	public virtual bool Drop(int i, int j, ref int type)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine what happens when the tile at the given coordinates is killed or hit with a hammer. Fail determines whether the tile is mined (whether it is killed).
	/// </summary>
	public virtual void KillWall(int i, int j, ref bool fail)
	{
	}

	/// <summary>
	/// Allows you to animate your wall. Use frameCounter to keep track of how long the current frame has been active, and use frame to change the current frame. Walls are drawn every 4 frames.
	/// </summary>
	public virtual void AnimateWall(ref byte frame, ref byte frameCounter)
	{
	}

	/// <summary>
	/// Called whenever this wall updates due to being placed or being next to a wall that is changed. Return false to stop the game from carrying out its default WallFrame operations. If you return false, make sure to set <see cref="Tile.WallFrameNumber"/>, <see cref="Tile.WallFrameX"/>, and <see cref="Tile.WallFrameY"/> according to the your desired custom framing design. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="randomizeFrame">True if the calling code intends that the frameNumber be randomly changed, such as when placing the wall initially or loading the world, but not when updating due to nearby tile or wall placements</param>
	/// <param name="style">The style or orientation that will be applied. Think of this like a bitmask, adding together the neighbors: 0 for no neighbors, 1 for left, 2 for right, 4 for below, and 8 for above. Additional style variety is added for walls with all 4 neighbors to avoid repeating patterns, meaning values from 15 to 19 all correspond to a wall with all 4 neighbors.</param>
	/// <param name="frameNumber">The random style that will be applied</param>
	public virtual bool WallFrame(int i, int j, bool randomizeFrame, ref int style, ref int frameNumber)
	{
		return true;
	}

	/// <inheritdoc cref="ModPlayer.CanBeTeleportedTo(Vector2, string)"/>
	public virtual bool CanBeTeleportedTo(int i, int j, Player player, string context)
	{
		return true;
	}

	/// <summary>
	/// Called when this wall has been converted to another wall type via biome conversion. Check <paramref name="fromType"/> and <paramref name="toType"/> to see the wall types the wall changed between. For ModWall, this will be called when converting both to and from the wall. <paramref name="conversionType"/> is a <see cref="BiomeConversionID"/> (or <see cref="ModBiomeConversion.Type"/>).
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fromType">The original wall type</param>
	/// <param name="toType">The new wall type</param>
	/// <param name="conversionType">A <see cref="BiomeConversionID"/> (or <see cref="ModBiomeConversion.Type"/>)</param>
	public virtual void OnWallConverted(int i, int j, int fromType, int toType, int conversionType)
	{
	}

	/// <summary>
	/// Called whenever the world randomly decides to update this wall in a given tick. Useful for things such as growing or spreading.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="underground">Whether the wall is considered underground. This usually means below <see cref="Main.worldSurface"/>, but if the "Don't dig up" special world seed (<see cref="Main.remixWorld"/>) is active tiles technically underground might be considered overground for random update purposes. </param>
	public virtual void RandomUpdate(int i, int j, bool underground)
	{
	}

	/// <summary>
	/// Allows you to change what happens when this wall is converted into another biome. If you need to override or add a new conversion to a vanilla tile, use <see cref="WallLoader.RegisterConversion"/>.
	/// <para/> Purification powder uses a separate conversionType, as it doesn't convert hallowed tiles back to purity tiles. Be sure to check for <see cref="BiomeConversionID.PurificationPowder"/> as well as <see cref="BiomeConversionID.Purity"/> when handling corruption/crimson tiles.
	/// <para/> You can use <see cref="WorldGen.ConvertWall"/> to automatically handle tile framing and multiplayer syncing.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <param name="conversionType">The <see cref="BiomeConversionID"/> of the conversion</param>
	public virtual void Convert(int i, int j, int conversionType)
	{
	}

	/// <summary>
	/// Allows you to stop this wall from being placed at the given coordinates. This method is called on the local client.
	/// <para/> For tiles this is also checked during block replacement, but <see cref="ModTile.CanReplace(int, int, int)"/> should be used for replace-specific logic.
	/// <para/> Return false to stop the wall from being placed. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool CanPlace(int i, int j)
	{
		return true;
	}

	/// <summary>
	/// Allows you to do something when this wall is placed. Called on the local Client and Single Player.
	/// <para/> Note that the coordinates in this method account for the placement origin and are not necessarily the coordinates of the top left tile of a multi-tile.
	/// </summary>
	/// <param name="i">The x position in tile coordinates. Equal to Player.tileTargetX</param>
	/// <param name="j">The y position in tile coordinates. Equal to Player.tileTargetY</param>
	/// <param name="item">The item used to place this wall.</param>
	public virtual void PlaceInWorld(int i, int j, Item item)
	{
	}

	/// <summary>
	/// Allows you to customize which sound you want to play when the wall at the given coordinates is hit. Return false to stop the game from playing its default sound for the wall. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fail">If true, the wall is only partially damaged. If false, the wall is fully destroyed.</param>
	public virtual bool KillSound(int i, int j, bool fail)
	{
		return true;
	}

	/// <summary>
	/// Allows you to change how many dust particles are created when the wall at the given coordinates is hit.
	/// <para/> Use <see cref="CreateDust(int, int, ref int)"/> to customize the dust spawned.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fail">If true, the tile is spawning dust for reasons other than the tile actually being destroyed. Worms, projectiles, and other effects cause dust to spawn aside from the usual case of the tile breaking.</param>
	/// <param name="num">The number of dust that will be spawned by the calling code</param>
	public virtual void NumDust(int i, int j, bool fail, ref int num)
	{
	}

	/// <summary>
	/// Allows you to modify the default type of dust created when the wall at the given coordinates is hit. Return false to stop the default dust (the type parameter) from being created. Returns true by default.
	/// <para/> The <paramref name="type"/> parameter defaults to <see cref="DustType"/>.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type">The dust type that will be spawned by the calling code</param>
	public virtual bool CreateDust(int i, int j, ref int type)
	{
		return true;
	}

	/// <summary>
	/// Whether or not the wall at the given coordinates can be killed by an explosion (ie. bombs). Returns true by default; return false to stop an explosion from destroying it.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool CanExplode(int i, int j)
	{
		return true;
	}
}
