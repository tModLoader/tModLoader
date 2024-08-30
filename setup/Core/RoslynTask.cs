using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core
{
	public abstract class RoslynTask : SetupOperation
	{
		private static bool MSBuildFound;

		protected RoslynTask(RoslynTaskParameters parameters)
		{
			this.Parameters = parameters;
		}

		protected RoslynTaskParameters Parameters { get; }

		protected abstract string Status { get; }

		protected virtual int MaxDegreeOfParallelism => 0;

		public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			using var taskProgress = GetTaskProgress(progress);
			using var workspace = CreateWorkspace(taskProgress);
			// todo proper error log
			workspace.WorkspaceFailed += (o, e) => Console.Error.WriteLine(e.Diagnostic.Message);

			taskProgress.ReportStatus($"Loading project '{Parameters.ProjectPath}'...'");

			// Attach progress reporter so we print projects as they are loaded.
			var project = await workspace.OpenProjectAsync(Parameters.ProjectPath, cancellationToken: cancellationToken);
			var workItems = project.Documents.Select(doc => new WorkItem(Status + " " + doc.Name, async ct => {
				var newDoc = await Process(doc, ct);
				var before = await doc.GetTextAsync(ct);
				var after = await newDoc.GetTextAsync(ct);
				if (before != after)
					await File.WriteAllTextAsync(newDoc.FilePath!, after.ToString(), ct);
			}));

			await ExecuteParallel(workItems.ToList(), taskProgress, maxDegreeOfParallelism: MaxDegreeOfParallelism, cancellationToken: cancellationToken);
		}

		private MSBuildWorkspace CreateWorkspace(ITaskProgress taskProgress) {
			if (!MSBuildFound) {
				taskProgress.ReportStatus("Finding MSBuild");
				var vsInst = MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(inst => inst.Version).First();
				MSBuildLocator.RegisterInstance(vsInst);
				taskProgress.ReportStatus($"Found MSBuild {vsInst.Version} at {vsInst.MSBuildPath}");
				MSBuildFound = true;
			}

			return MSBuildWorkspace.Create();
		}

		protected abstract ITaskProgress GetTaskProgress(IProgress progress);

		protected abstract Task<Document> Process(Document doc, CancellationToken cancellationToken = default);
	}
}