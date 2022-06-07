#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using UtfUnknown;
using static tModPorter.ProgressUpdate;

namespace tModPorter;

public class tModPorter
{
	private int documentCount;
	private int pass = 1;
	private int docsCompletedThisPass = 0;

	public bool DryRun { get; }
	public bool? MakeBackups { get; private set; }

	public tModPorter(bool dryRun = false, bool? makeBackups = null) {
		DryRun = dryRun;
		MakeBackups = makeBackups;
	}

	public async Task ProcessProject(string projectPath, Action<ProgressUpdate>? updateProgress = null) {
		MakeBackups ??= !IsUnderGit(projectPath);
		updateProgress ??= _ => { };
		var start = DateTime.Now;

		var obj_folder = Path.Combine(Path.GetDirectoryName(projectPath)!, "obj");
		if (Directory.Exists(obj_folder)) {
			try {
				Directory.Delete(obj_folder, true);
			} catch (Exception e) {
				updateProgress(new Warning(e.Message));
			}
		}

		MSBuildLocator.RegisterDefaults();

		using MSBuildWorkspace workspace = MSBuildWorkspace.Create();
		workspace.WorkspaceFailed += (o, e) => {
			if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
				throw new Exception(e.Diagnostic.ToString());

			updateProgress(new Warning(e.Diagnostic.ToString()));
		};

		var project = await workspace.OpenProjectAsync(projectPath, new ProjectLoadProgressAdapter(updateProgress));
		var updatedProject = await Process(project, updateProgress);
		var changedDocs = updatedProject.GetChanges(project).GetChangedDocuments().Count();
		updateProgress(new Complete(pass, changedDocs, documentCount, DateTime.Now - start));
	}

	public async Task<Project> Process(Project project, Action<ProgressUpdate> updateProgress) {
		documentCount = project.Documents.Count();

		while (true) {
			docsCompletedThisPass = 0;
			updateProgress(new Progress(pass, docsCompletedThisPass, documentCount));

			var tasks = project.Documents.Select(doc => Task.Run(async () => (doc.Id, root: await Process(doc, updateProgress)))).ToArray();
			var changed = (await Task.WhenAll(tasks)).Where(e => e.root != null).ToArray();
			if (!changed.Any())
				break;

			var sln = project.Solution;
			foreach (var (docId, root) in changed) {
				sln = sln.WithDocumentSyntaxRoot(docId, root!, PreservationMode.PreserveIdentity);
			}
			project = sln.GetProject(project.Id)!;
			pass++;
		}

		return project;
	}

	private async Task<SyntaxNode?> Process(Document doc, Action<ProgressUpdate> updateProgress) {
		var newDoc = await Rewrite(doc);
		if (newDoc != doc) {
			await Update(newDoc, updateProgress);
			updateProgress(new FileUpdated(doc.Name));
		}

		Interlocked.Increment(ref docsCompletedThisPass);
		updateProgress(new Progress(pass, docsCompletedThisPass, documentCount));

		return newDoc == doc ? null : await newDoc.GetSyntaxRootAsync();
	}

	protected virtual async Task Update(Document doc, Action<ProgressUpdate> updateProgress) {
		if (DryRun)
			return;

		var path = doc.FilePath ?? throw new NullReferenceException("No path? " + doc?.Name);

		Encoding encoding;
		using (Stream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
			DetectionResult detectionResult = CharsetDetector.DetectFromStream(fs);
			encoding = detectionResult.Detected.Encoding;
			if (detectionResult.Detected.Confidence < .95f)
				updateProgress(new Warning($"Less than 95% confidence about the file encoding of: {doc.FilePath}"));
		}

		if (MakeBackups!.Value) {
			int i = 2;
			string backupPath = $"{path}.bak";
			while (File.Exists(backupPath)) {
				backupPath = $"{path}.bak{i++}";
			}
			File.Move(path, backupPath);
		}

		await File.WriteAllTextAsync(path, (await doc.GetTextAsync()).ToString(), encoding);
	}

	public static async Task<Document> Rewrite(Document doc) {
		Document prevDoc;
		int i = 0;
		do {
			prevDoc = doc;
			foreach (var rewriter in Config.CreateRewriters()) {
				doc = await rewriter.Rewrite(doc);
			}
		} while (doc != prevDoc && ++i < 1000);

#if DEBUG
		if (i == 1000)
			throw new Exception("Infinite recursion");
#endif

		return doc;
	}

	private static bool IsUnderGit(string path) => Path.GetDirectoryName(path) is string parent && (Directory.Exists(Path.Combine(parent, ".git")) || IsUnderGit(parent));

	private class ProjectLoadProgressAdapter : IProgress<ProjectLoadProgress> {
		private Action<ProgressUpdate> updateProgress;

		public ProjectLoadProgressAdapter(Action<ProgressUpdate> updateProgress) {
			this.updateProgress = updateProgress;
		}

		public void Report(ProjectLoadProgress loadProgress) {
			string? pathAndFramework = Path.GetFileName(loadProgress.FilePath);
			if (loadProgress.TargetFramework != null) pathAndFramework += $" ({loadProgress.TargetFramework})";

			updateProgress(new ProjectLoading(loadProgress.Operation.ToString(), loadProgress.ElapsedTime, pathAndFramework));
		}
	}
}