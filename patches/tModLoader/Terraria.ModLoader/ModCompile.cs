using Mono.Cecil;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModLoader;

namespace Terraria.ModLoader
{
	//todo: further documentation
	internal class ModCompile
	{
		public interface IBuildStatus
		{
			void SetProgress(int i, int n);
			void SetStatus(string msg);
			void LogError(string msg, Exception e = null);
			void LogCompileErrors(string dllName, CompilerErrorCollection errors, string hint);
			void SetMod(string name);
		}

		private class ConsoleBuildStatus : IBuildStatus
		{
			public void SetProgress(int i, int n) { }

			public void SetMod(string name) { }

			public void SetStatus(string msg) {
				Console.WriteLine(msg);
			}

			public void LogError(string msg, Exception e = null) {
				Console.WriteLine(msg);
				if (e != null)
					Console.WriteLine(e);
			}

			public void LogCompileErrors(string dllName, CompilerErrorCollection errors, string hint) {
				if (hint != null)
					Console.WriteLine(hint);

				foreach (CompilerError error in errors)
					Console.WriteLine(error);
			}
		}

		private class BuildingMod : LocalMod
		{
			public string path;

			public BuildingMod(TmodFile modFile, BuildProperties properties, string path) : base(modFile, properties) {
				this.path = path;
			}
		}

		public static readonly string ModSourcePath = Path.Combine(Main.SavePath, "Mod Sources");

		internal static readonly string modCompileDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ModCompile");
		internal static readonly string modCompileVersionPath = Path.Combine(modCompileDir, "version");

		internal static string[] FindModSources() {
			Directory.CreateDirectory(ModSourcePath);
			return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => new DirectoryInfo(dir).Name[0] != '.').ToArray();
		}

		private static bool? developerMode;
		public static bool DeveloperMode {
			get {
				if (!developerMode.HasValue)
					developerMode = Directory.Exists(ModSourcePath) && FindModSources().Length > 0 || File.Exists(modCompileVersionPath);

				return developerMode.Value;
			}
			set {
				developerMode = value;
				Logging.LogFirstChanceExceptions(value);
			}
		}

		internal static bool DeveloperModeReady(out string errorKey) {
			return DotNet46Check(out errorKey) &&
				ModCompileVersionCheck(out errorKey) &&
				ReferenceAssembliesCheck(out errorKey);
		}

		internal static bool ModCompileVersionCheck(out string infoKey) {
			if (File.Exists(modCompileVersionPath) && File.ReadAllText(modCompileVersionPath) == versionTag) {
				infoKey = "tModLoader.DMModCompileSatisfied";
				return true;
			}
#if DEBUG
			infoKey = "tModLoader.DMModCompileDev";
#else
			infoKey = "tModLoader." + (Directory.Exists(modCompileDir) ? "DMModCompileUpdate" : "DMModCompileMissing");
#endif
			return false;
		}

		internal static bool DotNet46Check(out string infoKey) {
			bool ret = FrameworkVersion.Framework == ".NET Framework" && FrameworkVersion.Version > new Version(4, 6);
			infoKey = "tModLoader." + (ret ? "DMDotNetSatisfied" : "DMDotNet46Required");
			return ret;
		}

		private static string referenceAssembliesPath;
		internal static bool ReferenceAssembliesCheck() {
			if (referenceAssembliesPath != null)
				return true;

			referenceAssembliesPath = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)) + @"Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5";
			if (Directory.Exists(referenceAssembliesPath))
				return true;

			referenceAssembliesPath = Path.Combine(modCompileDir, "v4.5 Reference Assemblies");
			if (Directory.Exists(referenceAssembliesPath) && Directory.EnumerateFiles(referenceAssembliesPath).Any())
				return true;

			referenceAssembliesPath = null;
			return false;
		}

		internal static bool ReferenceAssembliesCheck(out string infoKey) {
			var ret = ReferenceAssembliesCheck();
			infoKey = "tModLoader." + (ret ? "DMReferenceAssembliesSatisfied" : "DMReferenceAssembliesMissing");
			return ret;
		}
		
		private static readonly string modReferencesPath = Path.Combine(Main.SavePath, "references");
		private static bool referencesUpdated = false;
		internal static void UpdateReferencesFolder() {
			if (referencesUpdated)
				return;

			if (!Directory.Exists(modReferencesPath))
				Directory.CreateDirectory(modReferencesPath);

			// this will extract all the embedded dlls, and grab a reference to the GAC assemblies
			var libs = GetTerrariaReferences(null, windows).ToList();

			// delete any extra references that no-longer exist
			foreach (var file in Directory.GetFiles(modReferencesPath, "*.dll"))
				if (!libs.Any(lib => Path.GetFileName(lib) == Path.GetFileName(file)))
					File.Delete(file);

			var tMLPath = libs[0];
			libs.RemoveAt(0);

			var tMLDir = Path.GetDirectoryName(tMLPath);
#if CLIENT
			var tMLServerName = Path.GetFileName(tMLPath).Replace("tModLoader", "tModLoaderServer");
			if (tMLServerName == "Terraria.exe")
				tMLServerName = "tModLoaderServer.exe";
			var tMLServerPath = Path.Combine(tMLDir, tMLServerName);
#else
			var tMLServerPath = tMLPath;
			var tMLClientName = Path.GetFileName(tMLPath).Replace("tModLoaderServer", "tModLoader");
			tMLPath = Path.Combine(tMLDir, tMLClientName);
			if (!File.Exists(tMLPath))
				tMLPath = Path.Combine(tMLDir, "Terraria.exe");
#endif
			
			string MakeRef(string path, string name = null) {
				if (name == null)
					name = Path.GetFileNameWithoutExtension(path);
				if (Path.GetDirectoryName(path) == modReferencesPath)
					path = "$(MSBuildThisFileDirectory)" + Path.GetFileName(path);
				return $"    <Reference Include=\"{name}\">\n      <HintPath>{path}</HintPath>\n    </Reference>";
			}
			var referencesXMLList = libs.Select(p => MakeRef(p)).ToList();
			referencesXMLList.Insert(0, MakeRef("$(tMLPath)", "Terraria"));

			var tModLoaderTargets = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <TerrariaSteamPath>%tMLDir%</TerrariaSteamPath>
    <tMLPath>%tMLPath%</tMLPath>
    <tMLServerPath>%tMLServerPath%</tMLServerPath>
  </PropertyGroup>
  <ItemGroup>
%references%
  </ItemGroup>
</Project>";
			tModLoaderTargets = tModLoaderTargets
				.Replace("%tMLDir%", tMLDir)
				.Replace("%tMLPath%", tMLPath)
				.Replace("%tMLServerPath%", tMLServerPath)
				.Replace("%references%", string.Join("\n", referencesXMLList));
			
			File.WriteAllText(Path.Combine(modReferencesPath, "tModLoader.targets"), tModLoaderTargets);
			referencesUpdated = true;
		}

		internal static IList<string> sourceExtensions = new List<string> { ".csproj", ".cs", ".sln" };

		private IBuildStatus status;
		public ModCompile(IBuildStatus status) {
			this.status = status;
			status.SetMod(null);
			UpdateReferencesFolder();
		}

		internal bool BuildAll() {
			var modList = new List<LocalMod>();
			foreach (var modFolder in FindModSources()) {
				var mod = ReadProperties(modFolder);
				if (mod == null)
					return false;

				modList.Add(mod);
			}

			//figure out which of the installed mods are required for building
			var installedMods = ModOrganizer.FindMods().Where(mod => !modList.Exists(m => m.Name == mod.Name)).ToList();

			var requiredFromInstall = new HashSet<LocalMod>();
			void Require(LocalMod mod, bool includeWeak) {
				foreach (var dep in mod.properties.RefNames(includeWeak)) {
					var depMod = installedMods.SingleOrDefault(m => m.Name == dep);
					if (depMod != null && requiredFromInstall.Add(depMod))
						Require(depMod, false);
				}
			}

			foreach (var mod in modList)
				Require(mod, true);

			modList.AddRange(requiredFromInstall);

			//sort and version check
			List<BuildingMod> modsToBuild;
			try {
				ModOrganizer.EnsureDependenciesExist(modList, true);
				ModOrganizer.EnsureTargetVersionsMet(modList);
				var sortedModList = ModOrganizer.Sort(modList);
				modsToBuild = sortedModList.OfType<BuildingMod>().ToList();
			}
			catch (ModSortingException e) {
				status.LogError(e.Message);
				return false;
			}

			//build
			int num = 0;
			foreach (var mod in modsToBuild) {
				status.SetProgress(num++, modsToBuild.Count);
				if (!Build(mod))
					return false;
			}

			return true;
		}

		internal static void BuildModCommandLine(string modFolder) {
			// Once we get to this point, the application is guaranteed to exit
			var lockFile = AcquireConsoleBuildLock();
			try {
				var modCompile = new ModCompile(new ConsoleBuildStatus());
				if (!modCompile.Build(modFolder))
					Environment.Exit(1);
			}
			catch (Exception e) {
				Console.WriteLine(e);
				Environment.Exit(1);
			}
			finally {
				lockFile.Close();
			}
			// Mod was built with success, exit code 0 indicates success.
			Environment.Exit(0);
		}

		internal bool Build(string modFolder) {
			var mod = ReadProperties(modFolder);
			return mod != null && Build(mod);
		}

		private BuildingMod ReadProperties(string modFolder) {
			if (modFolder.EndsWith("\\") || modFolder.EndsWith("/")) modFolder = modFolder.Substring(0, modFolder.Length - 1);
			var modName = Path.GetFileName(modFolder);
			status.SetStatus(Language.GetTextValue("tModLoader.ReadingProperties", modName));

			BuildProperties properties;
			try {
				properties = BuildProperties.ReadBuildFile(modFolder);
			}
			catch (Exception e) {
				status.LogError(Language.GetTextValue("tModLoader.BuildErrorFailedLoadBuildTxt", Path.Combine(modFolder, "build.txt")), e);
				return null;
			}

			var file = Path.Combine(ModPath, modName + ".tmod");
			var modFile = new TmodFile(file) {
				name = modName,
				version = properties.version
			};
			return new BuildingMod(modFile, properties, modFolder);
		}

		private static byte[] ReadIfExists(string path) {
			return File.Exists(path) ? File.ReadAllBytes(path) : null;
		}

		private bool Build(BuildingMod mod) {
			status.SetMod(mod.Name);
			status.SetStatus(Language.GetTextValue("tModLoader.Building", mod.Name));
			byte[] winDLL = null;
			byte[] monoDLL = null;
			byte[] winPDB = null;
			if (mod.properties.noCompile) {
				winDLL = monoDLL = ReadIfExists(Path.Combine(mod.path, "All.dll"));
				winPDB = ReadIfExists(Path.Combine(mod.path, "All.pdb"));

				if (winDLL == null) {
					winDLL = ReadIfExists(Path.Combine(mod.path, "Windows.dll"));
					monoDLL = ReadIfExists(Path.Combine(mod.path, "Mono.dll"));
					winPDB = ReadIfExists(Path.Combine(mod.path, "Windows.pdb"));
				}

				if (winDLL == null || monoDLL == null) {
					var missing = new List<string> { "All.dll" };
					if (winDLL == null) missing.Add("Windows.dll");
					if (monoDLL == null) missing.Add("Mono.dll");
					status.LogError(Language.GetTextValue("tModLoader.BuildErrorMissingDllFiles", string.Join(", ", missing)));
					return false;
				}
			}
			else {
				List<LocalMod> refMods;
				try {
					refMods = FindReferencedMods(mod.properties);
				}
				catch (Exception e) {
					status.LogError(e.Message, e.InnerException);
					return false;
				}

				if (Program.LaunchParameters.ContainsKey("-eac")) {
					if (!windows) {
						status.LogError(Language.GetTextValue("tModLoader.BuildErrorEACWindowsOnly"));
						return false;
					}

					var winPath = Program.LaunchParameters["-eac"];
					try {
						status.SetStatus(Language.GetTextValue("tModLoader.LoadingEAC"));
						var pdbPath = Path.ChangeExtension(winPath, "pdb");
						winDLL = File.ReadAllBytes(winPath);
						winPDB = File.ReadAllBytes(pdbPath);
						mod.properties.editAndContinue = true;
					}
					catch (Exception e) {
						status.LogError(Language.GetTextValue("tModLoader.BuildErrorEACLoadFailed", winPath + "/.pdb"), e);
						return false;
					}
				}
				else {
					status.SetStatus(Language.GetTextValue("tModLoader.CompilingWindows", mod));
					status.SetProgress(0, 2);
					CompileMod(mod, refMods, true, ref winDLL, ref winPDB);
				}
				if (winDLL == null)
					return false;

				status.SetStatus(Language.GetTextValue("tModLoader.CompilingMono", mod));
				status.SetProgress(1, 2);
				CompileMod(mod, refMods, false, ref monoDLL, ref winPDB);//the pdb reference won't actually be written to
				if (monoDLL == null)
					return false;
			}

			if (!VerifyName(mod.Name, winDLL) || !VerifyName(mod.Name, monoDLL))
				return false;

			status.SetStatus(Language.GetTextValue("tModLoader.Packaging", mod));
			status.SetProgress(0, 1);

			mod.modFile.AddFile("Info", mod.properties.ToBytes());

			if (Equal(winDLL, monoDLL)) {
				mod.modFile.AddFile("All.dll", winDLL);
				if (winPDB != null) mod.modFile.AddFile("All.pdb", winPDB);
			}
			else {
				mod.modFile.AddFile("Windows.dll", winDLL);
				mod.modFile.AddFile("Mono.dll", monoDLL);
				if (winPDB != null) mod.modFile.AddFile("Windows.pdb", winPDB);
			}

			foreach (var resource in Directory.GetFiles(mod.path, "*", SearchOption.AllDirectories)) {
				var relPath = resource.Substring(mod.path.Length + 1);
				if (mod.properties.ignoreFile(relPath) ||
						relPath == "build.txt" ||
						relPath == ".gitattributes" ||
						relPath == ".gitignore" ||
						relPath.StartsWith(".git" + Path.DirectorySeparatorChar) ||
						relPath.StartsWith(".vs" + Path.DirectorySeparatorChar) ||
						relPath.StartsWith(".idea" + Path.DirectorySeparatorChar) ||
						relPath.StartsWith("bin" + Path.DirectorySeparatorChar) ||
						relPath.StartsWith("obj" + Path.DirectorySeparatorChar) ||
						!mod.properties.includeSource && sourceExtensions.Contains(Path.GetExtension(resource)) ||
						Path.GetFileName(resource) == "Thumbs.db")
					continue;

				AddResource(mod.modFile, relPath, resource);
			}

			WAVCacheIO.ClearCache(mod.Name);

			GetMod(mod.Name)?.File?.Close(); // if the mod is currently loaded, the file-handle needs to be released
			mod.modFile.Save();
			mod.modFile.Close();
			EnableMod(mod.Name);
			return true;
		}

		private static void AddResource(TmodFile modFile, string relPath, string filePath) {
			using (var src = File.OpenRead(filePath))
			using (var dst = new MemoryStream()) {
				if (!ContentConverters.Convert(ref relPath, src, dst))
					src.CopyTo(dst);

				modFile.AddFile(relPath, dst.ToArray());
			}
		}

		private bool VerifyName(string modName, byte[] dll) {
			var asmDef = AssemblyDefinition.ReadAssembly(new MemoryStream(dll));
			var asmName = asmDef.Name.Name;
			if (asmName != modName) {
				status.LogError(Language.GetTextValue("tModLoader.BuildErrorModNameDoesntMatchAssemblyName", modName, asmName));
				return false;
			}

			if (modName.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase)) {
				status.LogError(Language.GetTextValue("tModLoader.BuildErrorModNamedTerraria"));
				return false;
			}

			// Verify that folder and namespace match up
			try {
				var modClassType = asmDef.MainModule.Types.Single(x => x.BaseType?.FullName == "Terraria.ModLoader.Mod");
				string topNamespace = modClassType.Namespace.Split('.')[0];
				if (topNamespace != modName) {
					status.LogError(Language.GetTextValue("tModLoader.BuildErrorNamespaceFolderDontMatch"));
					return false;
				}
			}
			catch {
				status.LogError(Language.GetTextValue("tModLoader.BuildErrorNoModClass"));
				return false;
			}

			return true;
		}

		private static bool Equal(byte[] a, byte[] b) {
			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return false;

			return true;
		}

		private List<LocalMod> FindReferencedMods(BuildProperties properties) {
			var mods = new Dictionary<string, LocalMod>();
			FindReferencedMods(properties, mods, true);
			return mods.Values.ToList();
		}

		private void FindReferencedMods(BuildProperties properties, Dictionary<string, LocalMod> mods, bool requireWeak) {
			foreach (var refName in properties.RefNames(true)) {
				if (mods.ContainsKey(refName))
					continue;
				
				bool isWeak = properties.weakReferences.Any(r => r.mod == refName);
				LocalMod mod;
				try {
					var modFile = new TmodFile(Path.Combine(ModPath, refName + ".tmod"));
					modFile.Read();
					mod = new LocalMod(modFile);
					modFile.Close();
				}
				catch (FileNotFoundException) when (isWeak && !requireWeak) { 
					// don't recursively require weak deps, if the mod author needs to compile against them, they'll have them installed
					continue;
				}
				catch (Exception ex) {
					throw new Exception(Language.GetTextValue("tModLoader.BuildErrorModReference", refName), ex);
				}
				mods[refName] = mod;
				FindReferencedMods(mod.properties, mods, false);
			}
		}

		private void CompileMod(BuildingMod mod, List<LocalMod> refMods, bool forWindows, ref byte[] dll, ref byte[] pdb) {
			if (!DeveloperModeReady(out string errorKey)) {
				status.LogError(Language.GetTextValue(errorKey));
				return;
			}

			bool generatePDB = mod.properties.includePDB && forWindows;
			if (generatePDB && !windows) {
				Console.WriteLine(Language.GetTextValue("tModLoader.BuildErrorPDBWindowsOnly"));
				generatePDB = false;
			}

			//collect all dll references
			var tempDir = Path.Combine(ModPath, "compile_temp");
			Directory.CreateDirectory(tempDir);
			var refs = new List<string>();

			//everything used to compile the tModLoader for the target platform
			refs.AddRange(GetTerrariaReferences(tempDir, forWindows));

			// add framework assemblies
			refs.AddRange(Directory.GetFiles(referenceAssembliesPath, "*.dll", SearchOption.AllDirectories)
				.Where(path => !path.EndsWith("Thunk.dll") && !path.EndsWith("Wrapper.dll")));

			//libs added by the mod
			refs.AddRange(mod.properties.dllReferences.Select(refDll => Path.Combine(mod.path, "lib/" + refDll + ".dll")));

			//all dlls included in all referenced mods
			foreach (var refMod in refMods) {
				using (refMod.modFile.EnsureOpen()) {
					var path = Path.Combine(tempDir, refMod + ".dll");
					File.WriteAllBytes(path, refMod.modFile.GetMainAssembly(forWindows));
					refs.Add(path);

					foreach (var refDll in refMod.properties.dllReferences) {
						path = Path.Combine(tempDir, refDll + ".dll");
						File.WriteAllBytes(path, refMod.modFile.GetBytes("lib/" + refDll + ".dll"));
						refs.Add(path);
					}
				}
			}

			var compileOptions = new CompilerParameters {
				OutputAssembly = Path.Combine(tempDir, mod + ".dll"),
				GenerateExecutable = false,
				GenerateInMemory = false,
				TempFiles = new TempFileCollection(tempDir, true),
				IncludeDebugInformation = generatePDB,
				CompilerOptions = "/optimize"
			};

			compileOptions.ReferencedAssemblies.AddRange(refs.ToArray());
			var files = Directory.GetFiles(mod.path, "*.cs", SearchOption.AllDirectories).Where(file => !mod.properties.ignoreFile(file.Substring(mod.path.Length + 1))).ToArray();

			try {

				var results = RoslynCompile(compileOptions, files);
				if (results.Errors.HasErrors) {
					status.LogCompileErrors(mod.Name + (forWindows ? "/Windows.dll" : "/Mono.dll"), results.Errors,
						forWindows != windows ? Language.GetTextValue("tModLoader.BuildErrorModCompileFolderHint") : null);
					return;
				}

				dll = File.ReadAllBytes(compileOptions.OutputAssembly);
				dll = PostProcess(dll, forWindows);
				if (generatePDB)
					pdb = File.ReadAllBytes(Path.Combine(tempDir, mod + ".pdb"));
			}
			finally {
				int numTries = 10;
				while (numTries > 0) {
					try {
						Directory.Delete(tempDir, true);
						numTries = 0;
					}
					catch {
						System.Threading.Thread.Sleep(1);
						numTries--;
					}
				}
			}
		}

		private static IEnumerable<string> GetTerrariaReferences(string tempDir, bool forWindows) {
			var refs = new List<string>();

			var xnaLibs = new[] {
				"Microsoft.Xna.Framework.dll",
				"Microsoft.Xna.Framework.Game.dll",
				"Microsoft.Xna.Framework.Graphics.dll",
				"Microsoft.Xna.Framework.Xact.dll",
				"FNA.dll" 
			};

			if (forWindows == windows) {
				var terrariaModule = Assembly.GetExecutingAssembly();
				refs.Add(terrariaModule.Location);
				// find xna in the currently referenced assemblies (eg, via GAC)
				refs.AddRange(terrariaModule.GetReferencedAssemblies().Select(refName => Assembly.Load(refName).Location).Where(loc => xnaLibs.Contains(Path.GetFileName(loc))));
				
				// avoid a double extract of the embedded dlls
				if (referencesUpdated) {
					refs.AddRange(Directory.GetFiles(modReferencesPath, "*.dll"));
					return refs;
				}

				//extract embedded resource dlls to the references path rather than the tempDir
				foreach (var resName in terrariaModule.GetManifestResourceNames().Where(n => n.EndsWith(".dll"))) {
					var path = Path.Combine(modReferencesPath, Path.GetFileName(resName));
					using (Stream res = terrariaModule.GetManifestResourceStream(resName), file = File.Create(path))
						res.CopyTo(file);

					refs.Add(path);
				}
			}
			else {
				var mainModulePath = Path.Combine(modCompileDir, forWindows ? "tModLoaderWindows.exe" : "tModLoaderMac.exe");
				refs.Add(mainModulePath);
				// find xna in the ModCompile folder
				refs.AddRange(xnaLibs.Select(f => Path.Combine(modCompileDir, f)).Where(File.Exists));

				//extract embedded resource dlls to a temporary folder
				var asm = AssemblyDefinition.ReadAssembly(mainModulePath);
				foreach (var res in asm.MainModule.Resources.OfType<EmbeddedResource>().Where(res => res.Name.EndsWith(".dll"))) {
					var path = Path.Combine(tempDir, Path.GetFileName(res.Name));
					using (Stream s = res.GetResourceStream(), file = File.Create(path))
						s.CopyTo(file);

					refs.Add(path);
				}
			}

			return refs;
		}

		/// <summary>
		/// Invoke the Roslyn compiler via reflection to avoid a .NET 4.6 dependency
		/// </summary>
		private static CompilerResults RoslynCompile(CompilerParameters compileOptions, string[] files) {
			var asm = Assembly.LoadFile(Path.Combine(modCompileDir, "RoslynWrapper.dll"));

			AppDomain.CurrentDomain.AssemblyResolve += (o, args) => {
				var name = new AssemblyName(args.Name).Name;
				var f = Path.Combine(modCompileDir, name + ".dll");
				return File.Exists(f) ? Assembly.LoadFile(f) : null;
			};

			var res = (CompilerResults)asm.GetType("Terraria.ModLoader.RoslynWrapper").GetMethod("Compile")
				.Invoke(null, new object[] { compileOptions, files });

			if (!res.Errors.HasErrors && compileOptions.IncludeDebugInformation)
				asm.GetType("Terraria.ModLoader.RoslynPdbFixer").GetMethod("Fix")
					.Invoke(null, new object[] { compileOptions.OutputAssembly });

			return res;
		}

		private static FileStream AcquireConsoleBuildLock() {
			var path = Path.Combine(modCompileDir, "buildlock");
			bool first = true;
			while (true) {
				try {
					return new FileStream(path, FileMode.OpenOrCreate);
				}
				catch (IOException) {
					if (first) {
						Console.WriteLine("Waiting for other builds to complete");
						first = false;
					}
				}
			}
		}

		private static AssemblyNameReference GetOrAddSystemCore(ModuleDefinition module) {
			var assemblyRef = module.AssemblyReferences.SingleOrDefault(r => r.Name == "System.Core");
			if (assemblyRef == null) {
				//System.Linq.Enumerable is in System.Core
				var name = Assembly.GetAssembly(typeof(Enumerable)).GetName();
				assemblyRef = new AssemblyNameReference(name.Name, name.Version) {
					Culture = name.CultureInfo.Name,
					PublicKeyToken = name.GetPublicKeyToken(),
					HashAlgorithm = (AssemblyHashAlgorithm)name.HashAlgorithm
				};
				module.AssemblyReferences.Add(assemblyRef);
			}
			return assemblyRef;
		}

		private static byte[] PostProcess(byte[] dll, bool forWindows) {
			if (forWindows)
				return dll;

			var asm = AssemblyDefinition.ReadAssembly(new MemoryStream(dll));

			// Extension methods are marked with an attribute which is located in mscorlib on .NET but in System.Core on Mono
			// Find all extension attributes and change their assembly references
			AssemblyNameReference SystemCoreRef = null;
			foreach (var module in asm.Modules)
				foreach (var type in module.Types)
					foreach (var met in type.Methods)
						foreach (var attr in met.CustomAttributes)
							if (attr.AttributeType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute")
								attr.AttributeType.Scope = SystemCoreRef ?? (SystemCoreRef = GetOrAddSystemCore(module));

			var ret = new MemoryStream();
			asm.Write(ret, new WriterParameters { SymbolWriterProvider = AssemblyManager.SymbolWriterProvider.instance });
			return ret.ToArray();
		}
	}
}
