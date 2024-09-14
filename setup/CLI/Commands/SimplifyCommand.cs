using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class SimplifyCommand : CancellableAsyncCommand<ProjectPathCommandSettings>
{
	private readonly TaskRunner taskRunner;
	private readonly IServiceProvider serviceProvider;

	public SimplifyCommand(TaskRunner taskRunner, IServiceProvider serviceProvider)
	{
		this.taskRunner = taskRunner;
		this.serviceProvider = serviceProvider;
	}

	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		ProjectPathCommandSettings settings,
		CancellationToken cancellationToken)
	{
		RoslynTaskParameters taskParameters = new() { ProjectPath = settings.ProjectPath };

		return await taskRunner.Run(new SimplifierTask(taskParameters), settings, cancellationToken: cancellationToken);
	}
}