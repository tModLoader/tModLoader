using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Terraria.ModLoader.Setup;

internal class UpdateLocalizationFilesTask : SetupOperation
{
	private const string UpdateLocalizationFilesPath = $"solutions/UpdateLocalizationFiles.py";

	public UpdateLocalizationFilesTask(ITaskInterface taskInterface) : base(taskInterface)
	{
	}

	public override void Run()
	{
		int result = Program.RunCmd("", "where", "python");
		if (result != 0) {
			MessageBox.Show("python 3 is needed to run this command", "python not found on PATH", MessageBoxButton.OK);
			taskInterface.SetStatus("Cancelled");
			return;
		}

		if (!File.Exists(UpdateLocalizationFilesPath)) {
			MessageBox.Show("UpdateLocalizationFiles.py is missing somehow", "UpdateLocalizationFiles.py missing", MessageBoxButton.OK);
			taskInterface.SetStatus("Cancelled");
			return;
		}

		Process p = Process.Start(new ProcessStartInfo {
			FileName = "python",
			Arguments = Path.GetFileName(UpdateLocalizationFilesPath),
			WorkingDirectory = new FileInfo(UpdateLocalizationFilesPath).Directory.FullName,
		});
		p.WaitForExit();
		// MessageBox.Show("Success. Make sure you diff tModLoader after this");
	}
}