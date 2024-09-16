using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class FormatCommand : CancellableAsyncCommand<ProjectPathCommandSettings>
{
	private readonly TaskRunner taskRunner;

	public FormatCommand(TaskRunner taskRunner)
	{
		this.taskRunner = taskRunner;
	}

	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		ProjectPathCommandSettings settings,
		CancellationToken cancellationToken)
	{
		FormatTaskParameters taskParameters = new() { ProjectPath = settings.ProjectPath };

		return await taskRunner.Run(new FormatTask(taskParameters), settings, cancellationToken: cancellationToken);
	}
}