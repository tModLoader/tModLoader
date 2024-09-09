using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.Core
{
	public abstract class SetupOperation
	{
		protected delegate void UpdateStatus(string status);
		protected delegate ValueTask Worker(UpdateStatus updateStatus, CancellationToken cancellationToken = default);

		protected class WorkItem
		{
			public WorkItem(string status, Worker worker) {
				Status = status;
				Worker = worker;
			}

			public WorkItem(string status, Func<CancellationToken, ValueTask> action) : this(status, (_, ct) => action(ct)) { }

			public WorkItem(string status, Action action) : this(status, (_, _) => { action(); return ValueTask.CompletedTask; }) { }

			public string Status { get; set; }

			public Worker Worker { get; }
		}

		protected async Task ExecuteParallel(
			List<WorkItem> items,
			ITaskProgress progress,
			int? maxDegreeOfParallelism = null,
			CancellationToken cancellationToken = default)
		{
			int currentProgress = 0;

			progress.SetCurrentProgress(0);
			progress.SetMaxProgress(items.Count);

			await Parallel.ForEachAsync(
				items,
				new ParallelOptions {
					MaxDegreeOfParallelism = maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism.Value : Environment.ProcessorCount,
					CancellationToken = cancellationToken,
				},
				async (item, ct) => {
					using var workItemProgress = progress.StartWorkItem(item.Status);

					void SetStatus(string s)
					{
						item.Status = s;
						workItemProgress.ReportStatus(s);
					}

					try {
						await item.Worker(SetStatus, ct);
					}
					catch(OperationCanceledException) { }
					catch (Exception exception) {
						throw new Exception($"Work item failed: \"{item.Status}\"", exception);
					}

					progress.SetCurrentProgress(Interlocked.Increment(ref currentProgress));
				})
				.WithAggregateException();
		}

		public static void CreateDirectory(string dir) {
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		public static void CreateParentDirectory(string path) {
			CreateDirectory(Path.GetDirectoryName(path)!);
		}

		public static void DeleteFile(string path) {
			if (File.Exists(path)) {
				File.SetAttributes(path,FileAttributes.Normal);
				File.Delete(path);
			}
		}

		protected static void Copy(string from, string to) {
			CreateParentDirectory(to);

			if (File.Exists(to)) {
				File.SetAttributes(to,FileAttributes.Normal);
			}

			File.Copy(from, to, true);
		}

		protected static IEnumerable<(string file, string relPath)> EnumerateFiles(string dir) =>
			Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
				.Select(path => (file: PathUtils.WithUnixSeparators(path), relPath: PathUtils.WithUnixSeparators(RelPath(dir, path))));

		private static string RelPath(string basePath, string path) {
			if (path.Last() == Path.DirectorySeparatorChar)
				path = path.Substring(0, path.Length - 1);

			if (basePath.Last() != Path.DirectorySeparatorChar)
				basePath += Path.DirectorySeparatorChar;

			if (path+Path.DirectorySeparatorChar == basePath) return "";

			if (!path.StartsWith(basePath)) {
				path = Path.GetFullPath(path);
				basePath = Path.GetFullPath(basePath);
			}

			if(!path.StartsWith(basePath))
				throw new ArgumentException("Path \""+path+"\" is not relative to \""+basePath+"\"");

			return path.Substring(basePath.Length);
		}
		protected static bool DeleteEmptyDirs(string dir) {
			if (!Directory.Exists(dir))
				return true;

			return DeleteEmptyDirsRecursion(dir);
		}

		private static bool DeleteEmptyDirsRecursion(string dir) {
			bool allEmpty = true;

			foreach (string subDir in Directory.EnumerateDirectories(dir))
				allEmpty &= DeleteEmptyDirsRecursion(subDir);

			if (!allEmpty || Directory.EnumerateFiles(dir).Any())
				return false;

			Directory.Delete(dir);

			return true;
		}

		/// <summary>
		///     Run the task, any exceptions thrown will be written to a log file and update the status label with the exception
		///     message
		/// </summary>
		public abstract Task Run(IProgress progress, CancellationToken cancellationToken = default);

		/// <summary>
		///     Display a configuration dialog. Return false if the operation should be cancelled.
		/// </summary>
		/// <param name="cancellationToken"></param>
		public virtual ValueTask ConfigurationPrompt(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

		/// <summary>
		///     Display a startup warning dialog
		/// </summary>
		/// <returns>true if the task should continue</returns>
		public virtual bool StartupWarning() => true;

		/// <summary>
		///     Returns a value indicating whether the task failed.
		/// </summary>
		/// <returns>A value indicating whether the task failed</returns>
		public virtual bool Failed() => false;

		/// <summary>
		///     Returns a value indicating whether the task completed with warnings.
		/// </summary>
		/// <returns>A value indicating whether the task completed with warnings</returns>
		public virtual bool Warnings() => false;

		/// <summary>
		///     Called to display a finished dialog if Failures() || warnings are not supressed and Warnings()
		/// </summary>
		public virtual void FinishedPrompt() { }
	}
}