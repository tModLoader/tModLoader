using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
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
                var all = Path.Combine(mod, "All.dll");
                var win = Path.Combine(mod, "Windows.dll");
                var mono = Path.Combine(mod, "Other.dll");
                if (File.Exists(all)) {
                    winDLL = monoDLL = File.ReadAllBytes(all);
                    if (File.Exists(Path.Combine(mod, "All.pdb")))
                        winPDB = monoPDB = File.ReadAllBytes(Path.Combine(mod, "All.pdb"));
                }
                else if (File.Exists(win) && File.Exists(mono)) {
                    winDLL = File.ReadAllBytes(win);
                    monoDLL = File.ReadAllBytes(mono);
                    if (File.Exists(Path.Combine(mod, "Windows.pdb")))
                        winPDB = File.ReadAllBytes(Path.Combine(mod, "Windows.pdb"));
                    if (File.Exists(Path.Combine(mod, "Other.pdb")))
                        monoPDB = File.ReadAllBytes(Path.Combine(mod, "Other.pdb"));
                }
                else {
                    ErrorLogger.LogDllBuildError(mod);
                    return false;
                }
            }
            else {
                var refMods = CreateModReferenceDlls(properties);
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
            string file = Path.Combine(ModPath, modName + ".tmod");
            var propertiesData = properties.ToBytes();
            var modFile = new TmodFile(file);
            using (var memoryStream = new MemoryStream()) {
                using (var writer = new BinaryWriter(memoryStream)) {
                    writer.Write(version);
                    writer.Write(propertiesData.Length);
                    writer.Write(propertiesData);
                    writer.Flush();
                }
                modFile.AddFile("Info", memoryStream.ToArray());
            }
            using (var memoryStream = new MemoryStream()) {
                using (var writer = new BinaryWriter(memoryStream)) {
                    writer.Write(modName);
                    foreach (var resource in Directory.GetFiles(mod, "*", SearchOption.AllDirectories)) {
                        var relPath = resource.Substring(mod.Length + 1);
                        if (properties.ignoreFile(relPath) ||
                                relPath == "build.txt" ||
                                !properties.includeSource && Path.GetExtension(resource) == ".cs" ||
                                Path.GetFileName(resource) == "Thumbs.db")
                            continue;
                        var resourcePath = Path.Combine(modName, relPath);
                        resourcePath = resourcePath.Replace(Path.DirectorySeparatorChar, '/');
                        var buffer = File.ReadAllBytes(resource);
                        writer.Write(resourcePath);
                        writer.Write(buffer.Length);
                        writer.Write(buffer);
                    }
                    writer.Write("end");
                    writer.Flush();
                    modFile.AddFile("Resources", memoryStream.ToArray());
                }
            }
            if (Equal(winDLL, monoDLL)) {
                modFile.AddFile("All", winDLL);
                if (winPDB != null) modFile.AddFile("All.pdb", winPDB);
            }
            else {
                modFile.AddFile("Windows", winDLL);
                modFile.AddFile("Other", monoDLL);
                if (winPDB != null) modFile.AddFile("Windows.pdb", winPDB);
                if (monoPDB != null) modFile.AddFile("Other.pdb", monoPDB);
            }
            modFile.Save();
            WAVCacheIO.ClearCache(modFile.Name);
            EnableMod(file);
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
        internal static TmodFile[] CreateModReferenceDlls(BuildProperties properties) {
            var refMods = properties.modReferences.Select(modRef =>
                new TmodFile(Path.Combine(ModPath, modRef + ".tmod"))).ToArray();
            foreach (var refMod in refMods) {
                refMod.Read();
                if (!refMod.ValidMod()) {
                    ErrorLogger.LogModReferenceError(refMod.Name);
                    return null;
                }
            }
            return refMods;
        }
        private static void CompileMod(string modDir, BuildProperties properties, TmodFile[] refMods, bool forWindows,
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
            Directory.CreateDirectory(DllPath);
            refs.AddRange(properties.dllReferences.Select(refDll => Path.Combine(DllPath, refDll + ".dll")));
            foreach (var refMod in refMods) {
                var dllBytes = refMod.HasFile("All")
                    ? refMod.GetFile("All")
                    : refMod.GetFile(forWindows ? "Windows" : "Other");
                File.WriteAllBytes(Path.Combine(ModSourcePath, refMod.Name+".dll"), dllBytes);
                refs.Add(refMod.Name);
            }
            var tempDir = Path.Combine(ModPath, "compile_temp");
            Directory.CreateDirectory(tempDir);
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
            foreach (var refMod in refMods)
                File.Delete(Path.Combine(ModSourcePath, refMod.Name + ".dll"));
            if (errors.HasErrors) {
                ErrorLogger.LogCompileErrors(errors);
                return;
            }
            dll = File.ReadAllBytes(compileOptions.OutputAssembly);
            if (properties.includePDB)
                pdb = File.ReadAllBytes(Path.Combine(tempDir, Path.GetFileName(modDir) + ".pdb"));
            Directory.Delete(tempDir, true);
        }
    }
}
