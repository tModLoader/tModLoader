using System;
using System.IO;
using System.Threading.Tasks;
using static System.Console;

namespace tModPorter;

public class Program {
	public static async Task Main(string[] args) {
		string projectPath = GetProjectPath(args);

		tModPorter porter = new();

		try {
			await porter.ProcessProject(projectPath, UpdateProgress);
		}
		catch (Exception ex) {
			WriteLine();
			WriteLine();
			WriteLine(ex);
		}

		ReadKey();
	}

	private static string GetProjectPath(string[] args) {
		// Check if the args have a valid file path
		if (args.Length > 0 && File.Exists(Path.ChangeExtension(args[0], ".csproj")))
			return args[0];

		// Ask the user for a path
		WriteLine("Enter the path to the .csproj of the mod you want to port");
		string filePath = Path.ChangeExtension(ReadLine(), ".csproj")!;

		// Continue asking until a valid file is passed
		while (!File.Exists(filePath)) {
			Clear();
			ForegroundColor = ConsoleColor.Yellow;
			WriteLine("The path you entered doesn't exist");
			ForegroundColor = ConsoleColor.Gray;

			filePath = Path.ChangeExtension(ReadLine(), ".csproj")!;
		}

		// Reset the console
		ForegroundColor = ConsoleColor.Gray;
		Clear();

		// Return the path passed in by the user
		return filePath;
	}

	private static void UpdateProgress(ProgressUpdate update) {
		const int ConsoleWidth = 60;
		const int BarWidth = 10;

		switch (update) {
			case ProgressUpdate.Progress(int Pass, int CurrentFile, int FileCount):
				string bar = new string('#', CurrentFile* BarWidth / FileCount).PadRight(BarWidth, '-');
				Write($"\r[{bar}] Pass {Pass}, {CurrentFile}/{FileCount}");
				break;
			default:
				WriteLine($"\r{update,-ConsoleWidth}");
				break;
		}
	}
}