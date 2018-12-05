using System.IO;

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
			HookGenWrapper.HookGenWrapper.HookGen(Program.TerrariaPath, outputPath, Program.ReferencesDir);
			
			taskInterface.SetStatus($"XnaToFna: TerrariaHooks.Windows.dll -> TerrariaHooks.Mono.dll");

			var monoPath = Path.Combine(Program.ReferencesDir, "TerrariaHooks.Mono.dll");
            if (File.Exists(monoPath))
                File.Delete(monoPath);

			File.Copy(outputPath, monoPath);
			HookGenWrapper.HookGenWrapper.XnaToFna(monoPath, Program.ReferencesDir);
			
            File.Delete(Path.ChangeExtension(monoPath, "pdb"));
		}
	}
}