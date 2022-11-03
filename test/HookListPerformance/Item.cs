using System.Linq;

namespace HookListPerformance
{
	class Item
	{
		public readonly GlobalItem[] globals;
		public readonly Instanced<GlobalItem>[] instancedGlobals;
		public readonly InstancedUnpacked<GlobalItem>[] instancedUnpackedGlobals;

		public Item(GlobalItem[] globals) {
			this.globals = globals;
			instancedGlobals = globals.Select(g => new Instanced<GlobalItem>(g, g.index)).ToArray();
			instancedUnpackedGlobals = globals.Select(g => new InstancedUnpacked<GlobalItem>(g, g.index)).ToArray();
		}
	}
}