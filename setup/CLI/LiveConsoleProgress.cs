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

		return new TaskProgress(description, table);
	}

	public void Dispose()
	{
		timer.Dispose();
	}

	private void RefreshTable(object? sender, ElapsedEventArgs e)
	{
		context.Refresh();
	}

	private sealed class TaskProgress : ITaskProgress
	{
		private readonly string description;
		private readonly Table table;

		private readonly int HeaderRow;
		private readonly int StatusRow;

		private int? maxProgress;
		private int? currentProgress;
		private string lastStatus = string.Empty;

		public TaskProgress(string description, Table table)
		{
			this.description = description;
			this.table = table;

			HeaderRow = table.Rows.Count;
			StatusRow = HeaderRow + 1;

			table.AddEmptyRow().AddEmptyRow();

			UpdateTaskRow();
		}

		public void Dispose()
		{
			table.RemoveRow(StatusRow);
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
			if (overwrite) {
				lastStatus = Indent(status);
			}
			else {
				string[] parts = [lastStatus, Indent(status)];
				lastStatus = string.Join(Environment.NewLine, parts.Where(x => !string.IsNullOrWhiteSpace(x)));
			}

			table.UpdateCell(StatusRow, 0, new Text(lastStatus));
		}

		private static string Indent(string status) => $"  {status.ReplaceLineEndings("\r\n  ")}";

		private void UpdateTaskRow()
		{
			string progress = string.Empty;

			if (currentProgress is int i && maxProgress is int n) {
				string maxProgressString = n.ToString();
				progress = $"[{(currentProgress != maxProgress ? "red" : "green")}]{i.ToString().PadLeft(maxProgressString.Length)}[/]/[green]{maxProgressString}[/]";
			}

			table.UpdateCell(HeaderRow, 0, new Markup($"{description,-70}{progress}"));
		}
	}
}