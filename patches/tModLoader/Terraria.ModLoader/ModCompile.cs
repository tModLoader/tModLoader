using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Mono.Cecil;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModLoader;
using System.Runtime.ExceptionServices;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	//todo: further documentation
	internal class ModCompile
	{
		public interface IBuildStatus
		{
			void SetProgress(int i, int n);
			void SetStatus(string msg);
			void LogError(string mod, string msg, Exception e = null);
			void LogCompileErrors(string mod, CompilerErrorCollection errors, string hint);
		}

		private class ConsoleBuildStatus : IBuildStatus
		{
			public void SetProgress(int i, int n) {}

			public void SetMod(string name) {}

			public void SetStatus(string msg)
			{
				Console.WriteLine(msg);
			}
			
			public void LogError(string mod, string msg, Exception e = null)
			{
				Console.WriteLine(msg);
				if (e != null)
					Console.WriteLine(e);
			}

			public void LogCompileErrors(string mod, CompilerErrorCollection errors, string hint)
			{
				if (hint != null)
					Console.WriteLine(hint);
				
				foreach (CompilerError error in errors)
					Console.WriteLine(error);
			}
		}

		private class BuildingMod : LocalMod
		{
			public string path;

			public BuildingMod(TmodFile modFile, BuildProperties properties, string path) : base(modFile, properties)
			{
				this.path = path;
			}
		}

		private static readonly string modCompileDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ModCompile");
		internal static IList<string> sourceExtensions = new List<string> { ".csproj", ".cs", ".sln" };
		private static IList<string> moduleReferences;

		internal static void LoadReferences()
		{
			if (moduleReferences != null)
				return;
			moduleReferences = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
				.Select(refName => Assembly.Load(refName).Location)
				.Where(loc => loc != "").ToList();
		}

		private IBuildStatus status;
		public ModCompile(IBuildStatus status)
		{
			this.status = status;
		}

		internal bool BuildAll(string[] modFolders)
		{
			var modList = new List<LocalMod>();
			//read mod sources folder
			foreach (var modFolder in modFolders)
			{
				var mod = ReadProperties(modFolder);
				if (mod == null)
					return false;

				modList.Add(mod);
			}

			//figure out which of the installed mods are required for building
			var installedMods = ModOrganizer.FindMods().Where(mod => !modList.Exists(m => m.Name == mod.Name)).ToList();

			var requiredFromInstall = new HashSet<LocalMod>();
			void Require(LocalMod mod)
			{
				foreach (var dep in mod.properties.RefNames(true))
				{
					var depMod = installedMods.SingleOrDefault(m => m.Name == dep);
					if (depMod != null && requiredFromInstall.Add(depMod))
						Require(depMod);
				}
			}

			foreach (var mod in modList)
				Require(mod);

			modList.AddRange(requiredFromInstall);

			//sort and version check
			List<BuildingMod> modsToBuild;
			try
			{
				ModOrganizer.EnsureDependenciesExist(modList, true);
				ModOrganizer.EnsureTargetVersionsMet(modList);
				var sortedModList = ModOrganizer.Sort(modList);
				modsToBuild = sortedModList.OfType<BuildingMod>().ToList();
			}
			catch (ModSortingException e)
			{
				status.LogError(null, e.Message);
				return false;
			}

			//build
			int num = 0;
			foreach (var mod in modsToBuild)
			{
				status.SetProgress(num++, modsToBuild.Count);
				if (!Build(mod))
					return false;
			}

			return true;
		}

		internal static void BuildModCommandLine(string modFolder)
		{
			// Once we get to this point, the application is guaranteed to exit
			var lockFile = AcquireConsoleBuildLock();
			try
			{
				var modCompile = new ModCompile(new ConsoleBuildStatus());
				if (!modCompile.Build(modFolder))
					Environment.Exit(1);

			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Environment.Exit(1);
			}
			finally
			{
				lockFile.Close();
			}
			// Mod was built with success, exit code 0 indicates success.
			Environment.Exit(0);
		}

		internal bool Build(string modFolder)
		{
			var mod = ReadProperties(modFolder);
			return mod != null && Build(mod);
		}

		private BuildingMod ReadProperties(string modFolder)
		{
			if (modFolder.EndsWith("\\") || modFolder.EndsWith("/")) modFolder = modFolder.Substring(0, modFolder.Length - 1);
			var modName = Path.GetFileName(modFolder);
			status.SetStatus(Language.GetTextValue("tModLoader.ReadingProperties", modName));

			BuildProperties properties;
			try
			{
				properties = BuildProperties.ReadBuildFile(modFolder);
			}
			catch (Exception e)
			{
				status.LogError(modName, Language.GetTextValue("tModLoader.BuildErrorFailedLoadBuildTxt", Path.Combine(modFolder, "build.txt")), e);
				return null;
			}

			var file = Path.Combine(ModPath, modName + ".tmod");
			var modFile = new TmodFile(file)
			{
				name = modName,
				version = properties.version
			};
			return new BuildingMod(modFile, properties, modFolder);
		}

		private static byte[] ReadIfExists(string path)
		{
			return File.Exists(path) ? File.ReadAllBytes(path) : null;
		}

		private bool Build(BuildingMod mod)
		{
			status.SetStatus(Language.GetTextValue("tModLoader.Building", mod.Name));
			byte[] winDLL = null;
			byte[] monoDLL = null;
			byte[] winPDB = null;
			if (mod.properties.noCompile)
			{
				winDLL = monoDLL = ReadIfExists(Path.Combine(mod.path, "All.dll"));
				winPDB = ReadIfExists(Path.Combine(mod.path, "All.pdb"));

				if (winDLL == null)
				{
					winDLL = ReadIfExists(Path.Combine(mod.path, "Windows.dll"));
					monoDLL = ReadIfExists(Path.Combine(mod.path, "Mono.dll"));
					winPDB = ReadIfExists(Path.Combine(mod.path, "Windows.pdb"));
				}

				if (winDLL == null || monoDLL == null)
				{
					var missing = new List<string> {"All.dll"};
					if (winDLL == null) missing.Add("Windows.dll");
					if (monoDLL == null) missing.Add("Mono.dll");
					status.LogError(mod.Name, Language.GetTextValue("tModLoader.BuildErrorMissingDllFiles", string.Join(", ", missing)));
					return false;
				}
			}
			else
			{
				List<LocalMod> refMods;
				try 
				{
					refMods = FindReferencedMods(mod.properties);
				}
				catch (Exception e)
				{
					status.LogError(mod.Name, e.Message, e.InnerException);
					return false;
				}

				if (Program.LaunchParameters.ContainsKey("-eac"))
				{
					if (!windows)
					{
						status.LogError(mod.Name, Language.GetTextValue("tModLoader.BuildErrorEACWindowsOnly"));
						return false;
					}

					var winPath = Program.LaunchParameters["-eac"];
					try
					{
						status.SetStatus(Language.GetTextValue("tModLoader.LoadingEAC"));
						var pdbPath = Path.ChangeExtension(winPath, "pdb");
						winDLL = File.ReadAllBytes(winPath);
						winPDB = File.ReadAllBytes(pdbPath);
						mod.properties.editAndContinue = true;
					}
					catch (Exception e)
					{
						status.LogError(mod.Name, Language.GetTextValue("tModLoader.BuildErrorEACLoadFailed", winPath + "/.pdb"), e);
						return false;
					}
				}
				else
				{
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

			if (Equal(winDLL, monoDLL))
			{
				mod.modFile.AddFile("All.dll", winDLL);
				if (winPDB != null) mod.modFile.AddFile("All.pdb", winPDB);
			}
			else
			{
				mod.modFile.AddFile("Windows.dll", winDLL);
				mod.modFile.AddFile("Mono.dll", monoDLL);
				if (winPDB != null) mod.modFile.AddFile("Windows.pdb", winPDB);
			}

			foreach (var resource in Directory.GetFiles(mod.path, "*", SearchOption.AllDirectories))
			{
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

			mod.modFile.Save();
			EnableMod(mod.Name);
			SetModderMode();
			return true;
		}

		private static void AddResource(TmodFile modFile, string relPath, string filePath)
		{
			if (relPath.EndsWith(".png") && relPath != "icon.png")
			{
				using (var fs = File.OpenRead(filePath))
				{
					var rawimg = ImageIO.ToRawBytes(fs);
					if (rawimg != null)
					{//some pngs can't be converted to rawimg
						modFile.AddFile(Path.ChangeExtension(relPath, "rawimg"), rawimg);
						return;
					}
				}
			}

			modFile.AddFile(relPath, File.ReadAllBytes(filePath));
		}

		private bool VerifyName(string modName, byte[] dll)
		{
			var asmDef = AssemblyDefinition.ReadAssembly(new MemoryStream(dll));
			var asmName = asmDef.Name.Name;
			if (asmName != modName)
			{
				status.LogError(modName, Language.GetTextValue("tModLoader.BuildErrorModNameDoesntMatchAssemblyName", modName, asmName));
				return false;
			}

			if (modName.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase))
			{
				status.LogError(modName, Language.GetTextValue("tModLoader.BuildErrorModNamedTerraria"));
				return false;
			}

			// Verify that folder and namespace match up
			try
			{
				var modClassType = asmDef.MainModule.Types.Single(x => x.BaseType?.FullName == "Terraria.ModLoader.Mod");
				string topNamespace = modClassType.Namespace.Split('.')[0];
				if (topNamespace != modName)
				{
					status.LogError(modName, Language.GetTextValue("tModLoader.BuildErrorNamespaceFolderDontMatch"));
					return false;
				}
			}
			catch
			{
				status.LogError(modName, Language.GetTextValue("tModLoader.BuildErrorNoModClass"));
				return false;
			}

			return true;
		}

		private static bool Equal(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return false;

			return true;
		}

		private List<LocalMod> FindReferencedMods(BuildProperties properties)
		{
			var mods = new Dictionary<string, LocalMod>();
			FindReferencedMods(properties, mods);
			return mods.Values.ToList();
		}

		private void FindReferencedMods(BuildProperties properties, Dictionary<string, LocalMod> mods)
		{
			foreach (var refName in properties.RefNames(true))
			{
				if (mods.ContainsKey(refName))
					continue;

				var modFile = new TmodFile(Path.Combine(ModPath, refName + ".tmod"));
				try
				{
					modFile.Read(TmodFile.LoadedState.Code);
				}
				catch (Exception ex)
				{
					throw new Exception(Language.GetTextValue("tModLoader.BuildErrorModReference", refName), ex);
				}
				var mod = new LocalMod(modFile);
				mods[refName] = mod;
				FindReferencedMods(mod.properties, mods);
			}
		}

		private void CompileMod(BuildingMod mod, List<LocalMod> refMods, bool forWindows,
				ref byte[] dll, ref byte[] pdb)
		{
			LoadReferences();
			bool generatePDB = mod.properties.includePDB && forWindows;
			if (generatePDB && !windows)
			{
				Console.WriteLine(Language.GetTextValue("tModLoader.BuildErrorPDBWindowsOnly"));
				generatePDB = false;
			}

			//collect all dll references
			var tempDir = Path.Combine(ModPath, "compile_temp");
			Directory.CreateDirectory(tempDir);
			var refs = new List<string>();

			//everything used to compile the tModLoader for the target platform
			refs.AddRange(GetTerrariaReferences(tempDir, forWindows));

			//libs added by the mod
			refs.AddRange(mod.properties.dllReferences.Select(refDll => Path.Combine(mod.path, "lib/" + refDll + ".dll")));

			//all dlls included in all referenced mods
			foreach (var refMod in refMods)
			{
				var path = Path.Combine(tempDir, refMod + ".dll");
				File.WriteAllBytes(path, refMod.modFile.GetMainAssembly(forWindows));
				refs.Add(path);

				foreach (var refDll in refMod.properties.dllReferences)
				{
					path = Path.Combine(tempDir, refDll + ".dll");
					File.WriteAllBytes(path, refMod.modFile.GetFile("lib/" + refDll + ".dll"));
					refs.Add(path);
				}
			}

			var compileOptions = new CompilerParameters
			{
				OutputAssembly = Path.Combine(tempDir, mod + ".dll"),
				GenerateExecutable = false,
				GenerateInMemory = false,
				TempFiles = new TempFileCollection(tempDir, true),
				IncludeDebugInformation = generatePDB,
				CompilerOptions = "/optimize"
			};

			compileOptions.ReferencedAssemblies.AddRange(refs.ToArray());
			var files = Directory.GetFiles(mod.path, "*.cs", SearchOption.AllDirectories).Where(file => !mod.properties.ignoreFile(file.Substring(mod.path.Length + 1))).ToArray();

			try
			{
				CompilerResults results;
				if (mod.properties.languageVersion == 6)
				{
					if (Environment.Version.Revision < 10000)
					{
						status.LogError(mod.Name, Language.GetTextValue("tModLoader.BuildErrorDotNet45forCS6"));
						return;
					}

					results = RoslynCompile(compileOptions, files);
				}
				else
				{
					var options = new Dictionary<string, string> { { "CompilerVersion", "v" + mod.properties.languageVersion + ".0" } };
					results = new CSharpCodeProvider(options).CompileAssemblyFromFile(compileOptions, files);
				}

				if (results.Errors.HasErrors)
				{
					status.LogCompileErrors(mod.Name + (forWindows ? "/Windows.dll" : "/Mono.dll"), results.Errors,
						forWindows != windows ? Language.GetTextValue("tModLoader.BuildErrorModCompileFolderHint") : null);
					return;
				}

				dll = File.ReadAllBytes(compileOptions.OutputAssembly);
				dll = PostProcess(dll, forWindows);
				if (generatePDB)
					pdb = File.ReadAllBytes(Path.Combine(tempDir, mod + ".pdb"));
			}
			finally
			{
				int numTries = 10;
				while (numTries > 0)
				{
					try
					{
						Directory.Delete(tempDir, true);
						numTries = 0;
					}
					catch
					{
						System.Threading.Thread.Sleep(1);
						numTries--;
					}
				}
			}
		}

		private static IEnumerable<string> GetTerrariaReferences(string tempDir, bool forWindows)
		{
			LoadReferences();
			var refs = new List<string>(moduleReferences);

			if (forWindows == windows)
			{
				var terrariaModule = Assembly.GetExecutingAssembly();
				refs.Add(terrariaModule.Location);

				//extract embedded resource dlls
				foreach (var resName in terrariaModule.GetManifestResourceNames().Where(n => n.EndsWith(".dll")))
				{
					var path = Path.Combine(tempDir, Path.GetFileName(resName));
					using (Stream res = terrariaModule.GetManifestResourceStream(resName), file = File.Create(path))
						res.CopyTo(file);

					refs.Add(path);
				}

			}
			else
			{
				//remove framework references
				refs.RemoveAll(path =>
				{
					var name = Path.GetFileName(path);
					return name == "FNA.dll" || name.StartsWith("Microsoft.Xna.Framework");
				});
				//replace with ModCompile contents
				var mainModulePath = Path.Combine(modCompileDir, forWindows ? "tModLoaderWindows.exe" : "tModLoaderMac.exe");
				refs.Add(mainModulePath);

				var frameworkNames = forWindows ? new[] {
						"Microsoft.Xna.Framework.dll",
						"Microsoft.Xna.Framework.Game.dll",
						"Microsoft.Xna.Framework.Graphics.dll",
						"Microsoft.Xna.Framework.Xact.dll"
					} : new[] {
						"FNA.dll"
					};
				refs.AddRange(frameworkNames.Select(f => Path.Combine(modCompileDir, f)));

				//extract embedded resource dlls
				var asm = AssemblyDefinition.ReadAssembly(mainModulePath);
				foreach (var res in asm.MainModule.Resources.OfType<EmbeddedResource>().Where(res => res.Name.EndsWith(".dll")))
				{
					var path = Path.Combine(tempDir, Path.GetFileName(res.Name));
					using (Stream s = res.GetResourceStream(), file = File.Create(path))
						s.CopyTo(file);

					refs.Add(path);
				}
			}

			return refs;
		}

		/// <summary>
		/// Invoke the Roslyn compiler via reflection to avoid a .NET 4.5 dependency
		/// </summary>
		private static CompilerResults RoslynCompile(CompilerParameters compileOptions, string[] files)
		{
			var terrariaDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var modCompileDir = Path.Combine(terrariaDir, "ModCompile");
			var asm = Assembly.LoadFile(Path.Combine(modCompileDir, "RoslynWrapper.dll"));

			AppDomain.CurrentDomain.AssemblyResolve += (o, args) =>
			{
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

		private static FileStream AcquireConsoleBuildLock()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/ModCompile/buildlock";
			bool first = true;
			while (true)
			{
				try
				{
					return new FileStream(path, FileMode.OpenOrCreate);
				}
				catch (IOException)
				{
					if (first)
					{
						Console.WriteLine("Waiting for other builds to complete");
						first = false;
					}
				}
			}
		}

		private static AssemblyNameReference GetOrAddSystemCore(ModuleDefinition module)
		{
			var assemblyRef = module.AssemblyReferences.SingleOrDefault(r => r.Name == "System.Core");
			if (assemblyRef == null)
			{
				//System.Linq.Enumerable is in System.Core
				var name = Assembly.GetAssembly(typeof(Enumerable)).GetName();
				assemblyRef = new AssemblyNameReference(name.Name, name.Version)
				{
					Culture = name.CultureInfo.Name,
					PublicKeyToken = name.GetPublicKeyToken(),
					HashAlgorithm = (AssemblyHashAlgorithm)name.HashAlgorithm
				};
				module.AssemblyReferences.Add(assemblyRef);
			}
			return assemblyRef;
		}

		private static byte[] PostProcess(byte[] dll, bool forWindows)
		{
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
