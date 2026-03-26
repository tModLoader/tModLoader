using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
	private static DetourList GetDetourList(Assembly asm)
	{
		if (asm == typeof(Action).Assembly)
			throw new ArgumentException("Cannot identify owning assembly of hook. Make sure there are no delegate type changing wrappers between the method/lambda and the Modify/Add/+= call. Eg `new ILContext.Manipulator(action)` is bad");

		return assemblyDetours.TryGetValue(asm, out var list) ? list : assemblyDetours[asm] = new();
	}

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

		TerrariaHooks.Logging.OnObsoleteHookSubscribed += (MethodBase method, Delegate modded) => {
			var owner = modded.Method.DeclaringType.Assembly;
			Logging.tML.Error($"The method {StringRep(method)} is Obsolete. It was hooked by the {StringRep(modded.Method)} method of {owner.GetName().Name}");
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
				Logging.tML.Debug($"Unloading {list.ilHooks.Count} IL hooks, {list.detours.Count} detours from {asm.GetName().Name} in {mod.Name}");

				foreach (var detour in list.detours)
					if (detour.IsApplied)
						detour.Undo();

				foreach (var ilHook in list.ilHooks)
					if (ilHook.IsApplied)
						ilHook.Undo();
			}
		}
	}

	internal static void Clear()
	{
		HookEndpointManager.Clear();
		assemblyDetours.Clear();
		_hookCache.Clear();

		// #4220 - Mitigation for bugs in reflection cache with mod reloads, and helps with assembly unloading
		var type = typeof(ReflectionHelper);
		FieldInfo[] caches = [
			type.GetField("AssemblyCache", BindingFlags.NonPublic | BindingFlags.Static),
			type.GetField("AssembliesCache", BindingFlags.NonPublic | BindingFlags.Static),
			type.GetField("ResolveReflectionCache", BindingFlags.NonPublic | BindingFlags.Static),
		];
		foreach (var cache in caches) {
			((IDictionary)cache.GetValue(null)).Clear();
		}
	}

	#region Obsolete HookEndpointManager method replacement
	// just exists to extend lifetime of Hook/ILHook so mods don't have to store the instances in static variables
	private static ConcurrentDictionary<(MethodBase, Delegate), IDisposable> _hookCache = new();
	private const string HookAlreadyAppliedMsg = "Delegate has already been applied to this method as a hook!";

	/// <summary>
	/// Adds a hook (implemented by <paramref name="hookDelegate"/>) to <paramref name="method"/>.
	/// </summary>
	/// <param name="method">The method to hook.</param>
	/// <param name="hookDelegate">The hook delegate to use.</param>
	public static void Add(MethodBase method, Delegate hookDelegate)
	{
		if (!_hookCache.TryAdd((method, hookDelegate), new Hook(method, hookDelegate)))
			throw new ArgumentException(HookAlreadyAppliedMsg);
	}

	/// <summary>
	/// Adds an IL hook (implemented by <paramref name="callback"/>) to <paramref name="method"/>.
	/// </summary>
	/// <param name="method">The method to hook.</param>
	/// <param name="callback">The hook delegate to use.</param>
	public static void Modify(MethodBase method, ILContext.Manipulator callback)
	{
		if (!_hookCache.TryAdd((method, callback), new ILHook(method, callback)))
			throw new ArgumentException(HookAlreadyAppliedMsg);
	}
	#endregion

	/// <summary>
	/// Dumps the list of currently registered IL hooks to the console. Useful for checking if a hook has been correctly added.
	/// </summary>
	/// <exception cref="Exception"></exception>
	public static void DumpILHooks()
	{
		var ilHooksField = typeof(HookEndpointManager).GetField("ILHooks", BindingFlags.NonPublic | BindingFlags.Static);
		object ilHooksFieldValue = ilHooksField.GetValue(null);
		if (ilHooksFieldValue is IReadOnlyDictionary<(MethodBase, Delegate), ILHook> ilHooks) {
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
		if (hooksFieldValue is IReadOnlyDictionary<(MethodBase, Delegate), Hook> detours) {
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
	/// Dumps the information about the given ILContext to a file in Logs/ILDumps/{Mod Name}/{Method Name}.txt<br/>
	/// It may be useful to use a tool such as <see href="https://www.diffchecker.com/"/> to compare the IL before and after edits
	/// </summary>
	/// <param name="mod"></param>
	/// <param name="il"></param>
	public static void DumpIL(Mod mod, ILContext il)
	{
		string methodName = il.Method.FullName.Replace(':', '_').Replace('<', '[').Replace('>', ']');
		if (methodName.Contains('?')) // MonoMod IL copies are created with mangled names like DMD<Terraria.Player::beeType>?38504011::Terraria.Player::beeType(Terraria.Player)
			methodName = methodName[(methodName.LastIndexOf('?') + 1)..];
		methodName = string.Join("_", methodName.Split(Path.GetInvalidFileNameChars())); // Catch any other illegal characters, just in case.
		int maxFileNameLength = 255 - 4; // Most OS, max filename length is 255, leave room for ".txt".
		if (methodName.Length > maxFileNameLength)
			methodName = methodName.Substring(0, maxFileNameLength);

		string filePath = Path.Combine(Logging.LogDir, "ILDumps", mod.Name, methodName + ".txt");
		string folderPath = Path.GetDirectoryName(filePath);

		if (!Directory.Exists(folderPath))
			Directory.CreateDirectory(folderPath);
		File.WriteAllText(filePath, il.ToString());

		Logging.tML.Debug($"Dumped ILContext \"{il.Method.FullName}\" to \"{filePath}\"");
	}
}

public class ILPatchFailureException : Exception
{
	public ILPatchFailureException(Mod mod, ILContext il, Exception innerException) : base($"Mod \"{mod.Name}\" failed to IL edit method \"{il.Method.FullName}\"", innerException)
	{
		MonoModHooks.DumpIL(mod, il);
	}
}
