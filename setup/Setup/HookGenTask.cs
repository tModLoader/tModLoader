using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using System.IO;
using XnaToFna;

namespace Terraria.ModLoader.Setup
{
	internal class HookGenTask : Task
	{
		public HookGenTask(ITaskInterface taskInterface) : base(taskInterface)
		{
		}

		public override void Run()
		{
			if (!File.Exists(Program.TerrariaPath))
				throw new FileNotFoundException(Program.TerrariaPath);
			
			var outputPath = Path.Combine(Program.ReferencesDir, "TerrariaHooks.Windows.dll");
            if (File.Exists(outputPath))
                File.Delete(outputPath);

			taskInterface.SetStatus($"Hooking: Terraria.exe -> TerrariaHooks.dll");

            using (MonoModder mm = new MonoModder {
                InputPath = Program.TerrariaPath,
                OutputPath = outputPath,
                ReadingMode = ReadingMode.Deferred,

                MissingDependencyThrow = false,
            }) {
                mm.Read();
                mm.MapDependencies();
				mm.DependencyCache["MonoMod.RuntimeDetour"] = ModuleDefinition.ReadModule(Path.Combine(Program.ReferencesDir, "MonoMod.RuntimeDetour.dll"));

                HookGenerator gen = new HookGenerator(mm, "TerrariaHooks") {
                    HookPrivate = true,
                };
                gen.Generate();
                gen.OutputModule.Write(outputPath);
            }
			
			taskInterface.SetStatus($"XnaToFna: TerrariaHooks.Windows.dll -> TerrariaHooks.Mono.dll");

			var monoPath = Path.Combine(Program.ReferencesDir, "TerrariaHooks.Mono.dll");
            if (File.Exists(monoPath))
                File.Delete(monoPath);

			File.Copy(outputPath, monoPath);

			using (var xnaToFnaUtil = new XnaToFnaUtil {
				HookCompatHelpers = false,
				HookEntryPoint = false,
				DestroyLocks = false,
				StubMixedDeps = false,
				DestroyMixedDeps = false,
				HookBinaryFormatter = false,
				HookReflection = false,
				AddAssemblyReference = false
			})
			{
				xnaToFnaUtil.ScanPath(Path.Combine(Program.ReferencesDir, "FNA.dll"));
				xnaToFnaUtil.ScanPath(monoPath);
				xnaToFnaUtil.RelinkAll();
			}

            File.Delete(Path.ChangeExtension(monoPath, "pdb"));
		}
	}
}