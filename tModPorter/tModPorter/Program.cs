using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace tModPorter;

public class Program {
	public static async Task Main(string[] args) {
		string projectPath = GetProjectPath(args.LastOrDefault());

		tModPorter porter = new();

		try {
			await porter.ProcessProject(projectPath, UpdateProgress);
		}
		catch (Exception ex) {
			WriteLine();
			WriteLine();
			WriteLine(ex);
		}

		WriteLine();
		WriteLine();
		WriteLine("Press any key to exit...");
		if (!IsInputRedirected) // ReadKey will throw error when no console is present, such as launching on Linux
			ReadKey();
	}

	private static string GetProjectPath(string path) {
		while (true) {
			if (path != null) {
				path = path.Trim('"');
				path = Path.ChangeExtension(path, ".csproj");
				if (File.Exists(path))
					return path;
			}

			Clear();
			if (path != null)
				Write("The path you entered doesn't exist. ");

			WriteLine("Enter the path to the .csproj of the mod you want to port:");
			path = ReadLine();
		}
	}

	private static void UpdateProgress(ProgressUpdate update) {
		const int ConsoleWidth = 60;
		const int BarWidth = 10;

		switch (update) {
			case ProgressUpdate.Progress(int Pass, int CurrentFile, int FileCount):
				string bar = new string('#', CurrentFile* BarWidth / FileCount).PadRight(BarWidth, '-');
				var s = $"[{bar}] Pass {Pass}, {CurrentFile}/{FileCount}";
				Write($"\r{s,-ConsoleWidth}");
				break;
			default:
				WriteLine($"\r{update,-ConsoleWidth}");
				break;
		}
	}
}