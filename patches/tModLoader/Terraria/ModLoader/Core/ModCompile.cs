#if NETCORE
using log4net.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Exceptions;

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

			public BuildingMod(TmodFile modFile, BuildProperties properties, string path) : base(modFile, properties)
			{
				this.path = path;
			}
		}

		public static readonly string ModSourcePath = Path.Combine(Program.SavePath, "Mod Sources");

		internal static string[] FindModSources()
		{
			Directory.CreateDirectory(ModSourcePath);
			return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => new DirectoryInfo(dir).Name[0] != '.').ToArray();
		}

		// Silence exception reporting in the chat unless actively modding.
		public static bool activelyModding;

		public static bool DeveloperMode => Debugger.IsAttached || Directory.Exists(ModSourcePath) && FindModSources().Length > 0;

		internal static readonly string modReferencesPath = Path.Combine(Program.SavePath, "references");
		private static bool referencesUpdated = false;
		internal static void UpdateReferencesFolder()
		{
			if (referencesUpdated)
				return;

			if (!Directory.Exists(modReferencesPath))
				Directory.CreateDirectory(modReferencesPath);

			var tMLDir = Assembly.GetExecutingAssembly().Location;

			// Version checking
			string touchStamp = BuildInfo.BuildIdentifier;
			string touchFile = Path.Combine(modReferencesPath, "touch");
			string lastTouch = File.Exists(touchFile) ? File.ReadAllText(touchFile) : null;
			if (touchStamp == lastTouch && false) { // TODO remove && false. this is temp
				referencesUpdated = true;
				return;
			}

			// this will extract all the embedded dlls, and grab a reference to the GAC assemblies
			var libs = GetTerrariaReferences();

			// delete any extra references that no-longer exist
			foreach (string file in Directory.GetFiles(modReferencesPath, "*.dll"))
				if (!libs.Any(lib => Path.GetFileName(lib) == Path.GetFileName(file)))
					File.Delete(file);

			// replace tML lib with inferred paths based on names 
			libs.RemoveAt(0);

			string MakeRef(string path, string name = null)
			{
				if (name == null)
					name = Path.GetFileNameWithoutExtension(path);

				if (Path.GetDirectoryName(path) == modReferencesPath)
					path = "$(MSBuildThisFileDirectory)" + Path.GetFileName(path);

				return $"    <Reference Include=\"{System.Security.SecurityElement.Escape(name)}\">\n      <HintPath>{System.Security.SecurityElement.Escape(path)}</HintPath>\n    </Reference>";
			}

			var referencesXMLList = libs.Select(p => MakeRef(p)).ToList();

			referencesXMLList.Insert(0, MakeRef("$(tMLPath)", "Terraria"));

			char s = Path.DirectorySeparatorChar;
			string tModLoaderTargets = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <TerrariaSteamPath>{System.Security.SecurityElement.Escape(tMLDir)}</TerrariaSteamPath>
    <tMLPath>$(TerrariaSteamPath){s}tModLoader.exe</tMLPath>
    <tMLServerPath>$(TerrariaSteamPath){s}tModLoaderServer.exe</tMLServerPath>
    <tMLBuildServerPath>$(TerrariaSteamPath){s}tModLoaderServer.dll</tMLBuildServerPath>
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
		public ModCompile(IBuildStatus status)
		{
			this.status = status;

			// *gasp*, side-effects
			activelyModding = true;
			Logging.ResetPastExceptions();
		}

		internal void BuildAll()
		{
			var modList = new List<LocalMod>();
			foreach (var modFolder in FindModSources())
				modList.Add(ReadBuildInfo(modFolder));

			//figure out which of the installed mods are required for building
			var installedMods = ModOrganizer.FindMods().Where(mod => !modList.Exists(m => m.Name == mod.Name)).ToList();

			var requiredFromInstall = new HashSet<LocalMod>();
			void Require(LocalMod mod, bool includeWeak)
			{
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

		internal static void BuildModCommandLine(string modFolder)
		{
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

		private BuildingMod ReadBuildInfo(string modFolder)
		{
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

		private void Build(BuildingMod mod)
		{
			try {
				status.SetStatus(Language.GetTextValue("tModLoader.Building", mod.Name));

				BuildModForPlatform(mod);

				if (Program.LaunchParameters.TryGetValue("-eac", out var eacValue)) {
					mod.properties.eacPath = Path.ChangeExtension(eacValue, "pdb");
					status.SetStatus(Language.GetTextValue("tModLoader.EnabledEAC", mod.properties.eacPath));
				}

				PackageMod(mod);

				if (ModLoader.TryGetMod(mod.Name, out var loadedMod)) {
					loadedMod.Close();
				}

				mod.modFile.Save();
				ModLoader.EnableMod(mod.Name);
			}
			catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
		}

		private void PackageMod(BuildingMod mod)
		{
			status.SetStatus(Language.GetTextValue("tModLoader.Packaging", mod));
			status.SetProgress(0, 1);

			mod.modFile.AddFile("Info", mod.properties.ToBytes());

			var resources = Directory.GetFiles(mod.path, "*", SearchOption.AllDirectories)
				.Where(res => !IgnoreResource(mod, res))
				.ToList();

			status.SetProgress(packedResourceCount = 0, resources.Count);
			Parallel.ForEach(resources, resource => AddResource(mod, resource));

			// add dll references from the -eac bin folder
			var libFolder = Path.Combine(mod.path, "lib");
			foreach (var dllPath in mod.properties.dllReferences.Select(dllName => DllRefPath(mod, dllName)))
				if (!dllPath.StartsWith(libFolder))
					mod.modFile.AddFile("lib/" + Path.GetFileName(dllPath), File.ReadAllBytes(dllPath));
		}

		private bool IgnoreResource(BuildingMod mod, string resource)
		{
			var relPath = resource.Substring(mod.path.Length + 1);
			return IgnoreCompletely(mod, resource) ||
				relPath == "build.txt" ||
				!mod.properties.includeSource && sourceExtensions.Contains(Path.GetExtension(resource)) ||
				Path.GetFileName(resource) == "Thumbs.db";
		}

		// Ignore for both Compile and Packaging
		private bool IgnoreCompletely(BuildingMod mod, string resource)
		{
			var relPath = resource.Substring(mod.path.Length + 1);
			return mod.properties.ignoreFile(relPath) ||
				relPath[0] == '.' ||
				relPath.StartsWith("bin" + Path.DirectorySeparatorChar) ||
				relPath.StartsWith("obj" + Path.DirectorySeparatorChar);
		}

		private int packedResourceCount;
		private void AddResource(BuildingMod mod, string resource)
		{
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

		private void VerifyModAssembly(string modName, string dllPath)
		{
			var newContext = new AssemblyLoadContext($"{modName} compile checks", true);
			try {
				var asm = newContext.LoadFromAssemblyPath(dllPath);

				var asmName = new AssemblyName(asm.FullName).Name;
				if (asmName != modName)
					throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorModNameDoesntMatchAssemblyName", modName, asmName));

				if (modName.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase))
					throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorModNamedTerraria"));

				// Verify that folder and namespace match up
				var modClassType = asm.DefinedTypes.SingleOrDefault(x => x.BaseType?.FullName == "Terraria.ModLoader.Mod");
				if (modClassType == null)
					throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorNoModClass"));

				string topNamespace = modClassType.Namespace.Split('.')[0];
				if (topNamespace != modName)
					throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorNamespaceFolderDontMatch"));
			}
			finally {
				newContext.Unload();
			}
		}

		private List<LocalMod> FindReferencedMods(BuildProperties properties)
		{
			var mods = new Dictionary<string, LocalMod>();
			FindReferencedMods(properties, mods, true);
			return mods.Values.ToList();
		}

		private void FindReferencedMods(BuildProperties properties, Dictionary<string, LocalMod> mods, bool requireWeak)
		{
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
		private void BuildModForPlatform(BuildingMod mod)
		{
			try {
				if (Directory.Exists(tempDir))
					Directory.Delete(tempDir, true);
				Directory.CreateDirectory(tempDir);

				string dllName = mod.Name + ".dll";
				string dllPath = null;

				// look for pre-compiled paths
				if (mod.properties.noCompile) {
					dllPath = Path.Combine(mod.path, dllName);
				}
				else if (Program.LaunchParameters.TryGetValue("-eac", out var eacValue)) {
					dllPath = eacValue;
				}

				// precompiled load, or fallback to Roslyn compile
				if (File.Exists(dllPath))
					status.SetStatus(Language.GetTextValue("tModLoader.LoadingPrecompiled", dllName, Path.GetFileName(dllPath)));
				else if (dllPath != null)
					throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorLoadingPrecompiled", dllPath));
				else {
					dllPath = Path.Combine(tempDir, dllName);
					CompileMod(mod, dllPath);
				}

				VerifyModAssembly(mod.Name, dllPath);

				// add mod assembly to file
				mod.modFile.AddFile(dllName, File.ReadAllBytes(dllPath));
			}
			finally {
				try {
					if (Directory.Exists(tempDir))
						Directory.Delete(tempDir, true);
				}
				catch { }
			}
		}

		private void CompileMod(BuildingMod mod, string outputPath)
		{
			UpdateReferencesFolder();

			status.SetStatus(Language.GetTextValue("tModLoader.Compiling", Path.GetFileName(outputPath)));

			var refs = new List<string>();

			//everything used to compile the tModLoader for the target platform
			refs.AddRange(GetTerrariaReferences());

			// TODO: do we need to always compile against reference assemblies?
			// add framework assemblies
			var frameworkAssembliesPath = Path.GetDirectoryName(typeof(File).Assembly.Location);
			refs.AddRange(Directory.GetFiles(frameworkAssembliesPath, "*.dll", SearchOption.AllDirectories));

			//libs added by the mod
			refs.AddRange(mod.properties.dllReferences.Select(dllName => DllRefPath(mod, dllName)));

			//all dlls included in all referenced mods
			foreach (var refMod in FindReferencedMods(mod.properties)) {
				using (refMod.modFile.Open()) {
					var path = Path.Combine(tempDir, refMod + ".dll");
					File.WriteAllBytes(path, refMod.modFile.GetModAssembly());
					refs.Add(path);

					foreach (var refDll in refMod.properties.dllReferences) {
						path = Path.Combine(tempDir, refDll + ".dll");
						File.WriteAllBytes(path, refMod.modFile.GetBytes("lib/" + refDll + ".dll"));
						refs.Add(path);
					}
				}
			}

			var files = Directory.GetFiles(mod.path, "*.cs", SearchOption.AllDirectories).Where(file => !IgnoreCompletely(mod, file)).ToArray();

			bool allowUnsafe =
				Program.LaunchParameters.TryGetValue("-unsafe", out var unsafeParam) &&
				bool.TryParse(unsafeParam, out var _allowUnsafe) && _allowUnsafe;

			var preprocessorSymbols = new List<string> { "FNA" };
			if (Program.LaunchParameters.TryGetValue("-define", out var defineParam))
				preprocessorSymbols.AddRange(defineParam.Split(';', ' '));

			var results = RoslynCompile(mod.Name, outputPath, refs.ToArray(), files, preprocessorSymbols.ToArray(), mod.properties.includePDB, allowUnsafe);

			int numWarnings = results.Count(e => e.Severity == DiagnosticSeverity.Warning);
			int numErrors = results.Length - numWarnings;
			status.LogCompilerLine(Language.GetTextValue("tModLoader.CompilationResult", numErrors, numWarnings), Level.Info);
			foreach (var line in results)
				status.LogCompilerLine(line.ToString(), line.Severity == DiagnosticSeverity.Warning ? Level.Warn : Level.Error);

			if (numErrors > 0) {
				var firstError = results.First(e => e.Severity == DiagnosticSeverity.Error);
				throw new BuildException(Language.GetTextValue("tModLoader.CompileError", Path.GetFileName(outputPath), numErrors, numWarnings) + $"\nError: {firstError}");
			}
		}

		private string DllRefPath(BuildingMod mod, string dllName)
		{
			string path = Path.Combine(mod.path, "lib", dllName) + ".dll";

			if (File.Exists(path))
				return path;

			if (Program.LaunchParameters.TryGetValue("-eac", out var eacPath)) {
				var outputCopiedPath = Path.Combine(Path.GetDirectoryName(eacPath), dllName + ".dll");

				if (File.Exists(outputCopiedPath))
					return outputCopiedPath;
			}

			throw new BuildException("Missing dll reference: " + path);
		}

		private static IList<string> GetTerrariaReferences() {
			var executingAssembly = Assembly.GetExecutingAssembly();

			var refs = new List<string> {
				executingAssembly.Location
			};

			// avoid a double extract of the embedded dlls
			if (referencesUpdated) {
				refs.AddRange(Directory.GetFiles(modReferencesPath, "*.dll"));
				return refs;
			}

			// extract embedded resource dlls to the references path rather than the tempDir
			foreach (string resName in executingAssembly.GetManifestResourceNames().Where(n => n.EndsWith(".dll"))) {
				string path = Path.Combine(modReferencesPath, Path.GetFileName(resName));

				using (Stream res = executingAssembly.GetManifestResourceStream(resName), file = File.Create(path))
					res.CopyTo(file);

				refs.Add(path);
			}

			return refs;
		}

		/// <summary>
		/// Invoke the Roslyn compiler via reflection to avoid a .NET 4.6 dependency
		/// </summary>
		private static Diagnostic[] RoslynCompile(string name, string outputPath, string[] references, string[] files, string[] preprocessorSymbols, bool includePdb, bool allowUnsafe)
		{
			var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
				assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default,
				optimizationLevel: preprocessorSymbols.Contains("DEBUG") ? OptimizationLevel.Debug : OptimizationLevel.Release,
				allowUnsafe: allowUnsafe);

			var parseOptions = new CSharpParseOptions(LanguageVersion.Preview, preprocessorSymbols: preprocessorSymbols);

			var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);

			var refs = references.Select(s => MetadataReference.CreateFromFile(s));
			var src = files.Select(f => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(f), parseOptions, f, Encoding.UTF8));
			var comp = CSharpCompilation.Create(name, src, refs, options);

			using var peStream = File.OpenWrite(outputPath);
			using var pdbStream = includePdb ? File.OpenWrite(Path.ChangeExtension(outputPath, "pdb")) : null;
			var results = comp.Emit(peStream, pdbStream, options: emitOptions);

			return results.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning).ToArray();
			/*errors.Add(new CompilerError {
				ErrorNumber = d.Id,
				IsWarning = d.Severity == DiagnosticSeverity.Warning,
				ErrorText = d.GetMessage(),
				FileName = loc.Path ?? "",
				Line = loc.StartLinePosition.Line + 1,
				Column = loc.StartLinePosition.Character
			});*/
		}

		private static FileStream AcquireConsoleBuildLock()
		{
			var path = Path.Combine(modReferencesPath, "buildlock");
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
#endif
