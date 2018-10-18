using Mono.Cecil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader
{
	internal static class MonoModHooks
	{
		private static bool isInitialized;
		internal static void Initialize()
		{
			if (isInitialized)
				return;
			
			HookEndpointManager.OnGenerateCecilModule += GenerateCecilModule;
			HookEndpointManager.OnAdd += (m, d) => {
				Logging.tML.Debug($"Hook On.{StringRep(m)} added by {GetOwnerName(d)}");
				return true;
			};
			HookEndpointManager.OnRemove += (m, d) => {
				Logging.tML.Debug($"Hook On.{StringRep(m)} removed by {GetOwnerName(d)}");
				return true;
			};
			HookEndpointManager.OnModify += (m, d) => {
				Logging.tML.Debug($"Hook IL.{StringRep(m)} modified by {GetOwnerName(d)}");
				return true;
			};
			HookEndpointManager.OnUnmodify += (m, d) => {
				Logging.tML.Debug($"Hook IL.{StringRep(m)} unmodified by {GetOwnerName(d)}");
				return true;
			};

			isInitialized = true;
		}

		private static string GetOwnerName(Delegate d)
		{
			return d.Method.DeclaringType.Assembly.GetName().Name;
		}

		private static string StringRep(MethodBase m)
		{
			var paramString = string.Join(", ", m.GetParameters().Select(p => {
				var s = p.ParameterType.Name;
				if (p.ParameterType.IsByRef)
					s = p.IsOut ? "out " : "ref ";
				return s;
			}));
			return $"{m.DeclaringType.FullName}::{m.Name}({paramString})";
		}

		internal static void RemoveAll(Mod mod)
		{
			if (mod is ModLoaderMod)
				return;

			foreach (var asm in AssemblyManager.GetModAssemblies(mod.Name))
				HookEndpointManager.RemoveAllOwnedBy(asm);
		}

		private static ModuleDefinition GenerateCecilModule(AssemblyName name)
		{
			string resourceName = name.Name + ".dll";
			resourceName = Array.Find(typeof(Program).Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));
			if (resourceName != null) {
				Logging.tML.DebugFormat("Generating ModuleDefinition for {0}", name);
				using (Stream stream = typeof(Program).Assembly.GetManifestResourceStream(resourceName))
					return ModuleDefinition.ReadModule(stream, new ReaderParameters(ReadingMode.Immediate));
			}

			var modAssemblyBytes = AssemblyManager.GetAssemblyBytes(name.Name);
			if (modAssemblyBytes != null) {
				Logging.tML.DebugFormat("Generating ModuleDefinition for {0}", name);
				using (MemoryStream stream = new MemoryStream(modAssemblyBytes))
                    return ModuleDefinition.ReadModule(stream, new ReaderParameters(ReadingMode.Immediate));
			}

			return null;
		}
	}
}