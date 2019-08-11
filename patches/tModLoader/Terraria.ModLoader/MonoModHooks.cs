using Mono.Cecil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

		private static DetourModManager manager = new DetourModManager();
		private static HashSet<Assembly> NativeDetouringGranted = new HashSet<Assembly>();

		private static bool isInitialized;
		internal static void Initialize() {
			if (isInitialized)
				return;

			DynamicMethodDefinition.PreferRuntimeILCopy = true;
			HookEndpointManager.OnGenerateCecilModule += AssemblyManager.GetMainModule;
			HookEndpointManager.OnAdd += (m, d) => {
				Logging.tML.Debug($"Hook On.{StringRep(m)} added by {GetOwnerName(d)}");
				return true;
			};
			HookEndpointManager.OnRemove += (m, d) => {
				Logging.tML.Debug($"Hook On.{StringRep(m)} removed by {GetOwnerName(d)}");
				return true;
			};
			HookEndpointManager.OnPostModify += (m, d) => {
				Logging.tML.Debug($"Hook IL.{StringRep(m)} modified by {GetOwnerName(d)}");
			};
			HookEndpointManager.OnUnmodify += (m, d) => {
				Logging.tML.Debug($"Hook IL.{StringRep(m)} unmodified by {GetOwnerName(d)}");
				return true;
			};

			manager.OnHook += (asm, from, to, target) => {
				NativeAccessCheck(asm);
				Logging.tML.Debug($"Hook {StringRep(from)} -> {StringRep(to)} by {asm.GetName().Name}");
			};

			manager.OnDetour += (asm, from, to) => {
				NativeAccessCheck(asm);
				Logging.tML.Debug($"Detour {StringRep(from)} -> {StringRep(to)} by {asm.GetName().Name}");
			};

			manager.OnNativeDetour += (asm, method, from, to) => {
				NativeAccessCheck(asm);
				Logging.tML.Debug($"NativeDetour {StringRep(method)} [{from} -> {to}] by {asm.GetName().Name}");
			};

			isInitialized = true;
		}

		private static void NativeAccessCheck(Assembly asm) {
			if (NativeDetouringGranted.Contains(asm))
				return;

			throw new UnauthorizedAccessException(
				$"Native detouring permissions not granted to {asm.GetName().Name}. \n" +
				$"Mods should use HookEndpointManager for compatibility. \n" +
				$"If Detour or NativeDetour are required, call MonoModHooks.RequestNativeAccess()");
		}

		public static void RequestNativeAccess() {
			var stack = new StackTrace();
			var asm = stack.GetFrame(1).GetMethod().DeclaringType.Assembly;
			NativeDetouringGranted.Add(asm);
			Logging.tML.Warn($"Granted native detouring access to {asm.GetName().Name}");
		}

		private static string GetOwnerName(Delegate d) {
			return d.Method.DeclaringType.Assembly.GetName().Name;
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

			int hooks = 0, detours = 0, ndetours = 0;
			bool OnHookUndo(object obj) {
				hooks++;
				return true;
			}
			bool OnDetourUndo(object obj) {
				detours++;
				return true;
			}
			bool OnNativeDetourUndo(object obj) {
				ndetours++;
				return true;
			}

			Hook.OnUndo += OnHookUndo;
			Detour.OnUndo += OnDetourUndo;
			NativeDetour.OnUndo += OnNativeDetourUndo;

			foreach (var asm in AssemblyManager.GetModAssemblies(mod.Name))
				manager.Unload(asm);

			Hook.OnUndo -= OnHookUndo;
			Detour.OnUndo -= OnDetourUndo;
			NativeDetour.OnUndo -= OnNativeDetourUndo;

			if (hooks > 0 || detours > 0 || ndetours > 0)
				Logging.tML.Debug($"Unloaded {hooks} hooks, {detours} detours and {ndetours} native detours from {mod.Name}");
		}
	}
}