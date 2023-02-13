using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader;

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
	internal static void Initialize()
	{
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

	private static string StringRep(MethodBase m)
	{
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

	internal static void RemoveAll(Mod mod)
	{
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

	// TODO: 'Detour' or 'On Hook'?
	// TODO: should it require the mod as a parameter instead of logging it with tML?
	/// <summary>
	/// Dumps the list of currently registered IL hooks to the console. Useful for checking if a hook has been correctly added.
	/// </summary>
	/// <exception cref="Exception"></exception>
	public static void DumpILHooks()
	{
		var ilHooksField = typeof(HookEndpointManager).GetField("ILHooks", BindingFlags.NonPublic | BindingFlags.Static);
		object ilHooksFieldValue = ilHooksField.GetValue(null);
		if (ilHooksFieldValue is Dictionary<(MethodBase, Delegate), ILHook> ilHooks) {
			Logging.tML.Debug("Dump of registered IL Hooks:");
			foreach (var item in ilHooks) {
				Logging.tML.Debug(item.Key + ": " + item.Value);
			}
		}
		else {
			throw new Exception($"Failed to get HookEndpointManager.ILHooks: Type is {ilHooksFieldValue.GetType()}");
		}
	}

	/// <summary>
	/// Dumps the list of currently registered On hooks to the console. Useful for checking if a hook has been correctly added.
	/// </summary>
	/// <exception cref="Exception"></exception>
	public static void DumpOnHooks()
	{
		var hooksField = typeof(HookEndpointManager).GetField("Hooks", BindingFlags.NonPublic | BindingFlags.Static);
		object hooksFieldValue = hooksField.GetValue(null);
		if (hooksFieldValue is Dictionary<(MethodBase, Delegate), Hook> detours) {
			Logging.tML.Debug("Dump of registered Detours:");
			foreach (var item in detours) {
				Logging.tML.Debug(item.Key + ": " + item.Value);
			}
		}
		else {
			throw new Exception($"Failed to get HookEndpointManager.Hooks: Type is {hooksFieldValue.GetType()}");
		}
	}

	/// <summary>
	/// A helper method that logs to the console that an IL patch failed.
	/// </summary>
	/// <param name="mod"></param>
	/// <param name="il"></param>
	/// <param name="reason"></param>
	public static void LogILPatchFailure(Mod mod, ILContext il, string reason)
	{
		// TODO: use StringRep function but for MethodDefinition instead of MethodBase
		// TODO: should this also include a patching class where the hooks are made and a feature name?
		mod.Logger.Warn($"Failed to IL edit method \"{il.Method.Name}\", because \"{reason}\"");
	}
}
