using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	public abstract class GlobalGeneration : ModType
	{
		protected override void Register() {
			GenerationHooks.globalGenerations.Add(this);
			ModTypeLookup<GlobalGeneration>.Register(this);
		}

		/// <summary>
		/// Allows you to set specific variables used before worldgen starts, like maxTilesX and maxTilesY (world size).
		/// </summary>
		public virtual void SetWorldGenDefaults(Generation generation) { }

		/// <summary>
		/// Allows a mod to run code before a world is generated.
		/// </summary>
		public virtual void PreWorldGen(Generation generation) { }

		/// <summary>
		/// A more advanced option to PostWorldGen, this method allows you modify the list of Generation Passes before a new world begins to be generated. <para/>
		/// For example, removing the "Planting Trees" pass will cause a world to generate without trees. Placing a new Generation Pass before the "Dungeon" pass will prevent the the mod's pass from cutting into the dungeon.
		/// </summary>
		public virtual void ModifyGenerationTasks(Generation generation, List<GenPass> tasks, ref float totalWeight) { }

		/// <summary>
		/// Use this method to place tiles in the world after world generation is complete.
		/// </summary>
		public virtual void PostWorldGen(Generation generation) { }
	}
}

