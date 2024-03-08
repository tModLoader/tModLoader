using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Terraria.ModLoader.Core;

public class GlobalHookList<TGlobal> where TGlobal : GlobalType<TGlobal>
{
	public readonly MethodInfo method;
	private TGlobal[] hookGlobals;
	private TGlobal[][] hookGlobalsByType;

	public GlobalHookList(MethodInfo method)
	{
		this.method = method;
		Update();
	}

	public ReadOnlySpan<TGlobal> Enumerate() => hookGlobals;
	public ReadOnlySpan<TGlobal> Enumerate(int type) => ForType(type);
	public EntityGlobalsEnumerator<TGlobal> Enumerate(IEntityWithGlobals<TGlobal> entity) => new(ForType(entity.Type), entity);

	private TGlobal[] ForType(int type)
	{
		return hookGlobals.Length == 0 ? hookGlobals : hookGlobalsByType[type];
	}

	public void Update()
	{
		hookGlobals = GlobalList<TGlobal>.Globals.WhereMethodIsOverridden(method).ToArray();
		hookGlobalsByType = GlobalTypeLookups<TGlobal>.Initialized ? GlobalTypeLookups<TGlobal>.BuildPerTypeGlobalLists(hookGlobals) : null;
	}

	public static GlobalHookList<TGlobal> Create(Expression<Func<TGlobal, Delegate>> expr) => Create<Delegate>(expr);
	public static GlobalHookList<TGlobal> Create<F>(Expression<Func<TGlobal, F>> expr) where F : Delegate
		=> new(expr.ToMethodInfo());
}