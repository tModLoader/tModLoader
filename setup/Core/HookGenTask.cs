using Microsoft.Extensions.DependencyInjection;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core
{
	public class HookGenTask : SetupOperation
	{
		private const string DotnetTargetVersion = "net8.0";
		private const string LibsPath = "src/tModLoader/Terraria/Libraries";
		private const string BinLibsPath = $"src/tModLoader/Terraria/bin/Release/{DotnetTargetVersion}/Libraries";

		private const string TmlAssemblyPath = $"src/tModLoader/Terraria/bin/Release/{DotnetTargetVersion}/tModLoader.dll";

		private readonly IUserPrompt userPrompt;

		public HookGenTask(IServiceProvider serviceProvider)
		{
			userPrompt = serviceProvider.GetRequiredService<IUserPrompt>();
		}

		public override Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			if (!File.Exists(TmlAssemblyPath)) {
				throw new FileNotFoundException($"\"{TmlAssemblyPath}\" does not exist.");
			}

			DateTime buildTimestamp = File.GetLastWriteTime(TmlAssemblyPath);
			int buildDaysAgo = (DateTime.Now - buildTimestamp).Days;
			if (buildDaysAgo > 1) {
				throw new Exception($"tModLoader.dll was last modified on {buildTimestamp} ({buildDaysAgo} day(s) ago). HookGen is run on the release build, make sure to build release.");
			}

			// Hopefully this always works since we should be running on a system install of a .NET Core sdk
			var dotnetPath = Path.GetFullPath(Path.GetDirectoryName(typeof(object).Assembly.Location) + "/../../..");
			var dotnetRefsLocation = Path.Combine(dotnetPath, $"packs/Microsoft.NETCore.App.Ref/{Environment.Version}/ref/{DotnetTargetVersion}");

			// Ensure that refs are present, for gods sake!
			if (!Directory.Exists(dotnetRefsLocation) || !Directory.GetFiles(dotnetRefsLocation, "*.dll").Any()) {
				throw new DirectoryNotFoundException($@"Unable to find reference libraries for .NET SDK '{Environment.Version}' - ""{dotnetRefsLocation}"" does not exist.");
			}

			string outputPath = Path.Combine(LibsPath, "Common", "TerrariaHooks.dll");

			if (File.Exists(outputPath))
				File.Delete(outputPath);

			using var taskProgress = progress.StartTask("Hooking: tModLoader.dll -> TerrariaHooks.dll");
			HookGen(taskProgress, TmlAssemblyPath, outputPath, dotnetRefsLocation, cancellationToken);

			File.Delete(Path.ChangeExtension(outputPath, "pdb"));

			return Task.CompletedTask;
		}

		public override void FinishedPrompt()
		{
			userPrompt.Inform("Success", "Success. Make sure you diff tModLoader after this");
		}

		private void HookGen(ITaskProgress taskProgress, string inputPath, string outputPath, string dotnetReferencesDirectory, CancellationToken cancellationToken = default)
		{
			using var workItemProgress = taskProgress.StartWorkItem("Loading");
			using var mm = new ProgressReportingMonoModder(workItemProgress) {
				InputPath = inputPath,
				OutputPath = outputPath,
				ReadingMode = ReadingMode.Deferred,
				DependencyDirs = { dotnetReferencesDirectory },
				MissingDependencyThrow = false,
				LogVerboseEnabled = true,
			};

			mm.DependencyDirs.AddRange(Directory.GetDirectories(BinLibsPath, "*", SearchOption.AllDirectories));

			mm.Read();

			var gen = new HookGenerator(mm, "TerrariaHooks") {
				HookPrivate = true,
			};

			gen.GenerateObsoleteLogging(mm);

			foreach (var type in mm.Module.Types) {
				cancellationToken.ThrowIfCancellationRequested();

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

		public class ProgressReportingMonoModder : MonoModder
		{
			private IWorkItemProgress progress;

			public ProgressReportingMonoModder(IWorkItemProgress progress)
			{
				this.progress = progress;
			}

			public override void Log(string text)
			{
				progress.ReportStatus(text);
			}
		}
	}
}