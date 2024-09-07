using System.ComponentModel;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class SetupCommandSettings : PatchCommandSettings
{
	[CommandOption("--terraria-steam-dir")]
	[Description("Path to the Terraria steam directory. This is usually auto-detected. The value is persisted to src/WorkspaceInfo.targets")]
	public string? TerrariaSteamDir { get; init; }

	[CommandOption("--tml-dev-steam-dir")]
	[Description("Path to the TML dev steam directory. On first setup this is derived from Terraria steam directory if not set. The value is persisted to src/WorkspaceInfo.targets")]
	public string? TMLDevSteamDir { get; init; }
}

public sealed class SetupCommand : CancellableAsyncCommand<SetupCommandSettings>
{
	private readonly IServiceProvider serviceProvider;
	private readonly TaskRunner taskRunner;

	public SetupCommand(TaskRunner taskRunner, IServiceProvider serviceProvider)
	{
		this.taskRunner = taskRunner;
		this.serviceProvider = serviceProvider;
	}

	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		SetupCommandSettings settings,
		CancellationToken cancellationToken)
	{
		var setupTask = new SetupTask(DecompileTaskParameters.CreateDefault(settings.TerrariaSteamDir, settings.TMLDevSteamDir), serviceProvider);

		return await taskRunner.Run(setupTask, settings, settings.NoPrompts, cancellationToken: cancellationToken);
	}
}