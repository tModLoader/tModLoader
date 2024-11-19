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
	private TaskProgress? currentTaskProgress;

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
		Debug.Assert(currentTaskProgress == null);

		timer.Start();

		currentTaskProgress = new TaskProgress(description, table, () => currentTaskProgress = null);

		return currentTaskProgress;
	}

	public void Dispose()
	{
		timer.Dispose();
	}

	private void RefreshTable(object? sender, ElapsedEventArgs e)
	{
		currentTaskProgress?.Refresh();
		context.Refresh();
	}

	private sealed class TaskProgress : TaskProgressBase
	{
		private readonly Table table;
		private readonly Action onCompleted;

		private readonly int headerRow;
		private readonly int statusRow;

		public TaskProgress(string description, Table table, Action onCompleted) : base(description)
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
			onCompleted();
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
			table.UpdateCell(statusRow, 0, Markup.Escape(State.Status));
		}
	}
}