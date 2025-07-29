using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

#nullable enable

public abstract class GlobalType<TGlobal> : ModType where TGlobal : GlobalType<TGlobal>
{
	/// <summary>
	/// Index of this global in the list of all globals of the same type, in registration order
	/// </summary>
	public short StaticIndex { get; internal set; }

	/// <summary>
	/// Index of this global in a <see cref="IEntityWithGlobals{TGlobal}.EntityGlobals"/> array <br/>
	/// -1 if this global does not have a <see cref="SlotPerEntity"/>
	/// </summary>
	public short PerEntityIndex { get; internal set; }

	/// <summary>
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

	/// <summary>
	/// Whether this global applies to some entities but not others
	/// </summary>
	public abstract bool ConditionallyAppliesToEntities { get; }

	protected override void ValidateType()
	{
		base.ValidateType();

		bool hasInstanceFields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.Any(f => f.DeclaringType!.IsSubclassOf(typeof(GlobalType<TGlobal>)));

		if (hasInstanceFields && !InstancePerEntity)
			throw new Exception($" {GetType().FullName} instance fields but {nameof(InstancePerEntity)} returns false. Either use static fields, or override {nameof(InstancePerEntity)} to return true");
	}

	protected override void Register()
	{
		ModTypeLookup<TGlobal>.Register((TGlobal)this);
		(StaticIndex, PerEntityIndex) = GlobalList<TGlobal>.Register((TGlobal)this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TResult GetGlobal<TResult>(int entityType, ReadOnlySpan<TGlobal> entityGlobals, TResult baseInstance) where TResult : TGlobal
		=> TryGetGlobal(entityType, entityGlobals, baseInstance, out TResult result) ? result : throw new KeyNotFoundException(baseInstance.FullName);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TResult GetGlobal<TResult>(int entityType, ReadOnlySpan<TGlobal> entityGlobals) where TResult : TGlobal
		=> TryGetGlobal(entityType, entityGlobals, out TResult result) ? result : throw new KeyNotFoundException(typeof(TResult).FullName);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetGlobal<TResult>(int entityType, ReadOnlySpan<TGlobal> entityGlobals, TResult baseInstance, out TResult result) where TResult : TGlobal
	{
		var slot = baseInstance.PerEntityIndex;
		if (entityType > 0 && slot >= 0) {
			result = (TResult)entityGlobals[slot];
			return result != null;
		}
		else if (GlobalTypeLookups<TGlobal>.AppliesToType(baseInstance, entityType)) {
			result = baseInstance;
			return true;
		}
		result = null;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetGlobal<TResult>(int entityType, ReadOnlySpan<TGlobal> entityGlobals, out TResult result) where TResult : TGlobal
		=> TryGetGlobal(entityType, entityGlobals, ModContent.GetInstance<TResult>(), out result);
}

public abstract class GlobalType<TEntity, TGlobal> : GlobalType<TGlobal> where TGlobal : GlobalType<TEntity, TGlobal> where TEntity : IEntityWithGlobals<TGlobal>
{
	private bool? _isCloneable;
	/// <summary>
	/// Whether or not this type is cloneable. Cloning is supported if<br/>
	/// all reference typed fields in each sub-class which doesn't override Clone are marked with [CloneByReference]
	/// </summary>
	public virtual bool IsCloneable => _isCloneable ??= Cloning.IsCloneable(this, m => m.Clone);

	/// <summary>
	/// Whether to create new instances of this mod type via <see cref="Clone"/> or via the default constructor
	/// Defaults to false (default constructor).
	/// </summary>
	protected virtual bool CloneNewInstances => false;

	private bool? _conditionallyAppliesToEntities;
	/// <summary>
	/// Whether this global applies to some entities but not others. <br/>
	/// True if the type overrides <see cref="AppliesToEntity(TEntity, bool)"/>
	/// </summary>
	public sealed override bool ConditionallyAppliesToEntities {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _conditionallyAppliesToEntities ??= LoaderUtils.HasOverride(this, m => m.AppliesToEntity);
	}

	/// <summary>
	/// Use this to control whether or not this global should be run on the provided entity instance. <br/>
	/// </summary>
	/// <param name="entity"> The entity for which the global instantiation is being checked. </param>
	/// <param name="lateInstantiation">
	/// Whether this check occurs before or after the ModX.SetDefaults call.
	/// <br/> If you're relying on entity values that can be changed by that call, you should likely prefix your return value with the following:
	/// <code> lateInstantiation &amp;&amp; ... </code>
	/// </param>
	public virtual bool AppliesToEntity(TEntity entity, bool lateInstantiation) => true;

	/// <summary>
	/// Allows you to set the properties of any and every instance that gets created.
	/// </summary>
	public virtual void SetDefaults(TEntity entity)
	{
	}

	/// <summary>
	/// Create a copy of this instanced global. Called when an entity is cloned.
	/// </summary>
	/// <param name="from">The entity being cloned. May be null if <see cref="CloneNewInstances"/> is true (via call from <see cref="NewInstance(TEntity)"/>)</param>
	/// <param name="to">The new clone of the entity</param>
	/// <returns>A clone of this global</returns>
	public virtual TGlobal Clone(TEntity? from, TEntity to)
	{
		if (!IsCloneable)
			Cloning.WarnNotCloneable(GetType());

		return (TGlobal)MemberwiseClone();
	}

	/// <summary>
	/// Only called if <see cref="GlobalType{TGlobal}.InstancePerEntity"/> and <see cref="AppliesToEntity"/>(<paramref name="target"/>, ...) are both true. <br/>
	/// <br/>
	/// Returning null is permitted but <b>not recommended</b> over <c>AppliesToEntity</c> for performance reasons. <br/>
	/// Only return null when the global is disabled based on some runtime property (eg world seed).
	/// </summary>
	/// <param name="target">The entity instance the global is being instantiated for</param>
	/// <returns></returns>
	public virtual TGlobal? NewInstance(TEntity target)
	{
		if (CloneNewInstances)
			return Clone(default, target);

		var inst = (TGlobal)Activator.CreateInstance(GetType(), true)!;
		inst.Mod = Mod;
		inst.StaticIndex = StaticIndex;
		inst.PerEntityIndex = PerEntityIndex;
		inst._isCloneable = _isCloneable;
		inst._conditionallyAppliesToEntities = _conditionallyAppliesToEntities;
		return inst;
	}

	public TGlobal Instance(TEntity entity)
	{
		TryGetGlobal(entity.Type, entity.EntityGlobals, (TGlobal)this, out TGlobal result);
		return result;
	}
}
