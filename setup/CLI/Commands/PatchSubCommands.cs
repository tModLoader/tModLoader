using System.ComponentModel;
using DiffPatch;
using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public class PatchCommandSettings : BaseCommandSettings
{
	[CommandOption("-m|--patch-mode <MODE>")]
	[Description("Set the patch mode.")]
	[DefaultValue(Patcher.Mode.EXACT)]
	public Patcher.Mode PatchMode { get; init; }

	[CommandOption("-f|--no-prompts")]
	[Description("Execute command without prompting for confirmation or any missing information.")]
	public bool NoPrompts { get; init; }
}
public sealed class PatchTerrariaCommand(TaskRunner taskRunner, ProgramSettings programSettings, IServiceProvider serviceProvider)
	: PatchBaseCommand(taskRunner, programSettings, serviceProvider)
{
	protected override PatchTaskParameters GetPatchTaskParameters(ProgramSettings programSettings) =>
		PatchTaskParameters.ForTerraria(programSettings);
}

public sealed class PatchTerrariaNetCoreCommand(TaskRunner taskRunner, ProgramSettings programSettings, IServiceProvider serviceProvider)
	: PatchBaseCommand(taskRunner, programSettings, serviceProvider)
{
	protected override PatchTaskParameters GetPatchTaskParameters(ProgramSettings programSettings) =>
		PatchTaskParameters.ForTerrariaNetCore(programSettings);
}

public sealed class PatchTModLoaderCommand(TaskRunner taskRunner, ProgramSettings programSettings, IServiceProvider serviceProvider)
	: PatchBaseCommand(taskRunner, programSettings, serviceProvider)
{
	protected override PatchTaskParameters GetPatchTaskParameters(ProgramSettings programSettings) =>
		PatchTaskParameters.ForTModLoader(programSettings);
}

public abstract class PatchBaseCommand : CancellableAsyncCommand<PatchCommandSettings>
{
	private readonly TaskRunner taskRunner;
	private readonly ProgramSettings programSettings;
	private readonly IServiceProvider serviceProvider;

	protected PatchBaseCommand(
		TaskRunner taskRunner,
		ProgramSettings programSettings,
		IServiceProvider serviceProvider)
	{
		this.taskRunner = taskRunner;
		this.programSettings = programSettings;
		this.serviceProvider = serviceProvider;
	}

	protected override async Task<int> ExecuteAsync(
		CommandContext context,
		PatchCommandSettings settings,
		CancellationToken cancellationToken)
	{
		programSettings.PatchMode = settings.PatchMode;
		PatchTask patchTask = new PatchTask(GetPatchTaskParameters(programSettings), serviceProvider);

		return await taskRunner.Run(patchTask, settings, settings.NoPrompts, cancellationToken: cancellationToken);
	}

	protected abstract PatchTaskParameters GetPatchTaskParameters(ProgramSettings programSettings);
}