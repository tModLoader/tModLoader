using System.Diagnostics;
using System.Timers;
using Spectre.Console;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Timer = System.Timers.Timer;

namespace Terraria.ModLoader.Setup.CLI;

public sealed class LiveConsoleProgress : IProgress, IDisposable
{
	private readonly LiveDisplayContext context;
	private readonly Table table;
	private readonly Timer timer;

	public LiveConsoleProgress(LiveDisplayContext context, Table table)
	{
		this.context = context;
		this.table = table;

		timer = new Timer(TimeSpan.FromMilliseconds(50));
		timer.Elapsed += RefreshTable;
	}

	public ITaskProgress StartTask(string description)
	{
		Debug.Assert(description.Length <= 60);

		timer.Start();

		return new TaskProgress(description, table.Rows.Count, this);
	}

	public void Dispose()
	{
		timer.Stop();
		timer.Elapsed -= RefreshTable;
		timer.Dispose();
	}

	private void RefreshTable(object? sender, ElapsedEventArgs e)
	{
		context.Refresh();
	}

	private sealed class TaskProgress : ITaskProgress
	{
		private readonly string description;
		private readonly int row;
		private readonly LiveConsoleProgress parent;
		private readonly int detailsRow;

		private int? maxProgress;
		private int? currentProgress;
		private string lastStatus = string.Empty;

		public TaskProgress(string description, int row, LiveConsoleProgress parent)
		{
			this.description = description;
			this.row = row;
			detailsRow = row + 1;
			this.parent = parent;

			UpdateTaskRow();
		}

		public void Dispose()
		{
			if (detailsRow < parent.table.Rows.Count)
				parent.table.RemoveRow(detailsRow);
		}

		public void SetMaxProgress(int max)
		{
			maxProgress = max;
			UpdateTaskRow();
		}

		public void SetCurrentProgress(int current)
		{
			currentProgress = current;
			UpdateTaskRow();
		}

		public void ReportStatus(string status, bool overwrite = false)
		{
			AddMissingRows(detailsRow);

			if (overwrite) {
				lastStatus = Indent(status);
				parent.table.UpdateCell(detailsRow, 0, new Text(lastStatus));
			}
			else {
				string[] parts = [lastStatus, Indent(status)];
				lastStatus = string.Join(Environment.NewLine, parts.Where(x => !string.IsNullOrWhiteSpace(x)));

				parent.table.UpdateCell(detailsRow, 0, new Text(lastStatus));
			}
		}

		private static string Indent(string status) => $"  {status.ReplaceLineEndings("\r\n  ")}";

		private void UpdateTaskRow()
		{
			AddMissingRows(row);

			string progress = string.Empty;

			if (currentProgress.HasValue && maxProgress.HasValue) {
				string maxProgressString = maxProgress.ToString();
				progress = $"[{(currentProgress != maxProgress ? "red" : "green")}]{currentProgress.ToString().PadLeft(maxProgressString.Length)}[/]/[green]{maxProgressString}[/]";
			}

			parent.table.UpdateCell(row, 0, new Markup($"{description,-70}{progress}"));
		}

		private void AddMissingRows(int rowIndex)
		{
			int rowsCount = parent.table.Rows.Count;
			for (int i = 0; i <= rowIndex - rowsCount; i++) {
				parent.table.AddEmptyRow();
			}
		}
	}
}