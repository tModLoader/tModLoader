using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class HookGenCommand : CancellableAsyncCommand<BaseCommandSettings>
{
	private readonly TaskRunner taskRunner;
	private readonly IServiceProvider serviceProvider;

	public HookGenCommand(TaskRunner taskRunner, IServiceProvider serviceProvider)
	{
		this.taskRunner = taskRunner;
		this.serviceProvider = serviceProvider;
	}

	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		BaseCommandSettings settings,
		CancellationToken cancellationToken)
	{
		return await taskRunner.Run(new HookGenTask(serviceProvider), settings, cancellationToken: cancellationToken);
	}
}