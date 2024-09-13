using Spectre.Console.Cli;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI.Commands;

public sealed class RegenSourceCommand : CancellableAsyncCommand<PatchCommandSettings>
{
	private readonly TaskRunner taskRunner;
	private readonly ProgramSettings programSettings;
	private readonly IServiceProvider serviceProvider;

	public RegenSourceCommand(
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

		return await taskRunner.Run(new RegenSourceTask(serviceProvider), settings, settings.NoPrompts, cancellationToken: cancellationToken);
	}
}