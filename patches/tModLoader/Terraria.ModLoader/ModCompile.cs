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
namespace Terraria.ModLoader
{
	//todo: further documentation
    internal class ModCompile
    {
        public interface IBuildStatus
        {
            void SetProgress(int i, int n);
            void SetStatus(string msg);
        }

        private class ConsoleBuildStatus : IBuildStatus
        {
            public void SetProgress(int i, int n) {
            }
            public void SetStatus(string msg) {
                Console.WriteLine(msg);
            }
        }

        private class BuildingMod : LoadingMod
        {
            public string path;

            public BuildingMod(TmodFile modFile, BuildProperties properties, string path) : base(modFile, properties) {
                this.path = path;
            }
        }

        private static readonly string modCompileDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ModCompile");
        private static IList<string> terrariaReferences;

        internal static void LoadReferences() {
            if (terrariaReferences != null)
                return;
            terrariaReferences = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .Select(refName => Assembly.Load(refName).Location)
                .Where(loc => loc != "").ToList();
        }

        internal static bool BuildAll(string[] modFolders, IBuildStatus status) {
            var modList = new List<LoadingMod>();
            foreach (var modFolder in modFolders) {
                var mod = ReadProperties(modFolder, status);
                if (mod == null)
                    return false;

                modList.Add(mod);
            }

            foreach (var modFile in FindMods()) {
                if (modList.Exists(m => m.Name == modFile.name))
                    continue;

                modList.Add(new LoadingMod(modFile, BuildProperties.ReadModFile(modFile)));
            }

            List<BuildingMod> modsToBuild;
            try {
                EnsureDependenciesExist(modList, true);
                EnsureTargetVersionsMet(modList);
                var sortedModList = Sort(modList);
                modsToBuild = sortedModList.OfType<BuildingMod>().ToList();
            }
            catch (ModSortingException e) {
                ErrorLogger.LogDependencyError(e.Message);
                return false;
            }

            int num = 0;
            foreach (var mod in modsToBuild) {
                status.SetProgress(num++, modsToBuild.Count);
                if (!Build(mod, status))
                    return false;
            }

            return true;
        }

        internal static void BuildModCommandLine(string modFolder) {
            var lockFile = AcquireConsoleBuildLock();
            try
            {
                if (!Build(modFolder, new ConsoleBuildStatus()))
                    Environment.ExitCode = 1;

            }
            catch (Exception e) {
                Console.WriteLine(e);
                Environment.ExitCode = 1;
            }
            finally
            {
                lockFile.Close();
            }
        }

        internal static bool Build(string modFolder, IBuildStatus status) {
            var mod = ReadProperties(modFolder, status);
            return mod != null && Build(mod, status);
        }

        private static BuildingMod ReadProperties(string modFolder, IBuildStatus status) {
            if (modFolder.EndsWith("\\") || modFolder.EndsWith("/")) modFolder = modFolder.Substring(0, modFolder.Length - 1);
            var modName = Path.GetFileName(modFolder);
            status.SetStatus("Reading Properties: " + modName);

            BuildProperties properties;
            try {
                properties = BuildProperties.ReadBuildFile(modFolder);
            }
            catch (Exception e) {
                ErrorLogger.LogBuildError("Failed to load " + Path.Combine(modFolder, "build.txt") + Environment.NewLine + e);
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

        private static bool Build(BuildingMod mod, IBuildStatus status) {
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
                    ErrorLogger.LogDllBuildError(mod.path);
                    return false;
                }
            }
            else {
                var refMods = FindReferencedMods(mod.properties);
                if (refMods == null)
                    return false;

                if (Program.LaunchParameters.ContainsKey("-eac")) {
                    if (!windows) {
                        ErrorLogger.LogBuildError("Edit and continue is only supported on windows");
                        return false;
                    }

                    try {
                        status.SetStatus("Loading pre-compiled Windows.dll with edit and continue support");
                        var winPath = Program.LaunchParameters["-eac"];
                        var pdbPath = Path.ChangeExtension(winPath, "pdb");
                        winDLL = File.ReadAllBytes(winPath);
                        winPDB = File.ReadAllBytes(pdbPath);
                        mod.properties.editAndContinue = true;
                    }
                    catch (Exception e) {
                        ErrorLogger.LogBuildError("Failed to load pre-compiled edit and continue dll "+e);
                        return false;
                    }
                }
                else {
                    status.SetStatus("Compiling " + mod + " for Windows...");
                    status.SetProgress(0, 2);
                    CompileMod(mod, refMods, true, ref winDLL, ref winPDB);
                }
                if (winDLL == null)
                    return false;

                status.SetStatus("Compiling " + mod + " for Mono...");
                status.SetProgress(1, 2);
                CompileMod(mod, refMods, false, ref monoDLL, ref winPDB);//the pdb reference won't actually be written to
                if (monoDLL == null)
                    return false;
            }

            if (!VerifyName(mod.Name, winDLL) || !VerifyName(mod.Name, monoDLL))
                return false;

            status.SetStatus("Building "+mod+"...");
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
                        !mod.properties.includeSource && Path.GetExtension(resource) == ".cs" ||
                        Path.GetFileName(resource) == "Thumbs.db")
                    continue;

                mod.modFile.AddFile(relPath, File.ReadAllBytes(resource));
            }

            WAVCacheIO.ClearCache(mod.Name);

            mod.modFile.Save();
            EnableMod(mod.modFile);
            return true;
        }

        private static bool VerifyName(string modName, byte[] dll) {
            var asmName = AssemblyDefinition.ReadAssembly(new MemoryStream(dll)).Name.Name;
            if (asmName != modName) {
                ErrorLogger.LogBuildError("Mod name \""+ modName+ "\" does not match assembly name \""+asmName+"\"");
                return false;
            }

            if (modName.Equals("Terraria",  StringComparison.InvariantCultureIgnoreCase)) {
                ErrorLogger.LogBuildError("Mods cannot be named Terraria");
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

        internal static List<LoadingMod> FindReferencedMods(BuildProperties properties) {
            var mods = new Dictionary<string, LoadingMod>();
            return FindReferencedMods(properties, mods) ? mods.Values.ToList() : null;
        }

        private static bool FindReferencedMods(BuildProperties properties, Dictionary<string, LoadingMod> mods) {
            foreach (var refName in properties.RefNames(true)) {
                if (mods.ContainsKey(refName))
                    continue;

                var modFile = new TmodFile(Path.Combine(ModPath, refName + ".tmod"));
                modFile.Read();
                var ex = modFile.ValidMod();
                if (ex != null) {
                    ErrorLogger.LogBuildError("Mod reference " + refName + " " + ex);
                    return false;
                }
                var mod = new LoadingMod(modFile, BuildProperties.ReadModFile(modFile));
                mods[refName] = mod;
                FindReferencedMods(mod.properties, mods);
            }

            return true;
        }

        private static void CompileMod(BuildingMod mod, List<LoadingMod> refMods, bool forWindows,
                ref byte[] dll, ref byte[] pdb) {
            LoadReferences();
            var terrariaModule = Assembly.GetExecutingAssembly();
            bool generatePDB = mod.properties.includePDB && forWindows;
            if (generatePDB && !windows) {
                Console.WriteLine("PDB files can only be generated for windows, on windows.");
                generatePDB = false;
            }

            var refs = new List<string>(terrariaReferences);
            if (forWindows == windows) {
                refs.Add(terrariaModule.Location);
            }
            else {
                refs = refs.Where(path => {
                    var name = Path.GetFileName(path);
                    return name != "FNA.dll" && !name.StartsWith("Microsoft.Xna.Framework");
                }).ToList();
                var names = forWindows
                    ? new[] {
                        "tModLoaderWindows.exe",
                        "Microsoft.Xna.Framework.dll",
                        "Microsoft.Xna.Framework.Game.dll",
                        "Microsoft.Xna.Framework.Graphics.dll",
                        "Microsoft.Xna.Framework.Xact.dll"
                    }
                    : new[] {
                        "tModLoaderMac.exe",
                        "FNA.dll"
                    };
                refs.AddRange(names.Select(f => Path.Combine(modCompileDir, f)));
            }

            refs.AddRange(mod.properties.dllReferences.Select(refDll => Path.Combine(mod.path, "lib/" + refDll + ".dll")));

            var tempDir = Path.Combine(ModPath, "compile_temp");
            Directory.CreateDirectory(tempDir);

            foreach (var resName in terrariaModule.GetManifestResourceNames().Where(n => n.EndsWith(".dll"))) {
                var path = Path.Combine(tempDir, Path.GetFileName(resName));
                using (Stream res = terrariaModule.GetManifestResourceStream(resName), file = File.Create(path))
                    res.CopyTo(file);

                refs.Add(path);
            }

            foreach (var refMod in refMods) {
                var path = Path.Combine(tempDir, refMod + ".dll");
                File.WriteAllBytes(path, refMod.modFile.GetMainAssembly(forWindows));
                refs.Add(path);

                foreach (var refDll in refMod.properties.dllReferences) {
                    path = Path.Combine(tempDir, refDll + ".dll");
                    File.WriteAllBytes(path, refMod.modFile.GetFile("lib/"+refDll+".dll"));
                    refs.Add(path);
                }
            }

            var compileOptions = new CompilerParameters {
                OutputAssembly = Path.Combine(tempDir, mod + ".dll"),
                GenerateExecutable = false,
                GenerateInMemory = false,
                TempFiles = new TempFileCollection(tempDir, true),
                IncludeDebugInformation = generatePDB
            };

            compileOptions.ReferencedAssemblies.AddRange(refs.ToArray());
            var files = Directory.GetFiles(mod.path, "*.cs", SearchOption.AllDirectories).Where(file => !mod.properties.ignoreFile(file.Substring(mod.path.Length + 1))).ToArray();

            try {
                CompilerResults results;
                if (mod.properties.languageVersion == 6) {
                    if (Environment.Version.Revision < 10000) {
                        ErrorLogger.LogBuildError(".NET Framework 4.5 must be installed to compile C# 6.0");
                        return;
                    }

                    results = RoslynCompile(compileOptions, files);
                }
                else {
                    var options = new Dictionary<string, string> { { "CompilerVersion", "v" + mod.properties.languageVersion + ".0" } };
                    results = new CSharpCodeProvider(options).CompileAssemblyFromFile(compileOptions, files);
                }

                if (results.Errors.HasErrors) {
                    ErrorLogger.LogCompileErrors(results.Errors, forWindows);
                    return;
                }

                dll = File.ReadAllBytes(compileOptions.OutputAssembly);
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

        /// <summary>
        /// Invoke the Roslyn compiler via reflection to avoid a .NET 4.5 dependency
        /// </summary>
        private static CompilerResults RoslynCompile(CompilerParameters compileOptions, string[] files) {
            var terrariaDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var modCompileDir = Path.Combine(terrariaDir, "ModCompile");
            var asm = Assembly.LoadFile(Path.Combine(modCompileDir, "RoslynWrapper.dll"));

            AppDomain.CurrentDomain.AssemblyResolve += (o, args) => {
                var name = new AssemblyName(args.Name).Name;
                var f = Path.Combine(modCompileDir, name + ".dll");
                return File.Exists(f) ? Assembly.LoadFile(f) : null;
            };

            var res = (CompilerResults) asm.GetType("Terraria.ModLoader.RoslynWrapper").GetMethod("Compile")
                .Invoke(null, new object[] {compileOptions, files});

            if (!res.Errors.HasErrors && compileOptions.IncludeDebugInformation)
                asm.GetType("Terraria.ModLoader.RoslynPdbFixer").GetMethod("Fix")
                    .Invoke(null, new object[] { compileOptions.OutputAssembly });
            
            return res;
        }

        private static FileStream AcquireConsoleBuildLock() {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/ModCompile/buildlock";
            bool first = true;
            while (true) {
                try
                {
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
