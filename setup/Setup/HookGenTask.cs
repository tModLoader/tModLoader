using System.IO;
using System.Windows;
using static Terraria.ModLoader.Setup.Program;

namespace Terraria.ModLoader.Setup
{
	internal class HookGenTask : Task
	{
		public HookGenTask(ITaskInterface taskInterface) : base(taskInterface)
		{
		}

		public override void Run()
		{
			string targetExePath = Path.Combine(baseDir, @"src\tModLoader\bin\WindowsDebug\net45\Terraria.exe");
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

			
			var outputPath = Path.Combine(ReferencesDir, "TerrariaHooks.XNA.dll");
            if (File.Exists(outputPath))
                File.Delete(outputPath);

			taskInterface.SetStatus($"Hooking: Terraria.exe -> TerrariaHooks.XNA.dll");
			HookGenWrapper.HookGenWrapper.HookGen(targetExePath, outputPath, ReferencesDir);
			
			taskInterface.SetStatus($"XnaToFna: TerrariaHooks.XNA.dll -> TerrariaHooks.FNA.dll");

			var monoPath = Path.Combine(ReferencesDir, "TerrariaHooks.FNA.dll");
            if (File.Exists(monoPath))
                File.Delete(monoPath);

			File.Copy(outputPath, monoPath);
			HookGenWrapper.HookGenWrapper.XnaToFna(monoPath, ReferencesDir);
			
            File.Delete(Path.ChangeExtension(monoPath, "pdb"));
		}
	}
}