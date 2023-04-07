using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Terraria.ModLoader.Core;

public class GlobalHookList<TGlobal> where TGlobal : GlobalType<TGlobal>
{
	public readonly MethodInfo method;
	private TGlobal[] hookGlobals;

	public GlobalHookList(MethodInfo method)
	{
		this.method = method;
		Update();
	}

	public ReadOnlySpan<TGlobal> Enumerate() => hookGlobals;
	public EntityGlobalsEnumerator<TGlobal> Enumerate(IEntityWithGlobals<TGlobal> entity) => new(hookGlobals, entity.EntityGlobals.array);

	public void Update()
		=> hookGlobals = GlobalList<TGlobal>.Globals.WhereMethodIsOverridden(method).ToArray();

	public static GlobalHookList<TGlobal> Create<F>(Expression<Func<TGlobal, F>> expr) where F : Delegate
		=> new(expr.ToMethodInfo());
}