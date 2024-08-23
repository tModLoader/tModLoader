using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class UpdateLocalizationFilesCommand : CancellableAsyncCommand<BaseCommandSettings>
{
	private readonly TaskRunner taskRunner;

	public UpdateLocalizationFilesCommand(TaskRunner taskRunner)
	{
		this.taskRunner = taskRunner;
	}

	public override async Task<int> ExecuteAsync(
		CommandContext context,
		BaseCommandSettings settings,
		CancellationToken cancellationToken)
	{
		return await taskRunner.Run(new UpdateLocalizationFilesTask(), settings.PlainProgress, cancellationToken: cancellationToken);
	}
}