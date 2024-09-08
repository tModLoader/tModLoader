using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.CLI;

public sealed class PlainConsoleProgress : IProgress
{
	public ITaskProgress StartTask(string description)
	{
		Console.WriteLine(description);

		return new TaskProgress();
	}

	private sealed class TaskProgress : ITaskProgress
	{
		public void Dispose() { }

		public void SetMaxProgress(int max) { }

		public void SetCurrentProgress(int current) { }

		public void ReportStatus(string status) => Console.WriteLine(Indent(status));

		public IWorkItemProgress StartWorkItem(string status) => new WorkItemProgress(status);

		private static string Indent(string status) => $"  {status.ReplaceLineEndings("\r\n  ")}";
	}

	private sealed class WorkItemProgress : IWorkItemProgress
	{
		public WorkItemProgress(string status)
		{
			WriteStatus(status);
		}

		public void Dispose() { }

		public void ReportStatus(string status) => WriteStatus(status);

		private void WriteStatus(string status) => Console.WriteLine($"  {status}");
	}
}