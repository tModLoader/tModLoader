using System;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.Core;

internal static class AssemblyResolving
{
	private static bool init;
	public static void Init()
	{
		if (init)
			return;
		init = true;

		// log all assembly resolutions
		AssemblyResolveEarly((_, args) => {
			Logging.tML.DebugFormat("Assembly Resolve: {0} -> {1}", args.RequestingAssembly, args.Name);
			return null;
		});
	}

	internal static void AssemblyResolveEarly(ResolveEventHandler handler)
	{
		// in newer .NET frameworks, the AssemblyResolve event is actually backed by an _AssemblyResolve field
		var backingField = typeof(AppDomain)
			.GetFields((BindingFlags)(-1))
			.First(f => f.Name.EndsWith("AssemblyResolve"));
		var a = (ResolveEventHandler)backingField.GetValue(AppDomain.CurrentDomain);
		backingField.SetValue(AppDomain.CurrentDomain, null);

		AppDomain.CurrentDomain.AssemblyResolve += handler;
		AppDomain.CurrentDomain.AssemblyResolve += a;
	}
}
