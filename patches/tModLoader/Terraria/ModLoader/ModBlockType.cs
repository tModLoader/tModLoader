using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// This is the superclass for ModTile and ModWall, combining common code
/// </summary>
public abstract class ModBlockType : ModTexturedType, ILocalizedModType
{
	/// <summary> The internal ID of this type of tile/wall. </summary>
	public ushort Type { get; internal set; }

	/// <summary>
	/// The default style of sound made when this tile/wall is hit.<br/>
	/// Defaults to SoundID.Dig, which is the sound used for tiles such as dirt and sand.
	/// </summary>
	public SoundStyle? HitSound { get; set; } = SoundID.Dig;

	/// <summary> The default type of dust made when this tile/wall is hit.
	/// <para/> Defaults to 0, which is <see cref="DustID.Dirt"/>. To prevent spawning any hit dust, set this to -1 instead. </summary>
	public int DustType { get; set; }

	/// <summary>
	/// The vanilla ID of what should replace the instance when a user unloads and subsequently deletes data from your mod in their save file.
	/// <br/><br/> <see cref="Main.tileFrameImportant"/> tiles attempting to fallback to a vanilla <see cref="Main.tileFrameImportant"/> tile need to match the layout (FrameX and FrameY values) of the fallback tile so that the resulting tiles aren't broken.
	/// <br/><br/> Also note that tiles with ModTileEntity won't be able to fallback to a working vanilla Tile+TileEntity. The user will have to mine and replace the tile to spawn the correct TileEntity.
	/// <br/><br/> Defaults to <see cref="TileID.Dirt"/> (0).
	/// </summary>
	public ushort VanillaFallbackOnModDeletion { get; set; } = 0;

	public abstract string LocalizationCategory { get; }

	/// <summary>
	/// Legacy helper method for creating a localization sub-key MapEntry
	/// </summary>
	/// <returns></returns>
	public LocalizedText CreateMapEntryName() => this.GetLocalization("MapEntry", PrettyPrintName);

	/// <summary>
	/// Allows you to modify the properties after initial loading has completed.
	/// <br/> This is where you would set the properties of this tile/wall. Many properties are stored as arrays throughout Terraria's code.
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
	/// Allows you to choose which minimap entry the tile/wall at the given coordinates will use. 0 is the first entry added by AddMapEntry, 1 is the second entry, etc. Returns 0 by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual ushort GetMapOption(int i, int j)
	{
		return 0;
	}

	/// <summary>
	/// Allows you to customize which sound you want to play when the tile/wall at the given coordinates is hit. Return false to stop the game from playing its default sound for the tile/wall. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fail">If true, the tile/wall is only partially damaged. If false, the tile/wall is fully destroyed.</param>
	public virtual bool KillSound(int i, int j, bool fail)
	{
		return true;
	}

	/// <summary>
	/// Allows you to change how many dust particles are created when the tile/wall at the given coordinates is hit.
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
	/// Allows you to modify the default type of dust created when the tile/wall at the given coordinates is hit. Return false to stop the default dust (the type parameter) from being created. Returns true by default.
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
	/// Allows you to stop this tile/wall from being placed at the given coordinates. This method is called on the local client.
	/// <para/> For tiles this is also checked during block replacement, but <see cref="ModTile.CanReplace(int, int, int)"/> should be used for replace-specific logic.
	/// <para/> Return false to stop the tile/wall from being placed. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool CanPlace(int i, int j)
	{
		return true;
	}

	/// <summary>
	/// Whether or not the tile/wall at the given coordinates can be killed by an explosion (ie. bombs). Returns true by default; return false to stop an explosion from destroying it.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool CanExplode(int i, int j)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things behind the tile/wall at the given coordinates. Return false to stop the game from drawing the tile normally. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	public virtual bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of the tile/wall at the given coordinates. This can also be used to do things such as creating dust.<para/>
	/// Note that this method will be called for tiles even when the tile is <see cref="Tile.IsTileInvisible"/> due to Echo Coating. Use the <see cref="GameContent.Drawing.TileDrawing.IsVisible(Tile)"/> method to skip effects that shouldn't show when the tile is invisible. This method won't be called for invisible walls.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	public virtual void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
	}

	/// <summary>
	/// Called whenever the world randomly decides to update this tile/wall in a given tick. Useful for things such as growing or spreading.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual void RandomUpdate(int i, int j)
	{
	}

	/// <summary>
	/// Allows you to do something when this tile/wall is placed. Called on the local Client and Single Player.
	/// <para/> Note that the coordinates in this method account for the placement origin and are not necessarily the coordinates of the top left tile of a multi-tile.
	/// </summary>
	/// <param name="i">The x position in tile coordinates. Equal to Player.tileTargetX</param>
	/// <param name="j">The y position in tile coordinates. Equal to Player.tileTargetY</param>
	/// <param name="item">The item used to place this tile/wall.</param>
	public virtual void PlaceInWorld(int i, int j, Item item)
	{
	}

	/// <summary>
	/// Allows you to determine how much light this tile/wall emits.<br/>
	/// If it is a tile, make sure you set Main.tileLighted[Type] to true in SetDefaults for this to work.<br/>
	/// If it is a wall, it can also let you light up the block in front of this wall.<br/>
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

	/// <summary>
	/// Allows you to change what happens when this tile/wall is converted into another biome. If you need to override or add a new conversion to a vanilla tile, use <see cref="TileLoader.RegisterConversion"/> and <see cref="WallLoader.RegisterConversion"/>.
	/// <para/> Purification powder uses a separate conversionType, as it doesn't convert hallowed tiles back to purity tiles. Be sure to check for <see cref="BiomeConversionID.PurificationPowder"/> as well as <see cref="BiomeConversionID.Purity"/> when handling corruption/crimson tiles.
	/// <para/> You can use <see cref="WorldGen.ConvertTile(int, int, int)"/> or <see cref="WorldGen.ConvertWall"/> to automatically handle tile framing and multiplayer syncing.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <param name="conversionType">The <see cref="BiomeConversionID"/> of the conversion</param>
	public virtual void Convert(int i, int j, int conversionType)
	{
	}
}
