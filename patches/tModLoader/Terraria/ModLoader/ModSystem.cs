﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	/// <summary>
	/// ModSystem is an abstract class that your classes can derive from. It contains general-use hooks, and, unlike Mod, can have unlimited amounts of types deriving from it.
	/// </summary>
	public abstract partial class ModSystem : ModType
	{
		protected override void Register() {
			SystemLoader.Add(this);
			ModTypeLookup<ModSystem>.Register(this);
		}

		/// <summary>
		/// Unlike other ModTypes, SetupContent is unsealed for you to do whatever you need. By default it just calls SetStaticDefaults.
		/// This is the place to finish initializing your mod's content. For content from other mods, and lookup tables, consider PostSetupContent
		/// </summary>
		public override void SetupContent() => SetStaticDefaults();

		//Hooks

		/// <summary>
		/// This hook is called right after Mod.Load(), which is guaranteed to be called after all content has been autoloaded.
		/// </summary>
		public virtual void OnModLoad() { }

		/// <summary>
		/// This hook is called right before Mod.UnLoad()
		/// </summary>
		public virtual void OnModUnload() { }

		/// <summary>
		/// Allows you to load things in your system after the mod's content has been setup (arrays have been resized to fit the content, etc).
		/// </summary>
		public virtual void PostSetupContent() { }

		/// <summary>
		/// Override this method to add recipes to the game.
		/// <br/> It is recommended that you do so through instances of Recipe, since it provides methods that simplify recipe creation.
		/// </summary>
		public virtual void AddRecipes() { }

		/// <summary>
		/// This provides a hook into the mod-loading process immediately after recipes have been added.
		/// <br/> You can use this to edit recipes added by other mods.
		/// </summary>
		public virtual void PostAddRecipes() { }

		/// <summary>
		/// Override this method to do treatment about recipes once they have been setup. You shouldn't edit any recipe here.
		/// </summary>
		public virtual void PostSetupRecipes() {
		}

		/// <summary>
		/// Override this method to add recipe groups to the game.
		/// <br/> You must add recipe groups by calling the <see cref="RecipeGroup.RegisterGroup"/> method here.
		/// <br/> A recipe group is a set of items that can be used interchangeably in the same recipe.
		/// </summary>
		public virtual void AddRecipeGroups() { }

		/// <summary>
		/// Called whenever a world is loaded. This can be used to initialize data structures, etc.
		/// <br/>If you need to access your data during worldgen, initialize it in <see cref="PreWorldGen"/> instead, unless you also save it on the world, then you need both.
		/// </summary>
		public virtual void OnWorldLoad() { }

		/// <summary>
		/// Called whenever a world is unloaded. Use this to deinitialize world-related data structures, etc.
		/// </summary>
		public virtual void OnWorldUnload() { }

		/// <summary>
		/// Use this hook to modify Main.screenPosition after weapon zoom and camera lerp have taken place.
		/// </summary>
		public virtual void ModifyScreenPosition() { }

		/// <summary>
		/// Allows you to set the transformation of the screen that is drawn. (Translations, rotations, scales, etc.)
		/// </summary>
		public virtual void ModifyTransformMatrix(ref SpriteViewMatrix Transform) { }

		/// <summary>
		/// Ran every update and suitable for calling Update for UserInterface classes
		/// </summary>
		public virtual void UpdateUI(GameTime gameTime) { }

		/// <summary>
		/// Use this if you want to do something before anything in the World gets updated.
		/// Called after UI updates, but before anything in the World (Players, NPCs, Projectiles, Tiles) gets updated.
		/// <para />
		/// When <see cref="Main.autoPause" /> is true or <see cref="Main.FrameSkipMode" /> is 0 or 2, the game may do a partial update. This means that it only updates menus and some animations, but not the World or Entities. This hook - and every hook after it - only gets called on frames with a full update.
		/// </summary>
		public virtual void PreUpdateEntities() { }

		/// <summary>
		/// Called before Players get updated.
		/// </summary>
		public virtual void PreUpdatePlayers() { }

		/// <summary>
		/// Called after Players get updated.
		/// </summary>
		public virtual void PostUpdatePlayers() { }

		/// <summary>
		/// Called before NPCs get updated.
		/// </summary>
		public virtual void PreUpdateNPCs() { }

		/// <summary>
		/// Called after NPCs get updated.
		/// </summary>
		public virtual void PostUpdateNPCs() { }

		/// <summary>
		/// Called before Gores get updated.
		/// </summary>
		public virtual void PreUpdateGores() { }

		/// <summary>
		/// Called after Gores get updated.
		/// </summary>
		public virtual void PostUpdateGores() { }

		/// <summary>
		/// Called before Projectiles get updated.
		/// </summary>
		public virtual void PreUpdateProjectiles() { }

		/// <summary>
		/// Called after Projectiles get updated.
		/// </summary>
		public virtual void PostUpdateProjectiles() { }

		/// <summary>
		/// Called before Items get updated.
		/// </summary>
		public virtual void PreUpdateItems() { }

		/// <summary>
		/// Called after Items get updated.
		/// </summary>
		public virtual void PostUpdateItems() { }

		/// <summary>
		/// Called before Dusts get updated.
		/// </summary>
		public virtual void PreUpdateDusts() { }

		/// <summary>
		/// Called after Dusts get updated.
		/// </summary>
		public virtual void PostUpdateDusts() { }

		/// <summary>
		/// Called before Time gets updated.
		/// </summary>
		public virtual void PreUpdateTime() { }

		/// <summary>
		/// Called after Time gets updated.
		/// </summary>
		public virtual void PostUpdateTime() { }

		/// <summary>
		/// Use this method to have things happen in the world. In vanilla Terraria, a good example of code suitable for this hook is how Falling Stars fall to the ground during the night.
		/// <para/> This hook is not called for multiplayer clients.
		/// </summary>
		public virtual void PreUpdateWorld() { }

		/// <summary>
		/// Use this method to have things happen in the world. In vanilla Terraria, a good example of code suitable for this hook is how Falling Stars fall to the ground during the night.
		/// <para/> This hook is not called for multiplayer clients.
		/// </summary>
		public virtual void PostUpdateWorld() { }

		/// <summary>
		/// Called before Invasions get updated.
		/// <para/> This hook is not called for multiplayer clients.
		/// </summary>
		public virtual void PreUpdateInvasions() { }

		/// <summary>
		/// Called after Invasions get updated.
		/// <para/> This hook is not called for multiplayer clients.
		/// </summary>
		public virtual void PostUpdateInvasions() { }

		/// <summary>
		/// Called after the Network got updated, this is the last hook that happens in an update.
		/// </summary>
		public virtual void PostUpdateEverything() { }

		/// <summary>
		/// Allows you to modify the elements of the in-game interface that get drawn. GameInterfaceLayer can be found in the Terraria.UI namespace. Check https://github.com/tModLoader/tModLoader/wiki/Vanilla-Interface-layers-values for vanilla interface layer names
		/// </summary>
		/// <param name="layers">The layers.</param>
		public virtual void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) { }

		/// <summary>
		/// Allows you to set the visibility of any added vanilla or modded GameTips. In order to add your OWN tips, add them in
		/// your localization file, with the key prefix of "Mods.ModName.GameTips".
		/// </summary>
		/// <param name="gameTips"> The current list of all added game tips. </param>
		public virtual void ModifyGameTipVisibility(IReadOnlyList<GameTipData> gameTips) { }

		/// <summary>
		/// Called after interface is drawn but right before mouse and mouse hover text is drawn. Allows for drawing interface.
		///
		/// Note: This hook should no longer be used. It is better to use the ModifyInterfaceLayers hook.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch.</param>
		public virtual void PostDrawInterface(SpriteBatch spriteBatch) { }

		/// <summary>
		/// Called right before map icon overlays are drawn. Use this hook to selectively hide existing <see cref="IMapLayer"/> or <see cref="ModMapLayer"/>
		/// </summary>
		/// <param name="layers"></param>
		/// <param name="mapOverlayDrawContext"></param>
		public virtual void PreDrawMapIconOverlay(IReadOnlyList<IMapLayer> layers, MapOverlayDrawContext mapOverlayDrawContext) { }

		/// <summary>
		/// Called while the fullscreen map is active. Allows custom drawing to the map. Using <see cref="ModMapLayer"/> is more compatible and allows drawing on the minimap and fullscreen maps.
		/// </summary>
		/// <param name="mouseText">The mouse text.</param>
		public virtual void PostDrawFullscreenMap(ref string mouseText) { }

		/// <summary>
		/// Called after the input keys are polled. Allows for modifying things like scroll wheel if your custom drawing should capture that.
		/// </summary>
		public virtual void PostUpdateInput() { }

		/// <summary>
		/// Called in SP or Client when the Save and Quit button is pressed. One use for this hook is clearing out custom UI slots to return items to the player.
		/// </summary>
		public virtual void PreSaveAndQuit() { }

		/// <summary>
		/// Called after drawing Tiles. Can be used for drawing a tile overlay akin to wires. Note that spritebatch should be begun and ended within this method.
		/// </summary>
		public virtual void PostDrawTiles() { }

		/// <summary>
		/// Called after all other time calculations. Can be used to modify the speed at which time should progress per tick in seconds, along with the rate at which the tiles in the world and the events in the world should update with it.
		/// All fields are measured in in-game minutes per real-life second (min/sec).
		/// You may want to consider <see cref="Main.fastForwardTime"/> and CreativePowerManager.Instance.GetPower&lt;CreativePowers.FreezeTime&gt;().Enabled here.
		/// </summary>
		/// <param name="timeRate">The speed at which time flows in min/sec.</param>
		/// <param name="tileUpdateRate">The speed at which tiles in the world update in min/sec.</param>
		/// <param name="eventUpdateRate">The speed at which various events in the world (weather changes, fallen star/fairy spawns, etc.) update in min/sec.</param>
		public virtual void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate) { }

		/// <summary>
		/// Allows you to save custom data for this system in the current world. Useful for things like saving world specific flags.
		/// <br/>For example, if your mod adds a boss and you want certain NPC to only spawn once it has been defeated, this is where you would store the information that that boss has been defeated in this world.
		/// <br/>
		/// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
		/// <br/><b>NOTE:</b> Try to only save data that isn't default values.
		/// </summary>
		/// <param name="tag"> The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument only for the sake of convenience and optimization. </param>
		public virtual void SaveWorldData(TagCompound tag) { }

		/// <summary>
		/// Allows you to load custom data you have saved for this system in the currently loading world.
		/// <br/><b>Try to write defensive loading code that won't crash if something's missing.</b>
		/// </summary>
		/// <param name="tag"> The TagCompound to load data from. </param>
		public virtual void LoadWorldData(TagCompound tag) { }

		/// <summary>
		/// Allows you to prevent the world and player from being loaded/selected as a valid combination, similar to Journey Mode pairing.
		/// </summary>
		public virtual bool CanWorldBePlayed(PlayerFileData playerData, WorldFileData worldFileData) {
			return true;
		}

		public virtual string WorldCanBePlayedRejectionMessage(PlayerFileData playerData, WorldFileData worldData) {
			return $"The selected character {playerData.Name} can not be used with the selected world {worldData.Name}.\n" +
							$"This could be due to mismatched Journey Mode or other mod specific changes.";
		}

		/// <summary>
		/// Allows you to send custom data between clients and server, which will be handled in <see cref="NetReceive"/>. This is useful for syncing information such as bosses that have been defeated.
		/// <br/>Called whenever <see cref="MessageID.WorldData"/> is successfully sent, for example after a boss is defeated, a new day starts, or a player joins the server.
		/// <br/>Only called on the server.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public virtual void NetSend(BinaryWriter writer) { }

		/// <summary>
		/// Use this to receive information that was sent in <see cref="NetSend"/>.
		/// <br/>Called whenever <see cref="MessageID.WorldData"/> is successfully received.
		/// <br/>Only called on the client.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public virtual void NetReceive(BinaryReader reader) { }

		/// <summary>
		/// Allows you to modify net message / packet information that is received before the game can act on it.
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <param name="reader">The reader.</param>
		/// <param name="playerNumber">The player number the message is from.</param>
		public virtual bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
			=> false;

		/// <summary>
		/// Hijacks the send data method. Only use if you absolutely know what you are doing. If any hooks return true, the message is not sent.
		/// </summary>
		public virtual bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
			=> false;

		/// <summary>
		/// Allows a mod to run code before a world is generated.
		/// <br/>If you use this to initialize data used during worldgen, which you save on the world, also initialize it in <see cref="OnWorldLoad"/>.
		/// </summary>
		public virtual void PreWorldGen() { }

		/// <summary>
		/// A more advanced option to PostWorldGen, this method allows you modify the list of Generation Passes before a new world begins to be generated. <para/>
		/// For example, removing the "Planting Trees" pass will cause a world to generate without trees. Placing a new Generation Pass before the "Dungeon" pass will prevent the the mod's pass from cutting into the dungeon.
		/// </summary>
		public virtual void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) { }

		/// <summary>
		/// Use this method to place tiles in the world after world generation is complete.
		/// </summary>
		public virtual void PostWorldGen() { }

		/// <summary>
		/// Use this to reset any fields you set in any of your ModTile.NearbyEffects hooks back to their default values.
		/// </summary>
		public virtual void ResetNearbyTileEffects() { }

		/// <summary>
		/// Similar to ModifyWorldGenTasks, but occurs in-game when Hardmode starts. Can be used to modify which tasks should be done and/or add custom tasks. By default the list will only contain 4 items, the vanilla hardmode tasks called "Hardmode Good", "Hardmode Evil", "Hardmode Walls", and "Hardmode Announcment"
		/// </summary>
		public virtual void ModifyHardmodeTasks(List<GenPass> list) { }

		/// <summary>
		/// Allows you to modify color of light the sun emits.
		/// </summary>
		/// <param name="tileColor">Tile lighting color</param>
		/// <param name="backgroundColor">Background lighting color</param>
		public virtual void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) { }

		/// <summary>
		/// Allows you to modify overall brightness of lights. Can be used to create effects similiar to what night vision and darkness (de)buffs give you. Values too high or too low might result in glitches. For night vision effect use scale 1.03
		/// </summary>
		/// <param name="scale">Brightness scale</param>
		public virtual void ModifyLightingBrightness(ref float scale) { }

		/// <summary>
		/// Allows you to store information about how many of each tile is nearby the player. This is useful for counting how many tiles of a certain custom biome there are.
		/// <br/> The <paramref name="tileCounts"/> parameter is a read-only span (treat this as an array) that stores the tile count indexed by tile type.
		/// </summary>
		public virtual void TileCountsAvailable(ReadOnlySpan<int> tileCounts) { }
	}
}
