using System.ComponentModel;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class DiffCommandSettings : BaseCommandSettings
{
	[CommandOption("-r|--reset-diff-timestamp")]
	[Description("Diff all files instead of only those which changed since the last diff/patch.")]
	public bool ResetDiffTimestamp { get; set; }
}

public sealed class DiffTerrariaCommand(TaskRunner taskRunner, ProgramSettings programSettings)
	: DiffBaseCommand(taskRunner, programSettings)
{
	protected override DiffTaskParameters GetDiffTaskParameters(ProgramSettings programSettings) =>
		DiffTaskParameters.ForTerraria(programSettings);
}

public sealed class DiffTerrariaNetCoreCommand(TaskRunner taskRunner, ProgramSettings programSettings)
	: DiffBaseCommand(taskRunner, programSettings)
{
	protected override DiffTaskParameters GetDiffTaskParameters(ProgramSettings programSettings) =>
		DiffTaskParameters.ForTerrariaNetCore(programSettings);
}

public sealed class DiffTModLoaderCommand(TaskRunner taskRunner, ProgramSettings programSettings)
	: DiffBaseCommand(taskRunner, programSettings)
{
	protected override DiffTaskParameters GetDiffTaskParameters(ProgramSettings programSettings) =>
		DiffTaskParameters.ForTModLoader(programSettings);
}

public abstract class DiffBaseCommand : CancellableAsyncCommand<DiffCommandSettings>
{
	private readonly TaskRunner taskRunner;
	private readonly ProgramSettings programSettings;

	protected DiffBaseCommand(TaskRunner taskRunner, ProgramSettings programSettings)
	{
		this.taskRunner = taskRunner;
		this.programSettings = programSettings;
	}

	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		DiffCommandSettings settings,
		CancellationToken cancellationToken)
	{
		DiffTaskParameters diffTaskParameters = GetDiffTaskParameters(programSettings);

		if (settings.ResetDiffTimestamp) {
			diffTaskParameters.Cutoff.Set(null);
		}

		DiffTask diffTask = new DiffTask(diffTaskParameters);

		return await taskRunner.Run(diffTask, settings, cancellationToken: cancellationToken);
	}

	protected abstract DiffTaskParameters GetDiffTaskParameters(ProgramSettings programSettings);
}