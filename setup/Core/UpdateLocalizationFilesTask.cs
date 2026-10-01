using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core;

public sealed class UpdateLocalizationFilesTask : SetupOperation
{
	private const string UpdateLocalizationFilesPath = "solutions/UpdateLocalizationFiles.py";

	private readonly IUserPrompt userPrompt;
	private bool success = false;

	public UpdateLocalizationFilesTask(IServiceProvider serviceProvider)
	{
		userPrompt = serviceProvider.GetRequiredService<IUserPrompt>();
	}

	public override Task Run(IProgress progress, CancellationToken cancellationToken = default)
	{
		using var taskProgress = progress.StartTask("Updating localization files...");

		string? err = null;
		int exitCode = RunCmd.Run(
			Path.GetDirectoryName(UpdateLocalizationFilesPath)!,
			"python",
			Path.GetFileName(UpdateLocalizationFilesPath),
			output: s => taskProgress.ReportStatus(s),
			error: s => err = s,
			cancel: cancellationToken);

		if (!string.IsNullOrEmpty(err))
			throw new Exception(err);

		success = exitCode == 0;

		return Task.CompletedTask;
	}

	public override void FinishedPrompt()
	{
		if (success) {
			userPrompt.Inform("Success", "Make sure you diff tModLoader after this.");
		}
	}
}