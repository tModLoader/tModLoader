using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

public abstract class GlobalType<TGlobal> : ModType where TGlobal : GlobalType<TGlobal>
{
	public short StaticIndex { get; internal set; }
	public short PerEntityIndex { get; internal set; }

	/// <summary>
	/// Whether this global has <see cref="InstancePerEntity"/> or can be conditionally applied (overrides <see cref="GlobalType{TEntity, TGlobal}.AppliesToEntity(TEntity, bool)"/>) <br/>
	/// If true, the global will be assigned a <see cref="PerEntityIndex"/> at load time, which can be used to access the instance in the <see cref="IEntityWithGlobals{TGlobal}.EntityGlobals"/> array. <br/>
	/// If false, the global will be a singleton applying to all entities
	/// </summary>
	public virtual bool SlotPerEntity => InstancePerEntity;

	/// <summary>
	/// Whether to create a new instance of this Global for every entity that exists.
	/// Useful for storing information on an entity. Defaults to false.
	/// Return true if you need to store information (have non-static fields).
	/// </summary>
	public virtual bool InstancePerEntity => false;

	protected override void ValidateType()
	{
		base.ValidateType();

		bool hasInstanceFields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.Any(f => f.DeclaringType.IsSubclassOf(typeof(GlobalType<TGlobal>)));

		if (hasInstanceFields && !InstancePerEntity)
			throw new Exception($" {GetType().FullName} instance fields but {nameof(InstancePerEntity)} returns false. Either use static fields, or override {nameof(InstancePerEntity)} to return true");
	}

	protected override void Register()
	{
		ModTypeLookup<TGlobal>.Register((TGlobal)this);
		(StaticIndex, PerEntityIndex) = GlobalList<TGlobal>.Register((TGlobal)this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TResult GetGlobal<TResult>(ReadOnlySpan<TGlobal> entityGlobals, TResult baseInstance) where TResult : TGlobal
		=> TryGetGlobal(entityGlobals, baseInstance, out TResult result) ? result : throw new KeyNotFoundException(baseInstance.FullName);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TResult GetGlobal<TResult>(ReadOnlySpan<TGlobal> entityGlobals) where TResult : TGlobal
		=> TryGetGlobal(entityGlobals, out TResult result) ? result : throw new KeyNotFoundException(typeof(TResult).FullName);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetGlobal<TResult>(ReadOnlySpan<TGlobal> entityGlobals, TResult baseInstance, out TResult result) where TResult : TGlobal
	{
		var slot = baseInstance.PerEntityIndex;
		if (slot >= 0) {
			result = (TResult)entityGlobals[slot];
			return result != null;
		}
		result = baseInstance;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetGlobal<TResult>(ReadOnlySpan<TGlobal> entityGlobals, out TResult result) where TResult : TGlobal
		=> TryGetGlobal(entityGlobals, ModContent.GetInstance<TResult>(), out result);
}

public abstract class GlobalType<TEntity, TGlobal> : GlobalType<TGlobal> where TGlobal : GlobalType<TEntity, TGlobal> where TEntity : IEntityWithGlobals<TGlobal>
{
	public override bool SlotPerEntity => base.SlotPerEntity || LoaderUtils.HasOverride(this, m => (Func<TEntity, bool, bool>)m.AppliesToEntity);

	private bool? _isCloneable;
	/// <summary>
	/// Whether or not this type is cloneable. Cloning is supported if<br/>
	/// all reference typed fields in each sub-class which doesn't override Clone are marked with [CloneByReference]
	/// </summary>
	public virtual bool IsCloneable => _isCloneable ??= Cloning.IsCloneable(this, m => (Func<TEntity, TEntity, TGlobal>)m.Clone);

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
	public virtual TGlobal Clone(TEntity from, TEntity to)
	{
		if (!IsCloneable)
			Cloning.WarnNotCloneable(GetType());

		return (TGlobal)MemberwiseClone();
	}

	/// <summary>
	/// Only called if <see cref="GlobalType{TGlobal}.InstancePerEntity"/> and <see cref="AppliesToEntity"/>(<paramref name="target"/>, ...) are both true
	/// </summary>
	/// <param name="target">The entity instance the global is being instantiated for</param>
	/// <returns></returns>
	public virtual TGlobal NewInstance(TEntity target)
	{
		if (CloneNewInstances)
			return Clone(default, target);

		var inst = (TGlobal)Activator.CreateInstance(GetType(), true)!;
		inst.Mod = Mod;
		inst.StaticIndex = StaticIndex;
		inst.PerEntityIndex = PerEntityIndex;
		inst._isCloneable = _isCloneable;
		return inst;
	}

	public TGlobal Instance(TEntity entity)
	{
		TryGetGlobal(entity.EntityGlobals, (TGlobal)this, out TGlobal result);
		return result;
	}
}
