using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary>
/// This is the superclass for <see cref="ModTile"/> and <see cref="ModWall"/>, combining common code
/// </summary>
public abstract class ModTileOrWall : ModBlockType
{
	/// <summary>
	/// The default style of sound made when this tile/wall is hit.<br/>
	/// Defaults to SoundID.Dig, which is the sound used for tiles such as dirt and sand.
	/// </summary>
	public SoundStyle? HitSound { get; set; } = SoundID.Dig;

	/// <summary> The default type of dust made when this tile/wall is hit.
	/// <para/> Defaults to 0, which is <see cref="DustID.Dirt"/>. To prevent spawning any hit dust, set this to -1 instead. </summary>
	public int DustType { get; set; }

	/// <summary>
	/// Called whenever the world randomly decides to update this tile/wall in a given tick. Useful for things such as growing or spreading.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="underground">Whether the tile/wall is considered underground. This usually means below <see cref="Main.worldSurface"/>, but if the "Don't dig up" special world seed (<see cref="Main.remixWorld"/>) is active tiles technically underground might be considered overground for random update purposes. </param>
	public virtual void RandomUpdate(int i, int j, bool underground) { }

	/// <summary>
	/// Allows you to change what happens when this tile/wall is converted into another biome. If you need to override or add a new conversion to a vanilla tile, use <see cref="TileLoader.RegisterConversion"/> and <see cref="WallLoader.RegisterConversion"/>.
	/// <para/> Purification powder uses a separate conversionType, as it doesn't convert hallowed tiles back to purity tiles. Be sure to check for <see cref="BiomeConversionID.PurificationPowder"/> as well as <see cref="BiomeConversionID.Purity"/> when handling corruption/crimson tiles.
	/// <para/> You can use <see cref="WorldGen.ConvertTile(int, int, int)"/> or <see cref="WorldGen.ConvertWall"/> to automatically handle tile framing and multiplayer syncing.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <param name="conversionType">The <see cref="BiomeConversionID"/> of the conversion</param>
	public virtual void Convert(int i, int j, int conversionType) { }

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
	/// Allows you to do something when this tile/wall is placed. Called on the local Client and Single Player.
	/// <para/> Note that the coordinates in this method account for the placement origin and are not necessarily the coordinates of the top left tile of a multi-tile.
	/// </summary>
	/// <param name="i">The x position in tile coordinates. Equal to Player.tileTargetX</param>
	/// <param name="j">The y position in tile coordinates. Equal to Player.tileTargetY</param>
	/// <param name="item">The item used to place this tile/wall.</param>
	public virtual void PlaceInWorld(int i, int j, Item item) { }

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
	public virtual void NumDust(int i, int j, bool fail, ref int num) { }

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
	/// Whether or not the tile/wall at the given coordinates can be killed by an explosion (ie. bombs). Returns true by default; return false to stop an explosion from destroying it.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool CanExplode(int i, int j)
	{
		return true;
	}

	/// <summary>
	/// Allows you to control whether the player can break or place wires at the given tile coordinates. One such example is how wires in the Jungle Temple are not breakable or placeable until Golem is defeated.
	/// <para/> Return true or false to respectively allow or disallow the player from breaking or placing wires, overriding vanilla rules.
	/// <para/> Returns null by default, which allows vanilla wire rules to function normally.
	/// <para/> See also <see cref="CanShowWireStuffHere"/>.
	/// <para/> Called on the local client only, when the player attempts to use <see href="https://terraria.wiki.gg/wiki/Mechanisms">Mechanisms</see>.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool? CanDoWireStuffHere(int i, int j)
	{
		return null;
	}

	/// <summary>
	/// Allows you to control whether the player can see wires at the given tile coordinates. One such example is how wires in the Jungle Temple are not visible until Golem is defeated.
	/// <para/> Return true or false to respectively allow or disallow the player from seeing wires, overriding vanilla rules.
	/// <para/> Returns null by default, which allows vanilla wire visibility rules to function normally.
	/// <para/> See also <see cref="CanDoWireStuffHere"/>.
	/// <para/> Called on the local client only, when the player attempts to use <see href="https://terraria.wiki.gg/wiki/Mechanisms">Mechanisms</see>.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool? CanShowWireStuffHere(int i, int j)
	{
		return null;
	}

	/// <summary>
	/// Allows you to draw things behind the tile/wall at the given coordinates. Return false to stop the game from drawing the tile normally. Returns true by default.
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
	/// Allows you to draw things in front of the tile/wall at the given coordinates. This can also be used to do things such as creating dust.<para/>
	/// Note that this method will be called for tiles even when the tile is <see cref="Tile.IsTileInvisible"/> due to Echo Coating. Use the <see cref="GameContent.Drawing.TileDrawing.IsVisible(Tile)"/> method to skip effects that shouldn't show when the tile is invisible. This method won't be called for invisible walls.
	/// <para/> Note that tiles are drawn one at a time in sequence (top to bottom, left to right), so this runs immediately after a single tile's sprite is drawn. Any effects drawn here will be overdrawn by sprites of tiles drawn after it. Use <see cref="ModTile.SpecialDraw(int, int, SpriteBatch)"/> via <see cref="ModTile.DrawEffects"/> to draw after all tiles on screen have been rendered.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	public virtual void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
	}
}
