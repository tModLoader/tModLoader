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
	private readonly ConcurrentDictionary<TaskProgress, object?> taskProgresses = [];

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

		var taskProgress = new TaskProgress(description, table, progress => taskProgresses.Remove(progress, out _));
		taskProgresses.TryAdd(taskProgress, null);

		return taskProgress;
	}

	public void Dispose()
	{
		timer.Dispose();
	}

	private void RefreshTable(object? sender, ElapsedEventArgs e)
	{
		foreach (TaskProgress taskProgress in taskProgresses.Keys) {
			taskProgress.Refresh();
		}
		context.Refresh();
	}

	private sealed class TaskProgress : TaskProgressBase
	{
		private readonly Table table;
		private readonly Action<TaskProgress> onCompleted;

		private readonly int headerRow;
		private readonly int statusRow;

		public TaskProgress(string description, Table table, Action<TaskProgress> onCompleted) : base(description)
		{
			this.table = table;
			this.onCompleted = onCompleted;

			headerRow = table.Rows.Count;
			statusRow = headerRow + 1;

			table.AddEmptyRow().AddEmptyRow();
		}

		public override void Dispose()
		{
			UpdateHeaderRow();
			table.RemoveRow(statusRow);
			onCompleted(this);
		}

		public void Refresh()
		{
			UpdateHeaderRow();
			UpdateStatusRow();
		}

		protected override string TransformStatus(string status) => $"  {status.ReplaceLineEndings("\r\n  ")}";

		private void UpdateHeaderRow()
		{
			string progress = string.Empty;
			var state = State;
			if (state.Max > 0) {
				string maxProgressString = state.Max.ToString();
				progress = $"[{(state.Current != state.Max ? "red" : "green")}]{state.Current.ToString().PadLeft(maxProgressString.Length)}[/]/[green]{maxProgressString}[/]";
			}

			table.UpdateCell(headerRow, 0, new Markup($"{Description,-70}{progress}"));
		}

		private void UpdateStatusRow()
		{
			table.UpdateCell(statusRow, 0, State.Status);
		}
	}
}