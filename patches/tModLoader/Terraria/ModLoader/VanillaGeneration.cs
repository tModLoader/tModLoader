using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	[Autoload(false)]
	internal sealed class VanillaGeneration : Generation
	{
		private readonly string name;

		public List<GenPass> generationTasks;
		public float totalWeight;

		public override string Name => name;

		public VanillaGeneration(string name) {
			this.name = name;
		}

		public override void ModifyGenerationTasks(List<GenPass> generationTasks, ref float totalWeight) {
			generationTasks.AddRange(this.generationTasks);
			totalWeight = this.totalWeight;
		}
	}
}
