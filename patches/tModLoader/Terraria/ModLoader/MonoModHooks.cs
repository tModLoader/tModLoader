using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader
{
	public static class MonoModHooks
	{
		private static Dictionary<Type, string> defaultAliases = new Dictionary<Type, string> {
			{ typeof(object), "object" },
			{ typeof(bool), "bool" },
			{ typeof(float), "float" },
			{ typeof(double), "double" },
			{ typeof(decimal), "decimal" },
			{ typeof(byte), "byte" },
			{ typeof(sbyte), "sbyte" },
			{ typeof(short), "short" },
			{ typeof(ushort), "ushort" },
			{ typeof(int), "int" },
			{ typeof(uint), "uint" },
			{ typeof(long), "long" },
			{ typeof(ulong), "ulong" },
			{ typeof(char), "char" },
			{ typeof(string), "string" }
		};

		private class DetourList
		{
			public readonly List<DetourInfo> detours = new();
			public readonly List<ILHookInfo> ilHooks = new();
		}

		private static Dictionary<Assembly, DetourList> assemblyDetours = new();
		private static DetourList GetDetourList(Assembly asm) => assemblyDetours.TryGetValue(asm, out var list) ? list : assemblyDetours[asm] = new();

		[Obsolete("No longer required. NativeDetour is gone. Detour should not be used. Hook is safe to use", true)]
		public static void RequestNativeAccess() { }

		private static bool isInitialized;
		internal static void Initialize() {
			if (isInitialized)
				return;

			DetourManager.DetourApplied += (info) => {
				var owner = info.Entry.DeclaringType.Assembly;
				GetDetourList(owner).detours.Add(info);
				var msg = $"Hook {StringRep(info.Method.Method)} added by {owner.GetName().Name}";

				var targetSig = MethodSignature.ForMethod(info.Method.Method);
				var detourSig = MethodSignature.ForMethod(info.Entry, ignoreThis: true);
				if (detourSig.ParameterCount != targetSig.ParameterCount + 1 || detourSig.FirstParameter.GetMethod("Invoke") is null) {
					msg += " WARNING! No orig delegate, incompatible with other hooks to this method";
				}

				Logging.tML.Debug(msg);
			};

			DetourManager.ILHookApplied += (info) => {
				var owner = info.ManipulatorMethod.DeclaringType.Assembly;
				GetDetourList(owner).ilHooks.Add(info);
				Logging.tML.Debug($"ILHook {StringRep(info.Method.Method)} added by {owner.GetName().Name}");
			};

			isInitialized = true;
		}

		private static string StringRep(MethodBase m) {
			var paramString = string.Join(", ", m.GetParameters().Select(p => {
				var t = p.ParameterType;
				var s = "";

				if (t.IsByRef) {
					s = p.IsOut ? "out " : "ref ";
					t = t.GetElementType();
				}

				return s + (defaultAliases.TryGetValue(t, out string n) ? n : t.Name);
			}));

			var owner = m.DeclaringType?.FullName ??
				(m is DynamicMethod ? "dynamic" : "unknown");
			return $"{owner}::{m.Name}({paramString})";
		}

		internal static void RemoveAll(Mod mod) {
			if (mod is ModLoaderMod)
				return;

			foreach (var asm in AssemblyManager.GetModAssemblies(mod.Name)) {
				if (assemblyDetours.TryGetValue(asm, out var list)) {
					Logging.tML.Debug($"Unloading {list.ilHooks.Count} IL hooks, {list.detours.Count} detours from {asm.GetName().Name} in {mod.DisplayName}");

					foreach (var detour in list.detours)
						if (detour.IsApplied)
							detour.Undo();

					foreach (var ilHook in list.ilHooks)
						if (ilHook.IsApplied)
							ilHook.Undo();
				}
			}

			HookEndpointManager.Clear();
			assemblyDetours.Clear();
		}
	}
}