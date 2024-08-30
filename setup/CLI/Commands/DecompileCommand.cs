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

	public override async Task<int> ExecuteAsync(CommandContext context, DecompileCommandSettings settings, CancellationToken cancellationToken)
	{
		DecompileTaskParameters decompileTaskParameters = DecompileTaskParameters.CreateDefault(settings.ServerOnly);

		return await taskRunner.Run(
			new DecompileTask(decompileTaskParameters, serviceProvider),
			settings.PlainProgress,
			settings.NoPrompts,
			cancellationToken: cancellationToken);
	}
}