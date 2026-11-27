using System.ComponentModel;
using CodeChicken.DiffPatch;
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

	[CommandOption("-s|--safe-mode")]
	[Description("Abort instead of overwriting source files edited since the last patch or diff.")]
	public bool SafeMode { get; init; }

	/// <summary>
	///     In safe mode, checks the layers about to be patched for edits which patching would discard. Writes an
	///     explanation to stderr and returns false if any are found.
	/// </summary>
	public bool CheckSafeMode(params PatchTaskParameters[] layers)
	{
		if (!SafeMode) {
			return true;
		}

		foreach (PatchTaskParameters layer in layers) {
			if (PatchTask.FindEditedFile(layer) is not string editedFile) {
				continue;
			}

			Console.Error.WriteLine(
				$"""
				 {layer.PatchedDir} has edits newer than the last patch or diff, including:
				   {editedFile}
				 Safe mode: not patching. Capture them with 'diff {layer.Name}', or run again without --safe-mode to discard them.
				 """);

			return false;
		}

		return true;
	}
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
		PatchTaskParameters parameters = GetPatchTaskParameters(programSettings);

		if (!settings.CheckSafeMode(parameters)) {
			return 1;
		}

		PatchTask patchTask = new PatchTask(parameters, serviceProvider);

		return await taskRunner.Run(patchTask, settings, settings.NoPrompts, cancellationToken: cancellationToken);
	}

	protected abstract PatchTaskParameters GetPatchTaskParameters(ProgramSettings programSettings);
}