using System.ComponentModel;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class DecompileCommandSettings : BaseCommandSettings
{
	private readonly string? terrariaSteamDir;
	private readonly string? tmlDevSteamDir;

	[CommandOption("--server-only")]
	[Description("Decompile only server code.")]
	public bool ServerOnly { get; init; }

	[CommandOption("-f|--no-prompts")]
	[Description("Execute command without prompting for confirmation or any missing information.")]
	public bool NoPrompts { get; init; }

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

	[CommandOption("--max-parallelism")]
	[Description("Maximum parallel decompile tasks. Default is CPU count.")]
	public int? MaxParallelism { get; init; }

	[CommandOption("-k|--key")]
	[Description("Terraria ownership key in hexadecimal format. This is used to decrypt the Windows Terraria executable on non-windows platforms. The key is usally derived from the installed Terraria.exe")]
	public string? Key { get; init; }
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
			settings.ServerOnly,
			settings.MaxParallelism,
			string.IsNullOrWhiteSpace(settings.Key) ? null : Convert.FromHexString(settings.Key));

		return await taskRunner.Run(
			new DecompileTask(decompileTaskParameters, serviceProvider),
			settings,
			settings.NoPrompts,
			cancellationToken: cancellationToken);
	}
}