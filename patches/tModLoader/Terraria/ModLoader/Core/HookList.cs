using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace Terraria.ModLoader.Core
{
	public class HookList<T> where T : GlobalType
	{
		public readonly MethodInfo method;

		private int[] registeredGlobalIndices = new int[0];

		internal HookList(MethodInfo method) {
			this.method = method;
		}

		protected internal IEnumerable<T> Enumerate(IEntityWithGlobals<T> entity) => Enumerate(entity.Globals.array);

		protected internal IEnumerable<T> Enumerate(Instanced<T>[] instances) {
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
