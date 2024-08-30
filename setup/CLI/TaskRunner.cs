using Spectre.Console;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI;

public sealed class TaskRunner
{
	private readonly ProgramSettings programSettings;
	private readonly TargetsFilesUpdater targetsFilesUpdater;
	private readonly TerrariaExecutableSetter terrariaExecutableSetter;

	public TaskRunner(
		ProgramSettings programSettings,
		TargetsFilesUpdater targetsFilesUpdater,
		TerrariaExecutableSetter terrariaExecutableSetter)
	{
		this.programSettings = programSettings;
		this.targetsFilesUpdater = targetsFilesUpdater;
		this.terrariaExecutableSetter = terrariaExecutableSetter;
	}

	public async Task<int> Run(
		SetupOperation task,
		bool plainProgress,
		bool noPrompts = false,
		bool strict = false,
		CancellationToken cancellationToken = default)
	{
		var errorLogFile = Path.Combine(ProgramSettings.LogsDir, "error.log");

		try {
			programSettings.NoPrompts = noPrompts;

			SetupOperation.DeleteFile(errorLogFile);

			await terrariaExecutableSetter.FindAndSetTerrariaDirectoryIfNecessary(cancellationToken);
			targetsFilesUpdater.Update();

			await task.ConfigurationPrompt(cancellationToken);

			if (!task.StartupWarning())
				return 0;

			if (plainProgress) {
				await task.Run(new PlainConsoleProgress(), cancellationToken);
			}
			else {
				var table = new Table()
					.HideRowSeparators()
					.HideHeaders()
					.HideFooters()
					.NoBorder()
					.Expand()
					.AddColumn(string.Empty);

				try {
					await AnsiConsole
						.Live(new LiveTableRenderer(table))
						.Cropping(VerticalOverflowCropping.Bottom)
						.AutoClear(true)
						.StartAsync(async ctx => {
							using LiveConsoleProgress progress = new LiveConsoleProgress(ctx, table);
							await task.Run(progress, cancellationToken);
						});
				}
				finally {
					AnsiConsole.Write(table);
				}
			}

			task.FinishedPrompt();

			(string text, Color color) = GetCompletionText(task);
			AnsiConsole.Write(new Text(text, new Style(foreground: color, decoration: Decoration.Bold)));

			if (task.Failed() || (strict && task.Warnings())) {
				return 1;
			}
		}
		catch (OperationCanceledException) { Console.WriteLine("Cancelled"); }
		catch (Exception exception) when (exception is not OperationCanceledException) {
			AnsiConsole.MarkupLineInterpolated($"[red]{exception.GetType().FullName}[/]: {exception.Message}");

			SetupOperation.CreateDirectory(ProgramSettings.LogsDir);
			await File.WriteAllTextAsync(errorLogFile, exception.ToString(), cancellationToken);

			AnsiConsole.MarkupLineInterpolated($"Log written to: {Path.GetFullPath(errorLogFile)}");

			return 1;
		}

		return 0;
	}

	private static (string Text, Color Color) GetCompletionText(SetupOperation task)
	{
		if (task.Failed())
			return ("Failed", Color.Red);

		if (task.Warnings())
			return ("Completed with warnings", Color.Yellow);

		return ("Completed", Color.Green);
	}
}