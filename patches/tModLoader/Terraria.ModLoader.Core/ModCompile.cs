using log4net.Core;
using Mono.Cecil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using ReLogic.OS;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.UI;
using Terraria.Utilities;

namespace Terraria.ModLoader.Core
{
	// TODO further documentation
	// TODO too many inner classes
	internal class ModCompile
	{
		public interface IBuildStatus
		{
			void SetProgress(int i, int n = -1);
			void SetStatus(string msg);
			void LogCompilerLine(string msg, Level level);
		}

		private class ConsoleBuildStatus : IBuildStatus
		{
			public void SetProgress(int i, int n) { }

			public void SetStatus(string msg) => Console.WriteLine(msg);

			public void LogCompilerLine(string msg, Level level) =>
				((level == Level.Error) ? Console.Error : Console.Out).WriteLine(msg);
		}

		private class BuildingMod : LocalMod
		{
			public string path;

			public BuildingMod(TmodFile modFile, BuildProperties properties, string path) : base(modFile, properties) {
				this.path = path;
			}
		}

		public static readonly string ModSourcePath = Path.Combine(Program.SavePath, "Mod Sources");

		internal static readonly string modCompileDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ModCompile");
		internal static readonly string modCompileVersionPath = Path.Combine(modCompileDir, "version");

		internal static string[] FindModSources() {
			Directory.CreateDirectory(ModSourcePath);
			return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => new DirectoryInfo(dir).Name[0] != '.').ToArray();
		}

		// Silence exception reporting in the chat unless actively modding.
		public static bool activelyModding;

		public static bool DeveloperMode {
			get {
				return Debugger.IsAttached || File.Exists(modCompileVersionPath) || Directory.Exists(ModSourcePath) && FindModSources().Length > 0;
			}
		}

		internal static bool DeveloperModeReady(out string msg) {
			return RoslynCompatibleFrameworkCheck(out msg) &&
				ModCompileVersionCheck(out msg) &&
				ReferenceAssembliesCheck(out msg);
		}

		internal static bool ModCompileVersionCheck(out string msg) {
			var modCompileVersion = File.Exists(modCompileVersionPath) ? File.ReadAllText(modCompileVersionPath) : "missing";
			if (modCompileVersion == ModLoader.versionTag) {
				msg = Language.GetTextValue("tModLoader.DMModCompileSatisfied");
				return true;
			}
#if DEBUG
			msg = Language.GetTextValue("tModLoader.DMModCompileDev", Path.GetFileName(Assembly.GetExecutingAssembly().Location));
#else
			if (modCompileVersion == "missing")
				msg = Language.GetTextValue("tModLoader.DMModCompileMissing");
			else
				msg = Language.GetTextValue("tModLoader.DMModCompileUpdate", ModLoader.versionTag, modCompileVersion);
#endif
			return false;
		}

		private static readonly Version minDotNetVersion = new Version(4, 6);
		private static readonly Version minMonoVersion = new Version(5, 20);
		internal static bool RoslynCompatibleFrameworkCheck(out string msg) {
			// mono 5.20 is required due to https://github.com/mono/mono/issues/12362
			if (FrameworkVersion.Framework == Framework.NetFramework && FrameworkVersion.Version >= minDotNetVersion ||
				FrameworkVersion.Framework == Framework.Mono && FrameworkVersion.Version >= minMonoVersion) {

				msg = Language.GetTextValue("tModLoader.DMDotNetSatisfied", $"{FrameworkVersion.Framework} {FrameworkVersion.Version}");
				return true;
			}

			if (FrameworkVersion.Framework == Framework.NetFramework)
				msg = Language.GetTextValue("tModLoader.DMDotNetUpdateRequired", minDotNetVersion);
			else if (SystemMonoCheck())
				msg = Language.GetTextValue("tModLoader.DMMonoRuntimeRequired", minMonoVersion);
			else
				msg = Language.GetTextValue("tModLoader.DMMonoUpdateRequired", minMonoVersion);

			return false;
		}

		internal static bool systemMonoSuitable;
		private static bool SystemMonoCheck() {
			try {
				var monoPath = "mono";
				if (Platform.IsOSX) //mono installs on OSX don't resolve properly outside of terminal
					monoPath = "/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono";

				string output = Process.Start(new ProcessStartInfo {
					FileName = monoPath,
					Arguments = "--version",
					UseShellExecute = false,
					RedirectStandardOutput = true
				}).StandardOutput.ReadToEnd();

				var monoVersion = new Version(new Regex("version (.+?) ").Match(output).Groups[1].Value);
				return systemMonoSuitable = monoVersion >= minMonoVersion;

			} catch (Exception e) {
				Logging.tML.Debug("System mono check failed: ", e);
				return false;
			}
		}

		internal static bool PlatformSupportsVisualStudio => !Platform.IsLinux;

		private static string referenceAssembliesPath;
		internal static bool ReferenceAssembliesCheck(out string msg) {
			msg = Language.GetTextValue("tModLoader.DMReferenceAssembliesSatisfied");
			if (referenceAssembliesPath != null)
				return true;

			if (Platform.IsWindows)
				referenceAssembliesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5";
			else if (Platform.IsOSX)
				referenceAssembliesPath = "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5-api";
			else if (Platform.IsLinux)
				referenceAssembliesPath = "/usr/lib/mono/4.5-api";

			if (Directory.Exists(referenceAssembliesPath))
				return true;

			referenceAssembliesPath = Path.Combine(modCompileDir, "v4.5 Reference Assemblies");
			if (Directory.Exists(referenceAssembliesPath) && Directory.EnumerateFiles(referenceAssembliesPath).Any(x => Path.GetExtension(x) != ".tmp"))
				return true;

			if (FrameworkVersion.Framework == Framework.Mono)
				msg = Language.GetTextValue("tModLoader.DMReferenceAssembliesMissingMono", "lib/mono/4.5-api");
			else
				msg = Language.GetTextValue("tModLoader.DMReferenceAssembliesMissing");

			referenceAssembliesPath = null;
			return false;
		}

		internal static readonly string modReferencesPath = Path.Combine(Program.SavePath, "references");
		private static bool referencesUpdated = false;
		internal static void UpdateReferencesFolder() {
			if (referencesUpdated)
				return;

			if (!Directory.Exists(modReferencesPath))
				Directory.CreateDirectory(modReferencesPath);

			var tMLPath = Assembly.GetExecutingAssembly().Location;
			var touchStamp = $"{tMLPath} @ {File.GetLastWriteTime(tMLPath)}";
			var touchFile = Path.Combine(modReferencesPath, "touch");
			var lastTouch = File.Exists(touchFile) ? File.ReadAllText(touchFile) : null;
			if (touchStamp == lastTouch) {
				referencesUpdated = true;
				return;
			}

			// this will extract all the embedded dlls, and grab a reference to the GAC assemblies
			var libs = GetTerrariaReferences(null, PlatformUtilities.IsXNA).ToList();

			// delete any extra references that no-longer exist
			foreach (var file in Directory.GetFiles(modReferencesPath, "*.dll"))
				if (!libs.Any(lib => Path.GetFileName(lib) == Path.GetFileName(file)))
					File.Delete(file);

			// replace tML lib with inferred paths based on names 
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
			var tMLBuildServerPath = tMLServerPath;
			if (FrameworkVersion.Framework == Framework.Mono)
				tMLBuildServerPath = tMLServerPath.Substring(0, tMLServerPath.Length - 4);

			string MakeRef(string path, string name = null) {
				if (name == null)
					name = Path.GetFileNameWithoutExtension(path);
				if (Path.GetDirectoryName(path) == modReferencesPath)
					path = "$(MSBuildThisFileDirectory)" + Path.GetFileName(path);
				return $"    <Reference Include=\"{name}\">\n      <HintPath>{path}</HintPath>\n    </Reference>";
			}
			var referencesXMLList = libs.Select(p => MakeRef(p)).ToList();
			referencesXMLList.Insert(0, MakeRef("$(tMLPath)", "Terraria"));

			var tModLoaderTargets = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <TerrariaSteamPath>{tMLDir}</TerrariaSteamPath>
    <tMLPath>{tMLPath}</tMLPath>
    <tMLServerPath>{tMLServerPath}</tMLServerPath>
    <tMLBuildServerPath>{tMLBuildServerPath}</tMLBuildServerPath>
  </PropertyGroup>
  <ItemGroup>
{string.Join("\n", referencesXMLList)}
  </ItemGroup>
</Project>";

			File.WriteAllText(Path.Combine(modReferencesPath, "tModLoader.targets"), tModLoaderTargets);
			File.WriteAllText(touchFile, touchStamp);
			referencesUpdated = true;
		}

		internal static IList<string> sourceExtensions = new List<string> { ".csproj", ".cs", ".sln" };

		private IBuildStatus status;
		public ModCompile(IBuildStatus status) {
			this.status = status;

			// *gasp*, side-effects
			activelyModding = true;
			Logging.ResetPastExceptions();
		}

		internal void BuildAll() {
			var modList = new List<LocalMod>();
			foreach (var modFolder in FindModSources())
				modList.Add(ReadBuildInfo(modFolder));

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
				throw new BuildException(e.Message);
			}

			//build
			int num = 0;
			foreach (var mod in modsToBuild) {
				status.SetProgress(num++, modsToBuild.Count);
				Build(mod);
			}
		}

		internal static void BuildModCommandLine(string modFolder) {
			// Once we get to this point, the application is guaranteed to exit
			var lockFile = AcquireConsoleBuildLock();
			try {
				new ModCompile(new ConsoleBuildStatus()).Build(modFolder);
			}
			catch (BuildException e) {
				Console.Error.WriteLine("Error: " + e.Message);
				if (e.InnerException != null)
					Console.Error.WriteLine(e.InnerException);
				Environment.Exit(1);
			}
			catch (Exception e) {
				Console.Error.WriteLine(e);
				Environment.Exit(1);
			}
			finally {
				lockFile.Close();
			}
			// Mod was built with success, exit code 0 indicates success.
			Environment.Exit(0);
		}

		internal void Build(string modFolder) => Build(ReadBuildInfo(modFolder));

		private BuildingMod ReadBuildInfo(string modFolder) {
			if (modFolder.EndsWith("\\") || modFolder.EndsWith("/")) modFolder = modFolder.Substring(0, modFolder.Length - 1);
			var modName = Path.GetFileName(modFolder);
			status.SetStatus(Language.GetTextValue("tModLoader.ReadingProperties", modName));

			BuildProperties properties;
			try {
				properties = BuildProperties.ReadBuildFile(modFolder);
			}
			catch (Exception e) {
				throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorFailedLoadBuildTxt", Path.Combine(modFolder, "build.txt")), e);
			}

			var file = Path.Combine(ModLoader.ModPath, modName + ".tmod");
			var modFile = new TmodFile(file, modName, properties.version);
			return new BuildingMod(modFile, properties, modFolder);
		}

		private void Build(BuildingMod mod) {
			try {
				status.SetStatus(Language.GetTextValue("tModLoader.Building", mod.Name));

				List<LocalMod> refMods = null;
				BuildModForPlatform(mod, ref refMods, true);
				BuildModForPlatform(mod, ref refMods, false);

				if (Program.LaunchParameters.TryGetValue("-eac", out var eacValue)) {
					mod.properties.eacPath = Path.ChangeExtension(eacValue, "pdb");
					status.SetStatus(Language.GetTextValue("tModLoader.EnabledEAC", mod.properties.eacPath));
				}

				PackageMod(mod);

				ModLoader.GetMod(mod.Name)?.Close();
				mod.modFile.Save();
				ModLoader.EnableMod(mod.Name);
			} catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
		}

		private void PackageMod(BuildingMod mod) {
			status.SetStatus(Language.GetTextValue("tModLoader.Packaging", mod));
			status.SetProgress(0, 1);

			mod.modFile.AddFile("Info", mod.properties.ToBytes());

			var resources = Directory.GetFiles(mod.path, "*", SearchOption.AllDirectories)
				.Where(res => !IgnoreResource(mod, res))
				.ToList();

			status.SetProgress(packedResourceCount = 0, resources.Count);
			Parallel.ForEach(resources, resource => AddResource(mod, resource));

			// add dll references from the bin folder
			var libFolder = Path.Combine(mod.path, "lib");
			foreach (var dllPath in mod.properties.dllReferences.Select(dllName => DllRefPath(mod, dllName)))
				if (!dllPath.StartsWith(libFolder))
					mod.modFile.AddFile("lib/"+Path.GetFileName(dllPath), File.ReadAllBytes(dllPath));
		}

		private bool IgnoreResource(BuildingMod mod, string resource) {
			var relPath = resource.Substring(mod.path.Length + 1);
			return mod.properties.ignoreFile(relPath) ||
				relPath == "build.txt" ||
				relPath[0] == '.' ||
				relPath.StartsWith("bin" + Path.DirectorySeparatorChar) ||
				relPath.StartsWith("obj" + Path.DirectorySeparatorChar) ||
				!mod.properties.includeSource && sourceExtensions.Contains(Path.GetExtension(resource)) ||
				Path.GetFileName(resource) == "Thumbs.db";
		}

		private int packedResourceCount;
		private void AddResource(BuildingMod mod, string resource) {
			var relPath = resource.Substring(mod.path.Length + 1);
			using (var src = File.OpenRead(resource))
			using (var dst = new MemoryStream()) {
				if (!ContentConverters.Convert(ref relPath, src, dst))
					src.CopyTo(dst);

				mod.modFile.AddFile(relPath, dst.ToArray());
				Interlocked.Increment(ref packedResourceCount);
				status.SetProgress(packedResourceCount);
			}
		}

		private void VerifyModAssembly(string modName, AssemblyDefinition asmDef) {
			var asmName = asmDef.Name.Name;
			if (asmName != modName)
				throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorModNameDoesntMatchAssemblyName", modName, asmName));

			if (modName.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase))
				throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorModNamedTerraria"));

			// Verify that folder and namespace match up
			var modClassType = asmDef.MainModule.Types.SingleOrDefault(x => x.BaseType?.FullName == "Terraria.ModLoader.Mod");
			if (modClassType == null)
				throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorNoModClass"));

			string topNamespace = modClassType.Namespace.Split('.')[0];
			if (topNamespace != modName)
				throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorNamespaceFolderDontMatch"));
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
					var modFile = new TmodFile(Path.Combine(ModLoader.ModPath, refName + ".tmod"));
					using (modFile.Open())
						mod = new LocalMod(modFile);
				}
				catch (FileNotFoundException) when (isWeak && !requireWeak) {
					// don't recursively require weak deps, if the mod author needs to compile against them, they'll have them installed
					continue;
				}
				catch (Exception ex) {
					throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorModReference", refName), ex);
				}
				mods[refName] = mod;
				FindReferencedMods(mod.properties, mods, false);
			}
		}

		private string tempDir = Path.Combine(ModLoader.ModPath, "compile_temp");
		private void BuildModForPlatform(BuildingMod mod, ref List<LocalMod> refMods, bool xna) {
			status.SetProgress(xna ? 0 : 1, 2);
			try {
				if (Directory.Exists(tempDir))
					Directory.Delete(tempDir, true);
				Directory.CreateDirectory(tempDir);

				string dllName = mod.Name + (xna ? ".XNA.dll" : ".FNA.dll");
				string dllPath = null;

				// look for pre-compiled paths
				if (mod.properties.noCompile) {
					var allPath = Path.Combine(mod.path, mod.Name + ".All.dll");
					dllPath = File.Exists(allPath) ? allPath : Path.Combine(mod.path, dllName);
				}
				else if (xna == PlatformUtilities.IsXNA && Program.LaunchParameters.TryGetValue("-eac", out var eacValue)) {
					dllPath = eacValue;
				}

				// precompiled load, or fallback to Roslyn compile
				if (File.Exists(dllPath))
					status.SetStatus(Language.GetTextValue("tModLoader.LoadingPrecompiled", dllName, Path.GetFileName(dllPath)));
				else if (dllPath != null)
					throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorLoadingPrecompiled", dllPath));
				else {
					dllPath = Path.Combine(tempDir, dllName);
					CompileMod(mod, dllPath, ref refMods, xna);
				}

				// add mod assembly to file
				mod.modFile.AddFile(dllName, File.ReadAllBytes(dllPath));

				// read mod assembly using cecil for verification and pdb processing
				using (var asmResolver = new DefaultAssemblyResolver()) {
					asmResolver.AddSearchDirectory(Path.GetDirectoryName(dllPath));
					asmResolver.AddSearchDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

					var asm = AssemblyDefinition.ReadAssembly(dllPath, new ReaderParameters { InMemory = true, ReadSymbols = mod.properties.includePDB, AssemblyResolver = asmResolver });
					VerifyModAssembly(mod.Name, asm);

					if (!mod.properties.includePDB)
						return;

					// when reading and writing a module with cecil, the debug sequence points need regenerating, even if the methods are not changed
					// write out the pdb file using cecil because doing it at runtime is difficult
					var tempDllPath = Path.Combine(tempDir, dllName); //use the temp dir to avoid overwriting a precompiled dll

					// force the native pdb writer when possible, to support stack traces on older .NET frameworks
					asm.Write(tempDllPath, new WriterParameters {
						WriteSymbols = true,
						SymbolWriterProvider = FrameworkVersion.Framework == Framework.NetFramework ? new NativePdbWriterProvider() : null
					});

					mod.modFile.AddFile(Path.ChangeExtension(dllName, "pdb"), File.ReadAllBytes(Path.ChangeExtension(tempDllPath, "pdb")));

					if (dllPath == tempDllPath) { // load the cecil produced dll, which has the correct debug header
						mod.modFile.AddFile(dllName, File.ReadAllBytes(dllPath));
					}
					else { // with a pre-loaded dll, the symbols won't match cecil symbols unless we splice in the cecil debug header
						using (var cecilAsmDef = AssemblyDefinition.ReadAssembly(tempDllPath))
							mod.modFile.AddFile(dllName + ".cecildebugheader", cecilAsmDef.MainModule.GetDebugHeader().GetBytes());
					}

					// make an mdb for FNA
					if (!xna) {
						asm.Write(tempDllPath, new WriterParameters { WriteSymbols = true, SymbolWriterProvider = new MdbWriterProvider() });
						mod.modFile.AddFile(dllName + ".mdb", File.ReadAllBytes(tempDllPath + ".mdb"));
					}
				}
			}
			finally {
				try {
					if (Directory.Exists(tempDir))
						Directory.Delete(tempDir, true);
				}
				catch { }
			}
		}

		private void CompileMod(BuildingMod mod, string outputPath, ref List<LocalMod> refMods, bool xna) {
			UpdateReferencesFolder();

			status.SetStatus(Language.GetTextValue("tModLoader.Compiling", Path.GetFileName(outputPath)));
			if (!DeveloperModeReady(out string msg))
				throw new BuildException(msg);

			if (refMods == null)
				refMods = FindReferencedMods(mod.properties);

			var refs = new List<string>();

			//everything used to compile the tModLoader for the target platform
			refs.AddRange(GetTerrariaReferences(tempDir, xna));

			// add framework assemblies
			refs.AddRange(Directory.GetFiles(referenceAssembliesPath, "*.dll", SearchOption.AllDirectories)
				.Where(path => !path.EndsWith("Thunk.dll") && !path.EndsWith("Wrapper.dll")));

			//libs added by the mod
			refs.AddRange(mod.properties.dllReferences.Select(dllName => DllRefPath(mod, dllName)));

			//all dlls included in all referenced mods
			foreach (var refMod in refMods) {
				using (refMod.modFile.Open()) {
					var path = Path.Combine(tempDir, refMod + ".dll");
					File.WriteAllBytes(path, refMod.modFile.GetModAssembly(xna));
					refs.Add(path);

					foreach (var refDll in refMod.properties.dllReferences) {
						path = Path.Combine(tempDir, refDll + ".dll");
						File.WriteAllBytes(path, refMod.modFile.GetBytes("lib/" + refDll + ".dll"));
						refs.Add(path);
					}
				}
			}

			var files = Directory.GetFiles(mod.path, "*.cs", SearchOption.AllDirectories).Where(file => !mod.properties.ignoreFile(file.Substring(mod.path.Length + 1))).ToArray();

			bool allowUnsafe =
				Program.LaunchParameters.TryGetValue("-unsafe", out var unsafeParam) &&
				bool.TryParse(unsafeParam, out var _allowUnsafe) && _allowUnsafe;

			var preprocessorSymbols = new List<string> { xna ? "XNA" : "FNA" };
			if (Program.LaunchParameters.TryGetValue("-define", out var defineParam))
				preprocessorSymbols.AddRange(defineParam.Split(';', ' '));

			var results = RoslynCompile(mod.Name, outputPath, refs.ToArray(), files, preprocessorSymbols.ToArray(), mod.properties.includePDB, allowUnsafe);

			int numWarnings = results.Cast<CompilerError>().Count(e => e.IsWarning);
			int numErrors = results.Count - numWarnings;
			status.LogCompilerLine(Language.GetTextValue("tModLoader.CompilationResult", numErrors, numWarnings), Level.Info);
			foreach (CompilerError line in results)
				status.LogCompilerLine(line.ToString(), line.IsWarning ? Level.Warn : Level.Error);

			if (results.HasErrors) {
				var firstError = results.Cast<CompilerError>().First(e => !e.IsWarning);
				throw new BuildException(Language.GetTextValue("tModLoader.CompileError", Path.GetFileName(outputPath), numErrors, firstError));
			}
		}

		private string DllRefPath(BuildingMod mod, string dllName) {
			var path = Path.Combine(mod.path, "lib", dllName + ".dll");
			if (File.Exists(path))
				return path;

			if (Program.LaunchParameters.TryGetValue("-eac", out var eacPath)) {
				var outputCopiedPath = Path.Combine(Path.GetDirectoryName(eacPath), dllName + ".dll");
				if (File.Exists(outputCopiedPath))
					return outputCopiedPath;
			}

			throw new BuildException("Missing dll reference: "+path);
		}

		private static IEnumerable<string> GetTerrariaReferences(string tempDir, bool xna) {
			var refs = new List<string>();

			var xnaAndFnaLibs = new[] {
				"Microsoft.Xna.Framework.dll",
				"Microsoft.Xna.Framework.Game.dll",
				"Microsoft.Xna.Framework.Graphics.dll",
				"Microsoft.Xna.Framework.Xact.dll",
				"FNA.dll"
			};

			if (xna == PlatformUtilities.IsXNA) {
				var terrariaModule = Assembly.GetExecutingAssembly();
				refs.Add(terrariaModule.Location);
				// find xna in the currently referenced assemblies (eg, via GAC)
				refs.AddRange(terrariaModule.GetReferencedAssemblies().Select(refName => Assembly.Load(refName).Location).Where(loc => xnaAndFnaLibs.Contains(Path.GetFileName(loc))));

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
				var mainModulePath = Path.Combine(modCompileDir, xna ? "tModLoader.XNA.exe" : "tModLoader.FNA.exe");
				refs.Add(mainModulePath);
				// find xna in the ModCompile folder
				refs.AddRange(xnaAndFnaLibs.Select(f => Path.Combine(modCompileDir, f)).Where(File.Exists));

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

		private static Type roslynWrapper;
		private static Type RoslynWrapper {
			get {
				if (roslynWrapper == null) {
					AppDomain.CurrentDomain.AssemblyResolve += (o, args) => {
						var name = new AssemblyName(args.Name).Name;
						var f = Path.Combine(modCompileDir, name + ".dll");
						return File.Exists(f) ? Assembly.LoadFile(f) : null;
					};
					roslynWrapper = Assembly.LoadFile(Path.Combine(modCompileDir, "RoslynWrapper.dll")).GetType("Terraria.ModLoader.RoslynWrapper");
				}
				return roslynWrapper;
			}
		}

		/// <summary>
		/// Invoke the Roslyn compiler via reflection to avoid a .NET 4.6 dependency
		/// </summary>
		private static CompilerErrorCollection RoslynCompile(string name, string outputPath, string[] references, string[] files, string[] preprocessorSymbols, bool includePdb, bool allowUnsafe)
		{
			return (CompilerErrorCollection)RoslynWrapper.GetMethod("Compile")
				.Invoke(null, new object[] { name, outputPath, references, files, preprocessorSymbols, includePdb, allowUnsafe });
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
	}
}
