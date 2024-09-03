using Microsoft.Extensions.DependencyInjection;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core
{
	public class HookGenTask : SetupOperation
	{
		private const string DotnetSdkVersion = "8.0.1";
		private const string DotnetTargetVersion = "net8.0";
		private const string LibsPath = "src/tModLoader/Terraria/Libraries";
		private const string BinLibsPath = $"src/tModLoader/Terraria/bin/Release/{DotnetTargetVersion}/Libraries";

		private const string TmlAssemblyPath = $"src/tModLoader/Terraria/bin/Release/{DotnetTargetVersion}/tModLoader.dll";
		private const string InstalledNetRefs = $"/dotnet/packs/Microsoft.NETCore.App.Ref/{DotnetSdkVersion}/ref/{DotnetTargetVersion}";

		private readonly IUserPrompt userPrompt;
		private bool success;

		public HookGenTask(IServiceProvider serviceProvider)
		{
			userPrompt = serviceProvider.GetRequiredService<IUserPrompt>();
		}

		public override Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			using var taskProgress = progress.StartTask("Generating Hooks...");
			if (!File.Exists(TmlAssemblyPath)) {
				throw new FileNotFoundException($"\"{TmlAssemblyPath}\" does not exist.");
			}

			string outputPath = Path.Combine(LibsPath, "Common", "TerrariaHooks.dll");

			if (File.Exists(outputPath))
				File.Delete(outputPath);

			taskProgress.ReportStatus("Hooking: tModLoader.dll -> TerrariaHooks.dll");

			HookGen(TmlAssemblyPath, outputPath);

			File.Delete(Path.ChangeExtension(outputPath, "pdb"));

			success = true;

			return Task.CompletedTask;
		}

		public override void FinishedPrompt()
		{
			if (!success) {
				return;
			}

			userPrompt.Inform("Success", "Success. Make sure you diff tModLoader after this");
		}

		private void HookGen(string inputPath, string outputPath)
		{
			string dotnetReferencesDirectory =
				Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + InstalledNetRefs;

			// Ensure that refs are present, for gods sake!
			if (!Directory.Exists(dotnetReferencesDirectory) ||
			    Directory.GetFiles(dotnetReferencesDirectory, "*.dll").Length == 0) {
				throw new DirectoryNotFoundException($@"Unable to find reference libraries for .NET SDK '{DotnetSdkVersion}' - ""{dotnetReferencesDirectory}"" does not exist.");
			}

			using var mm = new MonoModder {
				InputPath = inputPath,
				OutputPath = outputPath,
				ReadingMode = ReadingMode.Deferred,
				DependencyDirs = { dotnetReferencesDirectory },
				MissingDependencyThrow = false,
			};

			mm.DependencyDirs.AddRange(Directory.GetDirectories(BinLibsPath, "*", SearchOption.AllDirectories));

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