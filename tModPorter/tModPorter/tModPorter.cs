#nullable enable
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
	private int documentsThisPass;
	private int pass;
	private int docsCompletedThisPass;

	public bool DryRun { get; }
	public bool? MakeBackups { get; private set; }

	public tModPorter(bool dryRun = false, bool? makeBackups = null) {
		DryRun = dryRun;
		MakeBackups = makeBackups;
	}

	private static bool TreatErrorAsWarning(WorkspaceDiagnostic diagnostic)
	{
		var msg = diagnostic.Message;
		if (Regex.IsMatch(msg, "This mismatch may cause runtime failures")) return true;
		if (Regex.IsMatch(msg, "has a known \\w+ severity vulnerability")) return true;
		return false;
	}

	public async Task ProcessProject(string projectPath, Action<ProgressUpdate>? updateProgress = null) {

		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

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
			if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure && !TreatErrorAsWarning(e.Diagnostic))
				throw new Exception(e.Diagnostic.ToString());

			updateProgress(new Warning(e.Diagnostic.ToString()));
		};

		var project = await workspace.OpenProjectAsync(projectPath, new ProjectLoadProgressAdapter(updateProgress));
		var updatedProject = await Process(project, updateProgress);
		var changedDocs = updatedProject.GetChanges(project).GetChangedDocuments().Count();
		updateProgress(new Complete(pass, changedDocs, project.Documents.Count(), DateTime.Now - start));
	}

	public async Task<Project> Process(Project project, Action<ProgressUpdate> updateProgress) {
		pass = 0;
		bool finishingPass = false;

		// The most expensive operation is updating the semantic model, so we run a system where all docs in a pass come from the same project
		// As soon as a document syntax root gets changed, and its semantic model would need to be re-generated, we bail and re-queue it
		// When no more docus change, we check the remaining docs to see if there are any cross-document dependencies.
		var docs = project.Documents.ToArray();
		while (true) {
			pass++;
			docsCompletedThisPass = 0;
			documentsThisPass = docs.Length;
			updateProgress(new Progress(pass, docsCompletedThisPass, documentsThisPass));

			var tasks = docs.Select(doc => Task.Run(async () => (doc.Id, root: await Process(doc, updateProgress)))).ToArray();
			var changed = (await Task.WhenAll(tasks)).Where(e => e.root != null).ToArray();
			if (!changed.Any()) {
				if (finishingPass) {
					break;
				}

				finishingPass = true;
				docs = project.Documents.Except(docs).ToArray(); // rerun on all the other documents
				if (docs.Length == 0)
					break;

				continue;
			}

			var sln = project.Solution;
			foreach (var (docId, root) in changed) {
				sln = sln.WithDocumentSyntaxRoot(docId, root!, PreservationMode.PreserveIdentity);
			}
			project = sln.GetProject(project.Id)!;

			// rerun on just the documents which changed this pass
			var changedIds = changed.Select(e => e.Id).ToHashSet();
			docs = project.Documents.Where(doc => changedIds.Contains(doc.Id)).ToArray();
			finishingPass = false;
		}

		return project;
	}

	private async Task<SyntaxNode?> Process(Document doc, Action<ProgressUpdate> updateProgress) {
		try {
			var newDoc = await RewriteOnce(doc);
			if (newDoc != doc) {
				await Update(newDoc, updateProgress);
				updateProgress(new FileUpdated(doc.Name));
			}

			Interlocked.Increment(ref docsCompletedThisPass);
			updateProgress(new Progress(pass, docsCompletedThisPass, documentsThisPass));

			return newDoc == doc ? null : await newDoc.GetSyntaxRootAsync();
		}
		catch (Exception ex) {
			updateProgress(new Error(doc.Name, ex));
			return null;
		}
	}

	protected virtual async Task Update(Document doc, Action<ProgressUpdate> updateProgress) {
		if (DryRun)
			return;

		var path = doc.FilePath ?? throw new NullReferenceException("No path? " + doc?.Name);

		// TODO, store encoding with the document ids and calculate it on first read
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

	public static async Task<Document> RewriteOnce(Document doc) {
		var prevDoc = doc;
		foreach (var rewriter in Config.CreateRewriters()) {
			doc = await rewriter.Rewrite(doc);
			if (doc != prevDoc) return doc;
		}

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