using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A ModWorld instance represents an extension of a World. You can store fields in the ModWorld classes to keep track of mod-specific information on the world. It also contains hooks to insert your code into the world generation process.
	/// </summary>
	public class ModWorld
	{
		/// <summary>
		/// The mod that added this type of ModWorld.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The name of this ModWorld. Used for distinguishing between multiple ModWorlds added by a single Mod.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// Allows you to automatically add a ModWorld instead of using Mod.AddModWorld. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name. Use this to either force or stop an autoload, or change the name that identifies this type of ModWorld.
		/// </summary>
		public virtual bool Autoload(ref string name) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Called whenever the world is loaded. This can be used to initialize data structures, etc.
		/// </summary>
		public virtual void Initialize() {
		}

		/// <summary>
		/// Allows you to save custom data for this world. Useful for things like saving world specific flags. For example, if your mod adds a boss and you want certain NPC to only spawn once it has been defeated, this is where you would store the information that that boss has been defeated in this world. Returns null by default.
		/// </summary>
		public virtual TagCompound Save() {
			return null;
		}

		/// <summary>
		/// Allows you to load custom data you have saved for this world.
		/// </summary>
		public virtual void Load(TagCompound tag) {
		}

		/// <summary>
		/// Allows you to load pre-v0.9 custom data you have saved for this world.
		/// </summary>
		public virtual void LoadLegacy(BinaryReader reader) {
		}

		/// <summary>
		/// Allows you to send custom data between clients and server. This is useful for syncing information such as bosses that have been defeated.
		/// </summary>
		public virtual void NetSend(BinaryWriter writer) {
		}

		/// <summary>
		/// Allows you to do things with custom data that is received between clients and server.
		/// </summary>
		public virtual void NetReceive(BinaryReader reader) {
		}

		/// <summary>
		/// Allows a mod to run code before a world is generated.
		/// </summary>
		public virtual void PreWorldGen() {
		}

		/// <summary>
		/// A more advanced option to PostWorldGen, this method allows you modify the list of Generation Passes before a new world begins to be generated. For example, removing the "Planting Trees" pass will cause a world to generate without trees. Placing a new Generation Pass before the "Dungeon" pass will prevent the the mod's pass from cutting into the dungeon.
		/// </summary>
		public virtual void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
		}

		/// <summary>
		/// Use this method to place tiles in the world after world generation is complete.
		/// </summary>
		public virtual void PostWorldGen() {
		}

		/// <summary>
		/// Use this to reset any fields you set in any of your ModTile.NearbyEffects hooks back to their default values.
		/// </summary>
		public virtual void ResetNearbyTileEffects() {
		}

		/// <summary>
		/// Use this method to have things happen in the world. In vanilla Terraria, a good example of code suitable for this hook is how Falling Stars fall to the ground during the night. This hook is called every frame.
		/// </summary>
		public virtual void PreUpdate() {
		}

		/// <summary>
		/// Use this method to have things happen in the world. In vanilla Terraria, a good example of code suitable for this hook is how Falling Stars fall to the ground during the night. This hook is called every frame.
		/// </summary>
		public virtual void PostUpdate() {
		}

		/// <summary>
		/// Allows you to store information about how many of each tile is nearby the player. This is useful for counting how many tiles of a certain custom biome there are. The tileCounts parameter stores the tile count indexed by tile type.
		/// </summary>
		public virtual void TileCountsAvailable(int[] tileCounts) {
		}

		/// <summary>
		/// Allows you to change the water style (determines water color) that is currently being used.
		/// </summary>
		public virtual void ChooseWaterStyle(ref int style) {
		}

		/// <summary>
		/// Similar to ModifyWorldGenTasks, but occurs in-game when Hardmode starts. Can be used to modify which tasks should be done and/or add custom tasks. By default the list will only contain 4 items, the vanilla hardmode tasks called "Hardmode Good", "Hardmode Evil", "Hardmode Walls", and "Hardmode Announcment"
		/// </summary>
		public virtual void ModifyHardmodeTasks(List<GenPass> list) {
		}

		/// <summary>
		/// Called after drawing Tiles. Can be used for drawing a tile overlay akin to wires. Note that spritebatch should be begun and ended within this method.
		/// </summary>
		public virtual void PostDrawTiles() {
		}
	}
}
