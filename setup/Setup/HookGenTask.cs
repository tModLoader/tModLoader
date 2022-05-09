using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.IO;
using System.Windows;

namespace Terraria.ModLoader.Setup
{
	internal class HookGenTask : SetupOperation
	{
		private const string dotnetSdkVersion = "6.0.3";
		private const string libsPath = "src/tModLoader/Terraria/Libraries";
		private const string binLibsPath = "src/tModLoader/Terraria/bin/Release/net6.0/Libraries";
		private const string tmlAssemblyPath = @"src/tModLoader/Terraria/bin/Release/net6.0/tModLoader.dll";
		private const string installedNetRefs = $@"\dotnet\packs\Microsoft.NETCore.App.Ref\{dotnetSdkVersion}\ref\net6.0";

		public HookGenTask(ITaskInterface taskInterface) : base(taskInterface)
		{
		}

		public override void Run()
		{
			if (!File.Exists(tmlAssemblyPath)) {
				MessageBox.Show($"\"{tmlAssemblyPath}\" does not exist.", "tML exe not found", MessageBoxButton.OK);
				taskInterface.SetStatus("Cancelled");
				return;
			}

			string outputPath = Path.Combine(libsPath, "Common", "TerrariaHooks.dll");

			if (File.Exists(outputPath))
				File.Delete(outputPath);

			taskInterface.SetStatus($"Hooking: tModLoader.dll -> TerrariaHooks.dll");

			if (!HookGen(tmlAssemblyPath, outputPath)) {
				taskInterface.SetStatus("Cancelled");
				return;
			}

			File.Delete(Path.ChangeExtension(outputPath, "pdb"));

			MessageBox.Show("Success. Make sure you diff tModLoader after this");
		}

		public static bool HookGen(string inputPath, string outputPath)
		{
			string dotnetReferencesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + installedNetRefs;

			// Ensure that refs are present, for gods sake!
			if (!Directory.Exists(dotnetReferencesDirectory) || Directory.GetFiles(dotnetReferencesDirectory, "*.dll").Length == 0) {
				// Replace with exceptions if this is ever called in CLI.
				MessageBox.Show(
					$@"Unable to find reference libraries for .NET SDK '{dotnetSdkVersion}' - ""{dotnetReferencesDirectory}"" does not exist.",
					$".NET SDK {dotnetSdkVersion} not found",
					MessageBoxButton.OK
				);

				return false;
			}

			using var mm = new MonoModder {
				InputPath = inputPath,
				OutputPath = outputPath,
				ReadingMode = ReadingMode.Deferred,

				DependencyDirs = { dotnetReferencesDirectory },
				MissingDependencyThrow = false,
			};

			mm.DependencyDirs.AddRange(Directory.GetDirectories(binLibsPath, "*", SearchOption.AllDirectories));

			mm.Read();
			mm.MapDependencies();

			var gen = new HookGenerator(mm, "TerrariaHooks") {
				HookPrivate = true,
			};

			gen.Generate();

			RemoveModLoaderTypes(gen.OutputModule);

			gen.OutputModule.Write(outputPath);

			return true;
		}

		private static void RemoveModLoaderTypes(ModuleDefinition module)
		{
			for (int i = module.Types.Count - 1; i >= 0; i--)
				if (module.Types[i].FullName.Contains("Terraria.ModLoader"))
					module.Types.RemoveAt(i);
		}
	}
}