using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core
{
	public abstract class RoslynTask : SetupOperation
	{
		private static bool MSBuildFound;

		private readonly ICSharpProjectSelectionPrompt projectSelectionPrompt;

		protected RoslynTask(ICSharpProjectSelectionPrompt projectSelectionPrompt)
		{
			this.projectSelectionPrompt = projectSelectionPrompt;
		}

		protected string? ProjectPath { get; private set; }
		protected abstract string Status { get; }
		protected virtual int MaxDegreeOfParallelism => 0;

		public override async ValueTask<bool> ConfigurationPrompt(CancellationToken cancellationToken = default)
		{
			ProjectPath = await projectSelectionPrompt.Prompt(ProjectPath, cancellationToken).ConfigureAwait(false);

			return File.Exists(ProjectPath);
		}

		public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			using var taskProgress = GetTaskProgress(progress);
			using MSBuildWorkspace workspace = CreateWorkspace(taskProgress);
			// todo proper error log
			workspace.WorkspaceFailed += (o, e) => Console.Error.WriteLine(e.Diagnostic.Message);

			Console.WriteLine($"Loading project '{ProjectPath}'");

			// Attach progress reporter so we print projects as they are loaded.
			Project project = await workspace.OpenProjectAsync(ProjectPath!, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
			IEnumerable<WorkItem> workItems = project.Documents.Select(doc => new WorkItem(Status + " " + doc.Name,
				async ct => {
					Document newDoc = await Process(doc, ct).ConfigureAwait(false);
					SourceText before = await doc.GetTextAsync(ct).ConfigureAwait(false);
					SourceText after = await newDoc.GetTextAsync(ct).ConfigureAwait(false);
					if (before != after) {
						await File.WriteAllTextAsync(newDoc.FilePath!, after.ToString(), ct).ConfigureAwait(false);
					}
				}));

			await ExecuteParallel(workItems.ToList(), taskProgress, maxDegreeOfParallelism: MaxDegreeOfParallelism,
				cancellationToken: cancellationToken).ConfigureAwait(false);
		}

		private MSBuildWorkspace CreateWorkspace(ITaskProgress taskProgress)
		{
			if (!MSBuildFound) {
				taskProgress.ReportStatus("Finding MSBuild");
				VisualStudioInstance? vsInst = MSBuildLocator.QueryVisualStudioInstances()
					.OrderByDescending(inst => inst.Version)
					.First();
				MSBuildLocator.RegisterInstance(vsInst);
				taskProgress.ReportStatus($"Found MSBuild {vsInst.Version} at {vsInst.MSBuildPath}", overwrite: false);
				MSBuildFound = true;
			}

			return MSBuildWorkspace.Create();
		}

		protected abstract ITaskProgress GetTaskProgress(IProgress progress);

		protected abstract Task<Document> Process(Document doc, CancellationToken cancellationToken = default);
	}
}