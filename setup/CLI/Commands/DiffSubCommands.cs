using System.ComponentModel;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class DiffCommandSettings : BaseCommandSettings
{
	[CommandOption("-r|--reset-diff-timestamps")]
	[Description("Diff all files instead of only those which changed since the last diff/patch.")]
	public bool ResetDiffTimestamp { get; set; }
}

public sealed class DiffTerrariaCommand(TaskRunner taskRunner, ProgramSettings programSettings)
	: DiffBaseCommand(taskRunner, programSettings)
{
	protected override DiffTaskParameters GetDiffTaskParameters(ProgramSettings programSettings) =>
		DiffTaskParameters.ForTerraria(programSettings);

	protected override void ResetTimestamp(ProgramSettings programSettings) =>
		programSettings.TerrariaDiffCutoff = null;
}

public sealed class DiffTerrariaNetCoreCommand(TaskRunner taskRunner, ProgramSettings programSettings)
	: DiffBaseCommand(taskRunner, programSettings)
{
	protected override DiffTaskParameters GetDiffTaskParameters(ProgramSettings programSettings) =>
		DiffTaskParameters.ForTerrariaNetCore(programSettings);

	protected override void ResetTimestamp(ProgramSettings programSettings) =>
		programSettings.TerrariaNetCoreDiffCutoff = null;
}

public sealed class DiffTModLoaderCommand(TaskRunner taskRunner, ProgramSettings programSettings)
	: DiffBaseCommand(taskRunner, programSettings)
{
	protected override DiffTaskParameters GetDiffTaskParameters(ProgramSettings programSettings) =>
		DiffTaskParameters.ForTModLoader(programSettings);

	protected override void ResetTimestamp(ProgramSettings programSettings) =>
		programSettings.TModLoaderDiffCutoff = null;
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

	public override async Task<int> ExecuteAsync(
		CommandContext context,
		DiffCommandSettings settings,
		CancellationToken cancellationToken)
	{
		if (settings.ResetDiffTimestamp) {
			ResetTimestamp(programSettings);
		}

		DiffTask diffTask = new DiffTask(GetDiffTaskParameters(programSettings));

		return await taskRunner.Run(diffTask, settings.PlainProgress, cancellationToken: cancellationToken);
	}

	protected abstract DiffTaskParameters GetDiffTaskParameters(ProgramSettings programSettings);

	protected abstract void ResetTimestamp(ProgramSettings programSettings);
}