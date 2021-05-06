using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader.Core
{
	public class HookList<T> where T : GlobalType
	{
		// Don't change a single line without performance testing and checking the disassembly. As of NET 5.0.0, this implementation is on par with hand-coding
		// Disassembly checked using Relyze Desktop 3.3.0
		public ref struct InstanceEnumerator
		{
			// These have to be arrays rather than ReadOnlySpan as the JIT won't unpack/promote 'struct in struct' (or span in struct either)
			// Revisit with .NET 6 https://github.com/dotnet/runtime/issues/37924
			private readonly Instanced<T>[] instances;
			private readonly int[] hookInds;

			// ideally this would be Instanced<T> and drop the need for the ii variable in the MoveNext function
			// but again, struct in struct promotion (and also increasing the 'field count'
			private T current;
			// i and j are combined into a 64bit variable beacuse JIT currently won't unpack/promote structs with > 4 fields into registers
			// See https://github.com/dotnet/runtime/issues/6534
			private ulong ij;
			//private int i;
			//private int j;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public InstanceEnumerator(Instanced<T>[] instances, int[] hookInds) {
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

			public InstanceEnumerator GetEnumerator() => this;
		}

		public readonly MethodInfo method;

		private int[] registeredGlobalIndices = Array.Empty<int>();

		internal HookList(MethodInfo method) {
			this.method = method;
		}

		public InstanceEnumerator Enumerate(IEntityWithGlobals<T> entity) => Enumerate(entity.Globals.array);

		public InstanceEnumerator Enumerate(Instanced<T>[] instances) => new(instances, registeredGlobalIndices);

		public void Update(IList<T> instances) {
			registeredGlobalIndices = ModLoader.BuildGlobalHookNew(instances, method);
		}
	}

	public class HookList<TGlobal, TDelegate> : HookList<TGlobal>
		where TGlobal : GlobalType
		where TDelegate : Delegate
	{
		public TDelegate Invoke { get; private set; }

		public HookList(MethodInfo method, Func<HookList<TGlobal>, TDelegate> getInvoker) : base(method) {
			Invoke = getInvoker(this);
		}

		internal HookList(Expression<Func<TGlobal, TDelegate>> method, Func<HookList<TGlobal>, TDelegate> getInvoker) : base(ModLoader.Method(method)) {
			Invoke = getInvoker(this);
		}
	}
}
