using System.ComponentModel;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class DecompileCommandSettings : BaseCommandSettings
{
	[CommandOption("--server-only")]
	[Description("Decompile only server code.")]
	public bool ServerOnly { get; init; }

	[CommandOption("-f|--no-prompts")]
	[Description("Execute command without prompting for confirmation or any missing information.")]
	public bool NoPrompts { get; init; }

	[CommandOption("--terraria-steam-dir")]
	[Description("Path to the Terraria steam directory. This is usually auto-detected. The value is persisted to src/WorkspaceInfo.targets")]
	public string? TerrariaSteamDir { get; init; }

	[CommandOption("--tml-dev-steam-dir")]
	[Description("Path to the TML dev steam directory. On first setup this is derived from Terraria steam directory if not set. The value is persisted to src/WorkspaceInfo.targets")]
	public string? TMLDevSteamDir { get; init; }
}

public sealed class DecompileCommand : CancellableAsyncCommand<DecompileCommandSettings>
{
	private readonly TaskRunner taskRunner;
	private readonly IServiceProvider serviceProvider;

	public DecompileCommand(
		TaskRunner taskRunner,
		IServiceProvider serviceProvider)
	{
		this.taskRunner = taskRunner;
		this.serviceProvider = serviceProvider;
	}

	protected override async Task<int> ExecuteAsync(CommandContext context, DecompileCommandSettings settings, CancellationToken cancellationToken)
	{
		DecompileTaskParameters decompileTaskParameters = DecompileTaskParameters.CreateDefault(
			settings.TerrariaSteamDir,
			settings.TMLDevSteamDir,
			settings.ServerOnly);

		return await taskRunner.Run(
			new DecompileTask(decompileTaskParameters, serviceProvider),
			settings.PlainProgress,
			settings.NoPrompts,
			cancellationToken: cancellationToken);
	}
}