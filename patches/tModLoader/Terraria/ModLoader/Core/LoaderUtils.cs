using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.Core
{
	internal static class LoaderUtils
	{
		/// <summary> Calls static constructors on the provided type and, optionally, its nested types. </summary>
		public static void ResetStaticMembers(Type type, bool recursive) {
#if NETCORE
			var typeInitializer = type.TypeInitializer;

			if (typeInitializer != null) {
				var field = typeInitializer
					.GetType()
					.GetField("m_invocationFlags", BindingFlags.NonPublic | BindingFlags.Instance);

				object previousValue = field.GetValue(typeInitializer);

				//.NET Core uses invocation flags on static constructor to ensure that they're never called twice. We'll have to ignore the law, and remove these flags temporarily.
				field.SetValue(typeInitializer, (uint)0x00000001); //INVOCATION_FLAGS_INITIALIZED

				typeInitializer.Invoke(null, null);

				field.SetValue(typeInitializer, previousValue);
			}
#else
			type.TypeInitializer?.Invoke(null, null);
#endif

			if (recursive) {
				foreach (var nestedType in type.GetNestedTypes()) {
					ResetStaticMembers(nestedType, recursive);
				}
			}
		}

		public static void InstantiateGlobals<TGlobal, TEntity>(TEntity entity, IEnumerable<TGlobal> globals, ref Instanced<TGlobal>[] entityGlobals, Func<TGlobal, TGlobal> getInstance, Action midInstantiationAction) where TGlobal : GlobalType<TEntity> {
			entityGlobals = globals
				.Where(g => g.AppliesToEntity(entity, false))
				.Select(g => new Instanced<TGlobal>(g.index, getInstance(g)))
				.ToArray();

			midInstantiationAction();

			//Could potentially be sped up.
			var entityGlobalsCopy = entityGlobals;
			var lateInitGlobals = globals
				.Where(g => !entityGlobalsCopy.Any(i => i.index == g.index) && g.AppliesToEntity(entity, true))
				.Select(g => new Instanced<TGlobal>(g.index, getInstance(g)));

			entityGlobals = entityGlobals
				.Union(lateInitGlobals)
				.OrderBy(i => i.index)
				.ToArray();
		}
	}
}
