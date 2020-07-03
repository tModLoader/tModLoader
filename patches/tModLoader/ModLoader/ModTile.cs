using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a type of tile that can be added by a mod. Only one instance of this class will ever exist for each type of tile that is added. Any hooks that are called will be called by the instance corresponding to the tile type. This is to prevent the game from using a massive amount of memory storing tile instances.
	/// </summary>
	public class ModTile
	{
		/// <summary>
		/// The mod which has added this type of ModTile.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The name of this type of tile.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The internal ID of this type of tile.
		/// </summary>
		public ushort Type {
			get;
			internal set;
		}

		internal string texture;
		/// <summary>
		/// The highlight texture used when this tile is selected by smart interact. Defaults to adding "_Highlight" onto the main texture.
		/// </summary>
		public virtual string HighlightTexture => texture + "_Highlight";
		/// <summary>
		/// The default type of sound made when this tile is hit. Defaults to 0.
		/// </summary>
		public int soundType = 0;
		/// <summary>
		/// The default style of sound made when this tile is hit. Defaults to 1.
		/// </summary>
		public int soundStyle = 1;
		/// <summary>
		/// The default type of dust made when this tile is hit. Defaults to 0.
		/// </summary>
		public int dustType = 0;
		/// <summary>
		/// The default type of item dropped when this tile is killed. Defaults to 0, which means no item.
		/// </summary>
		public int drop = 0;
		/// <summary>
		/// The height of a group of animation frames for this tile. Defaults to 0, which disables animations.
		/// </summary>
		public int animationFrameHeight = 0;
		/// <summary>
		/// A multiplier describing how much this block resists harvesting. Higher values will make it take longer to harvest. Defaults to 1f.
		/// </summary>
		public float mineResist = 1f;
		/// <summary>
		/// The minimum pickaxe power required for pickaxes to mine this block. Defaults to 0.
		/// </summary>
		public int minPick = 0;
		/// <summary>
		/// Whether or not the smart cursor function is disabled when the cursor hovers above this tile. Defaults to false.
		/// </summary>
		public bool disableSmartCursor = false;
		/// <summary>
		/// Whether or not the smart tile interaction function is disabled when the cursor hovers above this tile. Defaults to false.
		/// </summary>
		public bool disableSmartInteract = false;
		/// <summary>
		/// An array of the IDs of tiles that this tile can be considered as when looking for crafting stations.
		/// </summary>
		public int[] adjTiles = new int[0];
		/// <summary>
		/// The ID of the tile that this door transforms into when it is closed. Defaults to -1, which means this tile isn't a door.
		/// </summary>
		public int closeDoorID = -1;
		/// <summary>
		/// The ID of the tile that this door transforms into when it is opened. Defaults to -1, which means this tile isn't a door.
		/// </summary>
		public int openDoorID = -1;
		/// <summary>
		/// The default name of this chest that is displayed when this 2x2 chest is open. Defaults to the empty string, which means that this tile isn't a chest. Setting this field will make the tile behave like a chest (meteors will avoid it, tiles underneath cannot be mined, etc.), but you will have to manually give it storage capabilities yourself. (See the ExampleMod for something you can copy/paste.)
		/// </summary>
		public string chest = "";
		/// <summary>
		/// The ID of the item that drops when this chest is destroyed. Defaults to 0. Honestly, this is only really used when the chest limit is reached on a server.
		/// </summary>
		public int chestDrop = 0;
		/// <summary>
		/// Same as chest, except use this if your block is a dresser (has a size of 3x2 instead of 2x2).
		/// </summary>
		public string dresser = "";
		/// <summary>
		/// The ID of the item that drops when this dresser is destroyed. Defaults to 0. Honestly, this is only really used when the chest limit is reached on a server.
		/// </summary>
		public int dresserDrop = 0;
		/// <summary>
		/// Whether or not this tile is a valid spawn point. Defaults to false. If you set this to true, you will still have to manually set the spawn yourself in the RightClick hook.
		/// </summary>
		public bool bed = false;
		/// <summary>
		/// Whether or not this tile behaves like a torch. If you are making a torch tile, then setting this to true is necessary in order for tile placement, tile framing, and the item's smart selection to work properly.
		/// </summary>
		public bool torch = false;
		/// <summary>
		/// Whether or not this tile is a sapling, which can grow into a modded tree or palm tree.
		/// </summary>
		public bool sapling = false;

		/// <summary>
		/// A convenient method for adding this tile's Type to the given array. This can be used with the arrays in TileID.Sets.RoomNeeds.
		/// </summary>
		public void AddToArray(ref int[] array) {
			Array.Resize(ref array, array.Length + 1);
			array[array.Length - 1] = Type;
		}

		/// <summary>
		/// Adds an entry to the minimap for this tile with the given color and display name. This should be called in SetDefaults.
		/// </summary>
		public void AddMapEntry(Color color, LocalizedText name = null) {
			if (!MapLoader.initialized) {
				MapEntry entry = new MapEntry(color, name);
				if (!MapLoader.tileEntries.Keys.Contains(Type)) {
					MapLoader.tileEntries[Type] = new List<MapEntry>();
				}
				MapLoader.tileEntries[Type].Add(entry);
			}
		}

		/// <summary>
		/// Creates a ModTranslation object that you can use in AddMapEntry.
		/// </summary>
		/// <param name="key">The key for the ModTranslation. The full key will be MapObject.ModName.key</param>
		/// <returns></returns>
		public ModTranslation CreateMapEntryName(string key = null) {
			if (string.IsNullOrEmpty(key)) {
				key = Name;
			}
			return mod.GetOrCreateTranslation(string.Format("Mods.{0}.MapObject.{1}", mod.Name, key));
		}

		/// <summary>
		/// Adds an entry to the minimap for this tile with the given color and display name. This should be called in SetDefaults.
		/// </summary>
		public void AddMapEntry(Color color, ModTranslation name) {
			if (!MapLoader.initialized) {
				MapEntry entry = new MapEntry(color, name);
				if (!MapLoader.tileEntries.Keys.Contains(Type)) {
					MapLoader.tileEntries[Type] = new List<MapEntry>();
				}
				MapLoader.tileEntries[Type].Add(entry);
			}
		}

		/// <summary>
		/// Adds an entry to the minimap for this tile with the given color, default display name, and display name function. The parameters for the function are the default display name, x-coordinate, and y-coordinate. This should be called in SetDefaults.
		/// </summary>
		public void AddMapEntry(Color color, LocalizedText name, Func<string, int, int, string> nameFunc) {
			if (!MapLoader.initialized) {
				MapEntry entry = new MapEntry(color, name, nameFunc);
				if (!MapLoader.tileEntries.Keys.Contains(Type)) {
					MapLoader.tileEntries[Type] = new List<MapEntry>();
				}
				MapLoader.tileEntries[Type].Add(entry);
			}
		}

		/// <summary>
		/// Adds an entry to the minimap for this tile with the given color, default display name, and display name function. The parameters for the function are the default display name, x-coordinate, and y-coordinate. This should be called in SetDefaults.
		/// </summary>
		public void AddMapEntry(Color color, ModTranslation name, Func<string, int, int, string> nameFunc) {
			if (!MapLoader.initialized) {
				MapEntry entry = new MapEntry(color, name, nameFunc);
				if (!MapLoader.tileEntries.Keys.Contains(Type)) {
					MapLoader.tileEntries[Type] = new List<MapEntry>();
				}
				MapLoader.tileEntries[Type].Add(entry);
			}
		}

		/// <summary>
		/// Allows this tile to grow the given modded tree.
		/// </summary>
		/// <param name="tree">The ModTree.</param>
		public void SetModTree(ModTree tree) {
			TileLoader.trees[Type] = tree;
		}

		/// <summary>
		/// Allows this tile to grow the given modded palm tree.
		/// </summary>
		/// <param name="palmTree">The ModPalmTree</param>
		public void SetModPalmTree(ModPalmTree palmTree) {
			TileLoader.palmTrees[Type] = palmTree;
		}

		/// <summary>
		/// Allows this tile to grow the given modded cactus.
		/// </summary>
		/// <param name="cactus">The ModCactus</param>
		public void SetModCactus(ModCactus cactus) {
			TileLoader.cacti[Type] = cactus;
		}

		/// <summary>
		/// Allows you to modify the name and texture path of this tile when it is autoloaded. Return true to autoload this tile. When a tile is autoloaded, that means you do not need to manually call Mod.AddTile. By default returns the mod's autoload property.
		/// </summary>
		/// <param name="name">The internal name.</param>
		/// <param name="texture">The texture path.</param>
		/// <returns>Whether or not to autoload this tile.</returns>
		public virtual bool Autoload(ref string name, ref string texture) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Allows you to set the properties of this tile. Many properties are stored as arrays throughout Terraria's code.
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Allows you to override some default properties of this tile, such as Main.tileNoSunLight and Main.tileObsidianKill.
		/// </summary>
		public virtual void PostSetDefaults() {
		}

		/// <summary>
		/// Whether or not the smart interact function can select this tile. Useful for things like chests. Defaults to false.
		/// </summary>
		/// <returns></returns>
		public virtual bool HasSmartInteract() {
			return false;
		}

		/// <summary>
		/// Allows you to customize which sound you want to play when the tile at the given coordinates is hit. Return false to stop the game from playing its default sound for the tile. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool KillSound(int i, int j) {
			return true;
		}

		/// <summary>
		/// Allows you to change how many dust particles are created when the tile at the given coordinates is hit.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void NumDust(int i, int j, bool fail, ref int num) {
		}

		/// <summary>
		/// Allows you to modify the default type of dust created when the tile at the given coordinates is hit. Return false to stop the default dust (the type parameter) from being created. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool CreateDust(int i, int j, ref int type) {
			type = dustType;
			return true;
		}

		/// <summary>
		/// Allows you to modify the chance the tile at the given coordinates has of spawning a certain critter when the tile is killed.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
		}

		/// <summary>
		/// Allows you to customize which items the tile at the given coordinates drops. Remember that the x, y (i, j) coordinates are in tile coordinates, you will need to multiply them by 16 if you want to drop an item using them. Return false to stop the game from dropping the tile's default item. Returns true by default. Please note that this hook currently only works for 1x1 tiles.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool Drop(int i, int j) {
			return true;
		}

		/// <summary>
		/// Allows you to determine whether or not the tile at the given coordinates can be hit by anything. Returns true by default. blockDamaged currently has no use.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool CanKillTile(int i, int j, ref bool blockDamaged) {
			return true;
		}

		/// <summary>
		/// Allows you to determine what happens when the tile at the given coordinates is killed or hit with a pickaxe. Fail determines whether the tile is mined, effectOnly makes it so that only dust is created, and noItem stops items from dropping.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
		}

		/// <summary>
		/// This hook is called exactly once whenever a block encompassing multiple tiles is destroyed. You can use it to make your multi-tile block drop a single item, for example.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void KillMultiTile(int i, int j, int frameX, int frameY) {
		}

		/// <summary>
		/// Whether or not the tile at the given coordinates can be killed by an explosion (ie. bombs). Returns true by default; return false to stop an explosion from destroying it.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool CanExplode(int i, int j) {
			return true;
		}

		/// <summary>
		/// Allows you to make things happen when this tile is within a certain range of the player (around the same range water fountains and music boxes work). The closer parameter is whether or not the tile is within the range at which things like campfires and banners work.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void NearbyEffects(int i, int j, bool closer) {
		}

		/// <summary>
		/// Allows you to determine how much light this block emits. Make sure you set Main.tileLighted[Type] to true in SetDefaults for this to work.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
		}

		/// <summary>
		/// Allows you to determine whether this block glows red when the given player has the Dangersense buff.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool Dangersense(int i, int j, Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to determine whether or not the tile will draw itself flipped in the world.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
		}

		/// <summary>
		/// Allows you to customize the position in which this tile is drawn. Width refers to the width of one frame of the tile, offsetY refers to how many pixels below its actual position the tile should be drawn, and height refers to the height of one frame of the tile. By default the values will be set to the values you give this tile's TileObjectData. If this tile has no TileObjectData then they will default to 16, 0, and 16, respectively.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height) {
		}

		/// <summary>
		/// Allows you to animate your tile. Use frameCounter to keep track of how long the current frame has been active, and use frame to change the current frame. This is called once an update. Use AnimateIndividualTile to animate specific tile instances directly.
		/// </summary>
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
		public virtual void AnimateTile(ref int frame, ref int frameCounter) {
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
		public virtual void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
		}

		/// <summary>
		/// Allows you to draw things behind the tile at the given coordinates. Return false to stop the game from drawing the tile normally. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			return true;
		}

		/// <summary>
		/// Allows you to make stuff happen whenever the tile at the given coordinates is drawn. For example, creating dust or changing the color the tile is drawn in.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		/// <param name="nextSpecialDrawIndex">The special draw count. Use with Main.specX and Main.specY and then increment to draw special things after the main tile drawing loop is complete via DrawSpecial.</param>
		public virtual void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
		}

		/// <summary>
		/// Allows you to draw things in front of the tile at the given coordinates. This can also be used to do things such as creating dust.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void PostDraw(int i, int j, SpriteBatch spriteBatch) {
		}

		/// <summary>
		/// Special Draw. Only called if coordinates are placed in Main.specX/Y during DrawEffects. Useful for drawing things that would otherwise be impossible to draw due to draw order, such as items in item frames.
		/// </summary>
		/// <param name="i">The i.</param>
		/// <param name="j">The j.</param>
		public virtual void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
		}

		/// <summary>
		/// Allows you to choose which minimap entry the tile at the given coordinates will use. 0 is the first entry added by AddMapEntry, 1 is the second entry, etc. Returns 0 by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual ushort GetMapOption(int i, int j) {
			return 0;
		}

		/// <summary>
		/// Called whenever the world randomly decides to update this tile in a given tick. Useful for things such as growing or spreading.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void RandomUpdate(int i, int j) {
		}

		/// <summary>
		/// Called whenever this tile updates due to being placed or being next to a tile that is changed. Return false to stop the game from carrying out its default TileFrame operations. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			return true;
		}

		/// <summary>
		/// Allows you to stop this tile from being placed at the given coordinates. Return false to block the tile from being placed. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool CanPlace(int i, int j) {
			return true;
		}

		/// <summary>
		/// Allows you to make something happen when this tile is right-clicked by the player.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		[Obsolete("ModTile.RightClick will return a bool value later. This version is deprecated since v0.11.5, please use ModTile.NewRightClick instead and return true if a tile interaction has occurred.")]
		public virtual void RightClick(int i, int j) {
		}

		/// <summary>
		/// Allows you to make something happen when this tile is right-clicked by the player. Return true to indicate that a tile interaction has occurred, preventing other right click actions like minion targetting from happening. Returns false by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		/// <returns>Return true to indicate that a tile interaction has occurred, preventing other right click actions like minion targetting from happening. Returns false by default.</returns>
		public virtual bool NewRightClick(int i, int j) {
			return false;
		}

		/// <summary>
		/// Allows you to make something happen when the mouse hovers over this tile. Useful for showing item icons or text on the mouse.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void MouseOver(int i, int j) {
		}

		/// <summary>
		/// Allows you to make something happen when the mouse hovers over this tile, even when the player is far away. Useful for showing what's written on signs, etc.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void MouseOverFar(int i, int j) {
		}

		/// <summary>
		/// Allows you to determine whether the given item can become selected when the cursor is hovering over this tile and the auto selection hotkey is pressed.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool AutoSelect(int i, int j, Item item) {
			return false;
		}

		/// <summary>
		/// Allows you to make something happen when a wire current passes through this tile.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual void HitWire(int i, int j) {
		}

		/// <summary>
		/// Allows you to control how hammers slope this tile. Return true to allow it to slope normally. Returns true by default. Called on the local Client and Single Player.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		public virtual bool Slope(int i, int j) {
			return true;
		}

		/// <summary>
		/// Allows you to make something happen when a player stands on this type of tile. For example, you can make the player slide as if on ice.
		/// </summary>
		/// <param name="player"></param>
		public virtual void FloorVisuals(Player player) {
		}

		/// <summary>
		/// Whether or not this tile creates dust when the player walks on it. Returns false by default.
		/// </summary>
		public virtual bool HasWalkDust() {
			return false;
		}

		/// <summary>
		/// Allows you to modify the dust created when the player walks on this tile. The makeDust parameter is whether or not to make dust; you can randomly set this to false to reduce the amount of dust produced.
		/// </summary>
		/// <param name="dustType"></param>
		/// <param name="makeDust"></param>
		/// <param name="color"></param>
		public virtual void WalkDust(ref int dustType, ref bool makeDust, ref Color color) {
		}

		/// <summary>
		/// Allows you to change the style of waterfall that passes through or over this type of tile.
		/// </summary>
		/// <param name="style"></param>
		public virtual void ChangeWaterfallStyle(ref int style) {
		}

		/// <summary>
		/// Allows this tile to support a sapling that can eventually grow into a tree. The type of the sapling should be returned here. Returns -1 by default. The style parameter will determine which sapling is chosen if multiple sapling types share the same ID; even if you only have a single sapling in an ID, you must still set this to 0.
		/// </summary>
		/// <param name="style"></param>
		/// <returns></returns>
		public virtual int SaplingGrowthType(ref int style) {
			return -1;
		}

		/// <summary>
		/// Allows you to do something when this tile is placed. Called on the local Client and Single Player.
		/// </summary>
		/// <param name="i">The x position in tile coordinates. Equal to Player.tileTargetX</param>
		/// <param name="j">The y position in tile coordinates. Equal to Player.tileTargetY</param>
		/// <param name="item">The item used to place this tile.</param>
		public virtual void PlaceInWorld(int i, int j, Item item) {
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
	}
}
