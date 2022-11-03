using System.Collections.Generic;

namespace HookListPerformance
{
	class YieldEnumerator : Implementation
	{
		private static IEnumerable<T> EnumerateInstances<T>(Instanced<T>[] instances, int[] hookInds) {
			int i = 0;
			var inst = instances[i];
			foreach (int globalIndex in hookInds) {
				while (inst.index < globalIndex) {
					if (++i == instances.Length)
						yield break;

					inst = instances[i];
				}

				if (inst.index == globalIndex)
					yield return inst.instance;
			}
		}

		private static float HookDoEffect(ref float input, Instanced<GlobalItem>[] instances, int[] hookInds) {
			float result = 0;
			foreach (var globalItem in EnumerateInstances(instances, hookInds))
				result += globalItem.DoEffect(ref input);

			return result;
		}

		public override float HookDoEffect(float input, Item[] items, int[] hookInds) {
			float result = 0;
			foreach (var item in items)
				result += HookDoEffect(ref input, item.instancedGlobals, hookInds);
			return result;
		}
	}
}