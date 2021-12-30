using System;
using System.Runtime.CompilerServices;

namespace HookListPerformance
{
	class StructEnumerator5Fields : Implementation
	{
		// Don't change a single line without performance testing and checking the disassembly. As of NET 5.0.0, this implementation is on par with hand-coding
		// Disassembly checked using Relyze Desktop 3.3.0
		ref struct InstanceEnumerator<T>
		{
			// These have to be arrays rather than ReadOnlySpan as the JIT won't unpack/promote 'struct in struct' (or span in struct either)
			// Revisit with .NET 6 https://github.com/dotnet/runtime/issues/37924
			private readonly Instanced<T>[] instances;
			private readonly int[] hookInds;

			// ideally this would be Instanced<T> and drop the need for the ii variable in the MoveNext function
			// but again, struct in struct promotion (and also increasing the 'field count'
			private T current;
			private int i;
			private int j;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public InstanceEnumerator(Instanced<T>[] instances, int[] hookInds) {
				this.instances = instances;
				this.hookInds = hookInds;
				current = default;
				i = 0;
				j = 0;
			}

			public T Current => current;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext() {
				var ii = -1;
				while (j < hookInds.Length) {
					int hookIndex = hookInds[j++];
					while (ii < hookIndex) {
						if (i == instances.Length)
							return false;

						var inst = instances[i++];
						ii = inst.index;
						current = inst.instance;
					}

					if (ii == hookIndex) {
						return true;
					}
				}

				return false;
			}

			public InstanceEnumerator<T> GetEnumerator() => this;
		}

		private static float HookDoEffect(ref float input, Instanced<GlobalItem>[] instances, int[] hookInds) {
			float result = 0;
			foreach (var globalItem in new InstanceEnumerator<GlobalItem>(instances, hookInds))
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