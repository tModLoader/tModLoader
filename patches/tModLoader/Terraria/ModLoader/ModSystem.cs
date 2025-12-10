using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader;

/// <summary>
/// ModSystem is an abstract class that your classes can derive from. It contains general-use hooks, and, unlike Mod, can have unlimited amounts of types deriving from it.
/// </summary>
public abstract partial class ModSystem : ModType
{
	protected override void ValidateType()
	{
		base.ValidateType();

		LoaderUtils.MustOverrideTogether(this, s => s.SaveWorldData, s => s.LoadWorldData);
		LoaderUtils.MustOverrideTogether(this, s => s.NetSend, s => s.NetReceive);
	}

	protected override void Register()
	{
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
	/// Allows mods to react to language changing. <br/>
	/// This happens whenever the language is changed, and when resource packs are reloaded.
	/// </summary>
	public virtual void OnLocalizationsLoaded() { }

	/// <summary>
	/// Override this method to add <see cref="Recipe"/>s to the game.<br/>
	/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Recipes">Basic Recipes Guide</see> teaches how to add new recipes to the game and how to manipulate existing recipes.<br/>
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
	public virtual void PostSetupRecipes() { }

	/// <summary>
	/// Override this method to add recipe groups to the game.
	/// <br/> You must add recipe groups by calling the <see cref="RecipeGroup.RegisterGroup"/> method here.
	/// <br/> A recipe group is a set of items that can be used interchangeably in the same recipe.
	/// </summary>
	public virtual void AddRecipeGroups() { }

	/// <summary>
	/// Called whenever a world is loaded, before <see cref="LoadWorldData"/> <br/>
	/// If you need to initialize tile or other world related data-structures, use <see cref="ClearWorld"/> instead
	/// </summary>
	public virtual void OnWorldLoad() { }

	/// <summary>
	/// Called whenever a world is loaded, after <see cref="LoadWorldData"/>.
	/// <br/><br/> At this point, the world is ready to be entered by players, and all modded world data is loaded.
	/// <br/><br/> Can be used to migrate modded world data across different <see cref="ModSystem"/>s, or to modify <see cref="Main.tile"/>. For deleting temporary tiles, use <see cref="TileID.Sets.ClearedOnWorldLoad"/> instead.
	/// <br/><br/> Only called in single player or on the server, not on multiplayer clients.
	/// </summary>
	public virtual void PostWorldLoad() { }

	/// <summary>
	/// Called whenever a world is unloaded.
	/// </summary>
	public virtual void OnWorldUnload() { }

	/// <summary>
	/// Called whenever the world is cleared. Use this to reset world-related data structures before world-gen or loading in both single and multiplayer. <br/>
	/// Also called just before mods are unloaded.
	/// </summary>
	public virtual void ClearWorld() { }

	/// <summary>
	/// Use this hook to modify <see cref="Main.screenPosition"/> after weapon zoom and camera lerp have taken place.
	/// <para/> Called on all clients.
	/// <para/> Also consider using <c>Main.instance.CameraModifiers.Add(CameraModifier);</c> as shown in ExampleMods MinionBossBody for screen shakes.
	/// </summary>
	public virtual void ModifyScreenPosition() { }

	/// <summary>
	/// Allows you to set the transformation of the screen that is drawn. (Translations, rotations, scales, etc.)
	/// <para/> Called on all clients.
	/// </summary>
	public virtual void ModifyTransformMatrix(ref SpriteViewMatrix Transform) { }

	/// <summary>
	/// Ran every update and suitable for calling Update for UserInterface classes
	/// <para/> Called on all clients.
	/// </summary>
	public virtual void UpdateUI(GameTime gameTime) { }

	/// <summary>
	/// Use this if you want to do something before anything in the World gets updated.
	/// Called after UI updates, but before anything in the World (Players, NPCs, Projectiles, Tiles) gets updated.
	/// <para /> When <see cref="Main.autoPause" /> is true or <see cref="Main.FrameSkipMode" /> is 0 or 2, the game may do a partial update. This means that it only updates menus and some animations, but not the World or Entities. This hook - and every hook after it - only gets called on frames with a full update.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PreUpdateEntities() { }

	/// <summary>
	/// Called before Players get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PreUpdatePlayers() { }

	/// <summary>
	/// Called after Players get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PostUpdatePlayers() { }

	/// <summary>
	/// Called before NPCs get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PreUpdateNPCs() { }

	/// <summary>
	/// Called after NPCs get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PostUpdateNPCs() { }

	/// <summary>
	/// Called before Gores get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PreUpdateGores() { }

	/// <summary>
	/// Called after Gores get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PostUpdateGores() { }

	/// <summary>
	/// Called before Projectiles get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PreUpdateProjectiles() { }

	/// <summary>
	/// Called after Projectiles get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PostUpdateProjectiles() { }

	/// <summary>
	/// Called before Items get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PreUpdateItems() { }

	/// <summary>
	/// Called after Items get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PostUpdateItems() { }

	/// <summary>
	/// Called before Dusts get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PreUpdateDusts() { }

	/// <summary>
	/// Called after Dusts get updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PostUpdateDusts() { }

	/// <summary>
	/// Called before Time gets updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PreUpdateTime() { }

	/// <summary>
	/// Called after Time gets updated.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PostUpdateTime() { }

	/// <summary>
	/// Use this method to have things happen in the world. In vanilla Terraria, a good example of code suitable for this hook is how Falling Stars fall to the ground during the night.
	/// <para/> Called in single player or on the server only.
	/// </summary>
	public virtual void PreUpdateWorld() { }

	/// <summary>
	/// Use this method to have things happen in the world. In vanilla Terraria, a good example of code suitable for this hook is how Falling Stars fall to the ground during the night.
	/// <para/> Called in single player or on the server only.
	/// </summary>
	public virtual void PostUpdateWorld() { }

	/// <summary>
	/// Called before Invasions get updated.
	/// <para/> Called in single player or on the server only.
	/// </summary>
	public virtual void PreUpdateInvasions() { }

	/// <summary>
	/// Called after Invasions get updated.
	/// <para/> Called in single player or on the server only.
	/// </summary>
	public virtual void PostUpdateInvasions() { }

	/// <summary>
	/// Called after the Network got updated, this is the last hook that happens in an update.
	/// <para/> Called in single player or on the server only.
	/// </summary>
	public virtual void PostUpdateEverything() { }

	/// <summary>
	/// Allows you to modify the elements of the in-game interface that get drawn. GameInterfaceLayer can be found in the Terraria.UI namespace. Check the <see href="https://github.com/tModLoader/tModLoader/wiki/Vanilla-Interface-layers-values">Vanilla Interface layers values wiki page</see> for vanilla interface layer names
	/// <para/> Called on all clients.
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
	/// Note: This hook should no longer be used. It is better to use the ModifyInterfaceLayers hook.
	/// <para/> Called on all clients.
	/// </summary>
	/// <param name="spriteBatch">The sprite batch.</param>
	public virtual void PostDrawInterface(SpriteBatch spriteBatch) { }

	/// <summary>
	/// Called right before map icon overlays are drawn. Use this hook to selectively hide existing <see cref="IMapLayer"/> or <see cref="ModMapLayer"/>
	/// <para/> Called on all clients.
	/// </summary>
	/// <param name="layers"></param>
	/// <param name="mapOverlayDrawContext"></param>
	public virtual void PreDrawMapIconOverlay(IReadOnlyList<IMapLayer> layers, MapOverlayDrawContext mapOverlayDrawContext) { }

	/// <summary>
	/// Called while the fullscreen map is active. Allows custom drawing to the map. Using <see cref="ModMapLayer"/> is more compatible and allows drawing on the minimap and fullscreen maps.
	/// <para/> Called on all clients.
	/// </summary>
	/// <param name="mouseText">The mouse text.</param>
	public virtual void PostDrawFullscreenMap(ref string mouseText) { }

	/// <summary>
	/// Called after the input keys are polled. Allows for modifying things like scroll wheel if your custom drawing should capture that.
	/// <para/> Called on all clients.
	/// </summary>
	public virtual void PostUpdateInput() { }

	/// <summary>
	/// Called when the Save and Quit button is pressed. One use for this hook is clearing out custom UI slots to return items to the player.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void PreSaveAndQuit() { }

	/// <summary>
	/// Called after drawing Tiles. Can be used for drawing a tile overlay akin to wires. Note that spritebatch should be begun and ended within this method.
	/// <para/> Called on all clients.
	/// </summary>
	public virtual void PostDrawTiles() { }

	/// <summary>
	/// Called after all other time calculations. Can be used to modify the speed at which time should progress per tick in seconds, along with the rate at which the tiles in the world and the events in the world should update with it.
	/// All fields are measured in in-game minutes per real-life second (min/sec).
	/// You may want to consider <see cref="Main.IsFastForwardingTime"/> and CreativePowerManager.Instance.GetPower&lt;CreativePowers.FreezeTime&gt;().Enabled here.
	/// <para/> Called on all clients and the server.
	/// </summary>
	/// <param name="timeRate">The speed at which time flows in min/sec.</param>
	/// <param name="tileUpdateRate">The speed at which tiles in the world update in min/sec.</param>
	/// <param name="eventUpdateRate">The speed at which various events in the world (weather changes, fallen star/fairy spawns, etc.) update in min/sec.</param>
	public virtual void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate) { }

	/// <summary>
	/// Allows you to save custom data for this system in the current world. Useful for things like saving world specific flags.
	/// <para/> For example, if your mod adds a boss and you want certain NPC to only spawn once it has been defeated, this is where you would store the information that that boss has been defeated in this world.
	/// <para/> <b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
	/// <para/> <b>NOTE:</b> Try to only save data that isn't default values.
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
	/// Allows you to save custom data for this system in the current world, and have that data accessible in the world select UI and during vanilla world loading.
	/// <br/><b>WARNING:</b> Saving too much data here will cause lag when opening the world select menu for users with many worlds.
	/// <br/>Can be retrieved via <see cref="WorldFileData.TryGetHeaderData(ModSystem, out TagCompound)"/> and <see cref="Main.ActiveWorldFileData"/>
	/// <br/>
	/// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
	/// <br/><b>NOTE:</b> Try to only save data that isn't default values.
	/// </summary>
	/// <param name="tag"> The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument only for the sake of convenience and optimization. </param>
	public virtual void SaveWorldHeader(TagCompound tag) { }

	/// <summary>
	/// Allows you to prevent the world and player from being loaded/selected as a valid combination, similar to Journey Mode pairing.
	/// </summary>
	public virtual bool CanWorldBePlayed(PlayerFileData playerData, WorldFileData worldFileData)
	{
		return true;
	}

	public virtual string WorldCanBePlayedRejectionMessage(PlayerFileData playerData, WorldFileData worldData)
	{
		return $"The selected character {playerData.Name} can not be used with the selected world {worldData.Name}.\n" +
						$"This could be due to mismatched Journey Mode or other mod specific changes.";
	}

	/// <summary>
	/// Allows you to send custom data between clients and server, which will be handled in <see cref="NetReceive"/>. This is useful for syncing information such as bosses that have been defeated.
	/// <para/> Called whenever <see cref="MessageID.WorldData"/> is successfully sent, for example after a boss is defeated, a new day starts, or a player joins the server.
	/// <para/> Only called on the server.
	/// </summary>
	/// <param name="writer">The writer.</param>
	public virtual void NetSend(BinaryWriter writer) { }

	/// <summary>
	/// Use this to receive information that was sent in <see cref="NetSend"/>.
	/// <para/> Called whenever <see cref="MessageID.WorldData"/> is successfully received.
	/// <para/> Only called on the client.
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
	/// For example, disabling the "Planting Trees" pass will cause a world to generate without trees. Placing a new Generation Pass before the "Dungeon" pass will prevent the mod's pass from cutting into the dungeon. <para/>
	/// To disable or hide generation passes, please use <see cref="GenPass.Disable"/> and defensive coding.
	/// <para/> See the <see href="https://github.com/tModLoader/tModLoader/wiki/World-Generation#determining-a-suitable-index">"Determining a suitable index" section of the World Generation wiki guide</see> for more information about how to properly use this for adding new world generation passes.
	/// </summary>
	public virtual void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) { }

	/// <summary>
	/// Use this method to place tiles in the world after world generation is complete.
	/// </summary>
	public virtual void PostWorldGen() { }

	/// <summary>
	/// Use this to reset any fields you set in any of your ModTile.NearbyEffects hooks back to their default values.
	/// <para/> Called on all clients.
	/// </summary>
	public virtual void ResetNearbyTileEffects() { }

	/// <summary>
	/// Similar to <see cref="ModifyWorldGenTasks(List{GenPass}, ref double)"/>, but occurs in-game when Hardmode starts. Can be used to modify which tasks should be done and/or add custom tasks. <para/>
	/// By default the list will only contain 5 items, the vanilla hardmode tasks called "Hardmode Good Remix", "Hardmode Good", "Hardmode Evil", "Hardmode Walls", and "Hardmode Announcement". "Hardmode Good Remix" will only be enabled on <see href="https://terraria.wiki.gg/wiki/Don%27t_dig_up">Don't dig up</see> worlds (<see cref="Main.remixWorld"/>) while "Hardmode Good" and "Hardmode Evil" will be enabled otherwise.<para/>
	/// To disable or hide tasks, please use <see cref="GenPass.Disable"/> and defensive coding.
	/// </summary>
	public virtual void ModifyHardmodeTasks(List<GenPass> list) { }

	/// <summary>
	/// Allows you to modify color of light the sun emits.
	/// <para/> Called on all clients.
	/// </summary>
	/// <param name="tileColor">Tile lighting color</param>
	/// <param name="backgroundColor">Background lighting color</param>
	public virtual void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) { }

	/// <summary>
	/// Allows you to modify overall brightness of lights. Can be used to create effects similar to what night vision and darkness (de)buffs give you. Values too high or too low might result in glitches. For night vision effect use scale 1.03
	/// <para/> Called on all clients.
	/// </summary>
	/// <param name="scale">Brightness scale</param>
	public virtual void ModifyLightingBrightness(ref float scale) { }

	/// <summary>
	/// Allows you to store information about how many of each tile is nearby the player. This is useful for counting how many tiles of a certain custom biome there are.
	/// <para/> The <paramref name="tileCounts"/> parameter is a read-only span (treat this as an array) that stores the tile count indexed by tile type.
	/// <para/> Called on all clients.
	/// </summary>
	public virtual void TileCountsAvailable(ReadOnlySpan<int> tileCounts) { }

	/// <summary>
	/// Called after all mods register content and before all mods start setting up content. This can be used to initialize data structures dependent on the count of various content IDs, usually through SetFactory instances directly.
	/// <para/> For example, <c>ItemID.Sets.Factory.CreateBoolSet(false, ItemID.PoisonDart, ItemID.PoisonedKnife)</c> will create an array with length equal to the total number of items in the game with the specified item types set to true.
	/// <para/> The CreateNamedXSet methods can be used to expose an ID set for other mods to use by name without a mod dependency.
	/// <para/> See also <see cref="ReinitializeDuringResizeArraysAttribute "/> for another similar option.
	/// </summary>
	public virtual void ResizeArrays() {
	}
}
