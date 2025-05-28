using System.ComponentModel;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class SetupCommandSettings : PatchCommandSettings
{
	private readonly string? terrariaSteamDir;
	private readonly string? tmlDevSteamDir;

	[CommandOption("--terraria-steam-dir")]
	[Description("Path to the Terraria steam directory. This is usually auto-detected. The value is persisted to src/WorkspaceInfo.targets")]
	public string? TerrariaSteamDir {
		get => terrariaSteamDir;
		init => terrariaSteamDir = value != null ? PathUtils.GetCrossPlatformFullPath(value) : null;
	}

	[CommandOption("--tml-dev-steam-dir")]
	[Description("Path to the TML dev steam directory. This is derived from the Terraria steam directory if no value is supplied. The value is persisted to src/WorkspaceInfo.targets")]
	public string? TMLDevSteamDir {
		get => tmlDevSteamDir;
		init => tmlDevSteamDir = value != null ? PathUtils.GetCrossPlatformFullPath(value) : null;
	}
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