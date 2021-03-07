using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	public abstract class Generation : ModType
	{
		public static readonly Generation WorldGeneration = new VanillaGeneration(nameof(WorldGeneration));
		public static readonly Generation AltarSmashGeneration = new VanillaGeneration(nameof(AltarSmashGeneration));
		public static readonly Generation MeteorGeneration = new VanillaGeneration(nameof(MeteorGeneration));
		public static readonly Generation HardmodeGeneration = new VanillaGeneration(nameof(HardmodeGeneration));

		protected override void Register() {
			GenerationHooks.generations.Add(this);
			ModTypeLookup<Generation>.Register(this);
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
		public virtual void ModifyGenerationTasks(List<GenPass> tasks, ref float totalWeight) { }

		/// <summary>
		/// Use this method to place tiles in the world after world generation is complete.
		/// </summary>
		public virtual void PostWorldGen() { }
	}
}

