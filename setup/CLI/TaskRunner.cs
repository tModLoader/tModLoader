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
		CancellationToken cancellationToken = default)
	{
		var errorLogFile = Path.Combine(programSettings.LogsDir, "error.log");

		try {
			programSettings.NoPrompts = noPrompts;

			SetupOperation.DeleteFile(errorLogFile);

			await terrariaExecutableSetter.FindAndSetTerrariaDirectoryIfNecessary(cancellationToken);
			targetsFilesUpdater.Update();

			if (!await task.ConfigurationPrompt(cancellationToken))
				return 0;

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
						.Live(table)
						.Overflow(VerticalOverflow.Visible)
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

			Text text = task.Failed()
				? new Text("Failed", new Style(foreground: Color.Red, decoration: Decoration.Bold))
				: new Text("Done", new Style(foreground: Color.Green, decoration: Decoration.Bold));

			AnsiConsole.Write(text);
		}
		catch (OperationCanceledException) { Console.WriteLine("Cancelled"); }
		catch (Exception exception) when (exception is not OperationCanceledException) {
			AnsiConsole.MarkupLineInterpolated($"[red]{exception.GetType().FullName}[/]: {exception.Message}");

			SetupOperation.CreateDirectory(programSettings.LogsDir);
			await File.WriteAllTextAsync(errorLogFile, exception.ToString(), cancellationToken);

			AnsiConsole.MarkupLineInterpolated($"Log written to: {Path.GetFullPath(errorLogFile)}");

			return 1;
		}

		return 0;
	}
}