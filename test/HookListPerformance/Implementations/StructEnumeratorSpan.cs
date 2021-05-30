using System;
using System.Runtime.CompilerServices;

namespace HookListPerformance
{
	class StructEnumeratorSpan : Implementation
	{
		ref struct InstanceEnumerator<T>
		{
			private readonly ReadOnlySpan<Instanced<T>> instances;
			private readonly ReadOnlySpan<int> hookInds;

			// ideally this would be Instanced<T> and drop the need for the ii variable in the MoveNext function
			// but again, struct in struct promotion (and also increasing the 'field count'
			private T current;
			// i and j are combined into a 64bit variable beacuse JIT currently won't unpack/promote structs with > 4 fields into registers
			// See https://github.com/dotnet/runtime/issues/6534
			private ulong ij;
			//private int i;
			//private int j;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public InstanceEnumerator(ReadOnlySpan<Instanced<T>> instances, ReadOnlySpan<int> hookInds) {
				this.instances = instances;
				this.hookInds = hookInds;
				current = default;
				ij = 0;
				//i = 0;
				//j = 0;
			}

			public T Current => current;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext() {
				var ii = -1;
				while ((int)(ij >> 32) < hookInds.Length) {
					int hookIndex = hookInds[(int)(ij >> 32)];
					ij += 1L << 32;
					while (ii < hookIndex) {
						if ((int)ij == instances.Length)
							return false;

						var inst = instances[(int)(ij++)];
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