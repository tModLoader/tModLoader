using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader
{
	public abstract class GlobalType : ModType, IIndexed
	{
		private static readonly MethodInfo instancePerEntityGetterMethod = typeof(GlobalType).GetProperty(nameof(InstancePerEntity)).GetGetMethod();

		public ushort Index { get; internal set; }

		/// <summary>
		/// Whether to create a new instance of this Global for every entity that exists.<br/>
		/// Useful for storing information on an entity. Defaults to false. <para/> 
		/// Return <b>true</b> if you need to store information per entity.<br/>
		/// Return <b>false</b> if you don't, or want to store instance data but keep the type singleton-like.<br/>
		/// Defaults to <b>false</b>, but this property must be overridden if your type contains instance fields.
		/// </summary>
		public virtual bool InstancePerEntity => false;

		internal GlobalType() { }

		protected override void ValidateType() {
			base.ValidateType();

			const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			var type = GetType();
			bool hasInstanceFields = type.GetFields(FieldFlags).Any(f => f.DeclaringType.IsSubclassOf(typeof(GlobalType)));
			bool overridesInstancePerEntity = LoaderUtils.HasOverride(type, instancePerEntityGetterMethod);

			if (hasInstanceFields && !overridesInstancePerEntity)
				throw new Exception($"{type.FullName} contains instance fields but does not override {nameof(InstancePerEntity)}. Either use static fields, or override {nameof(InstancePerEntity)} to return 'true' if the data is meant to be stored per-instance, or 'false' if it's meant to be singleton-like.");
		}

		public static T Instance<T>(Instanced<T>[] globals, ushort index) where T : class {
			for (int i = 0; i < globals.Length; i++) {
				var g = globals[i];

				if (g.Index == index) {
					return g.Instance;
				}
			}

			return default;
		}

		public static TResult GetGlobal<TEntity, TGlobal, TResult>(Instanced<TGlobal>[] globals, TResult baseInstance) where TGlobal : GlobalType where TResult : TGlobal
			=> TryGetGlobal(globals, baseInstance, out TResult result) ? result : throw new KeyNotFoundException($"Instance of '{typeof(TResult).Name}' does not exist on the current {typeof(TEntity).Name}.");

		public static TResult GetGlobal<TEntity, TGlobal, TResult>(Instanced<TGlobal>[] globals, bool exactType) where TGlobal : GlobalType where TResult : TGlobal
			=> TryGetGlobal(globals, exactType, out TResult result) ? result : throw new KeyNotFoundException($"Instance of '{typeof(TResult).Name}' does not exist on the current {typeof(TEntity).Name}.");

		public static bool TryGetGlobal<TGlobal, TResult>(Instanced<TGlobal>[] globals, TResult baseInstance, out TResult result) where TGlobal : GlobalType where TResult : TGlobal {
			if (baseInstance == null) {
				result = default;
				return false;
			}

			result = Instance(globals, baseInstance.Index) as TResult;
			return result != null;
		}

		public static bool TryGetGlobal<TGlobal, TResult>(Instanced<TGlobal>[] globals, bool exactType, out TResult result) where TGlobal : GlobalType where TResult : TGlobal {
			if (exactType) {
				return TryGetGlobal(globals, ModContent.GetInstance<TResult>(), out result);
			}

			for (int i = 0; i < globals.Length; i++) {
				if (globals[i].Instance is TResult t) {
					result = t;
					return true;
				}
			}

			result = default;
			return false;
		}
	}

	public abstract class GlobalType<TEntity, TGlobal> : GlobalType where TGlobal : GlobalType<TEntity, TGlobal>
	{
		private bool? _isCloneable;
		/// <summary>
		/// Whether or not this type is cloneable. Cloning is supported if<br/>
		/// all reference typed fields in each sub-class which doesn't override Clone are marked with [CloneByReference]
		/// </summary>
		public virtual bool IsCloneable => _isCloneable ??= Cloning.IsCloneable<GlobalType<TEntity, TGlobal>, Func<TEntity, TEntity, TGlobal>>(this, m => m.Clone);

		/// <summary>
		/// Whether to create new instances of this mod type via <see cref="Clone"/> or via the default constructor
		/// Defaults to false (default constructor).
		/// </summary>
		protected virtual bool CloneNewInstances => false;

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

		/// <summary>
		/// Create a copy of this instanced global. Called when an entity is cloned.
		/// </summary>
		/// <param name="from">The entity being cloned</param>
		/// <param name="to">The new clone of the entity</param>
		/// <returns>A clone of this global</returns>
		public virtual TGlobal Clone(TEntity from, TEntity to) {
			if (!IsCloneable)
				Cloning.WarnNotCloneable(GetType());

			return (TGlobal)MemberwiseClone();
		}

		/// <summary>
		/// Only called if <see cref="GlobalType.InstancePerEntity"/> and <see cref="AppliesToEntity"/>(<paramref name="target"/>, ...) are both true
		/// </summary>
		/// <param name="target">The entity instance the global is being instantiated for</param>
		/// <returns></returns>
		public virtual TGlobal NewInstance(TEntity target) {
			if (CloneNewInstances)
				return Clone(default, target);

			var inst = (TGlobal)Activator.CreateInstance(GetType(), true)!;
			inst.Mod = Mod;
			inst.Index = Index;
			return inst;
		}
	}
}
