using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// This is the superclass for ModTile and ModWall, combining common code
/// </summary>
[CloneByReference]
public abstract class ModBlockType : ModTexturedType, ILocalizedModType
{
	/// <summary> The internal ID of this type of tile/wall/liquid. </summary>
	public ushort Type { get; internal set; }

	// NOTE: HitSound and DustType could be implemented in IMineableBlock, but would require more effort to do so.
	/// <summary>
	/// The default style of sound made when this tile/wall is hit.<br/>
	/// Defaults to SoundID.Dig, which is the sound used for tiles such as dirt and sand.
	/// </summary>
	public SoundStyle? HitSound { get; set; } = SoundID.Dig;

	/// <summary> The default type of dust made when this tile/wall is hit.
	/// <para/> Defaults to 0, which is <see cref="DustID.Dirt"/>. To prevent spawning any hit dust, set this to -1 instead. </summary>
	public int DustType { get; set; }
	// End NOTE

	/// <summary>
	/// The vanilla ID of what should replace the instance when a user unloads and subsequently deletes data from your mod in their save file.
	/// <br/><br/> <see cref="Main.tileFrameImportant"/> tiles attempting to fallback to a vanilla <see cref="Main.tileFrameImportant"/> tile need to match the layout (FrameX and FrameY values) of the fallback tile so that the resulting tiles aren't broken.
	/// <br/><br/> Also note that tiles with ModTileEntity won't be able to fallback to a working vanilla Tile+TileEntity. The user will have to mine and replace the tile to spawn the correct TileEntity.
	/// <br/><br/> Defaults to <see cref="TileID.Dirt"/> (0) for tiles.
	/// </summary>
	public ushort VanillaFallbackOnModDeletion { get; set; } = 0;

	public abstract string LocalizationCategory { get; }

	public abstract void AddMapEntry(Color color, LocalizedText name = null);

	public abstract void AddMapEntry(Color color, LocalizedText name, Func<string, int, int, string> nameFunc);

	/// <summary>
	/// Legacy helper method for creating a localization sub-key MapEntry
	/// </summary>
	/// <returns></returns>
	public LocalizedText CreateMapEntryName() => this.GetLocalization("MapEntry", PrettyPrintName);

	/// <summary>
	/// Allows you to modify the properties after initial loading has completed.
	/// <br/> This is where you would set the properties of this tile/wall/liquid. Many properties are stored as arrays throughout Terraria's code.
	/// <br/> For example:
	/// <list type="bullet">
	/// <item> Main.tileSolid[Type] = true; </item>
	/// <item> Main.tileSolidTop[Type] = true; </item>
	/// <item> Main.tileBrick[Type] = true; </item>
	/// <item> Main.tileBlockLight[Type] = true; </item>
	/// </list>
	/// </summary>
	public override void SetStaticDefaults() { }

	/// <summary>
	/// Allows you to choose which minimap entry the tile/wall/liquid at the given coordinates will use. 0 is the first entry added by AddMapEntry, 1 is the second entry, etc. Returns 0 by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual ushort GetMapOption(int i, int j)
	{
		return 0;
	}

	/// <summary>
	/// Allows you to draw things behind the tile/wall/liquid at the given coordinates. Return false to stop the game from drawing the tile normally. Returns true by default.
	/// <para/> Note that tiles are drawn one at a time in sequence (top to bottom, left to right), so this runs immediately before a single tile's sprite is drawn, not before all tiles on screen are rendered.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	public virtual bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of the tile/wall/liquid at the given coordinates. This can also be used to do things such as creating dust.<para/>
	/// Note that this method will be called for tiles even when the tile is <see cref="Tile.IsTileInvisible"/> due to Echo Coating. Use the <see cref="GameContent.Drawing.TileDrawing.IsVisible(Tile)"/> method to skip effects that shouldn't show when the tile is invisible. This method won't be called for invisible walls.
	/// <para/> Note that tiles are drawn one at a time in sequence (top to bottom, left to right), so this runs immediately after a single tile's sprite is drawn. Any effects drawn here will be overdrawn by sprites of tiles drawn after it. Use <see cref="ModTile.SpecialDraw(int, int, SpriteBatch)"/> via <see cref="ModTile.DrawEffects"/> to draw after all tiles on screen have been rendered.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	public virtual void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
	}

	/// <summary>
	/// Allows you to determine how much light this tile/wall/liquid emits.<br/>
	/// If it is a tile, make sure you set Main.tileLighted[Type] to true in SetDefaults for this to work.<br/>
	/// If it is a wall or liquid, it can also let you light up the tile in front of this wall/liquid.<br/>
	/// See <see cref="Terraria.Graphics.Light.TileLightScanner.ApplyTileLight(Tile, int, int, ref Terraria.Utilities.FastRandom, ref Microsoft.Xna.Framework.Vector3)"/> for vanilla tile light values to use as a reference.<br/>
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="r">The red component of light, usually a value between 0 and 1</param>
	/// <param name="g">The green component of light, usually a value between 0 and 1</param>
	/// <param name="b">The blue component of light, usually a value between 0 and 1</param>
	public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
	}
}
