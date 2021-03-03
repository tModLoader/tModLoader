using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader.Core
{
	internal static class LoaderUtils
	{
		/// <summary> Calls static constructors on the provided type and, optionally, its nested types. </summary>
		public static void ResetStaticMembers(Type type, bool recursive) {
			type.TypeInitializer?.Invoke(null, null);

			if (recursive) {
				foreach (var nestedType in type.GetNestedTypes()) {
					ResetStaticMembers(nestedType, recursive);
				}
			}
		}

		public static void InstantiateGlobals<TGlobal, TEntity>(TEntity entity, IEnumerable<TGlobal> globals, ref Instanced<TGlobal>[] entityGlobals, Func<TGlobal, TGlobal> getInstance, Action midInstantiationAction) where TGlobal : GlobalType<TEntity> {
			entityGlobals = globals
				.Where(g => g.InstanceForEntity(entity, false))
				.Select(g => new Instanced<TGlobal>(g.index, getInstance(g)))
				.ToArray();

			midInstantiationAction();

			//Could potentially be sped up.
			var entityGlobalsCopy = entityGlobals;
			var lateInitGlobals = globals
				.Where(g => !entityGlobalsCopy.Any(i => i.index == g.index) && g.InstanceForEntity(entity, true))
				.Select(g => new Instanced<TGlobal>(g.index, getInstance(g)));

			entityGlobals = entityGlobals
				.Union(lateInitGlobals)
				.OrderBy(i => i.index)
				.ToArray();
		}
	}
}
