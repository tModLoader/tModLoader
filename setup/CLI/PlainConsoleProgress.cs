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
		private int? maxProgress;

		public void Dispose() { }

		public void SetMaxProgress(int max) => maxProgress = max;

		public void SetCurrentProgress(int current)
		{
			if (maxProgress.HasValue) {
				Console.WriteLine(Indent($"Progress: {current}/{maxProgress.Value}"));
			}
		}

		public void ReportStatus(string status, bool overwrite = false)
		{
			Console.WriteLine(Indent(status));
		}

		private static string Indent(string status) => $"  {status.ReplaceLineEndings("\r\n  ")}";
	}
}