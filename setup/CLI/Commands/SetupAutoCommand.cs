using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class SetupAutoCommandSettings : PatchCommandSettings
{
	[CommandArgument(0, "<TerrariaSteamDir>")]
	[Description("Path to the Terraria steam directory.")]
	public string TerrariaSteamDir { get; set; } = string.Empty;

	[CommandArgument(0, "<TMLDevSteamDir>")]
	[Description("Path to the TML dev steam directory.")]
	public string TMLDevSteamDir { get; set; } = string.Empty;
}

public sealed class SetupAutoCommand : CancellableAsyncCommand<SetupAutoCommandSettings>
{
	private readonly TaskRunner taskRunner;
	private readonly ProgramSettings programSettings;
	private readonly IServiceProvider serviceProvider;

	public SetupAutoCommand(TaskRunner taskRunner, ProgramSettings programSettings, IServiceProvider serviceProvider)
	{
		this.taskRunner = taskRunner;
		this.programSettings = programSettings;
		this.serviceProvider = serviceProvider;
	}

	public override async Task<int> ExecuteAsync(
		CommandContext context,
		SetupAutoCommandSettings settings,
		CancellationToken cancellationToken)
	{
		if (!string.IsNullOrWhiteSpace(settings.TerrariaSteamDir)) {
			programSettings.TerrariaSteamDir = settings.TerrariaSteamDir;
		}

		programSettings.PatchMode = settings.PatchMode;
		programSettings.TerrariaSteamDir = settings.TerrariaSteamDir;
		programSettings.TMLDevSteamDir = settings.TMLDevSteamDir;

		if (!Directory.Exists(settings.TMLDevSteamDir)) {
			Directory.CreateDirectory(settings.TMLDevSteamDir);
		}

		SetupOperation setupTask = GetSetupOperation();

		return await taskRunner.Run(setupTask, settings.PlainProgress, noPrompts: true, cancellationToken);
	}

	public override ValidationResult Validate(CommandContext context, SetupAutoCommandSettings settings)
	{
		if (!Directory.Exists(settings.TerrariaSteamDir)) {
			return ValidationResult.Error($"Directory '{settings.TerrariaSteamDir}' does not exist.");
		}

		return ValidationResult.Success();
	}

	private SetupOperation GetSetupOperation()
	{
		if (Directory.Exists("src/decompiled")) {
			Console.WriteLine("src/decompiled found. Skipping decompilation...");
			return new RegenSourceTask(serviceProvider);
		}

		return new SetupTask(DecompileTaskParameters.CreateDefault(maxDegreeOfParallelism: 1), serviceProvider);
	}
}