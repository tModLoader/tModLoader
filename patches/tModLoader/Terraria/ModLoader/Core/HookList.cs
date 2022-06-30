using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Terraria.ModLoader.Core
{
	public class HookList<T> where T : class
	{
		// Don't change a single line without performance testing and checking the disassembly. As of NET 6.0.0, this implementation is on par with hand-coding C#
		// Disassembly checked using Relyze Desktop 3.3.0
		public ref struct InstanceEnumerator
		{
			// These have to be arrays rather than ReadOnlySpan as the JIT won't unpack/promote 'struct in struct' (or span in struct either)
			// Revisit with .NET 6 https://github.com/dotnet/runtime/issues/37924
			private readonly Instanced<T>[] instances;
			private readonly int[] hookIndices;

			// ideally this would be Instanced<T> and drop the need for the ii variable in the MoveNext function
			// but again, struct in struct promotion (and also increasing the 'field count'
			private T current;
			// i and j are combined into a 64bit variable beacuse JIT currently won't unpack/promote structs with > 4 fields into registers
			// See https://github.com/dotnet/runtime/issues/6534
			private ulong ij;
			//private int i;
			//private int j;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public InstanceEnumerator(Instanced<T>[] instances, int[] hookIndices) {
				this.instances = instances;
				this.hookIndices = hookIndices;
				current = default;
				ij = 0;
				//i = 0;
				//j = 0;
			}

			public T Current => current;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext() {
				int ii = -1;

				while ((int)(ij >> 32) < hookIndices.Length) {
					int hookIndex = hookIndices[(int)(ij >> 32)];

					ij += 1L << 32;

					while (ii < hookIndex) {
						if ((int)ij == instances.Length)
							return false;

						var inst = instances[(int)(ij++)];
						ii = inst.Index;
						current = inst.Instance;
					}

					if (ii == hookIndex) {
						return true;
					}
				}

				return false;
			}

			public InstanceEnumerator GetEnumerator() => this;
		}

		public readonly MethodInfo method;

		private int[] indices = Array.Empty<int>();

		public HookList(MethodInfo method) {
			this.method = method;
		}
		
		public InstanceEnumerator Enumerate(Instanced<T>[] instances)
			=> new(instances, indices);

		public FilteredArrayEnumerator<T> Enumerate(T[] instances)
			=> new(instances, indices);

		public FilteredSpanEnumerator<T> Enumerate(ReadOnlySpan<T> instances)
			=> new(instances, indices);

		public FilteredSpanEnumerator<T> Enumerate(List<T> instances) =>
			Enumerate(CollectionsMarshal.AsSpan(instances));

		public void Update<U>(IList<U> instances) where U : IIndexed {
			indices = instances.WhereMethodIsOverridden(method).Select(g => (int)g.Index).ToArray();
		}

		public static HookList<T> Create<F>(Expression<Func<T, F>> expr) where F : Delegate
			=> new(expr.ToMethodInfo());
	}

	public static class HookList
	{
		public static HookList<U>.InstanceEnumerator Enumerate<U>(this HookList<U> hookList, IEntityWithGlobals<U> entity) where U : GlobalType
			=> hookList.Enumerate(entity.Globals.array);
	}
}
