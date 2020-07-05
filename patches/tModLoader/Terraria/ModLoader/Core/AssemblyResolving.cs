using System;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.Core
{
	internal static class AssemblyResolving
	{
		private static bool init;
		public static void Init() {
			if (init)
				return;
			init = true;

			// allow mods which reference embedded assemblies to reference a different version and be safely upgraded
			AssemblyResolveEarly((_, args) => {
				var name = new AssemblyName(args.Name);
				if (Array.Find(typeof(Program).Assembly.GetManifestResourceNames(), s => s.EndsWith(name.Name + ".dll")) == null)
					return null;

				var existing = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == name.Name);
				if (existing != null) {
					Logging.tML.Warn($"Upgraded Reference {name.Name} -> Version={name.Version} -> {existing.GetName().Version}");
					return existing;
				}

				return null;
			});

			// log all assembly resolutions
			AssemblyResolveEarly((_, args) => {
				Logging.tML.DebugFormat("Assembly Resolve: {0} -> {1}", args.RequestingAssembly, args.Name);
				return null;
			});
		}

		internal static void AssemblyResolveEarly(ResolveEventHandler handler) {
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
}
