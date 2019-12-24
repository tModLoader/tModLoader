using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.IO;
using XnaToFna;

namespace HookGenWrapper
{
	/// <summary>
	/// HookGen/MonoMod uses Cecil 0.10 while ILSpy uses Cecil 0.9, so separate projects are required
	/// </summary>
	public static class HookGenWrapper
    {
		public static void HookGen(string inputPath, string outputPath, string refsDir)
		{
            using (MonoModder mm = new MonoModder {
                InputPath = inputPath,
                OutputPath = outputPath,
                ReadingMode = ReadingMode.Deferred,

                MissingDependencyThrow = false,
            }) {
                mm.Read();
                mm.MapDependencies();
				mm.DependencyCache["MonoMod.RuntimeDetour"] = ModuleDefinition.ReadModule(Path.Combine(refsDir, "MonoMod.RuntimeDetour.dll"));
				mm.DependencyCache["MonoMod.Utils"] = ModuleDefinition.ReadModule(Path.Combine(refsDir, "MonoMod.Utils.dll"));
				
				HookGenerator gen = new HookGenerator(mm, "TerrariaHooks") {
                    HookPrivate = true,
                };
                gen.Generate();
				RemoveModLoaderTypes(gen.OutputModule);
				gen.OutputModule.Write(outputPath);
            }
		}

		private static void RemoveModLoaderTypes(ModuleDefinition module) {
			for (int i = module.Types.Count-1; i >= 0; i--)
				if (module.Types[i].FullName.Contains("Terraria.ModLoader"))
					module.Types.RemoveAt(i);
		}

		public static void XnaToFna(string inputPath, string refsDir)
		{
			using (var xnaToFnaUtil = new XnaToFnaUtil {
				HookCompat = false,
				HookHacks = false,
				HookEntryPoint = false,
				HookBinaryFormatter = false,
				HookReflection = false,
				AddAssemblyReference = false
			})
			{
				xnaToFnaUtil.ScanPath(Path.Combine(refsDir, "FNA.dll"));
				xnaToFnaUtil.ScanPath(inputPath);
				xnaToFnaUtil.RelinkAll();
			}
		}
    }
}
