using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Terraria.ModLoader.Setup
{
	public abstract class Task
	{
		protected class WorkItem
		{
			public readonly string status;
			public readonly Action action;

			public WorkItem(string status, Action action) {
				this.status = status;
				this.action = action;
			}
		}

		protected void ExecuteParallel(List<WorkItem> items, bool resetProgress = true, int maxDegree = 0) {
			try {
				if (resetProgress) {
					taskInterface.SetMaxProgress(items.Count());
					progress = 0;
				}

				var working = new List<string>();
				Action updateStatus = () => taskInterface.SetStatus(string.Join("\r\n", working));

				Parallel.ForEach(items,
					new ParallelOptions { MaxDegreeOfParallelism = maxDegree > 0 ? maxDegree : Environment.ProcessorCount },
					item => {
						taskInterface.CancellationToken().ThrowIfCancellationRequested();
						lock (working) {
							working.Add(item.status);
							updateStatus();
						}
						item.action();
						lock (working) {
							working.Remove(item.status);
							taskInterface.SetProgress(++progress);
							updateStatus();
						}
					});
			} catch (AggregateException ex) {
				var actual = ex.Flatten().InnerExceptions.Where(e => !(e is OperationCanceledException));
				if (!actual.Any())
					throw new OperationCanceledException();

				throw new AggregateException(actual);
			}
		}

		public static string RelPath(string basePath, string path) {
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

		public static void CreateDirectory(string dir) {
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		public static void CreateParentDirectory(string path) {
			CreateDirectory(Path.GetDirectoryName(path));
		}

		public static void Copy(string from, string to) {
			CreateParentDirectory(to);
			File.Copy(from, to, true);
		}
				

		protected readonly ITaskInterface taskInterface;
		protected int progress;

		protected Task(ITaskInterface taskInterface) {
			this.taskInterface = taskInterface;
		}

		/// <summary>
		/// Run the task, any exceptions thrown will be written to a log file and update the status label with the exception message
		/// </summary>
		public abstract void Run();

		/// <summary>
		/// Display a configuration dialog. Return false if the operation should be cancelled.
		/// </summary>
		public virtual bool ConfigurationDialog() {
			return true;
		}

		/// <summary>
		/// Display a startup warning dialog
		/// </summary>
		/// <returns>true if the task should continue</returns>
		public virtual bool StartupWarning() {
			return true;
		}

		/// <summary>
		/// Will prevent successive tasks from executing and cause FinishedDialog to be called
		/// </summary>
		/// <returns></returns>
		public virtual bool Failed() {
			return false;
		}

		/// <summary>
		/// Will cause FinishedDialog to be called if warnings are not supressed
		/// </summary>
		/// <returns></returns>
		public virtual bool Warnings() {
			return false;
		}

		/// <summary>
		/// Called to display a finished dialog if Failures() || warnings are not supressed and Warnings()
		/// </summary>
		public virtual void FinishedDialog() {}
	}
}
