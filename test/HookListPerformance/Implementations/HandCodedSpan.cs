using System;

namespace HookListPerformance
{
	class HandCodedSpan : Implementation
	{
		private static float HookDoEffect(ref float input, ReadOnlySpan<Instanced<GlobalItem>> instances, ReadOnlySpan<int> hookInds) {
			float result = 0;

			var ii = -1;
			int i = 0;
			GlobalItem current = null;
			foreach (int globalIndex in hookInds) {
				while (ii < globalIndex) {
					if (i == instances.Length)
						goto end;

					var inst = instances[i++];
					ii = inst.index;
					current = inst.instance;
				}

				if (ii == globalIndex)
					result += current.DoEffect(ref input);
			}

		end:
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