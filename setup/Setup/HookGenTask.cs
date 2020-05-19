using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using System.IO;
using System.Windows;
using XnaToFna;
using static Terraria.ModLoader.Setup.Program;

namespace Terraria.ModLoader.Setup
{
	internal class HookGenTask : SetupOperation
	{
		public HookGenTask(ITaskInterface taskInterface) : base(taskInterface)
		{
		}

		public override void Run()
		{
			string targetExePath = @"src\tModLoader\bin\WindowsDebug\net45\Terraria.exe";
			if (!File.Exists(targetExePath)) {
				var result = MessageBox.Show($"\"{targetExePath}\" does not exist. Use Vanilla exe instead?", "tML exe not found", MessageBoxButton.YesNo);
				if (result != MessageBoxResult.Yes) {
					taskInterface.SetStatus("Cancelled");
					return;
				}

				if (!File.Exists(TerrariaPath))
					throw new FileNotFoundException(TerrariaPath);

				targetExePath = TerrariaPath;
			}
			var outputPath = Path.Combine(referencesDir, "TerrariaHooks.XNA.dll");
			if (File.Exists(outputPath))
				File.Delete(outputPath);

			taskInterface.SetStatus($"Hooking: Terraria.exe -> TerrariaHooks.XNA.dll");
			HookGen(targetExePath, outputPath);

			taskInterface.SetStatus($"XnaToFna: TerrariaHooks.XNA.dll -> TerrariaHooks.FNA.dll");

			var monoPath = Path.Combine(referencesDir, "TerrariaHooks.FNA.dll");
			if (File.Exists(monoPath))
				File.Delete(monoPath);

			File.Copy(outputPath, monoPath);
			XnaToFna(monoPath);

			File.Delete(Path.ChangeExtension(monoPath, "pdb"));
		}

		public static void HookGen(string inputPath, string outputPath)
		{
			using var mm = new MonoModder {
				InputPath = inputPath,
				OutputPath = outputPath,
				ReadingMode = ReadingMode.Deferred,

				MissingDependencyThrow = false,
			};

			mm.Read();
			mm.MapDependencies();
			mm.DependencyCache["MonoMod.RuntimeDetour"] = ModuleDefinition.ReadModule(Path.Combine(referencesDir, "MonoMod.RuntimeDetour.dll"));
			mm.DependencyCache["MonoMod.Utils"] = ModuleDefinition.ReadModule(Path.Combine(referencesDir, "MonoMod.Utils.dll"));

			var gen = new HookGenerator(mm, "TerrariaHooks") {
				HookPrivate = true,
			};
			gen.Generate();
			RemoveModLoaderTypes(gen.OutputModule);
			gen.OutputModule.Write(outputPath);
		}

		private static void RemoveModLoaderTypes(ModuleDefinition module)
		{
			for (int i = module.Types.Count - 1; i >= 0; i--)
				if (module.Types[i].FullName.Contains("Terraria.ModLoader"))
					module.Types.RemoveAt(i);
		}

		public static void XnaToFna(string inputPath)
		{
			using var xnaToFnaUtil = new XnaToFnaUtil {
				HookCompat = false,
				HookHacks = false,
				HookEntryPoint = false,
				HookBinaryFormatter = false,
				HookReflection = false,
				AddAssemblyReference = false
			};
			xnaToFnaUtil.ScanPath(Path.Combine(referencesDir, "FNA.dll"));
			xnaToFnaUtil.ScanPath(inputPath);
			xnaToFnaUtil.RelinkAll();
		}
	}
}