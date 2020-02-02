using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using System.IO;
using XnaToFna;

namespace Terraria.ModLoader.Setup
{
	internal class HookGenTask : SetupOperation
	{
		public HookGenTask(ITaskInterface taskInterface) : base(taskInterface) {
		}

		public override void Run() {
			if (!File.Exists(Program.TerrariaPath))
				throw new FileNotFoundException(Program.TerrariaPath);

			var outputPath = Path.Combine(Program.referencesDir, "TerrariaHooks.Windows.dll");
			DeleteFile(outputPath);

			taskInterface.SetStatus($"Hooking: Terraria.exe -> TerrariaHooks.dll");
			HookGen(Program.TerrariaPath, outputPath, Program.referencesDir);

			taskInterface.SetStatus($"XnaToFna: TerrariaHooks.Windows.dll -> TerrariaHooks.Mono.dll");

			var monoPath = Path.Combine(Program.referencesDir, "TerrariaHooks.Mono.dll");
			DeleteFile(monoPath);

			Copy(outputPath, monoPath);
			XnaToFna(monoPath, Program.referencesDir);

			DeleteFile(Path.ChangeExtension(monoPath, "pdb"));
		}

		public static void HookGen(string inputPath, string outputPath, string refsDir) {
			using (var mm = new MonoModder {
				InputPath = inputPath,
				OutputPath = outputPath,
				ReadingMode = ReadingMode.Deferred,

				MissingDependencyThrow = false,
			}) {
				mm.Read();
				mm.MapDependencies();
				mm.DependencyCache["MonoMod.RuntimeDetour"] = ModuleDefinition.ReadModule(Path.Combine(refsDir, "MonoMod.RuntimeDetour.dll"));

				var gen = new HookGenerator(mm, "TerrariaHooks") {
					HookPrivate = true,
				};
				gen.Generate();
				gen.OutputModule.Write(outputPath);
			}
		}

		public static void XnaToFna(string inputPath, string refsDir) {
			using (var xnaToFnaUtil = new XnaToFnaUtil {
				HookCompat = false,
				HookHacks = false,
				HookEntryPoint = false,
				HookBinaryFormatter = false,
				HookReflection = false,
				AddAssemblyReference = false
			}) {
				xnaToFnaUtil.ScanPath(Path.Combine(refsDir, "FNA.dll"));
				xnaToFnaUtil.ScanPath(inputPath);
				xnaToFnaUtil.RelinkAll();
			}
		}
	}
}