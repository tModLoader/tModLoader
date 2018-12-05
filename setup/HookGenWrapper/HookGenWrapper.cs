using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
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

                HookGenerator gen = new HookGenerator(mm, "TerrariaHooks") {
                    HookPrivate = true,
                };
                gen.Generate();
                gen.OutputModule.Write(outputPath);
            }
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
