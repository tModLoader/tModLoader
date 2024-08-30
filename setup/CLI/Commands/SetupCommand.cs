using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class SetupCommand : CancellableAsyncCommand<PatchCommandSettings>
{
	private readonly IServiceProvider serviceProvider;
	private readonly TaskRunner taskRunner;

	public SetupCommand(TaskRunner taskRunner, IServiceProvider serviceProvider)
	{
		this.taskRunner = taskRunner;
		this.serviceProvider = serviceProvider;
	}

	public override async Task<int> ExecuteAsync(
		CommandContext context,
		PatchCommandSettings settings,
		CancellationToken cancellationToken)
	{
		var setupTask = new SetupTask(DecompileTaskParameters.CreateDefault(), serviceProvider);

		return await taskRunner.Run(setupTask, settings.PlainProgress, settings.NoPrompts, cancellationToken: cancellationToken);
	}
}