using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// This class represents a type of wall that can be added by a mod. Only one instance of this class will ever exist for each type of wall that is added. Any hooks that are called will be called by the instance corresponding to the wall type.
/// <br/><br/> The <see href="https://github.com/tModLoader/tModLoader/wiki/Wall">Wall Guide</see> teaches the basics of making a modded wall.
/// </summary>
public abstract class ModWall : ModBlockType
{
	public override string LocalizationCategory => "Walls";

	/// <summary>
	/// Adds an entry to the minimap for this wall with the given color and display name. This should be called in SetDefaults.
	/// </summary>
	public void AddMapEntry(Color color, LocalizedText name = null)
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
	public void AddMapEntry(Color color, LocalizedText name, Func<string, int, int, string> nameFunc)
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
	/// <param name="style">The style or orientation that will be applied</param>
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
}
