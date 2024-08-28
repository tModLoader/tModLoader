using System.Diagnostics;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core;

public sealed class UpdateLocalizationFilesTask : SetupOperation
{
	private const string UpdateLocalizationFilesPath = "solutions/UpdateLocalizationFiles.py";

	public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
	{
		using var taskProgress = progress.StartTask("Updating localization files...");

		int result = RunCmd.Run("", "where", "python");
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

		if (p != null) {
			await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}