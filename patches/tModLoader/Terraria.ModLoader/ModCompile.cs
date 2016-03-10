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
                var sortedModList = TopoSort(modList);
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
            try {
                if (!Build(modFolder, new ConsoleBuildStatus()))
                    Environment.ExitCode = 1;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                Environment.ExitCode = 1;
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
            byte[] monoPDB = null;
            if (mod.properties.noCompile) {
                winDLL = monoDLL = ReadIfExists(Path.Combine(mod.path, "All.dll"));
                winPDB = monoPDB = ReadIfExists(Path.Combine(mod.path, "All.pdb"));

                if (winDLL == null) {
                    winDLL = ReadIfExists(Path.Combine(mod.path, "Windows.dll"));
                    monoDLL = ReadIfExists(Path.Combine(mod.path, "Mono.dll"));
                    winPDB = ReadIfExists(Path.Combine(mod.path, "Windows.pdb"));
                    monoPDB = ReadIfExists(Path.Combine(mod.path, "Mono.pdb"));
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
                    try {
                        status.SetStatus("Loading pre-compiled Windows.dll with edit and continue support");
                        var winPath = Program.LaunchParameters["-eac"];
                        var pdbPath = Path.ChangeExtension(winPath, "pdb");
                        winDLL = File.ReadAllBytes(winPath);
                        winPDB = File.ReadAllBytes(pdbPath);
                        mod.properties.editAndContinue = true;
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failed to load pre-compiled edit and continue dll");
                        Console.WriteLine(e);
                        return false;
                    }
                }
                else {
                    status.SetStatus("Compiling " + mod.Name + " for Windows...");
                    status.SetProgress(0, 2);
                    CompileMod(mod, refMods, true, ref winDLL, ref winPDB);
                }

                status.SetStatus("Compiling " + mod.Name + " for Mono...");
                status.SetProgress(1, 2);
                CompileMod(mod, refMods, false, ref monoDLL, ref monoPDB);
                if (winDLL == null || monoDLL == null)
                    return false;
            }

            if (!VerifyName(mod.Name, winDLL) || !VerifyName(mod.Name, monoDLL))
                return false;

            status.SetStatus("Building "+mod.Name+"...");
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
                if (monoPDB != null) mod.modFile.AddFile("Mono.pdb", monoPDB);
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
            foreach (var refName in properties.modReferences) {
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

            var refs = new List<string>(terrariaReferences);
            if (forWindows == windows) {
                refs.Add(terrariaModule.Location);
            }
            else {
                refs = refs.Where(path => {
                    var name = Path.GetFileName(path);
                    return name != "FNA.dll" && !name.StartsWith("Microsoft.Xna.Framework");
                }).ToList();
                var terrariaDir = Path.GetDirectoryName(terrariaModule.Location);
                if (forWindows) {
                    refs.Add(Path.Combine(terrariaDir, "TerrariaWindows.exe"));
                    var xna = new[] {
                        "Microsoft.Xna.Framework.dll",
                        "Microsoft.Xna.Framework.Game.dll",
                        "Microsoft.Xna.Framework.Graphics.dll",
                        "Microsoft.Xna.Framework.Xact.dll"
                    };
                    refs.AddRange(xna.Select(f => Path.Combine(terrariaDir, f)));
                }
                else {
                    refs.Add(Path.Combine(terrariaDir, "TerrariaMac.exe"));
                    refs.Add(Path.Combine(terrariaDir, "FNA.dll"));
                }
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
                var path = Path.Combine(tempDir, refMod.Name + ".dll");
                File.WriteAllBytes(path, refMod.modFile.GetMainAssembly(forWindows));
                refs.Add(path);

                foreach (var refDll in refMod.properties.dllReferences) {
                    path = Path.Combine(tempDir, refDll + ".dll");
                    File.WriteAllBytes(path, refMod.modFile.GetFile("lib/"+refDll+".dll"));
                    refs.Add(path);
                }
            }

            var compileOptions = new CompilerParameters {
                OutputAssembly = Path.Combine(tempDir, mod.Name + ".dll"),
                GenerateExecutable = false,
                GenerateInMemory = false,
                TempFiles = new TempFileCollection(tempDir, true),
                IncludeDebugInformation = mod.properties.includePDB
            };

            compileOptions.ReferencedAssemblies.AddRange(refs.ToArray());
            var options = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            var codeProvider = new CSharpCodeProvider(options);
            var results = codeProvider.CompileAssemblyFromFile(compileOptions, Directory.GetFiles(mod.path, "*.cs", SearchOption.AllDirectories));
            var errors = results.Errors;

            if (errors.HasErrors) {
                ErrorLogger.LogCompileErrors(errors);
            }
            else {
                dll = File.ReadAllBytes(compileOptions.OutputAssembly);
                if (mod.properties.includePDB)
                    pdb = File.ReadAllBytes(Path.Combine(tempDir, mod.Name + ".pdb"));
            }

            Directory.Delete(tempDir, true);
        }
    }
}
