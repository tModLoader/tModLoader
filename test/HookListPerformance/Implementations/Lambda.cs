using System;

namespace HookListPerformance
{
	class Lambda : Implementation
	{
		private static void ForeachInstance<T>(Instanced<T>[] instances, int[] hookInds, Action<T> action) {
			if (instances.Length == 0)
				return;

			int i = 0;
			var inst = instances[i];
			foreach (int globalIndex in hookInds) {
				while (inst.index < globalIndex) {
					if (++i == instances.Length)
						return;

					inst = instances[i];
				}

				if (inst.index == globalIndex)
					action(inst.instance);
			}
		}

		private static float HookDoEffect(ref float input, Instanced<GlobalItem>[] instances, int[] hookInds) {
			float result = 0;
			float inputCopy = input;
			ForeachInstance(instances, hookInds,
				globalItem => result += globalItem.DoEffect(ref inputCopy));

			input = inputCopy;
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