using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader.Core
{
	public class HookList<T> where T : GlobalType
	{
		// Don't change a single line without performance testing and checking the disassembly. As of NET 5.0.0, this is the fastest implementation acheivable short of hand-coding
		public ref struct InstanceEnumerator
		{
			private readonly ReadOnlySpan<Instanced<T>> instances;
			private readonly ReadOnlySpan<int> hookInds;

			private T current;
			private int i;
			private int j;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public InstanceEnumerator(ReadOnlySpan<Instanced<T>> instances, ReadOnlySpan<int> hookInds) {
				this.instances = instances;
				this.hookInds = hookInds;

				current = default;
				i = 0;
				j = 0;
			}

			public T Current => current;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext() {
				int ii = -1;

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

			public InstanceEnumerator GetEnumerator() => this;
		}

		public readonly MethodInfo method;

		private int[] registeredGlobalIndices = Array.Empty<int>();

		internal HookList(MethodInfo method) {
			this.method = method;
		}

		public InstanceEnumerator Enumerate(IEntityWithGlobals<T> entity) => Enumerate(entity.Globals);

		public InstanceEnumerator Enumerate(ReadOnlySpan<Instanced<T>> instances) => new(instances, registeredGlobalIndices);

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
