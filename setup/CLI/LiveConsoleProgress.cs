using System.Collections.Concurrent;
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

		private readonly int headerRow;
		private readonly int statusRow;

		private int? maxProgress;
		private int? currentProgress;
		private string lastStatus = string.Empty;
		private readonly ConcurrentDictionary<Guid, GenericWorkItemProgress> currentWorkItems = [];

		public TaskProgress(string description, Table table)
		{
			this.description = description;
			this.table = table;

			headerRow = table.Rows.Count;
			statusRow = headerRow + 1;

			table.AddEmptyRow().AddEmptyRow();

			UpdateHeaderRow();
		}

		public void Dispose()
		{
			table.RemoveRow(statusRow);
		}

		public void SetMaxProgress(int max)
		{
			maxProgress = max;
			UpdateHeaderRow();
		}

		public void SetCurrentProgress(int current)
		{
			currentProgress = current;
			UpdateHeaderRow();
		}

		public void ReportStatus(string status)
		{
			string[] parts = [lastStatus, Indent(status)];
			status = string.Join(Environment.NewLine, parts.Where(x => !string.IsNullOrWhiteSpace(x)));

			table.UpdateCell(statusRow, 0, new Text(status));
			lastStatus = status;
		}

		public IWorkItemProgress StartWorkItem(string status)
		{
			lastStatus = string.Empty;
			var progress = new GenericWorkItemProgress(
				status,
				UpdateStatusFromWorkItems,
				x => currentWorkItems.Remove(x.Id, out _));

			currentWorkItems.TryAdd(progress.Id, progress);
			UpdateStatusFromWorkItems();

			return progress;
		}

		private void UpdateStatusFromWorkItems()
		{
			lock (table) {
				table.UpdateCell(statusRow, 0, new Text(string.Join(Environment.NewLine, currentWorkItems.Select(x => Indent(x.Value.Status)))));
			}
		}

		private static string Indent(string status) => $"  {status.ReplaceLineEndings("\r\n  ")}";

		private void UpdateHeaderRow()
		{
			string progress = string.Empty;

			if (currentProgress is int i && maxProgress is int n) {
				string maxProgressString = n.ToString();
				progress = $"[{(currentProgress != maxProgress ? "red" : "green")}]{i.ToString().PadLeft(maxProgressString.Length)}[/]/[green]{maxProgressString}[/]";
			}

			table.UpdateCell(headerRow, 0, new Markup($"{description,-70}{progress}"));
		}
	}
}