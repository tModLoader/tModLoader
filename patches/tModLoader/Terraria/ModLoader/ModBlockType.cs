using System;
using System.Collections.Generic;
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
	
	internal void AddMapEntryToMapLoader(IDictionary<ushort, IList<MapEntry>> mapEntries, Color color, LocalizedText name, Func<string, int, int, string> nameFunc)
	{
		if (!MapLoader.initialized) {
			MapEntry entry = new MapEntry(color, name, nameFunc);
			if (!mapEntries.Keys.Contains(Type)) {
				mapEntries[Type] = new List<MapEntry>();
			}
			mapEntries[Type].Add(entry);
		}
	}

	/// <summary>
	/// Adds an entry to the minimap for this tile/wall/liquid with the given color and display name. This should be called in SetDefaults.
	/// <br/> For a typical tile that has a map display name, use <see cref="ModBlockType.CreateMapEntryName"/> as the name parameter for a default key using the pattern "Mods.{ModName}.Tiles.{ContentName}.MapEntry".
	/// <br/> If a tile will be using multiple map entries, it is suggested to use <c>this.GetLocalization("CustomMapEntryName")</c>. Modders can also re-use the display name localization of items, such as <c>ModContent.GetInstance&lt;ItemThatPlacesThisStyle&gt;().DisplayName</c>.
	/// <br/><br/> Multiple map entries are suitable for tiles that need a different color or hover text for different tile styles. Vanilla code uses this mostly only for chest and dresser tiles. Map entries will be given a corresponding map option value, counting from 0, according to the order in which they are added. Map option values don't necessarily correspond to tile styles.
	/// <br/> <see cref="ModBlockType.GetMapOption"/> will be used to choose which map entry is used for a given coordinate.
	/// <br/><br/> Vanilla map entries for most furniture tiles tend to be fairly generic, opting to use a single map entry to show "Table" for all styles of tables instead of the style-specific text such as "Wooden Table", "Honey Table", etc. To use these existing localizations, use the <see cref="Language.GetText(string)"/> method with the appropriate key, such as "MapObject.Chair", "MapObject.Door", "ItemName.WorkBench", etc. Consult the source code or ExampleMod to find the existing localization keys for common furniture types. The <c>array</c> array in <c>MapHelper.Initialize</c> has vanilla tile color values and <c>Lang.BuildMapAtlas</c> has the text.
	/// /// <br/><br/> <b>Optional parameter:</b> This overload has an optional <paramref name="nameFunc"/> parameter. This function will be used to dynamically adjust the hover text. The parameters for the function are the default display name, x-coordinate, and y-coordinate. This function is most typically used for chests and dressers to show the current chest name, if assigned, instead of the default chest name. <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Furniture/ExampleChest.cs">ExampleMod's ExampleChest</see> is one example of this functionality.
	/// </summary>
	public abstract void AddMapEntry(Color color, LocalizedText name = null, Func<string, int, int, string> nameFunc = null);

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
