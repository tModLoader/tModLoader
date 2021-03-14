using System.Collections.Generic;
using System.Reflection;

namespace Terraria.ModLoader.Core
{
	public class HookList<T> where T : GlobalType
	{
		public readonly MethodInfo method;

		private int[] registeredGlobalIndices = new int[0];

		public HookList(MethodInfo method) {
			this.method = method;
		}

		public IEnumerable<T> Enumerate(IEntityWithGlobals<T> entity) => Enumerate(entity.Globals.array);

		public IEnumerable<T> Enumerate(Instanced<T>[] instances) {
			if (instances.Length == 0) {
				yield break;
			}

			int i = 0;
			var instance = instances[i];

			foreach (int globalIndex in registeredGlobalIndices) {
				while (instance.index < globalIndex) {
					if (++i == instances.Length)
						yield break;

					instance = instances[i];
				}

				if (instance.index == globalIndex) {
					yield return instance.instance;
				}
			}
		}

		public void Update(IList<T> instances) => registeredGlobalIndices = ModLoader.BuildGlobalHookNew(instances, method);
	}
}
