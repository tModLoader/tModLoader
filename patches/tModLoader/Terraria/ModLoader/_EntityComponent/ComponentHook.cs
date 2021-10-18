using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Terraria.ModLoader
{
	public abstract class ComponentHook
	{
		internal abstract void Update(IReadOnlyList<Component> instances);
	}

	public unsafe class ComponentHook<TComponentDelegate> : ComponentHook
		where TComponentDelegate : Delegate
	{
		// Don't change a single line without performance testing and checking the disassembly. As of NET 5.0.0, this implementation is on par with hand-coding
		// Disassembly checked using Relyze Desktop 3.3.0
		public ref struct InstanceEnumerator
		{
			// These have to be arrays rather than ReadOnlySpan as the JIT won't unpack/promote 'struct in struct' (or span in struct either)
			// Revisit with .NET 6 https://github.com/dotnet/runtime/issues/37924
			private readonly ReadOnlySpan<Instanced<Component>> instances;
			private readonly (int index, TComponentDelegate componentDelegate)[] registeredComponentInfo;
			// i and j are combined into a 64bit variable beacuse JIT currently won't unpack/promote structs with > 4 fields into registers
			// See https://github.com/dotnet/runtime/issues/6534
			private ulong ij;

			// ideally this would be Instanced<T> and drop the need for the ii variable in the MoveNext function
			// but again, struct in struct promotion (and also increasing the 'field count')
			public (Component component, TComponentDelegate componentDelegate) Current { get; private set; }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public InstanceEnumerator(ReadOnlySpan<Instanced<Component>> instances, (int index, TComponentDelegate componentDelegate)[] registeredComponentInfo) {
				this.instances = instances;
				this.registeredComponentInfo = registeredComponentInfo;

				Current = default;
				ij = 0;
				//i = 0;
				//j = 0;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext() {
				int ii = -1;

				while ((int)(ij >> 32) < registeredComponentInfo.Length) {
					var (hookIndex, hookDelegate) = registeredComponentInfo[(int)(ij >> 32)];

					ij += 1L << 32;

					while (ii < hookIndex) {
						if ((int)ij == instances.Length)
							return false;

						var inst = instances[(int)ij++];

						ii = inst.index;
						Current = (inst.instance, hookDelegate);
					}

					if (ii == hookIndex) {
						return true;
					}
				}

				return false;
			}

			public InstanceEnumerator GetEnumerator() => this;
		}

		private readonly MethodInfo method;

		private (int index, TComponentDelegate componentDelegate)[] registeredComponentInfo = Array.Empty<(int, TComponentDelegate)>();

		public ComponentHook(MethodInfo method) {
			this.method = method;

			ComponentLoader.RegisterComponentHook(this);
		}

		public InstanceEnumerator Enumerate(GameObject gameObject)
			=> Enumerate(CollectionsMarshal.AsSpan(gameObject.components));

		public InstanceEnumerator Enumerate(ReadOnlySpan<Instanced<Component>> instances)
			=> new(instances, registeredComponentInfo);

		internal override void Update(IReadOnlyList<Component> instances) {
			if (!method.IsVirtual)
				throw new ArgumentException("Cannot build hook for non-virtual method " + method);

			var baseDeclaringType = method.DeclaringType;

			if (!baseDeclaringType.IsInterface)
				throw new ArgumentException("Method must come from an interface.");

			var argTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
			int? interfaceMethodId = null;
			var componentInfoList = new List<(int, TComponentDelegate)>();

			for (int i = 0; i < instances.Count; i++) {
				var currentType = instances[i].GetType();

				// In case of interfaces, we can skip shenanigans that 'explicit interface method implementations' bring,
				// and just check if the provider implements the interface.
				if (!baseDeclaringType.IsAssignableFrom(currentType)) {
					continue;
				}

				var interfaceMap = currentType.GetInterfaceMap(baseDeclaringType);

				interfaceMethodId ??= Array.IndexOf(interfaceMap.InterfaceMethods, method);

				var targetMethod = interfaceMap.TargetMethods[interfaceMethodId.Value];
				var delegateParameters = targetMethod.GetParameters().Select(p => p.ParameterType).Prepend(targetMethod.DeclaringType).Append(targetMethod.ReturnType).ToArray();
				var delegateType = Expression.GetDelegateType(delegateParameters);
				var componentDelegate = Unsafe.As<TComponentDelegate>(Delegate.CreateDelegate(delegateType, targetMethod));

				componentInfoList.Add((i, componentDelegate));
			}

			registeredComponentInfo = componentInfoList.ToArray();
		}
	}
}
