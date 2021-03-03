using System.Collections.Generic;

namespace Terraria.ModLoader.Core
{
	internal static class GlobalUtils
	{
		public static T Instance<T>(Instanced<T>[] globals, ushort index) where T : GlobalType {
			for (int i = 0; i < globals.Length; i++) {
				var g = globals[i];

				if (g.index == index) {
					return g.instance;
				}
			}

			return null;
		}

		public static TResult GetGlobal<TEntity, TGlobal, TResult>(Instanced<TGlobal>[] globals, TResult baseInstance) where TGlobal : GlobalType<TEntity> where TResult : TGlobal
			=> Instance(globals, baseInstance.index) as TResult ?? throw new KeyNotFoundException($"Instance of '{typeof(TResult).Name}' does not exist on the current {typeof(TEntity).Name}.");

		public static TResult GetGlobal<TEntity, TGlobal, TResult>(Instanced<TGlobal>[] globals, bool exactType) where TGlobal : GlobalType<TEntity> where TResult : TGlobal {
			if (exactType) {
				return GetGlobal<TEntity, TGlobal, TResult>(globals, ModContent.GetInstance<TResult>());
			}

			for (int i = 0; i < globals.Length; i++) {
				if (globals[i].instance is TResult result) {
					return result;
				}
			}

			throw new KeyNotFoundException($"Instance of '{typeof(TResult).Name}' does not exist on the current {typeof(TEntity).Name}.");
		}

		public static bool TryGetGlobal<TEntity, TGlobal, TResult>(Instanced<TGlobal>[] globals, TResult baseInstance, out TResult result) where TGlobal : GlobalType<TEntity> where TResult : TGlobal {
			if (baseInstance == null) {
				result = default;

				return false;
			}

			result = Instance(globals, baseInstance.index) as TResult;

			return result != null;
		}

		public static bool TryGetGlobal<TEntity, TGlobal, TResult>(Instanced<TGlobal>[] globals, bool exactType, out TResult result) where TGlobal : GlobalType<TEntity> where TResult : TGlobal {
			if (exactType) {
				return TryGetGlobal<TEntity, TGlobal, TResult>(globals, ModContent.GetInstance<TResult>(), out result);
			}

			for (int i = 0; i < globals.Length; i++) {
				if (globals[i].instance is TResult t) {
					result = t;

					return true;
				}
			}

			result = default;

			return false;
		}
	}
}
