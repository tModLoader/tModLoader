using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public abstract class GlobalType : ModType
	{
		internal ushort index;

		/// <summary>
		/// Whether to create a new instance of this Global for every entity that exists. 
		/// Useful for storing information on an entity. Defaults to false. 
		/// Return true if you need to store information (have non-static fields).
		/// </summary>
		public virtual bool InstancePerEntity => false;

		internal GlobalType() { }

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

		public static bool TryGetGlobal<TGlobal, TResult>(Instanced<TGlobal>[] globals, TResult baseInstance, out TResult result) where TGlobal : GlobalType where TResult : TGlobal {
			if (baseInstance == null) {
				result = default;

				return false;
			}

			result = Instance(globals, baseInstance.index) as TResult;

			return result != null;
		}

		public static bool TryGetGlobal<TGlobal, TResult>(Instanced<TGlobal>[] globals, bool exactType, out TResult result) where TGlobal : GlobalType where TResult : TGlobal {
			if (exactType) {
				return TryGetGlobal(globals, ModContent.GetInstance<TResult>(), out result);
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

	public abstract class GlobalType<TEntity> : GlobalType
	{
		/// <summary>
		/// Use this to control whether or not this global should be associated with the provided entity instance.
		/// </summary>
		/// <param name="entity"> The entity for which the global instantion is being checked. </param>
		/// <param name="lateInstantiation">
		/// Whether this check occurs before or after the ModX.SetDefaults call.
		/// <br/> If you're relying on entity values that can be changed by that call, you should likely prefix your return value with the following:
		/// <code> lateInstantiation &amp;&amp; ... </code>
		/// </param>
		public virtual bool AppliesToEntity(TEntity entity, bool lateInstantiation) => true;
	}
}
