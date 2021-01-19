using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	public abstract class ModGeneration : ModType
	{
		protected override void Register() {
			GenerationHooks.Add(this);
			ModTypeLookup<ModGeneration>.Register(this);
		}

		/// <summary>
		/// Allows you to set specific variables used before worldgen starts, like maxTilesX and maxTilesY (world size).
		/// </summary>
		public virtual void SetWorldGenDefaults() { }

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
		/// Similar to ModifyWorldGenTasks, but occurs in-game when a meteor drops. Can be used to modify which tasks should be done and/or add custom tasks. By default the list will only contain 2 items, the vanilla meteor tasks called "Meteor Spawn" and "Meteor Announcement".
		/// </summary>
		public virtual void ModifyMeteorTasks(List<GenPass> list) { }

		/// <summary>
		/// Similar to ModifyWorldGenTasks, but occurs in-game when Hardmode starts. Can be used to modify which tasks should be done and/or add custom tasks. By default the list will only contain 4 items, the vanilla hardmode tasks called "Hardmode Good", "Hardmode Evil", "Hardmode Walls", and "Hardmode Announcment"
		/// </summary>
		public virtual void ModifyHardmodeTasks(List<GenPass> list) { }

		/// <summary>
		/// Similar to ModifyWorldGenTasks, but occurs in-game when a Demon Altar or Crimson Altar is broken. Can be used to modify which tasks should be done and/or add custom tasks. By default the list will only contain 4 items, the vanilla altar tasks called "Altar Decide Ore", "Altar Spawn Ore", "Altar Convert Stone", and "Altar Spawn Wraith".
		/// </summary>
		public virtual void ModifyAltarTasks(List<GenPass> list) { }

	}
}

