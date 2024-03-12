#if NETCORE
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Terraria.ModLoader.UI;
using System.Runtime.Loader;
using System.Runtime.CompilerServices;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace Terraria.ModLoader.Core;

//todo: further documentation
public static class AssemblyManager
{
	private class ModLoadContext : AssemblyLoadContext
	{
		public readonly TmodFile modFile;
		public readonly BuildProperties properties;

		public List<ModLoadContext> dependencies = new List<ModLoadContext>();

		public Assembly assembly;
		public IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
		public IDictionary<string, byte[]> assemblyBytes = new Dictionary<string, byte[]>();
		public IDictionary<Assembly, Type[]> loadableTypes = new Dictionary<Assembly, Type[]>();
		public long bytesLoaded = 0;

		public ModLoadContext(LocalMod mod) : base(mod.Name, true)
		{
			modFile = mod.modFile;
			properties = mod.properties;

			Unloading += ModLoadContext_Unloading;
		}

		private void ModLoadContext_Unloading(AssemblyLoadContext obj)
		{
			// required for this to actually unload
			dependencies = null;
			assembly = null;
			assemblies = null;
			loadableTypes = null;
		}

		public void AddDependency(ModLoadContext dep)
		{
			dependencies.Add(dep);
		}

		public void LoadAssemblies()
		{
			try {
				using (modFile.Open()) {
					foreach (var dll in properties.dllReferences) {
						LoadAssembly(modFile.GetBytes("lib/" + dll + ".dll"));
					}

					assembly = Debugger.IsAttached && File.Exists(properties.eacPath) ?
						LoadAssembly(modFile.GetModAssembly(), File.ReadAllBytes(properties.eacPath)): //load the unmodified dll and EaC pdb
						LoadAssembly(modFile.GetModAssembly(), modFile.GetModPdb());
				}

				var mlc = new MetadataLoadContext(new MetadataResolver(this));
				loadableTypes = GetLoadableTypes(this, mlc);
			}
			catch (Exception e) {
				e.Data["mod"] = Name;
				throw;
			}
		}

		private Assembly LoadAssembly(byte[] code, byte[] pdb = null)
		{
			using var codeStrm = new MemoryStream(code, false);
			using var pdbStrm = pdb == null ? null : new MemoryStream(pdb, false);
			var asm = LoadFromStream(codeStrm, pdbStrm);

			var name = asm.GetName().Name;
			assemblyBytes[name] = code;
			assemblies[name] = asm;

			bytesLoaded += code.LongLength + (pdb?.LongLength ?? 0);

			if (Program.LaunchParameters.ContainsKey("-dumpasm")) {
				var dumpdir = Path.Combine(Main.SavePath, "asmdump");
				Directory.CreateDirectory(dumpdir);
				File.WriteAllBytes(Path.Combine(dumpdir, asm.FullName+".dll"), code);
				if (pdb != null)
					File.WriteAllBytes(Path.Combine(dumpdir, asm.FullName+".pdb"), code);
			}

			return asm;
		}

		protected override Assembly Load(AssemblyName assemblyName)
		{
			if (assemblies.TryGetValue(assemblyName.Name, out var asm))
				return asm;

			return dependencies.Select(dep => dep.Load(assemblyName)).FirstOrDefault(a => a != null);
		}

		internal bool IsModDependencyPresent(string name) => name == Name || dependencies.Any(d => d.IsModDependencyPresent(name));


		private class MetadataResolver : MetadataAssemblyResolver
		{
			private readonly ModLoadContext mod;

			public MetadataResolver(ModLoadContext mod)
			{
				this.mod = mod;
			}

			public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
			{
				var existing = context.GetAssemblies().SingleOrDefault(a => a.GetName().FullName == assemblyName.FullName);
				if (existing != null)
					return existing;

				var runtime = mod.LoadFromAssemblyName(assemblyName);
				if (string.IsNullOrEmpty(runtime.Location))
					return context.LoadFromByteArray(((ModLoadContext)GetLoadContext(runtime)).assemblyBytes[assemblyName.Name]);


				return context.LoadFromAssemblyPath(runtime.Location);
			}
		}

		internal void ClearAssemblyBytes()
		{
			assemblyBytes.Clear();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void Unload() {
		foreach (var alc in loadedModContexts.Values) {
			oldLoadContexts.Add(new WeakReference<AssemblyLoadContext>(alc));
			alc.Unload();
		}

		loadedModContexts.Clear();

		for (int i = 0; i < 10; i++) {
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}

	internal static IEnumerable<string> OldLoadContexts()
	{
		foreach (var alcRef in oldLoadContexts)
			if (alcRef.TryGetTarget(out var alc))
				yield return alc.Name;
	}

	private static readonly List<WeakReference<AssemblyLoadContext>> oldLoadContexts = new();

	private static readonly Dictionary<string, ModLoadContext> loadedModContexts = new();

	//private static CecilAssemblyResolver cecilAssemblyResolver = new CecilAssemblyResolver();

	private static bool assemblyResolverAdded;
	private static void AddAssemblyResolver()
	{
		if (assemblyResolverAdded)
			return;
		assemblyResolverAdded = true;

		AppDomain.CurrentDomain.AssemblyResolve += TmlCustomResolver;
	}

	private static Assembly TmlCustomResolver(object sender, ResolveEventArgs args)
	{
		//Legacy: With FNA and .Net5 changes, had aimed to eliminate the variants of tmodloader (tmodloaderdebug, tmodloaderserver) and Terraria as assembly names.
		// However, due to uncertainty in that elimination, in particular for Terraria, have opted to retain the original check. - Solxan
		var name = new AssemblyName(args.Name).Name;
		if (name.Contains("tModLoader") || name == "Terraria")
			return Assembly.GetExecutingAssembly();

		if (name == "FNA")
			return typeof(Vector2).Assembly;

		if (name is "Ionic.Zip" or "Ionic.Zip.Reduced" or "Ionic.Zip.CF")
			return typeof(ZipFile).Assembly;

		return null;
	}

	private static Mod Instantiate(ModLoadContext mod)
	{
		try {
			VerifyMod(mod.Name, mod.assembly, out var modType);
			var m = (Mod)Activator.CreateInstance(modType, true)!;
			m.File = mod.modFile;
			m.Code = mod.assembly;
			m.Logger = LogManager.GetLogger(m.Name);
			m.Side = mod.properties.side;
			m.DisplayName = mod.properties.displayName;
			m.TModLoaderVersion = mod.properties.buildVersion;
			m.TranslationForMods = mod.properties.translationMod ? mod.properties.RefNames(true).ToList() : null; 
			return m;
		}
		catch (Exception e) {
			e.Data["mod"] = mod.Name;
			throw;
		}
		finally {
			MemoryTracking.Update(mod.Name).code += mod.bytesLoaded;
		}
	}

	private static void VerifyMod(string modName, Assembly assembly, out Type modType)
	{
		string asmName = new AssemblyName(assembly.FullName).Name;

		if (asmName != modName)
			throw new Exception(Language.GetTextValue("tModLoader.BuildErrorModNameDoesntMatchAssemblyName", modName, asmName));

		// at least one of the types must be in a namespace that starts with the mod name
		if (!GetLoadableTypes(assembly).Any(t => t.Namespace?.StartsWith(modName) == true))
			throw new Exception(Language.GetTextValue("tModLoader.BuildErrorNamespaceFolderDontMatch"));

		var modTypes = GetLoadableTypes(assembly).Where(t => t.IsSubclassOf(typeof(Mod))).ToArray();

		if (modTypes.Length > 1)
			throw new Exception($"{modName} has multiple classes extending Mod. Only one Mod per mod is supported at the moment");

		modType = modTypes.SingleOrDefault() ?? typeof(Mod); // Mods don't really need a class extending Mod, we can always just make one for them
	}

	internal static List<Mod> InstantiateMods(List<LocalMod> modsToLoad, CancellationToken token)
	{
		AddAssemblyResolver();

		var modList = modsToLoad.Select(m => new ModLoadContext(m)).ToList();
		foreach (var mod in modList)
			loadedModContexts.Add(mod.Name, mod);

		foreach (var mod in modList)
			foreach (var depName in mod.properties.RefNames(true))
				if (loadedModContexts.TryGetValue(depName, out var dep))
					mod.AddDependency(dep);

		// todo, assign an AssemblyLoadContext to each group of mods

		if (Debugger.IsAttached)
			ModCompile.activelyModding = true;

		try {
			// can no longer load assemblies in parallel due to cecil assembly resolver during ModuleDefinition.Write requiring dependencies
			// could use a topological parallel load but I doubt the performance is worth the development effort - Chicken Bones
			Interface.loadMods.SetLoadStage("tModLoader.MSSandboxing", modsToLoad.Count);
			int i = 0;
			foreach (var mod in modList) {
				token.ThrowIfCancellationRequested();
				Interface.loadMods.SetCurrentMod(i++, mod.Name, mod.properties?.displayName ?? "", mod.modFile.Version);
				mod.LoadAssemblies();
			}

			foreach (var mod in modList)
				mod.ClearAssemblyBytes();

			//Assemblies must be loaded before any instantiation occurs to satisfy dependencies
			Interface.loadMods.SetLoadStage("tModLoader.MSInstantiating");
			MemoryTracking.Checkpoint();
			return modList.Select(mod => {
				token.ThrowIfCancellationRequested();
				return Instantiate(mod);
			}).ToList();
		}
		catch (AggregateException ae) {
			ae.Data["mods"] = ae.InnerExceptions.Select(e => (string)e.Data["mod"]).ToArray();
			throw;
		}
	}

	private static string GetModAssemblyFileName(this TmodFile modFile) => $"{modFile.Name}.dll";

	public static byte[] GetModAssembly(this TmodFile modFile) => modFile.GetBytes(modFile.GetModAssemblyFileName());

	public static byte[] GetModPdb(this TmodFile modFile) => modFile.GetBytes(Path.ChangeExtension(modFile.GetModAssemblyFileName(), "pdb"));

	private static ModLoadContext GetLoadContext(string name) => loadedModContexts.TryGetValue(name, out var value) ? value : throw new KeyNotFoundException(name);

	public static IEnumerable<Assembly> GetModAssemblies(string name) => GetLoadContext(name).assemblies.Values;

	public static bool GetAssemblyOwner(Assembly assembly, out string modName)
	{
		modName = null;
		if (AssemblyLoadContext.GetLoadContext(assembly) is not ModLoadContext mlc)
			return false;

		modName = mlc.Name;
		if (loadedModContexts[modName] != mlc)
			throw new Exception("Attempt to retrieve owner for mod assembly from a previous load");

		return true;
	}

	internal static bool FirstModInStackTrace(StackTrace stack, out string modName)
	{
		for (int i = 0; i < stack.FrameCount; i++) {
			StackFrame frame = stack.GetFrame(i);
			var assembly = frame.GetMethod()?.DeclaringType?.Assembly;
			if (assembly != null && GetAssemblyOwner(assembly, out modName))
				return true;
		}

		modName = null;
		return false;
	}

	public static IEnumerable<Mod> GetDependencies(Mod mod) => GetLoadContext(mod.Name).dependencies.Select(m => ModLoader.GetMod(mod.Name));

	public static Type[] GetLoadableTypes(Assembly assembly) => AssemblyLoadContext.GetLoadContext(assembly) is ModLoadContext mlc ? mlc.loadableTypes[assembly] : assembly.GetTypes();

	private static IDictionary<Assembly, Type[]> GetLoadableTypes(ModLoadContext mod, MetadataLoadContext mlc)
	{
		try {
			return mod.Assemblies.ToDictionary(a => a, asm =>
				mlc.LoadFromAssemblyName(asm.GetName()).GetTypes()
					.Where(mType => IsLoadable(mod, mType))
					.Select(mType => asm.GetType(mType.FullName, throwOnError: true, ignoreCase: false))
					.ToArray());
		}
		catch (Exception e) {
			throw new Exceptions.GetLoadableTypesException(
				"This mod seems to inherit from classes in another mod. Use the [ExtendsFromMod] attribute to allow this mod to load when that mod is not enabled." + "\n\n" + (e.Data["type"] is Type type ? $"The \"{type.FullName}\" class caused this error.\n\n" : "") + e.Message,
				e
			);
		}
	}

	private static bool IsLoadable(ModLoadContext mod, Type type)
	{
		try {
			foreach (var attr in type.GetCustomAttributesData()) {
				if (attr.AttributeType.AssemblyQualifiedName == typeof(ExtendsFromModAttribute).AssemblyQualifiedName) {
					var modNames = (IEnumerable<CustomAttributeTypedArgument>)attr.ConstructorArguments[0].Value;
					if (!modNames.All(v => mod.IsModDependencyPresent((string)v.Value)))
						return false;
				}
			}

			if (type.BaseType != null && !IsLoadable(mod, type.BaseType))
				return false;

			if (type.DeclaringType != null && !IsLoadable(mod, type.DeclaringType))
				return false;

			return type.GetInterfaces().All(i => IsLoadable(mod, i));
		}
		catch (FileNotFoundException e) {
			e.Data["type"] = type;
			throw;
		}
	}

	internal static void JITMod(Mod mod) => JITAssemblies(GetModAssemblies(mod.Name), mod.PreJITFilter);

	public static void JITAssemblies(IEnumerable<Assembly> assemblies, PreJITFilter filter)
	{
		var exceptions = new System.Collections.Concurrent.ConcurrentQueue<(Exception exception, MethodBase method)>();
		foreach (var assembly in assemblies) {
			const BindingFlags ALL = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

			bool ShouldJITRecursive(Type type) => filter.ShouldJIT(type) && (type.DeclaringType is null || ShouldJITRecursive(type.DeclaringType));

			var methodsToJIT = GetLoadableTypes(assembly)
				.Where(ShouldJITRecursive)
				.SelectMany(type =>
					type.GetMethods(ALL)
						.Where(m => !m.IsSpecialName) // exclude property accessors, collect them below after checking ShouldJIT on the PropertyInfo
						.Concat<MethodBase>(type.GetConstructors(ALL))
						.Concat(type.GetProperties(ALL).Where(filter.ShouldJIT).SelectMany(p => p.GetAccessors()))
						.Where(m => !m.IsAbstract && !m.ContainsGenericParameters && m.DeclaringType == type)
						.Where(filter.ShouldJIT)
				)
				.ToArray();

			if (Environment.ProcessorCount > 1) {
				methodsToJIT.AsParallel().AsUnordered().ForAll(method => {
					try {
						ForceJITOnMethod(method);
					}
					catch (Exception e) {
						exceptions.Enqueue((e, method));
					}
				});
			}
			else {
				foreach (var method in methodsToJIT) {
					try {
						ForceJITOnMethod(method);
					}
					catch (Exception e) {
						exceptions.Enqueue((e, method));
					}
				}
			}
		}

		if (exceptions.IsEmpty)
			return;

		var message = "\n";
		if (exceptions.Select(x => x.exception).OfType<FileNotFoundException>().FirstOrDefault() is Exception ex && Regex.Match(ex.Message, "'(\\w+), Version=") is { Success: true } m) {
			var modName = m.Groups[1].Value;
			message += $"If {modName} is an assembly from a weak-referenced mod consider adding a [JitWhenModsEnabled(\"ModName\")] attribute to the method, property, lambda or containing class.\n";

			if (exceptions.Any(x => x.exception is FileNotFoundException && x.method.Name.Contains("<lambda>")))
				message += "Make sure to apply the [JitWhenModsEnabled] attribute directly to the lambda or a containing class. Attributes on methods do not apply to lambdas inside them.\n";

			message += "\n";
		}

		message += string.Join("\n", exceptions.Select(x => $"In {x.method.DeclaringType.FullName}.{x.method.Name}, {x.exception.Message}")) + "\n";
		throw new Exceptions.JITException(message);
	}

	private static void ForceJITOnMethod(MethodBase method)
	{
		if (method.GetMethodBody() != null)
			RuntimeHelpers.PrepareMethod(method.MethodHandle);

		// Here we check for overrides that override methods that no longer exist.
		// tModLoader contributors should consult https://github.com/tModLoader/tModLoader/wiki/tModLoader-Style-Guide#be-aware-of-breaking-changes to properly handle breaking changes, especially once 1.4 is stable.
		if (method is MethodInfo methodInfo) {
			// This logic checks if the method is not an override.
			bool isNewSlot = (methodInfo.Attributes & MethodAttributes.VtableLayoutMask) == MethodAttributes.NewSlot;

			// By combining the logic of the method being virtual and also an override, by checking if GetBaseDefinition returns itself, we can determine that the original base definition is gone now.
			// Note: GetBaseDefinition returns the calling instance if the instance's declaring type is an interface -- absoluteAquarian
			if (methodInfo.IsVirtual && !isNewSlot && methodInfo.DeclaringType?.IsInterface is false && methodInfo.GetBaseDefinition() == methodInfo)
				throw new Exception($"{method} overrides a method which doesn't exist in any base class");
		}
	}
}
#endif
