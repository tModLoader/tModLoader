using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria.Graphics;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	partial class ModSystem
	{
		/// <summary>
		/// This hook is called right after Mod.Load(), which is guaranteed to be called after all content has been autoloaded.
		/// </summary>
		public virtual void OnModLoad() { }

		/// <summary>
		/// Allows you to load things in your system after the mod's content has been setup (arrays have been resized to fit the content, etc).
		/// </summary>
		public virtual void PostSetupContent() { }

		/// <summary>
		/// Called whenever a world is loaded. This can be used to initialize data structures, etc.
		/// </summary>
		public virtual void OnWorldLoad() { }

		/// <summary>
		/// Allows you to determine what music should currently play.
		/// </summary>
		/// <param name="music">The music.</param>
		/// <param name="priority">The music priority.</param>
		public virtual void UpdateMusic(ref int music, ref MusicPriority priority) { }

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
		/// <para />
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdatePlayerNPC" />.
		/// </summary>
		public virtual void PreUpdateEntities() { }

		/// <summary>
		/// Called after Players got updated, but before any NPCs get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="PreUpdateEntities" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateNPCGore" />.
		/// </summary>
		public virtual void MidUpdatePlayerNPC() { }

		/// <summary>
		/// Called after NPCs got updated, but before any Gores get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdatePlayerNPC" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateGoreProjectile" />.
		/// </summary>
		public virtual void MidUpdateNPCGore() { }

		/// <summary>
		/// Called after Gores got updated, but before any Projectiles get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateNPCGore" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateProjectileItem" />.
		/// </summary>
		public virtual void MidUpdateGoreProjectile() { }

		/// <summary>
		/// Gets called immediately after all Projectiles are updated, but before any Items get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateGoreProjectile" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateItemDust" />.
		/// </summary>
		public virtual void MidUpdateProjectileItem() { }

		/// <summary>
		/// Called after Items got updated, but before any Dust gets updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateProjectileItem" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateDustTime" />.
		/// </summary>
		public virtual void MidUpdateItemDust() { }

		/// <summary>
		/// Called after Dust got updated, but before Time (day/night, events, etc.) gets updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateItemDust" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateTimeWorld" />.
		/// </summary>
		public virtual void MidUpdateDustTime() { }

		/// <summary>
		/// Called after Time got updated, but before the World gets updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateDustTime" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateInvasionNet" />.
		/// </summary>
		public virtual void MidUpdateTimeWorld() { }

		/// <summary>
		/// Called after Invasions got updated. The only thing that is updated after this is the Network.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateTimeWorld" />.
		/// If you are looking to hook even after the Network is updated, see <see cref="PostUpdateEverything" />.
		/// </summary>
		public virtual void MidUpdateInvasionNet() { }

		/// <summary>
		/// Called after the Network got updated, this is the last hook that happens in an update.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateInvasionNet" />.
		/// </summary>
		public virtual void PostUpdateEverything() { }

		/// <summary>
		/// Allows you to modify the elements of the in-game interface that get drawn. GameInterfaceLayer can be found in the Terraria.UI namespace. Check https://github.com/tModLoader/tModLoader/wiki/Vanilla-Interface-layers-values for vanilla interface layer names
		/// </summary>
		/// <param name="layers">The layers.</param>
		public virtual void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) { }

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
		/// Called after interface is drawn but right before mouse and mouse hover text is drawn. Allows for drawing interface.
		/// 
		/// Note: This hook should no longer be used. It is better to use the ModifyInterfaceLayers hook.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch.</param>
		public virtual void PostDrawInterface(SpriteBatch spriteBatch) { }

		/// <summary>
		/// Called while the fullscreen map is active. Allows custom drawing to the map.
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
		/// Called after all other time calculations. Can be used to modify the speed at which time should progress per tick in seconds, along with the rate at which the world should update with it.
		/// You may want to consider Main.fastForwardTime and CreativePowerManager.Instance.GetPower<CreativePowers.FreezeTime>().Enabled here.
		/// </summary>
		public virtual void ModifyTimeRate(ref int timeRate, ref int tileUpdateRate) { }

		/// <summary>
		/// Allows you to save custom data for this system in the current world. Useful for things like saving world specific flags. For example, if your mod adds a boss and you want certain NPC to only spawn once it has been defeated, this is where you would store the information that that boss has been defeated in this world. Returns null by default.
		/// </summary>
		public virtual TagCompound SaveWorldData() => null;

		/// <summary>
		/// Allows you to load custom data you have saved for this system in the currently loading world.
		/// </summary>
		public virtual void LoadWorldData(TagCompound tag) { }

		/// <summary>
		/// Allows you to send custom data between clients and server. This is useful for syncing information such as bosses that have been defeated.
		/// </summary>
		public virtual void NetSend(BinaryWriter writer) { }

		/// <summary>
		/// Allows you to do things with custom data that is received between clients and server.
		/// </summary>
		public virtual void NetReceive(BinaryReader reader) { }

		/// <summary>
		/// Allows a mod to run code before a world is generated.
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
		/// Use this method to have things happen in the world. In vanilla Terraria, a good example of code suitable for this hook is how Falling Stars fall to the ground during the night. This hook is called every frame.
		/// </summary>
		public virtual void PreUpdateWorld() { }

		/// <summary>
		/// Use this method to have things happen in the world. In vanilla Terraria, a good example of code suitable for this hook is how Falling Stars fall to the ground during the night. This hook is called every frame.
		/// </summary>
		public virtual void PostUpdateWorld() { }

		/// <summary>
		/// Allows you to store information about how many of each tile is nearby the player. This is useful for counting how many tiles of a certain custom biome there are. The tileCounts parameter stores the tile count indexed by tile type.
		/// </summary>
		public virtual void TileCountsAvailable(int[] tileCounts) { }

		/// <summary>
		/// Allows you to change the water style (determines water color) that is currently being used.
		/// </summary>
		public virtual void ChooseWaterStyle(ref int style) { }

		/// <summary>
		/// Similar to ModifyWorldGenTasks, but occurs in-game when Hardmode starts. Can be used to modify which tasks should be done and/or add custom tasks. By default the list will only contain 4 items, the vanilla hardmode tasks called "Hardmode Good", "Hardmode Evil", "Hardmode Walls", and "Hardmode Announcment"
		/// </summary>
		public virtual void ModifyHardmodeTasks(List<GenPass> list) { }
	}
}
