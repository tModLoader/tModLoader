using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Terraria.ModLoader.Core;

public class GlobalHookList<TGlobal> where TGlobal : GlobalType<TGlobal>
{
	public LoaderUtils.MethodOverrideQuery<TGlobal> HookOverrideQuery { get; }
	public MethodInfo Method => HookOverrideQuery.Method;

	private TGlobal[] hookGlobals;
	private TGlobal[][] hookGlobalsByType;

	public GlobalHookList(LoaderUtils.MethodOverrideQuery<TGlobal> hook)
	{
		HookOverrideQuery = hook;
		Update();
	}

	[Obsolete("Use HookList.Create instead", error: true)]
	public GlobalHookList(MethodInfo method) : this(method.ToBindingExpression<TGlobal>().ToOverrideQuery()) { }

	public ReadOnlySpan<TGlobal> Enumerate() => hookGlobals;
	public ReadOnlySpan<TGlobal> Enumerate(int type) => ForType(type);
	public EntityGlobalsEnumerator<TGlobal> Enumerate(IEntityWithGlobals<TGlobal> entity) => new(ForType(entity.Type), entity);

	private TGlobal[] ForType(int type)
	{
		return hookGlobals.Length == 0 ? hookGlobals : hookGlobalsByType[type];
	}

	public void Update()
	{
		hookGlobals = GlobalList<TGlobal>.Globals.Where(HookOverrideQuery.HasOverride).ToArray();
		hookGlobalsByType = GlobalTypeLookups<TGlobal>.Initialized ? GlobalTypeLookups<TGlobal>.BuildPerTypeGlobalLists(hookGlobals) : null;
	}

	/// <summary>
	/// <inheritdoc cref="LoaderUtils.ToOverrideQuery"/>
	/// </summary>
	public static GlobalHookList<TGlobal> Create(Expression<Func<TGlobal, Delegate>> expr) => Create<Delegate>(expr);

	/// <summary>
	/// <inheritdoc cref="LoaderUtils.ToOverrideQuery"/>
	/// </summary>
	public static GlobalHookList<TGlobal> Create<F>(Expression<Func<TGlobal, F>> expr) where F : Delegate
		=> new(expr.ToOverrideQuery());
}