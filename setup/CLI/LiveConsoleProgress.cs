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

		this.timer = new Timer(TimeSpan.FromMilliseconds(50));
		this.timer.Elapsed += RefreshTable;
	}

	public ITaskProgress StartTask(string description)
	{
		Debug.Assert(description.Length <= 60);

		this.timer.Start();

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
		if (table.Rows.Count > 0) {
			context.Refresh();
		}
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
			this.detailsRow = row + 1;
			this.parent = parent;

			this.UpdateTaskRow();
		}

		public void Dispose()
		{
			parent.table.RemoveRow(this.detailsRow);
			parent.context.Refresh();
		}

		public void SetMaxProgress(int max)
		{
			this.maxProgress = max;
			this.UpdateTaskRow();
		}

		public void SetCurrentProgress(int current)
		{
			this.currentProgress = current;
			this.UpdateTaskRow();
		}

		public void ReportStatus(string status, bool overwrite = false)
		{
			AddMissingRows(this.detailsRow);

			if (overwrite) {
				lastStatus = Indent(status);
				this.parent.table.UpdateCell(this.detailsRow, 0, new Text(lastStatus));
			}
			else {
				string[] parts = [lastStatus, Indent(status)];
				lastStatus = string.Join(Environment.NewLine, parts.Where(x => !string.IsNullOrWhiteSpace(x)));

				this.parent.table.UpdateCell(this.detailsRow, 0, new Text(lastStatus));
			}
		}

		private static string Indent(string status) => $"  {status.ReplaceLineEndings("\r\n  ")}";

		private void UpdateTaskRow()
		{
			AddMissingRows(this.row);

			string progress = string.Empty;

			if (this.currentProgress.HasValue && this.maxProgress.HasValue) {
				string maxProgressString = this.maxProgress.ToString();
				progress = $"[{(currentProgress != maxProgress ? "red" : "green")}]{currentProgress.ToString().PadLeft(maxProgressString.Length)}[/]/[green]{maxProgressString}[/]";
			}

			this.parent.table.UpdateCell(this.row, 0, new Markup($"{this.description,-70}{progress}"));
		}

		private void AddMissingRows(int rowIndex)
		{
			int rowsCount = this.parent.table.Rows.Count;
			for (int i = 0; i <= rowIndex - rowsCount; i++) {
				this.parent.table.AddEmptyRow();
			}
		}
	}
}