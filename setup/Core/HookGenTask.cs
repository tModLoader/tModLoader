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
		private const string dotnetSdkVersion = "8.0.1";
		private const string dotnetTargetVersion = "net8.0";
		private const string libsPath = "src/tModLoader/Terraria/Libraries";
		private const string binLibsPath = $"src/tModLoader/Terraria/bin/Release/{dotnetTargetVersion}/Libraries";
		private const string tmlAssemblyPath = @$"src/tModLoader/Terraria/bin/Release/{dotnetTargetVersion}/tModLoader.dll";
		private const string installedNetRefs = $@"\dotnet\packs\Microsoft.NETCore.App.Ref\{dotnetSdkVersion}\ref\{dotnetTargetVersion}";

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

			var gen = new HookGenerator(mm, "TerrariaHooks") {
				HookPrivate = true,
			};

			foreach (var type in mm.Module.Types) {
				if (!type.FullName.StartsWith("Terraria") || type.FullName.StartsWith("Terraria.ModLoader"))
					continue;

				gen.GenerateFor(type, out var hookType, out var hookILType);
				if (hookType == null || hookILType == null || hookType.IsNested)
					continue;

				AdjustNamespaceStyle(hookType);
				AdjustNamespaceStyle(hookILType);

				gen.OutputModule.Types.Add(hookType);
				gen.OutputModule.Types.Add(hookILType);
			}

			gen.OutputModule.Write(outputPath);

			return true;
		}


		// convert
		//   On.Namespace.Type -> Namespace.On_Type
		//   IL.Namespace.Type -> Namespace.IL_Type
		private static void AdjustNamespaceStyle(TypeDefinition type)
		{
			if (string.IsNullOrEmpty(type.Namespace))
				return;

			type.Name = type.Namespace[..2] + '_' + type.Name;
			type.Namespace = type.Namespace[Math.Min(3, type.Namespace.Length)..];
		}
	}
}