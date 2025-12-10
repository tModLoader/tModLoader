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
	/// <summary> The height of a group of animation frames for this tile. Defaults to 0, which disables animations.
	/// <para/> Used in conjunction with <see cref="AnimateTile(ref int, ref int)"/> to automatically animate tiles. Use <see cref="AnimateIndividualTile(int, int, int, ref int, ref int)"/> as well if needed.
	/// <para/> An easy way to set this correctly without doing any math is to set this to the value of <see cref="TileObjectData.CoordinateFullHeight"/>.
	/// <para/> Note that this assumes animation frames are laid out vertically in the tile spritesheet, if that is not the case then <see cref="AnimateIndividualTile"/> will need to be used to apply AnimationFrameHeight to X coordinates instead.
	/// </summary>
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

	/// <summary>
	/// The highlight texture used when this tile is selected by smart interact. Defaults to adding "_Highlight" onto the main texture.
	/// <br/><br/> <see cref="TileID.Sets.HasOutlines"/> must be set to true for this to texture to be used.
	/// </summary>
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
	/// <br/><br/> Vanilla map entries for most furniture tiles tend to be fairly generic, opting to use a single map entry to show "Table" for all styles of tables instead of the style-specific text such as "Wooden Table", "Honey Table", etc. To use these existing localizations, use the <see cref="Language.GetText(string)"/> method with the appropriate key, such as "MapObject.Chair", "MapObject.Door", "ItemName.WorkBench", etc. Consult the source code or ExampleMod to find the existing localization keys for common furniture types. The <c>array</c> array in <c>MapHelper.Initialize</c> has vanilla tile color values and <c>Lang.BuildMapAtlas</c> has the text.
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
	/// <br/><br/> <b>Overload specific:</b> This overload has an additional <paramref name="nameFunc"/> parameter. This function will be used to dynamically adjust the hover text. The parameters for the function are the default display name, x-coordinate, and y-coordinate. This function is most typically used for chests and dressers to show the current chest name, if assigned, instead of the default chest name. <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Furniture/ExampleChest.cs">ExampleMod's ExampleChest</see> is one example of this functionality.
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
	/// Manually registered item drops take precedence over the automatic item drop system.<br/><br/>
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
	/// Allows you to make things happen when this tile is within a certain range of the player, such as how banners, campfire, and monoliths work.
	/// <para/> This method will be called on tiles within 2 specific ranges, once for calculating visual effects and another time for calculating gameplay effects:
	/// <para/> When calculating <b>visual effects</b>, the <paramref name="closer"/> parameter will be <see langword="true"/>. The visual effect range depend on the game window resolution, zoom level, and lighting mode, so it will not be reliable for gameplay effects but rather it adjusts to the players view of the game world. This is suitable for monoliths, water fountains, and music boxes.
	/// <para/> When calculating <b>gameplay effects</b>, the <paramref name="closer"/> parameter will be <see langword="false"/>. The gameplay effect range will always be a 169x124 tile rectangle centered on the player. This is suitable for tiles that give buffs like the sunflower, banners, heart lantern, and campfire.
	/// <para/> Make sure to check <paramref name="closer"/> when using this method to ensure the effects of this tile are applying to the intended range.
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
	/// Allows you to animate your tile. Use frameCounter to keep track of how long the current frame has been active, and use frame to change the current frame. This is called once an update.
	/// <para/> <see cref="AnimationFrameHeight"/> must be set for the animation timing set in this method to actually apply to the tile drawing. Use <see cref="AnimateIndividualTile(int, int, int, ref int, ref int)"/> to animate specific tile instances directly.
	/// <example><code>// Cycle 5 frames of animation spending 8 ticks on each
	///if (++frameCounter >= 8) {
	///	frameCounter = 0;
	///	if (++frame >= 5) {
	///		frame = 0;
	///	}
	///}</code>
	/// or, more compactly:
	/// <code>if (++frameCounter >= 8) {
	/// 	frameCounter = 0;
	/// 	frame = ++frame % 5;
	///}</code>
	///	or, to mimic another tile, simply:
	///	<code>frame = Main.tileFrame[TileID.FireflyinaBottle];</code></example>
	///	<para/> <b>Note:</b> For the smoothest animation, <paramref name="frameCounter"/> should count up to a multiple of 4 before advancing the <paramref name="frame"/> value. This is because tiles are rendered every 4 game draws but this method is called every game update. Values that aren't multiples of 4 would result in some frames drawing for twice as long as the next animation frame, resulting in jerky animation. Similarly, attempting to change frames at intervals shorter than 4 will result in skipped animation frames.
	/// </summary>
	public virtual void AnimateTile(ref int frame, ref int frameCounter)
	{
	}

	/// <summary>
	/// Animates an individual tile. <paramref name="i"/> and <paramref name="j"/> are the coordinates of the Tile in question. <paramref name="frameXOffset"/> and <paramref name="frameYOffset"/> should be used to specify an offset from the tiles <see cref="Tile.TileFrameX"/> and <see cref="Tile.TileFrameY"/>. <c>frameYOffset = modTile.AnimationFrameHeight * Main.tileFrame[type];</c> will already be set before this hook is called, taking into account the TileID-wide animation set via <see cref="AnimateTile(ref int, ref int)"/>.
	/// <para/> Use this hook for off-sync animations (lightning bug in a bottle), state specific animations (campfires), temporary animations (trap chests), or TileEntities to achieve unique animation behaviors without having to manually draw the tile via <see cref="ModBlockType.PreDraw(int, int, SpriteBatch)"/>.
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
	/// Allows you to adjust how the tile at the given coordinates is drawn. For example, changing the color the tile is drawn in.
	/// <para/> Can also be used to register this tile location for additional rendering after all tiles are drawn normally. <see cref="SpecialDraw(int, int, SpriteBatch)"/> will be called if coordinates are added using <c>Main.instance.TilesRenderer.AddSpecialLegacyPoint</c> or <c>Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileCounterType.CustomNonSolid or CustomSolid)</c> here or in <see cref="ModBlockType.PreDraw(int, int, SpriteBatch)"/>.
	/// <para/> <b>Note:</b> Previously ExampleMod examples showed spawning particles (dust or gore) in this method, but they should be spawned in <see cref="EmitParticles(int, int, Tile, short, short, Color, bool)"/> instead now. This is because particles are only spawned under specific conditions and those conditions are baked into the logic calling EmitParticles.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	/// <param name="drawData">Various information about the tile that is being drawn, such as color, framing, glow textures, etc.</param>
	public virtual void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
	}

	/// <summary>
	/// Used to spawn Dust or Gore particle effects.
	/// <para/> Note that this is called even if the tile is invisible due to echo coating, so check <paramref name="visible"/> if dust should only be spawned if the tile is visible. Tiles that still spawn particle effects while invisible can be useful to builders. Some tiles that spawn dust even when invisible include BubbleMachine, FogMachine, BrazierSuspended, Campfire, Chimney, SillyBalloonMachine, LeafBlock, and PoopBlock.
	/// <para/> The <paramref name="tileFrameX"/> and <paramref name="tileFrameY"/> values differ from the Tile frame values in that they incorporate the changes from <see cref="SetDrawPositions"/> and should normally be used instead of <see cref="Tile.TileFrameX"/> and Y directly.
	/// <para/> This method is only called under the conditions where particle effects are intended to spawn, that being the game is active, not paused, and at the intended frequency determined by the lighting mode. There is no need to check for these conditions in this method.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="tile">The tile at the coordinates</param>
	/// <param name="tileFrameX">The tile frame that is being drawn.</param>
	/// <param name="tileFrameY">The tile frame that is being drawn.</param>
	/// <param name="tileLight">The color the tile is being drawn using.</param>
	/// <param name="visible">Whether or not the tile is visible due to echo coating.</param>
	public virtual void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
	{
	}

	/// <summary>
	/// Special Draw. Allows for additional rendering after all tiles are drawn normally. Only called if coordinates are added using <c>Main.instance.TilesRenderer.AddSpecialLegacyPoint</c> or <c>Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileCounterType.CustomNonSolid or CustomSolid)</c> during <see cref="DrawEffects(int, int, SpriteBatch, ref TileDrawInfo)"/> or <see cref="ModBlockType.PreDraw(int, int, SpriteBatch)"/>. Useful for drawing things that would otherwise be impossible to draw due to draw order, such as items in item frames.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	public virtual void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
	{
	}

	/// <summary>
	/// Allows you to draw behind this multi-tile's regular placement preview rendering, or change relevant drawing parameters. This is ran for each rendered section of the multi-tile.
	/// <br/><br/> Make sure to use <paramref name="frame"/> for logic rather than the TileFrameX/Y values of the tile at the provided coordinates, this tile isn't placed yet.
	/// <br/><br/> Return false to stop this section from drawing normally. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	/// <param name="frame">The source rectangle that this section will use for rendering.</param>
	/// <param name="position">The position at which this section will be drawn.</param>
	/// <param name="color">The color with which this section will be drawn. This is red when overlapping with another tile.</param>
	/// <param name="validPlacement">Indicates if the tile can occupy this location.</param>
	/// <param name="spriteEffects">The <see cref="SpriteEffects"/> that will be used to draw this section.</param>
	public virtual bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw in front of this multi-tile's placement preview rendering. This is ran for each rendered section of the multi-tile.
	/// <br/><br/> Make sure to use <paramref name="frame"/> for logic rather than the TileFrameX/Y values of the tile at the provided coordinates, this tile isn't placed yet.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	/// <param name="frame">The source rectangle that was used for rendering this section.</param>
	/// <param name="position">The position at which this section was drawn.</param>
	/// <param name="color">The color with which this section was drawn.</param>
	/// <param name="validPlacement">Indicates if the tile can occupy this location.</param>
	/// <param name="spriteEffects">The <see cref="SpriteEffects"/> that were used to draw this section.</param>
	public virtual void PostDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, Rectangle frame, Vector2 position, Color color, bool validPlacement, SpriteEffects spriteEffects)
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
	/// Allows you to modify the frame of a tile after the vanilla framing code has set <see cref="Tile.TileFrameX"/> and <see cref="Tile.TileFrameY"/>. Useful for offsetting the final frame position without entirely overriding the vanilla framing logic
	/// <br/> Only called if <see cref="TileFrame(int, int, ref bool, ref bool)"/> returns true
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="up">The merge type of the tile above. Uninitialized if the tile is <see cref="Main.tileFrameImportant"/>.</param>
	/// <param name="down">The merge type of the tile below. Uninitialized if the tile is <see cref="Main.tileFrameImportant"/>.</param>
	/// <param name="left">The merge type of the tile to the left. Uninitialized if the tile is <see cref="Main.tileFrameImportant"/>.</param>
	/// <param name="right">The merge type of the tile to the right. Uninitialized if the tile is <see cref="Main.tileFrameImportant"/>.</param>
	/// <param name="upLeft">The merge type of the tile on the top left. Uninitialized if the tile is <see cref="Main.tileFrameImportant"/>.</param>
	/// <param name="upRight">The merge type of the tile on the top right. Uninitialized if the tile is <see cref="Main.tileFrameImportant"/>.</param>
	/// <param name="downLeft">The merge type of the tile on the bottom left. Uninitialized if the tile is <see cref="Main.tileFrameImportant"/>.</param>
	/// <param name="downRight">The merge type of the tile on the bottom right. Uninitialized if the tile is <see cref="Main.tileFrameImportant"/>.</param>
	public virtual void PostTileFrame(int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight)
	{
	}

	/// <summary>
	/// Allows you to change the merge type of the adjacent tiles before <see cref="Tile.TileFrameX"/> and <see cref="Tile.TileFrameY"/> is picked by vanilla framing code. Useful to make tiles that only selectively connect with others, or for tiles
	/// <br/> Tiles can be easily made to use custom merge frames with non-dirt tiles by using <see cref="TileID.Sets.ChecksForMerge"/> in combination with <see cref="WorldGen.TileMergeAttempt(int, bool[], ref int, ref int, ref int, ref int, ref int, ref int, ref int, ref int)"/> to set the adjacent tile's merge type to -2
	/// <br/> Only called if <see cref="TileFrame(int, int, ref bool, ref bool)"/> returns true
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="up">The merge type of the tile above. The tile will merge with it if the value is the same as its tile type <br/>-1 Means it'll consider the tile empty, and -2 means tiles that use <see cref="Main.tileMergeDirt"/> or <see cref="TileID.Sets.ChecksForMerge"/> will attempt to use their custom merge frames with that tile</param>
	/// <param name="down">The merge type of the tile below. The tile will merge with it if the value is the same as its tile type <br/>-1 Means it'll consider the tile empty, and -2 means tiles that use <see cref="Main.tileMergeDirt"/> or <see cref="TileID.Sets.ChecksForMerge"/> will attempt to use their custom merge frames with that tile</param>
	/// <param name="left">The merge type of the tile to the keft. The tile will merge with it if the value is the same as its tile type <br/>-1 Means it'll consider the tile empty, and -2 means tiles that use <see cref="Main.tileMergeDirt"/> or <see cref="TileID.Sets.ChecksForMerge"/> will attempt to use their custom merge frames with that tile</param>
	/// <param name="right">The merge type of the tile to the right. The tile will merge with it if the value is the same as its tile type <br/>-1 Means it'll consider the tile empty, and -2 means tiles that use <see cref="Main.tileMergeDirt"/> or <see cref="TileID.Sets.ChecksForMerge"/> will attempt to use their custom merge frames with that tile</param>
	/// <param name="upLeft">The merge type of the tile on the top left. The tile will merge with it if the value is the same as its tile type <br/>-1 Means it'll consider the tile empty, and -2 means tiles that use <see cref="Main.tileMergeDirt"/> or <see cref="TileID.Sets.ChecksForMerge"/> will attempt to use their custom merge frames with that tile</param>
	/// <param name="upRight">The merge type of the tile on the top right. The tile will merge with it if the value is the same as its tile type <br/>-1 Means it'll consider the tile empty, and -2 means tiles that use <see cref="Main.tileMergeDirt"/> or <see cref="TileID.Sets.ChecksForMerge"/> will attempt to use their custom merge frames with that tile</param>
	/// <param name="downLeft">The merge type of the tile on the bottom left. The tile will merge with it if the value is the same as its tile type <br/>-1 Means it'll consider the tile empty, and -2 means tiles that use <see cref="Main.tileMergeDirt"/> or <see cref="TileID.Sets.ChecksForMerge"/> will attempt to use their custom merge frames with that tile</param>
	/// <param name="downRight">The merge type of the tile on the bottom right. The tile will merge with it if the value is the same as its tile type <br/>-1 Means it'll consider the tile empty, and -2 means tiles that use <see cref="Main.tileMergeDirt"/> or <see cref="TileID.Sets.ChecksForMerge"/> will attempt to use their custom merge frames with that tile</param>
	public virtual void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
	{
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
	/// Called when <see cref="Wiring.HitSwitch"/> is called on the tile. Ordinarily this only happens for modded tiles if they opt in to specific functionality, such as <see cref="TileID.Sets.PressurePlate"/>, but mods can call it directly as well.
	/// <br/><br/> Can be used for running code on the server and all clients for tile interactions, unlike <see cref="HitWire(int, int)"/> which runs on the server or <see cref="RightClick(int, int)"/> which runs on the local client.
	/// <br/><br/> Code in HitWire, RightClick, or SwitchTiles could call <see cref="Wiring.HitSwitch"/> followed by <c>NetMessage.SendData(MessageID.HitSwitch...)</c> (Or just <see cref="Wiring.HitSwitchAndSync"/> by itself), which would result in <see cref="Wiring.HitSwitch"/> and consequently this method running on the server and all clients. Essentially, this can be used to sync a tile interaction effect without making a custom ModPacket, and it is up to the modder to decide how that interaction is triggered.
	/// <br/><br/> The most common usage of this is to sync playing a sound.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual void HitSwitch(int i, int j)
	{
	}

	/// <summary>
	/// Called in <see cref="Collision.SwitchTiles(Entity, Vector2, int, int, Vector2, int)"/>. This hook allows acting on entities colliding with tiles.
	/// <br/><br/> The <paramref name="position"/>, <paramref name="width"/>, and <paramref name="height"/> parameters indicate the hitbox of the entity, while <paramref name="oldPosition"/> is the position of the entity on the previous update. You'll need to use these to determine if a collision is occurring and if the entity is entering or leaving the collision bounds this tile is interested in. This is called on every
	/// <br/><br/> <include file = 'CommonDocs.xml' path='Common/SwitchTilesObjType' />
	/// <br/><br/> Called on the local client for owned projectile and the player, and on the server for boulder projectiles and NPC.
	/// <br/><br/> Returns false by default. Return true to indicate that the tile had some sort of interaction occur. This return value is only used to force specific friendly NPC to keep walking, preventing them from resting on pressure plates, for example.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="entity">The entity colliding with this tile</param>
	/// <param name="position">Position of the colliding entity</param>
	/// <param name="width">Width of the colliding entity</param>
	/// <param name="height">Height of the colliding entity</param>
	/// <param name="oldPosition">Position of the colliding entity on the previous update</param>
	/// <param name="objType"><include file = 'CommonDocs.xml' path='Common/SwitchTilesObjType' /></param>
	public virtual bool SwitchTiles(int i, int j, Entity entity, Vector2 position, int width, int height, Vector2 oldPosition, int objType)
	{
		return false;
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
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="tileTypeBeingPlaced"></param>
	/// <returns></returns>
	public virtual bool CanReplace(int i, int j, int tileTypeBeingPlaced)
	{
		return true;
	}

	/// <summary>
	/// Called when this tile at the given coordinates is being replaced via the block swap feature. For supported multi-tiles, the coordinates will be the top left corner. <paramref name="targetType"/> and <paramref name="targetStyle"/> are the tile type and style that will replace the current tile.
	/// <br/><br/> An implementation of this method might end up duplicating some custom KillMultiTile or KillTile code, but not all. For example, for a chest or dresser tile you wouldn't want to call Chest.DestroyChest.
	/// <br/><br/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void ReplaceTile(int i, int j, int targetType, int targetStyle)
	{
	}

	/// <summary>
	/// Customizes a tile drawn using <see cref="GameContent.Drawing.TileDrawing.AddSpecialPoint"/> with <see cref="GameContent.Drawing.TileDrawing.TileCounterType.MultiTileVine"/>, specifically how the tile reacts to wind and player interactions.
	/// <para/> The parameters are as follows:
	/// <br/> <b><paramref name="overrideWindCycle"/>:</b> <inheritdoc cref="AdjustMultiTileVineParameters" path="/param[@name='overrideWindCycle']"/>
	/// <br/> <b><paramref name="windPushPowerX"/>:</b> <inheritdoc cref="AdjustMultiTileVineParameters" path="/param[@name='windPushPowerX']"/>
	/// <br/> <b><paramref name="windPushPowerY"/>:</b> <inheritdoc cref="AdjustMultiTileVineParameters" path="/param[@name='windPushPowerY']"/>
	/// <br/> <b><paramref name="dontRotateTopTiles"/>:</b> <inheritdoc cref="AdjustMultiTileVineParameters" path="/param[@name='dontRotateTopTiles']"/>
	/// <br/> <b><paramref name="totalWindMultiplier"/>:</b> <inheritdoc cref="AdjustMultiTileVineParameters" path="/param[@name='totalWindMultiplier']"/>
	/// <br/> <b><paramref name="glowTexture"/>:</b> <inheritdoc cref="AdjustMultiTileVineParameters" path="/param[@name='glowTexture']"/>
	/// <br/> <b><paramref name="glowColor"/>:</b> <inheritdoc cref="AdjustMultiTileVineParameters" path="/param[@name='glowColor']"/>
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="overrideWindCycle">Defaults to null - Set this to a value to apply physics to all rows of a multitile evenly instead of proportional to how tall the tile is. Set this to 1 for tiles representing solid objects or any tile where misaligned tiles would look wrong.</param>
	/// <param name="windPushPowerX">Defaults to 1f - How much the forces will push the tile horizontally, although it is currently unused.</param>
	/// <param name="windPushPowerY">Defaults to -4f - How much the forces will push the tile vertically. Tiles representing solid objects should set this to 0, the default value works well for cloth objects like banners.</param>
	/// <param name="dontRotateTopTiles">Defaults to false - If true, the top row will not be affected and will be stationary</param>
	/// <param name="totalWindMultiplier">Defaults to 0.15f - Scales all wind forces</param>
	/// <param name="glowTexture">Defaults to null - Defines an additional texture to be drawn using glowColor</param>
	/// <param name="glowColor">Defaults to Color.Transparent - The color glowTexture should be drawn using</param>
	public virtual void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor)
	{
	}

	/// <summary>
	/// Use to populate <paramref name="tileFlameData"/> with flame drawing parameters.
	/// <para/> Currently only supported for tiles drawn using <see cref="GameContent.Drawing.TileDrawing.AddSpecialPoint"/> with <see cref="GameContent.Drawing.TileDrawing.TileCounterType.MultiTileVine"/>, other tiles should draw flames manually in <see cref="ModBlockType.PostDraw(int, int, SpriteBatch)"/> as shown in <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExampleLamp.cs#L124">ExampleLamp.cs</see>.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="tileFlameData">Contains parameters for drawing the flame.</param>
	public virtual void GetTileFlameData(int i, int j, ref GameContent.Drawing.TileDrawing.TileFlameData tileFlameData)
	{
	}

	/// <summary>
	/// Called when this tile has been converted to another tile type via biome conversion. Check <paramref name="fromType"/> and <paramref name="toType"/> to see the tile types the tile changed between. For ModTile, this will be called when converting both to and from the tile. <paramref name="conversionType"/> is a <see cref="BiomeConversionID"/> (or <see cref="ModBiomeConversion.Type"/>).
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="fromType">The original tile type</param>
	/// <param name="toType">The new tile type</param>
	/// <param name="conversionType">A <see cref="BiomeConversionID"/> (or <see cref="ModBiomeConversion.Type"/>)</param>
	public virtual void OnTileConverted(int i, int j, int fromType, int toType, int conversionType)
	{
	}
}
