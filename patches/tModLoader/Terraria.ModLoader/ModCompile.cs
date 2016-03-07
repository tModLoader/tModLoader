using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
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

        private static IList<string> terrariaReferences;

        internal static void LoadReferences() {
            if (terrariaReferences != null)
                return;
            terrariaReferences = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .Select(refName => Assembly.Load(refName).Location)
                .Where(loc => loc != "").ToList();
        }

        internal static bool BuildAll(string[] mods, IBuildStatus status) {
            int num = 0;
            bool success = true;
            foreach (string modFolder in mods) {
                status.SetProgress(num, mods.Length);
                success &= Build(modFolder, status);
                num++;
            }
            return success;
        }

        internal static void BuildModCommandLine(string mod) {
            try {
                Build(mod, new ConsoleBuildStatus());
            }
            catch (Exception e) {
                Console.WriteLine(e);
                Environment.ExitCode = 1;
            }
        }

        internal static bool Build(string mod, IBuildStatus status) {
            if (mod.EndsWith("\\") || mod.EndsWith("/")) mod = mod.Substring(0, mod.Length - 1);
            var modName = Path.GetFileName(mod);
            status.SetStatus("Reading Properties: " + modName);
            var properties = BuildProperties.ReadBuildFile(mod);
            
            byte[] winDLL = null;
            byte[] monoDLL = null;
            byte[] winPDB = null;
            byte[] monoPDB = null;
            if (properties.noCompile) {
                if (File.Exists(Path.Combine(mod, "All.dll"))) {
                    winDLL = monoDLL = File.ReadAllBytes(Path.Combine(mod, "All.dll"));

                    if (File.Exists(Path.Combine(mod, "All.pdb")))
                        winPDB = monoPDB = File.ReadAllBytes(Path.Combine(mod, "All.pdb"));
                }
                else if (File.Exists(Path.Combine(mod, "Windows.dll")) && File.Exists(Path.Combine(mod, "Mono.dll"))) {
                    winDLL = File.ReadAllBytes(Path.Combine(mod, "Windows.dll"));
                    monoDLL = File.ReadAllBytes(Path.Combine(mod, "Mono.dll"));

                    if (File.Exists(Path.Combine(mod, "Windows.pdb")))
                        winPDB = File.ReadAllBytes(Path.Combine(mod, "Windows.pdb"));
                    if (File.Exists(Path.Combine(mod, "Mono.pdb")))
                        monoPDB = File.ReadAllBytes(Path.Combine(mod, "Mono.pdb"));
                }
                else {
                    ErrorLogger.LogDllBuildError(mod);
                    return false;
                }
            }
            else {
                var refMods = FindReferencedMods(properties);
                if (refMods == null)
                    return false;

                status.SetStatus("Compiling "+modName+" for Windows...");
                status.SetProgress(0, 2);
                CompileMod(mod, properties, refMods, true, ref winDLL, ref winPDB);
                status.SetStatus("Compiling " + modName + " for Mono...");
                status.SetProgress(1, 2);
                CompileMod(mod, properties, refMods, false, ref monoDLL, ref monoPDB);
                if (winDLL == null || monoDLL == null)
                    return false;
            }

            status.SetStatus("Building "+modName+"...");
            status.SetProgress(0, 1);

            var file = Path.Combine(ModPath, modName + ".tmod");
            var modFile = new TmodFile(file) {
                name = modName,
                version = properties.version
            };

            modFile.AddFile("Info", properties.ToBytes());

            if (Equal(winDLL, monoDLL)) {
                modFile.AddFile("All.dll", winDLL);
                if (winPDB != null) modFile.AddFile("All.pdb", winPDB);
            }
            else {
                modFile.AddFile("Windows.dll", winDLL);
                modFile.AddFile("Mono.dll", monoDLL);
                if (winPDB != null) modFile.AddFile("Windows.pdb", winPDB);
                if (monoPDB != null) modFile.AddFile("Mono.pdb", monoPDB);
            }

            foreach (var resource in Directory.GetFiles(mod, "*", SearchOption.AllDirectories)) {
                var relPath = resource.Substring(mod.Length + 1);
                if (properties.ignoreFile(relPath) ||
                        relPath == "build.txt" ||
                        !properties.includeSource && Path.GetExtension(resource) == ".cs" ||
                        Path.GetFileName(resource) == "Thumbs.db")
                    continue;

                modFile.AddFile(relPath, File.ReadAllBytes(resource));
            }

            WAVCacheIO.ClearCache(modFile.name);

            modFile.Save();
            EnableMod(modFile);
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
                    ErrorLogger.LogModReferenceError(ex, refName);
                    return false;
                }
                var mod = new LoadingMod(modFile, BuildProperties.ReadModFile(modFile));
                mods[refName] = mod;
                FindReferencedMods(mod.properties, mods);
            }

            return true;
        }

        private static void CompileMod(string modDir, BuildProperties properties, List<LoadingMod> refMods, bool forWindows,
                ref byte[] dll, ref byte[] pdb) {
            LoadReferences();
            var refs = new List<string>(terrariaReferences);
            if (forWindows == windows) {
                refs.Add(Assembly.GetExecutingAssembly().Location);
            }
            else {
                refs = refs.Where(path => {
                    var name = Path.GetFileName(path);
                    return name != "FNA.dll" && !name.StartsWith("Microsoft.Xna.Framework");
                }).ToList();
                var terrariaDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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

            refs.AddRange(properties.dllReferences.Select(refDll => Path.Combine(modDir, "lib/" + refDll + ".dll")));

            var tempDir = Path.Combine(ModPath, "compile_temp");
            Directory.CreateDirectory(tempDir);

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
                OutputAssembly = Path.Combine(tempDir, Path.GetFileName(modDir) + ".dll"),
                GenerateExecutable = false,
                GenerateInMemory = false,
                TempFiles = new TempFileCollection(tempDir, true),
                IncludeDebugInformation = properties.includePDB
            };

            compileOptions.ReferencedAssemblies.AddRange(refs.ToArray());
            var options = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            var codeProvider = new CSharpCodeProvider(options);
            var results = codeProvider.CompileAssemblyFromFile(compileOptions, Directory.GetFiles(modDir, "*.cs", SearchOption.AllDirectories));
            var errors = results.Errors;

            if (errors.HasErrors) {
                ErrorLogger.LogCompileErrors(errors);
            }
            else {
                dll = File.ReadAllBytes(compileOptions.OutputAssembly);
                if (properties.includePDB)
                    pdb = File.ReadAllBytes(Path.Combine(tempDir, Path.GetFileName(modDir) + ".pdb"));
            }

            Directory.Delete(tempDir, true);
        }
    }
}
