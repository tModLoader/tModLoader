using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
    internal class AssemblyManager
    {
        private class LoadedMod
        {
            public TmodFile modFile;
            public BuildProperties properties;
            public string Name => modFile.name;

            public readonly List<LoadedMod> dependencies = new List<LoadedMod>();
            public readonly List<LoadedMod> dependents = new List<LoadedMod>();

            public IEnumerable<LoadedMod> DependentSet => dependents.Union(dependencies.SelectMany(mod => mod.DependentSet));

            private int loadIndex;
            private bool eacEnabled;
            public bool needsReload = true;
            public Assembly assembly;

            public string AssemblyName => eacEnabled ? Name : Name + '_' + loadIndex;

            public string DllName(string dll) {
                return eacEnabled ? dll : Name + '_' + dll + '_' + loadIndex;
            }

            public void SetMod(ModLoader.LoadingMod mod) {
                if (modFile == null ||
                    modFile.version != mod.modFile.version ||
                    !modFile.hash.SequenceEqual(mod.modFile.hash))
                    SetNeedsReload();

                modFile = mod.modFile;
                properties = mod.properties;
            }

            public void SetNeedsReload() {
                if (!needsReload)
                    loadIndex++;

                needsReload = true;
                eacEnabled = false;

                foreach (var dep in dependents)
                    dep.SetNeedsReload();
            }

            public void AddDependency(LoadedMod dep) {
                dependencies.Add(dep);
                dep.dependents.Add(this);
            }

            public bool CanEaC() {
                return eacEnabled ||
                       !loadedAssemblies.ContainsKey(modFile.name) && dependencies.All(dep => dep.CanEaC());
            }

            public void EnableEaC() {
                if (eacEnabled)
                    return;

                eacEnabled = true;
                //this assembly is changing name, so any dependents 
                foreach (var dep in DependentSet)
                    if (!dep.eacEnabled)
                        dep.needsReload = true;
            }

            public void Reload() {
                foreach (var dll in properties.dllReferences)
                    LoadAssembly(EncapsulateReferences(modFile.GetFile("lib/" + dll + ".dll")));

                assembly = LoadAssembly(EncapsulateReferences(modFile.GetMainAssembly()), modFile.GetMainPDB());
                needsReload = false;
            }

            private byte[] EncapsulateReferences(byte[] code) {
                if (eacEnabled)
                    return code;

                var asm = AssemblyDefinition.ReadAssembly(new MemoryStream(code));
                asm.Name.Name = EncapsulateName(asm.Name.Name);

                //randomise the module version id so that the debugger can detect it as a different module (even if it has the same content)
                asm.MainModule.Mvid = Guid.NewGuid();

                foreach (var mod in asm.Modules)
                    foreach (var asmRef in mod.AssemblyReferences)
                        asmRef.Name = EncapsulateName(asmRef.Name);

                var ret = new MemoryStream();
                asm.Write(ret);
                return ret.ToArray();
            }

            private string EncapsulateName(string name) {
                if (name == Name)
                    return AssemblyName;

                if (properties.dllReferences.Contains(name))
                    return DllName(name);

                foreach (var dep in dependencies) {
                    var _name = dep.EncapsulateName(name);
                    if (_name != name)
                        return _name;
                }

                return name;
            }
        }

        private static readonly IDictionary<string, LoadedMod> loadedMods = new Dictionary<string, LoadedMod>();  
        private static readonly IDictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();

        static AssemblyManager() {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                string name = args.Name;
                if (name.IndexOf(',') >= 0)
                    name = name.Substring(0, name.IndexOf(','));

                if (name == "Terraria")
                    return Assembly.GetExecutingAssembly();

                Assembly a;
                loadedAssemblies.TryGetValue(name, out a);
                return a;
            };
        }

        private static void RecalculateReferences() {
            foreach (var mod in loadedMods.Values) {
                mod.dependencies.Clear();
                mod.dependents.Clear();
            }

            foreach (var mod in loadedMods.Values)
                foreach (var depName in mod.properties.modReferences)
                    mod.AddDependency(loadedMods[depName]);
        }

        private static Assembly LoadAssembly(byte[] code, byte[] pdb = null) {
            var asm = Assembly.Load(code, pdb);
            loadedAssemblies[asm.GetName().Name] = asm;
            return asm;
        }

        internal static List<Mod> InstantiateMods(List<ModLoader.LoadingMod> modsToLoad) {
            var modList = new List<LoadedMod>();
            foreach (var loading in modsToLoad) {
                LoadedMod mod;
                if (!loadedMods.TryGetValue(loading.Name, out mod))
                    mod = loadedMods[loading.Name] = new LoadedMod();

                mod.SetMod(loading);
                modList.Add(mod);
            }

            RecalculateReferences();

            if (Debugger.IsAttached) {
                foreach (var mod in modList.Where(mod => mod.properties.editAndContinue && mod.CanEaC()))
                    mod.EnableEaC();
            }

            var modInstances = new List<Mod>();

            int i = 0;
            foreach (var mod in modList) {
                Interface.loadMods.SetProgressCompatibility(mod.Name, i++, modsToLoad.Count);
                try {
                    Interface.loadMods.SetProgressReading(mod.Name, 0, 1);
                    if (mod.needsReload)
                        mod.Reload();

                    Interface.loadMods.SetProgressReading(mod.Name, 1, 2);
                    var modType = mod.assembly.GetTypes().Single(t => t.IsSubclassOf(typeof(Mod)));
                    var m = (Mod)Activator.CreateInstance(modType);
                    m.File = mod.modFile;
                    m.Code = mod.assembly;
                    modInstances.Add(m);
                }
                catch (Exception e) {
                    ModLoader.DisableMod(mod.modFile);
                    ErrorLogger.LogLoadingError(mod.Name, mod.modFile.tModLoaderVersion, e);
                    return null;
                }
            }

            return modInstances;
        }
    }
}
