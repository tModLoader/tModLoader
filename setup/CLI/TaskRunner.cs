using Spectre.Console;
using Terraria.ModLoader.Setup.CLI.Commands;
using Terraria.ModLoader.Setup.Core;

namespace Terraria.ModLoader.Setup.CLI;

public sealed class TaskRunner
{
	private readonly ProgramSettings programSettings;
	private readonly WorkspaceInfo workspaceInfo;

	public TaskRunner(ProgramSettings programSettings, WorkspaceInfo workspaceInfo)
	{
		this.programSettings = programSettings;
		this.workspaceInfo = workspaceInfo;
	}

	public async Task<int> Run(
		SetupOperation task,
		BaseCommandSettings settings,
		bool noPrompts = false,
		CancellationToken cancellationToken = default)
	{
		var errorLogFile = Path.Combine(ProgramSettings.LogsDir, "error.log");

		try {
			programSettings.NoPrompts = noPrompts;

			SetupOperation.DeleteFile(errorLogFile);

			workspaceInfo.UpdateGitInfo();

			await task.ConfigurationPrompt(cancellationToken);

			if (!task.StartupWarning())
				return 0;

			if (settings.PlainProgress) {
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
			AnsiConsole.Write(new Text(text + '\n', new Style(foreground: color, decoration: Decoration.Bold)));

			if (task.Failed() || (settings.Strict && task.Warnings())) {
				return 1;
			}
		}
		catch (OperationCanceledException) { Console.WriteLine("Cancelled"); }
		catch (Exception exception)
		{
			if (exception is AggregateException aggregateException) {
				exception = aggregateException.Flatten();
			}

			AnsiConsole.WriteException(exception);

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