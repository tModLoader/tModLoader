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

	public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
	{
		using var taskProgress = progress.StartTask("Updating localization files...");

		int result = RunCmd.Run("", "where", "python", cancel: cancellationToken);
		if (result != 0) {
			throw new InvalidOperationException("python 3 is needed to run this command");
		}

		if (!File.Exists(UpdateLocalizationFilesPath)) {
			throw new InvalidOperationException("UpdateLocalizationFiles.py missing");
		}

		Process? p = Process.Start(new ProcessStartInfo {
			FileName = "python",
			Arguments = Path.GetFileName(UpdateLocalizationFilesPath),
			WorkingDirectory = new FileInfo(UpdateLocalizationFilesPath).Directory!.FullName,
		});

		if (p == null) {
			throw new Exception("Failed to start python process.");
		}

		await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

		success = p.ExitCode == 0;
	}

	public override void FinishedPrompt()
	{
		if (success) {
			userPrompt.Inform("Success", "Make sure you diff tModLoader after this.");
		}
	}
}