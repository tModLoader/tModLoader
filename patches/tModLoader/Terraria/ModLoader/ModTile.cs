using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Terraria.ModLoader;

/// <summary>
/// This class represents a type of tile that can be added by a mod. Only one instance of this class will ever exist for each type of tile that is added. Any hooks that are called will be called by the instance corresponding to the tile type. This is to prevent the game from using a massive amount of memory storing tile instances.<br/>
/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Tile">Basic Tile Guide</see> teaches the basics of making a modded tile.
/// </summary>
public abstract class ModTile : ModBlockType
{
	/// <summary> The height of a group of animation frames for this tile. Defaults to 0, which disables animations. </summary>
	public int AnimationFrameHeight { get; set; }

	/// <summary> A multiplier describing how much this block resists harvesting. Higher values will make it take longer to harvest. <br/> Defaults to 1f.
	/// <para/> For example a MineResist value of 2f, such as used by <see cref="TileID.Pearlstone"/>, would require roughly twice as many hits to mine. Conversely, a MineResist value of 0.5f, such as used by <see cref="TileID.Sand"/>, would require roughtly half as many hits to mine
	/// <para/> To find an appropriate value, see the <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#mineresist">wiki</see>.
	/// <para/> Use <see cref="MinPick"/> to adjust the minimum pickaxe power required to mine this tile. </summary>
	public float MineResist { get; set; } = 1f;

	/// <summary> The minimum pickaxe power required for pickaxes to mine this block. <br/> Defaults to 0.
	/// <para/> For example a MinPick value of 50, such as what <see cref="TileID.Meteorite"/> uses, would require a pickaxe with at least 50% pickaxe power (<see cref="Item.pick"/>) to break.
	/// <para/> To find an appropriate value, see the <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#minpick">wiki</see>.
	/// <para/> Use <see cref="MineResist"/> to adjust how long a tile takes to be mined.</summary>
	public int MinPick { get; set; }

	/// <summary> An array of the IDs of tiles that this tile can be considered as when looking for crafting stations. </summary>
	public int[] AdjTiles { get; set; } = new int[0];

	public override string LocalizationCategory => "Tiles";

	/// <summary> The highlight texture used when this tile is selected by smart interact. Defaults to adding "_Highlight" onto the main texture. </summary>
	public virtual string HighlightTexture => Texture + "_Highlight";

	public bool IsDoor => TileID.Sets.OpenDoorID[Type] != -1 || TileID.Sets.CloseDoorID[Type] != -1;

	/// <summary>
	/// A convenient method for adding this tile's Type to the given array. This can be used with the arrays in TileID.Sets.RoomNeeds.
	/// </summary>
	public void AddToArray(ref int[] array)
	{
		Array.Resize(ref array, array.Length + 1);
		array[array.Length - 1] = Type;
	}

	/// <summary>
	/// Adds an entry to the minimap for this tile with the given color and display name. This should be called in SetDefaults.
	/// <br/> For a typical tile that has a map display name, use <see cref="ModBlockType.CreateMapEntryName"/> as the name parameter for a default key using the pattern "Mods.{ModName}.Tiles.{ContentName}.MapEntry".
	/// <br/> If a tile will be using multiple map entries, it is suggested to use <c>this.GetLocalization("CustomMapEntryName")</c>. Modders can also re-use the display name localization of items, such as <c>ModContent.GetInstance&lt;ItemThatPlacesThisStyle&gt;().DisplayName</c>. 
	/// <br/><br/> Multiple map entries are suitable for tiles that need a different color or hover text for different tile styles. Vanilla code uses this mostly only for chest and dresser tiles. Map entries will be given a corresponding map option value, counting from 0, according to the order in which they are added. Map option values don't necessarily correspond to tile styles.
	/// <br/> <see cref="ModBlockType.GetMapOption"/> will be used to choose which map entry is used for a given coordinate.
	/// <br/><br/> Vanilla map entries for most furniture tiles tend to be fairly generic, opting to use a single map entry to show "Table" for all styles of tables instead of the style-specific text such as "Wooden Table", "Honey Table", etc. To use these existing localizations, use the <see cref="Language.GetText(string)"/> method with the appropriate key, such as "MapObject.Chair", "MapObject.Door", "ItemName.WorkBench", etc. Consult the source code or ExampleMod to find the existing localization keys for common furniture types.
	/// </summary>
	public void AddMapEntry(Color color, LocalizedText name = null)
	{
		if (!MapLoader.initialized) {
			MapEntry entry = new MapEntry(color, name);
			if (!MapLoader.tileEntries.Keys.Contains(Type)) {
				MapLoader.tileEntries[Type] = new List<MapEntry>();
			}
			MapLoader.tileEntries[Type].Add(entry);
		}
	}

	/// <summary>
	/// <inheritdoc cref="AddMapEntry(Color, LocalizedText)"/>
	/// <br/><br/> <b>Overload specific:</b> This overload has an additional <paramref name="nameFunc"/> parameter. This function will be used to dynamically adjust the hover text. The parameters for the function are the default display name, x-coordinate, and y-coordinate. This function is most typically used for chests and dressers to show the current chest name, if assigned, instead of the default chest name. <see href="https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/Furniture/ExampleChest.cs">ExampleMod's ExampleChest</see> is one example of this functionality.
	/// </summary>
	public void AddMapEntry(Color color, LocalizedText name, Func<string, int, int, string> nameFunc)
	{
		if (!MapLoader.initialized) {
			MapEntry entry = new MapEntry(color, name, nameFunc);
			if (!MapLoader.tileEntries.Keys.Contains(Type)) {
				MapLoader.tileEntries[Type] = new List<MapEntry>();
			}
			MapLoader.tileEntries[Type].Add(entry);
		}
	}

	/// <summary>
	/// Manually registers an item to drop for the provided tile styles. Use this for tile styles that don't have an item that places them. For example, open door tiles don't have any item that places them, but they should drop an item when destroyed. A tile style with no registered drop and no fallback drop will not drop anything when destroyed.<br/><br/>
	/// This method can also be used to register the fallback item drop. The fallback item will drop for any tile with a style that does not have a manual or automatic item drop.<br/>
	/// To register the fallback item, omit the tileStyles parameter.<br/><br/>
	/// If a mod removes content, manually specifying a replacement/fallback item allows users to recover something from the tile.<br/>
	/// If more control over tile item drops is required, such as conditional drops, custom data on dropped items, or multiple item drops, use <see cref="GetItemDrops(int, int)"/>.<br/>
	/// </summary>
	/// <param name="itemType"></param>
	/// <param name="tileStyles"></param>
	public void RegisterItemDrop(int itemType, params int[] tileStyles) {
		// Runs before TileLoader.FinishSetup
		if (tileStyles == null || tileStyles.Length == 0) {
			TileLoader.tileTypeAndTileStyleToItemType[(Type, -1)] = itemType;
			return;
		}
		foreach (var tileStyle in tileStyles) {
			TileLoader.tileTypeAndTileStyleToItemType[(Type, tileStyle)] = itemType;
		}
	}

	protected sealed override void Register()
	{
		ModTypeLookup<ModTile>.Register(this);
		Type = (ushort)TileLoader.ReserveTileID();
		TileLoader.tiles.Add(this);
	}

	public sealed override void SetupContent()
	{
		TextureAssets.Tile[Type] = ModContent.Request<Texture2D>(Texture);

		SetStaticDefaults();

		//in Terraria.ObjectData.TileObject data make the following public:
		//  newTile, newSubTile, newAlternate, addTile, addSubTile, addAlternate
		if (TileObjectData.newTile.Width > 1 || TileObjectData.newTile.Height > 1) {
			TileObjectData.FixNewTile();
			throw new Exception("It appears that you have an error surrounding TileObjectData.AddTile in " + GetType().FullName) { HelpLink = "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-FAQ#tileobjectdataaddtile-issues" };
		}
		if (Main.tileLavaDeath[Type])
			Main.tileObsidianKill[Type] = true;

		if (Main.tileSolid[Type])
			Main.tileNoSunLight[Type] = true;

		PostSetDefaults();

		if (TileID.Sets.HasOutlines[Type])
			TextureAssets.HighlightMask[Type] = ModContent.Request<Texture2D>(HighlightTexture);

		TileID.Search.Add(FullName, Type);
	}

	/// <summary>
	/// Allows you to override some default properties of this tile, such as Main.tileNoSunLight and Main.tileObsidianKill.
	/// </summary>
	public virtual void PostSetDefaults()
	{
	}

	/// <summary>
	/// Whether or not the smart interact function can select this tile. Useful for things like chests. Defaults to false.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="settings">Use if you need special conditions, like settings.player.HasItem(ItemID.LihzahrdPowerCell)</param>
	/// <returns></returns>
	public virtual bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
	{
		return false;
	}

	/// <summary>
	/// Allows you to modify the smart interact parameters for the tile. Parameters already preset by deriving from TileObjectData defined for the tile.
	/// <br/>Example usage: Beds/Dressers which have separate interactions based on where to click.
	/// </summary>
	/// <param name="width">Amount of tiles in x direction for which the smart interact should select for</param>
	/// <param name="height">Amount of tiles in y direction for which the smart interact should select for</param>
	/// <param name="frameWidth">Width of each tile, in pixels</param>
	/// <param name="frameHeight">Height of each tile, in pixels</param>
	/// <param name="extraY">Additional offset applied after calculations with frameHeight, in pixels</param>
	public virtual void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
	{
	}

	/// <summary>
	/// Modify the parameters for the entity sitting on this furniture tile with its type registered to <see cref="TileID.Sets.CanBeSatOnForPlayers"/>.
	/// <br/>This is also called on NPCs sitting on this tile! To access the entity (player or NPC), use info.restingEntity.
	/// <br/>This gets called when calling <see cref="PlayerSittingHelper.SitDown"/>, when the town NPC decides to sit, and each tick while the player is sitting on a suitable furniture. i and j derived from "(entity.Bottom + new Vector2(0f, -2f)).ToTileCoordinates()" or from the tile coordinates the player clicked on.
	/// <br/>Formula: anchorTilePosition.ToWorldCoordinates(8f, 16f) + finalOffset + new Vector2(0, targetDirection * directionOffset).
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="info">The parameters for setting the anchor and offsets. You need to edit this</param>
	public virtual void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
	{
	}

	/// <summary>
	/// Modify the visual player offset when sleeping on this tile with its type registered to <see cref="TileID.Sets.CanBeSleptIn"/>.
	/// <br/>This gets called when calling <see cref="PlayerSleepingHelper.SetIsSleepingAndAdjustPlayerRotation"/>, and each tick while the player is resting in the bed, i and j derived from "(player.Bottom + new Vector2(0f, -2f)).ToTileCoordinates()" or from the tile coordinates the player clicked on.
	/// <br/>Formula: new Point(anchorTilePosition.X, anchorTilePosition.Y + 1).ToWorldCoordinates(8f, 16f) + finalOffset + new Vector2(0, targetDirection * directionOffset).
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="info">The parameters for setting the anchor and offsets. Default values match the regular vanilla bed.</param>
	public virtual void ModifySleepingTargetInfo(int i, int j, ref TileRestingInfo info)
	{
	}

	/// <summary>
	/// Allows you to modify the chance the tile at the given coordinates has of spawning a certain critter when the tile is killed.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="wormChance">Chance for a worm to spawn. Value corresponds to a chance of 1 in X. Vanilla values include: Grass-400, Plants-200, Various Piles-6</param>
	/// <param name="grassHopperChance">Chance for a grass hopper to spawn. Value corresponds to a chance of 1 in X. Vanilla values include: Grass-100, Plants-50</param>
	/// <param name="jungleGrubChance">Chance for a jungle grub to spawn. Value corresponds to a chance of 1 in X. Vanilla values include: JungleVines-250, JunglePlants2-40, PlantDetritus-10</param>
	public virtual void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
	{
	}

	/// <summary>
	/// Allows prevention of item drops from the tile dropping at the given coordinates. Return false to stop the game from dropping the tile's item(s). Returns true by default. See <see cref="GetItemDrops"/> to customize the item drop.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool CanDrop(int i, int j)
	{
		return true;
	}

	/// <summary>
	/// Allows customization of the items the tile at the given coordinates drops.<br/><br/>
	/// The default item drop is determined by finding an item with <see cref="Item.createTile"/> and <see cref="Item.placeStyle"/> matching the type and style of this tile. 
	/// <see cref="ModTile.RegisterItemDrop(int, int[])"/> can be used to manually register item drops for tile styles with no corresponding item. It can also be used to register a fallback item, which will be dropped if no suitable item is found.<br/><br/>
	/// The default behavior should cover 99% of use cases, meaning that overriding this method should only be necessary in extremely unique tiles, such as tiles dropping multiple items, tiles dropping items with custom data, or tiles with custom tile style code.<br/><br/>
	/// When overriding, use <c>yield return new Item(ItemTypeHere);</c> for each spawned item. Note that a random prefix will be applied to these items, if applicable, so if specific prefixes or no prefix is needed for an item drop, it will have to be spawned in manually using <see cref="KillMultiTile(int, int, int, int)"/> or <see cref="KillTile(int, int, ref bool, ref bool, ref bool)"/>.<br/><br/>
	/// The style based drop logic is based on <see cref="TileObjectData"/>. If a tile has custom 'styles' but still wants to make use of <see cref="ModTile.RegisterItemDrop(int, int[])"/>, <c>TileLoader.GetItemDropFromTypeAndStyle(Type, style)</c> can be used to retrieve the associated item drop.<br/><br/>
	/// Use <see cref="CanDrop"/> to conditionally prevent any item drops. Use <see cref="KillMultiTile(int, int, int, int)"/> or <see cref="KillTile(int, int, ref bool, ref bool, ref bool)"/> for other logic such as cleaning up TileEntities or killing chests or signs.<br/>
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual IEnumerable<Item> GetItemDrops(int i, int j)
	{
		// Automatic item derived from item.createTile and item.placeStyle
		Tile tile = Main.tile[i, j];
		int style = TileObjectData.GetTileStyle(tile);
		if (style == -1) {
			style = 0;
		}
		int dropItem = TileLoader.GetItemDropFromTypeAndStyle(tile.type, style);
		if (dropItem > 0) {
			return new[] { new Item(dropItem) };
		}
		return null;
	}

	/// <summary>
	/// Allows you to determine whether or not the tile at the given coordinates can be hit by anything. Returns true by default. blockDamaged currently has no use.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="blockDamaged"></param>
	public virtual bool CanKillTile(int i, int j, ref bool blockDamaged)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine what happens when the tile at the given coordinates is killed or hit with a pickaxe. Fail determines whether the tile is mined, effectOnly makes it so that only dust is created, and noItem stops items from dropping. Use this to clean up extra data such as Chests, Signs, or TileEntities. For tiles larger than 1x1, use <see cref="KillMultiTile(int, int, int, int)"/>.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fail">If true, the tile won't be mined</param>
	/// <param name="effectOnly">If true, only the dust visuals will happen</param>
	/// <param name="noItem">If true, the corresponding item won't drop</param>
	public virtual void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
	}

	/// <summary>
	/// This hook is called exactly once whenever a block encompassing multiple tiles is destroyed. Use this to clean up extra data such as Chests, Signs, or TileEntities. For tiles that are 1x1, use <see cref="KillTile(int, int, ref bool, ref bool, ref bool)"/>.
	/// <para/> The <paramref name="i"/> and <paramref name="j"/> coordinates will be the top left tile of a multi-tile.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="frameX">The TileFrameX of the Tile at the coordinates</param>
	/// <param name="frameY">The TileFrameY of the Tile at the coordinates</param>
	public virtual void KillMultiTile(int i, int j, int frameX, int frameY)
	{
	}

	/// <summary>
	/// Allows you to make things happen when this tile is within a certain range of the player (around the same range water fountains and music boxes work). The closer parameter is whether or not the tile is within the range at which aesthetics like monoliths and music boxes and clocks work. It is false for campfires and heart lanterns.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="closer"></param>
	public virtual void NearbyEffects(int i, int j, bool closer)
	{
	}

	/// <summary>
	/// Only called for torches, when there is one nearby. Use this to contribute to vanilla torch luck calculations.
	/// Typical return values are 1f for a torch in its biome, 0.5f for a weak positive torch, -1f for a torch in an opposing biome, and -0.5f for a weak negative torch.
	/// </summary>
	/// <param name="player">Main.LocalPlayer</param>
	/// <returns></returns>
	public virtual float GetTorchLuck(Player player) => 0f;

	/// <summary>
	/// Allows you to determine whether this tile glows red when the given player has the Dangersense buff.
	/// <br/>This is only called on the local client.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="player">Main.LocalPlayer</param>
	public virtual bool IsTileDangerous(int i, int j, Player player)
	{
		return false;
	}

	/// <summary>
	/// Allows you to determine whether this tile glows <paramref name="sightColor"/> while the local player has the <see href="https://terraria.wiki.gg/wiki/Biome_Sight_Potion">Biome Sight buff</see>.
	/// <br/>Return true and assign to <paramref name="sightColor"/> to allow this tile to glow.
	/// <br/>This is only called on the local client.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="sightColor">The color this tile should glow with, which defaults to <see cref="Color.White"/>.</param>
	public virtual bool IsTileBiomeSightable(int i, int j, ref Color sightColor)
	{
		return false;
	}

	/// <summary>
	/// Allows you to customize whether this tile can glow yellow while having the Spelunker buff, and is also detected by various pets.
	/// <br/>This is only called if Main.tileSpelunker[type] is false.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool IsTileSpelunkable(int i, int j)
	{
		return false;
	}

	/// <summary>
	/// Allows you to determine whether or not the tile will draw itself flipped in the world.
	/// <para/> If flipping, consider setting <see cref="TileObjectData.DrawFlipHorizontal"/> or <see cref="TileObjectData.DrawFlipVertical"/> as well to ensure that the tile placement preview is also flipped.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteEffects"></param>
	public virtual void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
	{
	}

	/// <summary>
	/// Allows you to customize the position in which this tile is drawn. Width refers to the width of one frame of the tile, offsetY refers to how many pixels below its actual position the tile should be drawn, height refers to the height of one frame of the tile.
	/// <para> By default the values will be set to the values you give this tile's TileObjectData. If this tile has no TileObjectData then they will default to 16, 0, and 16, respectively.</para>
	/// <para> tileFrameX and tileFrameY allow you to change which frames are drawn, keeping tile.TileFrameX/Y intact for other purposes.</para>
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="width"></param>
	/// <param name="offsetY"></param>
	/// <param name="height"></param>
	/// <param name="tileFrameX"></param>
	/// <param name="tileFrameY"></param>
	public virtual void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
	{
	}

	/// <summary>
	/// Allows you to animate your tile. Use frameCounter to keep track of how long the current frame has been active, and use frame to change the current frame. This is called once an update. Use AnimateIndividualTile to animate specific tile instances directly.
	/// <example><code>if (++frameCounter > 8)
	///{
	///	frameCounter = 0;
	///	if (++frame > 5)
	///	{
	///		frame = 0;
	///	}
	///}</code>
	///	or, to mimic another tile, simply:
	///	<code>frame = Main.tileFrame[TileID.FireflyinaBottle];</code></example>
	/// </summary>
	public virtual void AnimateTile(ref int frame, ref int frameCounter)
	{
	}

	/// <summary>
	/// Animates an individual tile. i and j are the coordinates of the Tile in question. frameXOffset and frameYOffset should be used to specify an offset from the tiles frameX and frameY. "frameYOffset = modTile.animationFrameHeight * Main.tileFrame[type];" will already be set before this hook is called, taking into account the TileID-wide animation set via AnimateTile.
	/// Use this hook for off-sync animations (lightning bug in a bottle), temporary animations (trap chests), or TileEntities to achieve unique animation behaviors without having to manually draw the tile via PreDraw.
	/// </summary>
	/// <param name="type">The tile type.</param>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="frameXOffset">The offset to frameX.</param>
	/// <param name="frameYOffset">The offset to frameY.</param>
	public virtual void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
	{
	}

	/// <summary>
	/// Allows you to make stuff happen whenever the tile at the given coordinates is drawn. For example, creating dust or changing the color the tile is drawn in.
	/// SpecialDraw will only be called if coordinates are added using Main.instance.TilesRenderer.AddSpecialLegacyPoint here.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	/// <param name="drawData">Various information about the tile that is being drawn, such as color, framing, glow textures, etc.</param>
	public virtual void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
	}

	/// <summary>
	/// Special Draw. Only called if coordinates are added using Main.instance.TilesRenderer.AddSpecialLegacyPoint during DrawEffects. Useful for drawing things that would otherwise be impossible to draw due to draw order, such as items in item frames.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	public virtual void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
	{
	}

	/// <summary>
	/// Called whenever this tile updates due to being placed or being next to a tile that is changed. Return false to stop the game from carrying out its default TileFrame operations. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="resetFrame"></param>
	/// <param name="noBreak"></param>
	public virtual bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make something happen when this tile is right-clicked by the player. Return true to indicate that a tile interaction has occurred, preventing other right click actions like minion targeting from happening. Returns false by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <returns>Return true to indicate that a tile interaction has occurred, preventing other right click actions like minion targeting from happening. Returns false by default.</returns>
	public virtual bool RightClick(int i, int j)
	{
		return false;
	}

	/// <summary>
	/// Allows you to make something happen when the mouse hovers over this tile. Useful for showing item icons or text on the mouse.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual void MouseOver(int i, int j)
	{
	}

	/// <summary>
	/// Allows you to make something happen when the mouse hovers over this tile, even when the player is far away. Useful for showing what's written on signs, etc.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual void MouseOverFar(int i, int j)
	{
	}

	/// <summary>
	/// Allows you to determine whether the given item can become selected when the cursor is hovering over this tile and the auto selection keybind is pressed.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="item">The inventory item</param>
	public virtual bool AutoSelect(int i, int j, Item item)
	{
		return false;
	}

	/// <summary>
	/// Allows you to make something happen when a wire current passes through this tile. Both <see cref="Wiring.SkipWire(int, int)"/> and <see cref="NetMessage.SendTileSquare(int, int, int, int, TileChangeType)"/> are usually required in the logic used in this method to correctly work.
	/// <br/>Only called on the server and single player. All wiring happens on the world, not multiplayer clients. 
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual void HitWire(int i, int j)
	{
	}

	/// <summary>
	/// Allows you to control how hammers slope this tile. Return true to allow it to slope normally. Returns true by default. Called on the local Client and Single Player.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual bool Slope(int i, int j)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make something happen when a player stands on this type of tile. For example, you can make the player slide as if on ice.
	/// </summary>
	/// <param name="player"></param>
	public virtual void FloorVisuals(Player player)
	{
	}

	/// <summary>
	/// Whether or not this tile creates dust when the player walks on it. Returns false by default.
	/// <para/> Customize the dust spawned using <see cref="WalkDust(ref int, ref bool, ref Color)"/>. The default dust is <see cref="DustID.Snow"/> otherwise.
	/// </summary>
	public virtual bool HasWalkDust()
	{
		return false;
	}

	/// <summary>
	/// Allows you to modify the dust created when the player walks on this tile. The makeDust parameter is whether or not to make dust; you can randomly set this to false to reduce the amount of dust produced.
	/// <para/> The default dust (<paramref name="dustType"/>) is <see cref="DustID.Snow"/> 
	/// </summary>
	/// <param name="dustType"></param>
	/// <param name="makeDust"></param>
	/// <param name="color"></param>
	public virtual void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
	{
	}

	/// <summary>
	/// Allows you to change the style of waterfall that passes through or over this type of tile.
	/// </summary>
	/// <param name="style"></param>
	public virtual void ChangeWaterfallStyle(ref int style)
	{
	}

	/// <summary>
	/// Can be used to adjust tile merge related things that are not possible to do in <see cref="ModBlockType.SetStaticDefaults"/> due to timing.
	/// </summary>
	public virtual void PostSetupTileMerge()
	{
	}

	/// <summary>
	/// Return true if this Tile corresponds to a chest that is locked. Prevents Quick Stacking items into the chest.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <returns></returns>
	public virtual bool IsLockedChest(int i, int j) => false;

	/// <summary>
	/// Allows customization of how a chest unlock is accomplished. By default, frameXAdjustment will be -36, shifting the frameX over to the left
	/// by 1 chest style. If your chests are in a different order, adjust frameXAdjustment accordingly.
	/// This hook is called on the client, and if successful will be called on the server and other clients as the action is synced.
	/// Make sure that the logic is consistent and not dependent on local player data.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="frameXAdjustment">The adjustment made to each Tile.frameX, defaults to -36</param>
	/// <param name="dustType">The dust spawned, defaults to 11</param>
	/// <param name="manual">Set this to true to bypass the code playing the unlock sound, adjusting the tile frame, and spawning dust. Network syncing will still happen.</param>
	/// <returns>Return true if this tile truly is a locked chest and the chest can be unlocked</returns>
	public virtual bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual) => false;

	/// <summary>
	/// Allows customization of how locking a chest is accomplished. By default, frameXAdjustment will be 36, shifting the frameX over to the right
	/// by 1 chest style. If your chests are in a different order, adjust frameXAdjustment accordingly.
	/// This hook is called on the client, and if successful will be called on the server and other clients as the action is synced.
	/// Make sure that the logic is consistent and not dependent on local player data.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="frameXAdjustment">The adjustment made to each Tile.frameX, defaults to 36</param>
	/// <param name="manual">Set this to true to bypass the code playing the lock sound and adjusting the tile frame. Network syncing will still happen.</param>
	/// <returns>Return true if this tile truly is an unlocked chest and the chest can be locked</returns>
	public virtual bool LockChest(int i, int j, ref short frameXAdjustment, ref bool manual) => false;

	/// <summary>
	/// Returns the default name for a chest or dresser with the provided FrameX and FrameY values. <br/>
	/// A typical implementation of a tile with only a single name might return <c>CreateMapEntryName()</c> <br/>
	/// A container with multiple styles might return <c>this.GetLocalization("MapEntry" + option)</c> where option is determined using similar logic to <see cref="ModBlockType.GetMapOption"/> to match the <see cref="AddMapEntry(Color, LocalizedText)"/> entries. Another option is using <c>return Lang._mapLegendCache[MapHelper.TileToLookup(Type, option)];</c>, this will reuse the localizations used for the map entries.
	/// </summary>
	public virtual LocalizedText DefaultContainerName(int frameX, int frameY)
	{
		return null;
	}

	/// <summary>
	/// Allows you to stop this tile at the given coordinates from being replaced via the block swap feature. The tileTypeBeingPlaced parameter is the tile type that will replace the current tile.
	/// <br/> This method is called on the local client. This method is only called if the local player has sufficient pickaxe power to mine the existing tile.
	/// <br/> Return false to block the tile from being replaced. Returns true by default.
	/// <br/> Use this for dynamic logic. <see cref="ID.TileID.Sets.DoesntGetReplacedWithTileReplacement"/>, <see cref="ID.TileID.Sets.DoesntPlaceWithTileReplacement"/>, and <see cref="ID.TileID.Sets.PreventsTileReplaceIfOnTopOfIt"/> cover the most common use cases and should be used instead if possible.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <param name="tileTypeBeingPlaced"></param>
	/// <returns></returns>
	public virtual bool CanReplace(int i, int j, int tileTypeBeingPlaced)
	{
		return true;
	}
}
